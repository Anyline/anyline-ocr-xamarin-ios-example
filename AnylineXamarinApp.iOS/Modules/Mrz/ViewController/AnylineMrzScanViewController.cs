 using Foundation;
using System;
using UIKit;
using AnylineXamarinSDK.iOS;
using CoreGraphics;

namespace AnylineXamarinApp.iOS
{
    public class AnylineMrzScanViewController : UIViewController, IAnylineMRZModuleDelegate
    {
        string licenseKey = AnylineViewController.LICENSE_KEY;

        AnylineMRZModuleView anylineMrzView;
        AnylineIdentificationView idView;
        NSError error;
        bool success = false;
        bool isScanning = false;
        UIAlertView alert;
        
        public AnylineMrzScanViewController (String name)
        {
            this.Title = name;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Initializing the MRZ module.
            CGRect frame = UIScreen.MainScreen.ApplicationFrame;
            frame = new CGRect(frame.X,
                frame.Y + NavigationController.NavigationBar.Frame.Size.Height,
                frame.Width,
                frame.Height - NavigationController.NavigationBar.Frame.Size.Height);

            anylineMrzView = new AnylineMRZModuleView(frame);
        
            error = null;
            // We tell the module to bootstrap itself with the license key and delegate. The delegate will later get called
            // by the module once we start receiving results.
            success = anylineMrzView.SetupWithLicenseKey(licenseKey, this.Self, out error);
            // SetupWithLicenseKey:delegate:error returns true if everything went fine. In the case something wrong
            // we have to check the error object for the error message.
            if (!success)
            {
                // Something went wrong. The error object contains the error description
                (alert = new UIAlertView("Error", error.DebugDescription, null, "OK", null)).Show();
            }

            //we'll manually cancel scanning
            anylineMrzView.CancelOnResult = false;

            // After setup is complete we add the module to the view of this view controller
            View.AddSubview(anylineMrzView);

            /*
             ALIdentificationView will present the scanned values. Here we start listening for taps
             to later dismiss the view.
             */
            idView = new AnylineIdentificationView(new CGRect(0,0,300,300/1.4f));
            idView.AddGestureRecognizer(new UITapGestureRecognizer(this, new ObjCRuntime.Selector("ViewTapSelector:")));
            
            idView.Center = View.Center;
            idView.Alpha = 0;
            
            View.AddSubview(idView);
        }

        /*
         Dismiss the view if the user taps the screen
         */
        [Export("ViewTapSelector:")]
        protected void AnimateFadeOut(UIGestureRecognizer sender)
        {
            idView.Transform = CGAffineTransform.MakeScale((nfloat)1, (nfloat)1);
            if (idView.Alpha == 1.0)
            {
                UIView.Animate(0.5, 0, UIViewAnimationOptions.CurveEaseInOut, () =>
                {
                    idView.Alpha = 0;
                    idView.Transform = CGAffineTransform.MakeScale((nfloat)0, (nfloat)0);
                }, () => StartAnyline());
            }
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

        /*
         A little animation for the user to see the scanned document.
         */
        private void AnimateFadeIn()
        {
            StopAnyline();

            //View.BringSubviewToFront(idView);

            //properties before animation
            idView.Center = View.Center;
            idView.Alpha = 0;
            idView.Transform = CGAffineTransform.MakeScale ((nfloat)0.2, (nfloat)0.2);

            UIView.Animate(0.3, 0, UIViewAnimationOptions.CurveEaseInOut,
                    () =>
                    {
                        idView.Alpha = 1;
                        idView.Transform = CGAffineTransform.MakeScale((nfloat)1.1, (nfloat)1.1);
                    },
                    () =>
                    {
                        UIView.Animate(0.2, 0, UIViewAnimationOptions.CurveEaseInOut,
                            () =>
                            {
                                idView.Transform = CGAffineTransform.MakeScale((nfloat)1, (nfloat)1);
                            }, null);
                    }
            );
        }

        public void StartAnyline()
        {
            if (isScanning) return;

            //send the identification view to the back before we start scanning
            View.SendSubviewToBack(idView);

            error = null;
            success = anylineMrzView.StartScanningAndReturnError(out error);
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
            if (!anylineMrzView.CancelScanningAndReturnError(out error))
            {
                (alert = new UIAlertView("Error", error.DebugDescription, null, "OK", null)).Show();
            }
            else
                isScanning = false;

            View.BringSubviewToFront(idView);
        }

        /*
        This is the main delegate method Anyline uses to report its results
        */
        public void DidFindScanResult(AnylineMRZModuleView anylineMRZModuleView, ALIdentification scanResult, bool allCheckDigitsValid, UIImage image)
        {
            // Because there is a lot of information to be passed along the module
            // uses ALIdentification.
            idView.UpdateIdentification(scanResult);
            
            // Present the information to the user
            AnimateFadeIn();
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

            //remove identification view
            idView.RemoveFromSuperview();
            idView.Dispose();
            idView = null;

            //we have to erase the scan view so that there are no dependencies for the viewcontroller left.
            anylineMrzView.RemoveFromSuperview();
            anylineMrzView.Dispose();
            anylineMrzView = null;

            base.Dispose();
        }        
    }
}
