using Foundation;
using System;
using UIKit;
using AnylineXamarinSDK.iOS;
using CoreGraphics;

namespace AnylineXamarinApp.iOS
{
    public class AnylineLicensePlateDEScanViewController : AnylineLicensePlateScanViewController
    {        
        public AnylineLicensePlateDEScanViewController(String name)
        {
            this.Title = name;
        }

        public override void SetupLicensePlateConfig()
        {
            // We'll define the OCR Config here:
            ocrConfig = new ALOCRConfig();
            ocrConfig.CustomCmdFilePath = NSBundle.MainBundle.PathForResource(@"Modules/OCR/license_plates_d", @"ale");
        }
    }
}
