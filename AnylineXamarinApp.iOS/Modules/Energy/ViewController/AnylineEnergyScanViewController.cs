using Foundation;
using System;
using UIKit;
using AnylineXamarinSDK.iOS;
using CoreGraphics;
using System.Collections.Generic;
using System.Linq;

namespace AnylineXamarinApp.iOS
{
    public class AnylineEnergyScanViewController : UIViewController, IAnylineEnergyModuleDelegate
    {
        string licenseKey = AnylineViewController.LICENSE_KEY;

        AnylineEnergyModuleView anylineEnergyView;
        UISegmentedControl meterTypeSegment;
        UIAlertView alert;
        CGRect frame;
        NSError error;
        bool success;
        bool isScanning = false;
        bool keepScanViewControllerAlive = false;

        UILabel selectionLabel;
        UILabel infoLabel;
        string labelText;
        int defaultIndex = 0;

        Dictionary<String, ALScanMode> segmentItems;

        public AnylineEnergyScanViewController(String name, Dictionary<String,ALScanMode> segmentItems, string labelText, int defaultIndex)
        {
            this.Title = name;
            this.segmentItems = segmentItems;
            this.labelText = labelText;
            this.defaultIndex = defaultIndex;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Initializing the energy module.
            frame = UIScreen.MainScreen.ApplicationFrame;
            frame = new CGRect(frame.X,
                frame.Y + NavigationController.NavigationBar.Frame.Size.Height,
                frame.Width,
                frame.Height - NavigationController.NavigationBar.Frame.Size.Height);

            anylineEnergyView = new AnylineEnergyModuleView(frame);
            
            error = null;
            success = anylineEnergyView.SetupWithLicenseKey(licenseKey, this.Self, out error);
            if (!success)
            {
                (alert = new UIAlertView("Error", error.DebugDescription, null, "OK", null)).Show();
            }

            //we'll stop scanning manually
            anylineEnergyView.CancelOnResult = false;
            
            // After setup is complete we add the module to the view of this view controller
            View.AddSubview(anylineEnergyView);

            anylineEnergyView.ScanMode = segmentItems.ElementAt(defaultIndex).Value;
            
            //we don't need a segment control for only one option:
            if (segmentItems.Count > 1)
            {
                meterTypeSegment = new UISegmentedControl(segmentItems.Keys.ToArray());
                
                //adjust the segmentcontrol size so all elements fit to the view
                if (meterTypeSegment.NumberOfSegments > 3)
                {
                    for (int i = 0; i < meterTypeSegment.NumberOfSegments; i++)
                        meterTypeSegment.SetWidth(View.Frame.Width / (meterTypeSegment.NumberOfSegments + 1), i);
                    
                    meterTypeSegment.ApportionsSegmentWidthsByContent = true;
                }
                meterTypeSegment.Center = new CGPoint(View.Center.X, View.Frame.Size.Height - 40);

                meterTypeSegment.SelectedSegment = this.defaultIndex;
                meterTypeSegment.ValueChanged += HandleSegmentChange;
                View.AddSubview(meterTypeSegment);

                selectionLabel = new UILabel(new CGRect(meterTypeSegment.Frame.X, meterTypeSegment.Frame.Y - 35, meterTypeSegment.Frame.Width, 35));
                selectionLabel.TextColor = UIColor.White;
                selectionLabel.Text = labelText;
                View.AddSubview(selectionLabel);

                infoLabel = new UILabel(new CGRect(0, View.Frame.Y + NavigationController.NavigationBar.Frame.Size.Height + 13, View.Frame.Width, 35));
                infoLabel.TextColor = UIColor.White;
                infoLabel.Text = "";
                infoLabel.TextAlignment = UITextAlignment.Center;
                View.AddSubview(infoLabel);

                UpdateInfoLabel(anylineEnergyView.ScanMode);
            
            }
        }

        private void HandleSegmentChange(object sender, EventArgs e)
        {
            var selectedSegmentId = (sender as UISegmentedControl).SelectedSegment;

            var scanMode = segmentItems.ElementAt((int)selectedSegmentId).Value;
            Console.WriteLine("Scanmode: {0}", scanMode.ToString());

            UpdateInfoLabel(scanMode);
            anylineEnergyView.ScanMode = scanMode;

        }
        
