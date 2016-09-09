##  AnylineSDK for Xamarin.iOS  ##

- AnylineXamarinSDK.iOS.dll: contains the Anyline SDK
- AnylineResources.bundle: contains necessary resources for the SDK
- AnylineXamarinApp.iOS: contains a simple app where each module is implemented, it can be installed right away
- LICENSE_IOS.md: contains third party license agreements
- README_IOS.md: this readme

For detailed information and guides, please visit https://documentation.anyline.io/

### Requirements


- minimum iOS 7.0
- minimum iPhone4s
- Xamarin account
- Visual Studio / Xamarin Studio (currently not tested)


### Quick Start - Setup


##### 1. Add AnylineXamarinSDK.iOS.dll to your References


##### 2. Simply drag & drop the AnylineResources.bundle folder into the "Resources" folder your project tree.


##### 3. Init an AnylineModuleView in your ViewController or Storyboard
There are module specific options - take a look at the description in the desired module description below.


##### 4. Enjoy scanning and have fun :)


### Modules

### Barcode Module

With the Anyline-Barcode-Module any kind of bar- and qr-codes can be scanned.
The result will be simply a String representation of the code.



Restrictions for the Barcode-Module Config:
- Flash mode "auto" is not (yet) supported.


Init Anyline:
- Init an AnylineBarcodeModuleView View with frame or add the View to your Storyboard.
- Setup the AnylineBarcodeModuleView with a valid license key and delegate
- Optional you may also limit the barcode scanning to one or multiple barcode formats (see also 'Available Barcode Formats' below)
- Call StartScanningAndReturnError() at ViewDidAppear() or later.
- Implement the interface IAnylineBarcodeModuleDelegate

Example Code:

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        // Initializing the scan view
        frame = UIScreen.MainScreen.Bounds;
        anylineBarcodeView = new AnylineBarcodeModuleView(frame);

        NSError error;
        bool success = anylineBarcodeView.SetupWithLicenseKey(licenseKey, this, out error);

        if (!success)
        {
            // handle error here
        }
    }

    public override void ViewDidAppear(bool animated)
    {
        base.ViewDidAppear(animated);

        NS error;
        bool success = anylineBarcodeView.StartScanningAndReturnError(out error);

        if (!success)
        {
            // handle error here
        }
    }

    public void IAnylineBarcodeModuleDelegate.DidFindScanResult(AnylineBarcodeModuleView anylineBarcodeModuleView, string scanResult, string barcodeFormat, UIImage image)
    {
        // handle result here
    }

Available Barcode Formats:
        AZTEC
        CODABAR
        CODE_39
        CODE_93
        CODE_128
        DATA_MATRIX
        EAN_8
        EAN_13
        ITF
        PDF_417
        QR_CODE
        RSS_14
        RSS_EXPANDED
        UPC_A
        UPC_E
        UPC_EAN_EXTENSION




### MRZ Module

The Anyline-MRZ-Module provides the functionality to scan passports and other IDs with a MRZ (machine-readable-zone).
For each scan result the module generates an Identification Object, containing all relevant information 
(e.g. document type and number, name, day of birth, etc.) as well as the image of the scanned document.



Restrictions for the MRZ-Module Config:
- Flash mode "auto" is not (yet) supported.


Init Anyline:
- Initialize the module in ViewDidLoad()
- Start the scanning process in ViewDidAppear()
- Implement the interface IAnylineMRZModuleDelegate and receive results

Example Code:

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        // Initializing the scan view
        CGRect frame = UIScreen.MainScreen.Bounds;
        anylineMrzView = new AnylineMRZModuleView(frame);

        NSError error;
        bool success = anylineMrzView.SetupWithLicenseKey(licenseKey, this, out error);
        if (!success)
        {
            // handle error here
        }

        View.AddSubview(anylineMrzView);
    }

    public override void ViewDidAppear(bool animated)
    {
        base.ViewDidAppear(animated);

        NSError error;
        bool success = anylineMrzView.StartScanningAndReturnError(out error);
        if (!success)
        {
            // handle error here
        }
    }

    public void IAnylineMRZModuleDelegate.DidFindScanResult(AnylineMRZModuleView anylineMRZModuleView, ALIdentification scanResult, bool allCheckDigitsValid, UIImage image)
    {
        // handle result here
    }

### Energy Module

The Anyline-Energy-Module is capable of scanning analog electric- and gas-meter-readings.
Moreover, it is possible to scan bar- and qr-codes for meter identification.

For each successful scan, you will receive four result-attributes (images will be null for bar/qr code mode):
    ScanMode: the mode the result belongs to
    result (for meter reading): the detected value as a String
    resultImage (for meter reading): the cropped image that has been used to scan the meter value
    fullImage (for meter reading): the full image (before cropping)

Restrictions for the Energy-Module Config:
- Flash mode "auto" is not (yet) supported.

Init Anyline:
- Initialize the module in ViewDidLoad()
- Start the scanning process in ViewDidAppear()
- Implement the interface IAnylineEnergyModuleDelegate and receive results

Example Code:

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

		// Initialize scan view
        frame = UIScreen.MainScreen.Bounds;
        anylineEnergyView = new AnylineEnergyModuleView(frame);

        NSError error;
        bool success = anylineEnergyView.SetupWithLicenseKey(licenseKey, this.Self, out error);
        if (!success)
        {
            // handle error here
        }

        View.AddSubview(anylineEnergyView);
    }

    public override void ViewDidAppear(bool animated)
    {
        base.ViewDidAppear(animated);

        NSError error;
        bool success = anylineEnergyView.StartScanningAndReturnError(out error);
        if (!success)
        {
            // handle error here
        }
    }

    void IAnylineEnergyModuleDelegate.DidFindScanResult(AnylineEnergyModuleView anylineEnergyModuleView, string scanResult, UIImage image, UIImage fullImage, ALScanMode scanMode)
    {
        // handle result here
    }