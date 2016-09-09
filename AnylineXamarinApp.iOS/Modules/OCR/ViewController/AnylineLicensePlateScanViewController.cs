using Foundation;
using System;
using UIKit;
using AnylineXamarinSDK.iOS;
using CoreGraphics;

namespace AnylineXamarinApp.iOS
{
    public class AnylineLicensePlateScanViewController : UIViewController, IAnylineOCRModuleDelegate
    {
        string licenseKey = AnylineViewController.LICENSE_KEY;

        AnylineOCRModuleView scanView;
        ResultOverlayView resultView;

        protected ALOCRConfig ocrConfig;

        NSError error;
        bool success = false;
        bool isScanning = false;
        UIAlertView alert;

        protected AnylineLicensePlateScanViewController() { }

        public AnylineLicensePlateScanViewController(String name)
        {
            this.Title = name;
        }

        public virtual void SetupLicensePlateConfig()
        {
            // We'll define the OCR Config here:
            ocrConfig = new ALOCRConfig();
            ocrConfig.CustomCmdFilePath = NSBundle.MainBundle.PathForResource(@"Modules/OCR/license_plates", @"ale");
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Initializing the Voucher Code module.
            CGRect frame = UIScreen.MainScreen.ApplicationFrame;
            frame = new CGRect(frame.X,
                frame.Y + NavigationController.NavigationBar.Frame.Size.Height,
                frame.Width,
                frame.Height - NavigationController.NavigationBar.Frame.Size.Height);

            scanView = new AnylineOCRModuleView(frame);

            SetupLicensePlateConfig();

            // We'll copy the required traineddata files here
            error = null;
            success = scanView.CopyTrainedData(NSBundle.MainBundle.PathForResource(@"Modules/OCR/Alte", @"traineddata"),
                @"f52e3822cdd5423758ba19ed75b0cc32", out error);
            if (!success)
            {
                (alert = new UIAlertView("Error", error.DebugDescription, null, "OK", null)).Show();
            }
            error = null;
            success = scanView.CopyTrainedData(NSBundle.MainBundle.PathForResource(@"Modules/OCR/Arial", @"traineddata"),
                @"9a5555eb6ac51c83cbb76d238028c485", out error);
            if (!success)
            {
                (alert = new UIAlertView("Error", error.DebugDescription, null, "OK", null)).Show();
            }
            error = null;
            success = scanView.CopyTrainedData(NSBundle.MainBundle.PathForResource(@"Modules/OCR/GL-Nummernschild-Mtl7_uml", @"traineddata"),
                @"8ea050e8f22ba7471df7e18c310430d8", out error);
            if (!success)
            {
                (alert = new UIAlertView("Error", error.DebugDescription, null, "OK", null)).Show();
            }
            
            // We tell the module to bootstrap itself with the license key and delegate. The delegate will later get called
            // by the module once we start receiving results.
            error = null;
            success = scanView.SetupWithLicenseKey(licenseKey, this.Self, ocrConfig, out error);
            // SetupWithLicenseKey:delegate:error returns true if everything went fine. In the case something wrong
            // we have to check the error object for the error message.
            if (!success)
            {
                // Something went wrong. The error object contains the error description
                (alert = new UIAlertView("Error", error.DebugDescription, null, "OK", null)).Show();
            }

            // We stop scanning manually
            scanView.CancelOnResult = false;

            // We load the UI config for our VoucherCode view from a .json file.
            String configFile = NSBundle.MainBundle.PathForResource(@"Modules/OCR/license_plate_view_config", @"json");
            scanView.CurrentConfiguration = ALUIConfiguration.CutoutConfigurationFromJsonFile(configFile);
            scanView.TranslatesAutoresizingMaskIntoConstraints = false;

            // After setup is complete we add the module to the view of this view controller
            View.AddSubview(scanView);

            /*
             The following view will present the scanned values. Here we start listening for taps
             to later dismiss the view.
             */
            resultView = new ResultOverlayView(new CGRect(0, 0, View.Frame.Width, View.Frame.Height), UIImage.FromBundle(@"drawable/license_plate_background.png"));
            resultView.AddGestureRecognizer(new UITapGestureRecognizer(this, new ObjCRuntime.Selector("ViewTapSelector:")));

            resultView.Center = View.Center;
            resultView.Alpha = 0;

            resultView.Result.Center = new CGPoint(View.Center.X, View.Center.Y);
            resultView.Result.Font = UIFont.BoldSystemFontOfSize(27);
            resultView.Result.TextColor = UIColor.Black;

            View.AddSubview(resultView);
        }

        /*
         Dismiss the view if the user taps the screen
         */
        [Export("ViewTapSelector:")]
        protected void AnimateFadeOut(UIGestureRecognizer sender)
        {
            resultView.AnimateFadeOut(this.View, StartAnyline);
        }

        /*
         This method will be called once the view controller and its subviews have appeared on screen
         */
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            
            // We use this subroutine to start Anyline. The reason it has its own subroutine is
            // so that we can later use it to restart the scanning process.
            StartAnyline();
        }

        public void StartAnyline()
        {
            if (isScanning) return;

            //send the result view to the back before we start scanning
            View.SendSubviewToBack(resultView);

            error = null;
            success = scanView.StartScanningAndReturnError(out error);
            if (!success)
            {
                (alert = new UIAlertView("Error", error.DebugDescription, null, "OK", null)).Show();
            }
            else
                isScanning = true;
        }

        public void StopAnyline()
        {
            if (!isScanning) return;

            error = null;
            if (!scanView.CancelScanningAndReturnError(out error))
            {
                (alert = new UIAlertView("Error", error.DebugDescription, null, "OK", null)).Show();
            }
            else
                isScanning = false;

            View.BringSubviewToFront(resultView);
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

            //remove result view
            resultView.RemoveFromSuperview();
            resultView.Dispose();
            resultView = null;

            //we have to erase the scan view so that there are no dependencies for the viewcontroller left.
            scanView.RemoveFromSuperview();
            scanView.Dispose();
            scanView = null;

            base.Dispose();
        }


        /*
        This is the main delegate method Anyline uses to report its results
        */
        void IAnylineOCRModuleDelegate.DidFindResult(AnylineOCRModuleView anylineOCRModuleView, ALOCRResult result)
        {
            
            StopAnyline();
            View.BringSubviewToFront(resultView);

            resultView.UpdateResult(result.Text);

            // Present the information to the user
            resultView.AnimateFadeIn(this.View);
        }

        void IAnylineOCRModuleDelegate.ReportsRunFailure(AnylineOCRModuleView anylineOCRModuleView, ALOCRError error)
        {
            switch (error)
            {
                case ALOCRError.ConfidenceNotReached:
                    Console.WriteLine("Confidence not reached.");
                    break;
                case ALOCRError.NoLinesFound:
                    Console.WriteLine("No lines found.");
                    break;
                case ALOCRError.NoTextFound:
                    Console.WriteLine("No text found.");
                    break;
                case ALOCRError.ResultNotValid:
                    Console.WriteLine("Result is not valid.");
                    break;
                case ALOCRError.SharpnessNotReached:
                    Console.WriteLine("Sharpness is not reached.");
                    break;
                case ALOCRError.Unkown:
                    Console.WriteLine("Unknown run error.");
                    break;
            }
        }

        void IAnylineOCRModuleDelegate.ReportsVariable(AnylineOCRModuleView anylineOCRModuleView, string variableName, NSObject value) { }

        bool IAnylineOCRModuleDelegate.TextOutlineDetected(AnylineOCRModuleView anylineOCRModuleView, ALSquare outline) { return false; }
    }
}
