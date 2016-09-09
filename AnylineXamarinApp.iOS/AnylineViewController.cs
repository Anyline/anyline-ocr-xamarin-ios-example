using AnylineXamarinSDK.iOS;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using UIKit;

namespace AnylineXamarinApp.iOS
{
	public partial class AnylineViewController : UITableViewController
	{
        //public UITableView table;

        public Dictionary<string, string[]> tableItems = new Dictionary<string, string[]>()
        {
            { "Energy", new string[] {
                    "Analog Electric Meter Scan",
                    "Analog Gas Meter Scan",
                    "Analog Water Meter Scan",
                    "Digital Meter Scan (Alpha)",
                    "Heat Meter Scan (Alpha)"
                }},
            { "Identification", new string[] {
                    "Passport / ID MRZ Scan"
                }},
            { "Barcodes", new string[] {
                    "Barcode Scan"
                }},
            { "Fintech", new string[] {
                    "IBAN Scan"
                }},
            { "Document", new string[] {
                    "Document Scan"
                }},
            { "Loyalty", new string[] {
                    "Voucher Code Scan",
                    "Bottlecap Code Scan",
                    "License Plate Scan (Alpha)",
                    "AT License Plate Scan (Alpha)",
                    "DE License Plate Scan (Alpha)"
                }}
        };

        public static readonly string LICENSE_KEY = "eyJzY29wZSI6WyJBTEwiXSwicGxhdGZvcm0iOlsiaU9TIiwiQW5kcm9pZCIsIldpbmRvd3MiXSwidmFsaWQiOiIyMDE2LTExLTAxIiwibWFqb3JWZXJzaW9uIjoiMyIsImlzQ29tbWVyY2lhbCI6ZmFsc2UsImlvc0lkZW50aWZpZXIiOlsiQVQuTmluZXlhcmRzLkFueWxpbmUuQW55bGluZVhhbWFyaW5BcHAuaU9TIl0sImFuZHJvaWRJZGVudGlmaWVyIjpbIkFULk5pbmV5YXJkcy5BbnlsaW5lLkFueWxpbmVYYW1hcmluQXBwLkFuZHJvaWQiXSwid2luZG93c0lkZW50aWZpZXIiOlsiQVQuTmluZXlhcmRzLkFueWxpbmUuQW55bGluZVhhbWFyaW5BcHAuV2luZG93cyJdfQp1VVFjTW5GUEd2WmhmdHR5bjFabDBYL3F6bkdkVHFjZXBubnpYYUVjSjFCOGRMNUJOelMvVVU0R2JiNjcvQ2FXc2c2d252TGNwMmFSMWhwUlhjUVR2Q21QQTg4Szc2NG9YMGVsME5PSXBKQ0RJY3VlWVNzVWVmR3dTbEM5b0JwSnkrVmcxYjhpZ0JSY0oyamVabkR5SFdBV1E0bWlKbUFZTjZUZDFneUFaWVdCU251TFM1QW5mQUVQWnhwdEJWTlBKSlNvUThndkNpNlZiT0x1bG4wNk5tTWJVSVUwZGVUR1FtUG1oNm05MXJjYklFb0J0VHJGeWdUU1M4SFZhQWJETlJ2VVV5MzYxbEMrTS96M3RUNmhERDhiamRNcDY5dFZ6aXZ6NzJTQXBRR1k3ckFtTkhDVERuYmFMNFBqTUNzbEEwWi9PS0MvdTEvVk5vMUdFQWFJcVE9PQ==";

        public AnylineViewController (IntPtr handle) : base (handle){ }
		
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView = new UITableView(View.Bounds, UITableViewStyle.Grouped);
            TableView.Source = new TableSource(tableItems, this);
        }

        //lock orientation to portrait
        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations ()
        {
            return UIInterfaceOrientationMask.Portrait;
        }