        //update the info text for certain energy scan modes
        private void UpdateInfoLabel(ALScanMode scanMode)
        {
            var desc = "";
            switch (scanMode)
            {
                case ALScanMode.ALAnalogMeter4:
                    desc = "4 pre-decimal places";
                    break;
                case ALScanMode.ALElectricMeter:
                    desc = "5 or 6 pre-decimal places";
                    break;
                case ALScanMode.ALGasMeter:
                case ALScanMode.ALElectricMeter5_1:
                    desc = "5 pre-decimal places";
                    break;
                case ALScanMode.ALGasMeter6:
                case ALScanMode.ALElectricMeter6_1:
                    desc = "6 pre-decimal places";
                    break;
                case ALScanMode.ALAnalogMeter7:
                    desc = "7 pre-decimal places";
                    break;
                case ALScanMode.ALAnalogMeterWhite:
                    desc = "5 or 6 pre-decimal (white background)";
                    break;
                default:
                    break;
            }
            infoLabel.Text = desc;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            StartAnyline();

            //we want to kill the controller if we navigate back to the previous viewcontroller
            keepScanViewControllerAlive = false;
        }

        /*
         * This is the place where we tell Anyline to start receiving and displaying images from the camera.
         * Success/error tells us if everything went fine.
         */
        public void StartAnyline()
        {
            if (isScanning) return;
            
            error = null;
            success = anylineEnergyView.StartScanningAndReturnError(out error);

            if (!success)
            {
                (alert = new UIAlertView("Error", error.DebugDescription, null, "OK", null)).Show();
            }
            else
                isScanning = true;
        }

        /*
         * We'll stop scanning and if something goes wrong, we display it as an alert.
         */
        public void StopAnyline()
        {
            if (!isScanning) return;

            error = null;
            success = anylineEnergyView.CancelScanningAndReturnError(out error);

            if (!success)
            {
                (alert = new UIAlertView("Error", error.DebugDescription, null, "OK", null)).Show();
            }
            else
                isScanning = false;
        }

        /*
         * This is the main delegate method Anyline uses to report its scanned codes
         */
        void IAnylineEnergyModuleDelegate.DidFindScanResult(AnylineEnergyModuleView anylineEnergyModuleView, string scanResult, UIImage image, UIImage fullImage, ALScanMode scanMode)
        {
            StopAnyline();

            //we'll go to a temporary new view controller, so we keep this one alive
            keepScanViewControllerAlive = true;

            try
            {
                AnylineEnergyScanViewResultController vc = new AnylineEnergyScanViewResultController(scanResult, scanMode);
                vc.MeterImage = image;

                NavigationController.PushViewController(vc, true);
            }
            catch (Exception) { }
        }
        
        public override void ViewWillDisappear(bool animated)
        {
         	base.ViewWillDisappear(animated);

            // We have to stop scanning before the view dissapears
            StopAnyline();

        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            //don't clean up objects, if we want the controller to be kept alive
            if (keepScanViewControllerAlive) return;

            //we have to un-register the event handlers because else the whole viewcontroller will be kept in the garbage collector.
            if (meterTypeSegment != null) meterTypeSegment.ValueChanged -= HandleSegmentChange;

            if (meterTypeSegment != null)
                meterTypeSegment.Dispose();
            meterTypeSegment = null;

            if (selectionLabel != null)
                selectionLabel.Dispose();
            selectionLabel = null;

            if (infoLabel != null)
                infoLabel.Dispose();
            infoLabel = null;

            segmentItems = null;
            
            if (alert != null)
                alert.Dispose();
            alert = null;

            if (error != null)
                error.Dispose();
            error = null;

            //we have to erase the scan view so that there are no dependencies for the viewcontroller left.
            anylineEnergyView.RemoveFromSuperview();
            anylineEnergyView.Dispose();
            anylineEnergyView = null;

            base.Dispose();
        }
    }
}
