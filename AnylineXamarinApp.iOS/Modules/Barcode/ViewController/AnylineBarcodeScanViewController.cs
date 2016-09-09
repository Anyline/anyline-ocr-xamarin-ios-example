using Foundation;
using System;
using UIKit;
using AnylineXamarinSDK.iOS;
using CoreGraphics;

namespace AnylineXamarinApp.iOS
{
    public class AnylineBarcodeScanViewController : UIViewController, IAnylineBarcodeModuleDelegate
    {
        string licenseKey = AnylineViewController.LICENSE_KEY;

        AnylineBarcodeModuleView anylineBarcodeView;
        UILabel resultLabel;
        UIAlertView alert;
        NSError error;
        bool success;
        CGRect frame;
        
        public AnylineBarcodeScanViewController(String name)
        {
            this.Title = name;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Initializing the barcode module.
            frame = UIScreen.MainScreen.ApplicationFrame;
            frame = new CGRect(frame.X,
                frame.Y + NavigationController.NavigationBar.Frame.Size.Height,
                frame.Width,
                frame.Height - NavigationController.NavigationBar.Frame.Size.Height);

            anylineBarcodeView = new AnylineBarcodeModuleView(frame);

            error = null;
            success = anylineBarcodeView.SetupWithLicenseKey(licenseKey, this.Self, out error);

            if (!success)
            {
                (alert = new UIAlertView("Error", error.DebugDescription, null, "OK", null)).Show();
            }

            // Anyline will stop searching for new results once it found a valid code. Here we tell it to continue scanning
            // after it found and reported the result.
            anylineBarcodeView.CancelOnResult = false;

            anylineBarcodeView.BeepOnResult = true;

            // After setup is complete we add the module to the view of this view controller
            View.AddSubview(anylineBarcodeView);
    
            // The resultLabel is used as a debug view to see the scanned results. We set its text
            // in anylineBarcodeModuleView:didFindScanResult:atImage below
            resultLabel = new UILabel(new CGRect(0,View.Frame.Size.Height-100,View.Frame.Size.Width, 50));

            resultLabel.TextAlignment = UITextAlignment.Center;
            resultLabel.TextColor = UIColor.White;
            resultLabel.Font = UIFont.FromName(@"HelveticaNeue-UltraLight",35);
            resultLabel.AdjustsFontSizeToFitWidth = true;

            View.AddSubview(resultLabel);

        }

        /*
         * This method will be called once the view controller and its subviews have appeared on screen
         */
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            /*
             This is the place where we tell Anyline to start receiving and displaying images from the camera.
             Success/error tells us if everything went fine.
             */
            error = null;
            success = anylineBarcodeView.StartScanningAndReturnError(out error);
            
            if (!success)
            {
                // Something went wrong. The error object contains the error description
                (alert = new UIAlertView(@"Start Scanning Error", error.DebugDescription, null, "OK", null)).Show();
            }
        }

        /*
         * This is the main delegate method Anyline uses to report its scanned codes
         */
        public void DidFindScanResult(AnylineBarcodeModuleView anylineBarcodeModuleView, string scanResult, string barcodeFormat, UIImage image)
        {
            resultLabel.Text = scanResult;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            // We have to stop scanning before the view dissapears
            error = null;
            if (!anylineBarcodeView.CancelScanningAndReturnError(out error))
            {
                (alert = new UIAlertView("Error", error.DebugDescription, null, "OK", null)).Show();
            }
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            //un-register any event handlers here, if you have any
            
            //we have to erase the scan view so that there are no dependencies for the viewcontroller left.
            anylineBarcodeView.RemoveFromSuperview();
            anylineBarcodeView.Dispose();
            anylineBarcodeView = null;

            base.Dispose();
        }        
    }
}