        //stop rotating away from portrait
        public override bool ShouldAutorotate()
        {
            return false;
        }
	};

    public class TableSource : UITableViewSource
    {
        protected Dictionary<string, string[]> tableItems;

        protected AnylineViewController parent;

        protected string cellIdentifier = "TableCell";

        public TableSource(Dictionary<string, string[]> tableItems, AnylineViewController parent)
        {
            this.tableItems = tableItems;
            this.parent = parent;
        }
        
        public override string TitleForHeader(UITableView tableView, nint section)
        {
            try
            {
                if (section == NumberOfSections(tableView) - 1)
                {
                    var sdkVer = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString").ToString();
                    return String.Format("SDK: {1}", sdkVer);
                }
                return tableItems.Keys.ToList().ElementAt((int)section);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {            
            try
            {
                return tableItems.ToList().ElementAt((int)section).Value.Count();
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public override nint NumberOfSections(UITableView tableView)
        {
            return (nint)tableItems.Keys.Count + 1;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // request a recycled cell to save memory
            UITableViewCell cell = tableView.DequeueReusableCell(cellIdentifier);
            if (cell == null)
                cell = new UITableViewCell(UITableViewCellStyle.Default, cellIdentifier);

            cell.TextLabel.Text = tableItems.ToList().ElementAt(indexPath.Section).Value.ToList().ElementAt(indexPath.Row);
            cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;

            return cell;
        }

        /*
         * Navigate to the selected use case and initialize the appropriate viewcontroller with the appropriate settings
         */
        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {

            var name = tableItems.ElementAt(indexPath.Section).Value.ElementAt(indexPath.Row);            
            switch (indexPath.Section)
            {
                case 0: //ENERGY
                    Dictionary<String,ALScanMode> scanModeItems = new Dictionary<string,ALScanMode>();
                    string labelText = "";
                    int defaultIndex = 0;

                    switch (indexPath.Row)
                    {
                        case 0: // Analog Electric Meter Scan
                            scanModeItems.Add("5 or 6", ALScanMode.ALElectricMeter);
                            scanModeItems.Add(" 5 ", ALScanMode.ALElectricMeter5_1);
                            scanModeItems.Add(" 6 ", ALScanMode.ALElectricMeter6_1);
                            scanModeItems.Add(" 7 ", ALScanMode.ALAnalogMeter7);
                            scanModeItems.Add("5-6 (W)", ALScanMode.ALAnalogMeterWhite);

                            labelText = "Pre-decimal places:";

                            break;
                        case 1: //Analog Gas Meter Scan
                            scanModeItems.Add("4", ALScanMode.ALAnalogMeter4);
                            scanModeItems.Add("5", ALScanMode.ALGasMeter);
                            scanModeItems.Add("6", ALScanMode.ALGasMeter6);
                            scanModeItems.Add("7", ALScanMode.ALAnalogMeter7);
                            
                            labelText = "Pre-decimal places:";
                            defaultIndex = 1;

                            break;
                        case 2: //Analog Water Meter Scan
                            scanModeItems.Add("White background", ALScanMode.ALWaterMeterWhiteBackground);
                            scanModeItems.Add("Black background", ALScanMode.ALWaterMeterBlackBackground);

                            break;
                        case 3: //Digital Meter Scan (Alpha)
                            scanModeItems.Add("", ALScanMode.ALDigitalMeter);
                            break;
                        case 4: //Heat Meter Scan (Alpha)
                            scanModeItems.Add("4 digits", ALScanMode.ALHeatMeter4);
                            scanModeItems.Add("5 digits", ALScanMode.ALHeatMeter5);
                            scanModeItems.Add("6 digits", ALScanMode.ALHeatMeter6);
                            break;

                        default:
                            break;
                    }

                    parent.NavigationController.PushViewController(new AnylineEnergyScanViewController(name, scanModeItems, labelText, defaultIndex), true);
                    break;

                case 1: //Identification
                    //Passport / ID MRZ Scan
                    parent.NavigationController.PushViewController(new AnylineMrzScanViewController(name), true);
                    break;

                case 2: //Barcodes
                    //Barcode Scan
                    parent.NavigationController.PushViewController(new AnylineBarcodeScanViewController(name), true);
                    break;

                case 3: //Fintech
                    //Iban Scan
                    parent.NavigationController.PushViewController(new AnylineIBANScanViewController(name), true);
                    break;

                case 4: //Document
                    //Document Scan
                    parent.NavigationController.PushViewController(new AnylineDocumentScanViewController(name), true);
                    break;

                case 5: //Loyalty
                    if (indexPath.Row == 0) //Voucher Code Scan
                        parent.NavigationController.PushViewController(new AnylineVoucherScanViewController(name), true);
                    if (indexPath.Row == 1) //Bottlecap Code Scan
                        parent.NavigationController.PushViewController(new AnylineBottlecapScanViewController(name), true);                    
                    if (indexPath.Row == 2) //License Plate Scan
                        parent.NavigationController.PushViewController(new AnylineLicensePlateScanViewController(name), true);
                    if (indexPath.Row == 3) //AT License Plate Scan
                        parent.NavigationController.PushViewController(new AnylineLicensePlateATScanViewController(name), true);
                    if (indexPath.Row == 4) //DE License Plate Scan
                        parent.NavigationController.PushViewController(new AnylineLicensePlateDEScanViewController(name), true);
                    break;
                default:
                    break;
            }
            
            tableView.DeselectRow(indexPath, true);
        }

    };
}
