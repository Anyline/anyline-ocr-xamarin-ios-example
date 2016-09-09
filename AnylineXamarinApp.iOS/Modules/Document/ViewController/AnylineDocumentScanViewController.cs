using Foundation;
using System;
using UIKit;
using AnylineXamarinSDK.iOS;
using CoreGraphics;

namespace AnylineXamarinApp.iOS
{
    public class AnylineDocumentScanViewController : UIViewController, IAnylineDocumentModuleDelegate
    {
        string licenseKey = AnylineViewController.LICENSE_KEY;

        AnylineDocumentModuleView scanView;
        NotificationView notificationView;
        UIImage resultImage;

        bool showingNotification = false;
        bool keepScanViewControllerAlive = false;

        NSError error;
        bool success = false;
        UIAlertView alert;

        public AnylineDocumentScanViewController(String name)
        {
            this.Title = name;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Initializing the Bottlecap scan module.
            CGRect frame = UIScreen.MainScreen.ApplicationFrame;
            frame = new CGRect(frame.X,
                frame.Y + NavigationController.NavigationBar.Frame.Size.Height,
                frame.Width,
                frame.Height - NavigationController.NavigationBar.Frame.Size.Height);

            scanView = new AnylineDocumentModuleView(frame);

            // We tell the module to bootstrap itself with the license key and delegate. The delegate will later get called
            // by the module once we start receiving results.
            error = null;
            success = scanView.SetupWithLicenseKey(licenseKey, this.Self, out error);
            // SetupWithLicenseKey:delegate:error returns true if everything went fine. In the case something wrong
            // we have to check the error object for the error message.
            if (!success)
            {
                // Something went wrong. The error object contains the error description
                (alert = new UIAlertView("Error", error.DebugDescription, null, "OK", null)).Show();
            }

            //stop scanning on result
            scanView.CancelOnResult = true;

            scanView.TranslatesAutoresizingMaskIntoConstraints = false;

            // After setup is complete we add the module to the view of this view controller
            View.AddSubview(scanView);

            // This view notifies the user of any problems that occur while scanning
            notificationView = new NotificationView(new CGRect(20, 115, View.Bounds.Width - 40, 30));
            notificationView.fillColor = new UIColor((nfloat)(98.0 / 255.0), (nfloat)(39.0 / 255.0), (nfloat)(232.0 / 255.0), (nfloat)(0.6)).CGColor;
            notificationView.textLabel.Text = "";
            notificationView.Alpha = 0;
            View.Add(notificationView);

        }

        /*
         This method will be called once the view controller and its subviews have appeared on screen
         */
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            //we want to kill the controller if we navigate back to the previous viewcontroller
            keepScanViewControllerAlive = false;

            // We use this subroutine to start Anyline. The reason it has its own subroutine is
            // so that we can later use it to restart the scanning process.
            StartAnyline();
        }

        public void StartAnyline()
        {
            error = null;
            success = scanView.StartScanningAndReturnError(out error);
            if (!success)
            {
                (alert = new UIAlertView("Error", error.DebugDescription, null, "OK", null)).Show();
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            // We have to stop scanning before the view dissapears
            error = null;
            scanView.CancelScanningAndReturnError(out error);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            //don't clean up objects, if we want the controller to be kept alive
            if (keepScanViewControllerAlive)
                return;

            //remove notification view
            if (notificationView != null)
            {
                notificationView.RemoveFromSuperview();
                notificationView.Dispose();
                notificationView = null;
            }
            //remove image
            if (resultImage != null)
            {
                resultImage.Dispose();
                resultImage = null;
            }
            //we have to erase the scan view so that there are no dependencies for the viewcontroller left.
            scanView.RemoveFromSuperview();
            scanView.Dispose();
            scanView = null;

            base.Dispose();
        }

        /*
         A little helper method to show live scanning errors.
         */
        private void ShowNotification<T>(T info)
        {
            String txt = "";

            if (typeof(T) == typeof(ALDocumentError))
            {
                ALDocumentError error = (ALDocumentError)Convert.ChangeType(info, typeof(ALDocumentError));
                switch (error)
                {
                    case ALDocumentError.ImageTooDark:
                        txt = "Too dark";
                        break;
                    case ALDocumentError.NotSharp:
                        txt = "Document not sharp";
                        break;
                    case ALDocumentError.ShakeDetected:
                        txt = "Shake detected";
                        break;
                    case ALDocumentError.SkewTooHigh:
                        txt = "Wrong perspective";
                        break;
                    default:
                        break;
                }
            }

            if (typeof(T) == typeof(String))
            {
                txt = (String)Convert.ChangeType(info, typeof(String));
            }

            if (showingNotification || txt.Equals(""))
                return;

            showingNotification = true;
            notificationView.textLabel.Text = txt;

            //fade in
            UIView.Animate(0.8, 0, UIViewAnimationOptions.CurveEaseInOut, () =>
            {
                notificationView.Alpha = 1;
            }, () => //fade out
            {
                UIView.Animate(0.8, 0, UIViewAnimationOptions.CurveEaseInOut, () =>
                {
                    notificationView.Alpha = 0;
                }, () => { showingNotification = false; });
            });
        }

        bool IAnylineDocumentModuleDelegate.AnglesValid(AnylineDocumentModuleView anylineDocumentModuleView, ALSquare outline, bool anglesValid) { return false; }

        /*
         This method is called when a result has been found. We'll show the transformed image on a new viewController.
         */
        void IAnylineDocumentModuleDelegate.HasResult(AnylineDocumentModuleView anylineDocumentModuleView, UIImage transformedImage, UIImage fullFrame)
        {
            //we'll go to a temporary new view controller, so we keep this one alive
            keepScanViewControllerAlive = true;

            this.resultImage = transformedImage;

            using (UIViewController vc = new UIViewController())
            {
                UIImageView iv = new UIImageView(scanView.Frame);
                iv.Image = resultImage;
                iv.ContentMode = UIViewContentMode.ScaleAspectFit;
                vc.View.AddSubview(iv);

                this.NavigationController.PushViewController(vc, true);
            }

            if (fullFrame != null)
                fullFrame.Dispose();
            if (transformedImage != null)
                transformedImage.Dispose();

            fullFrame = null;
            transformedImage = null;
        }

        /*
         This method receives preview errors that occured during the scan.
         */
        void IAnylineDocumentModuleDelegate.ReportsPreviewProcessingFailure(AnylineDocumentModuleView anylineDocumentModuleView, ALDocumentError error)
        {
            ShowNotification<ALDocumentError>(error);
        }

        /*
         This method receives picture errors that occured during the scan.
         */
        void IAnylineDocumentModuleDelegate.ReportsPictureProcessingFailure(AnylineDocumentModuleView anylineDocumentModuleView, ALDocumentError error)
        {
            ShowNotification<ALDocumentError>(error);
        }

        void IAnylineDocumentModuleDelegate.ReportsPreviewResult(AnylineDocumentModuleView anylineDocumentModuleView, UIImage image)
        {
            showingNotification = false;
            ShowNotification<String>("Processing.. Please hold still.");
        }

        void IAnylineDocumentModuleDelegate.TakePictureError(AnylineDocumentModuleView anylineDocumentModuleView, NSError error)
        {
            showingNotification = false;
            ShowNotification<String>("Failed to take picture.");
        }

        void IAnylineDocumentModuleDelegate.TakePictureSuccess(AnylineDocumentModuleView anylineDocumentModuleView)
        {
            new UIAlertView("Success", "Picture successfully taken.", null, "OK", null).Show();
        }
    }
}
