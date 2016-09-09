using AnylineXamarinSDK.iOS;
using CoreGraphics;
using UIKit;

namespace AnylineXamarinApp.iOS
{
    public class AnylineEnergyScanViewResultController : UIViewController
    {
        public ALScanMode ScanMode {get; set;}
        public UIImage MeterImage {get; set;}
        public string Result {get; set;}

        EnergyMeterReadingView meterReadingView;
        UIImageView meterImageView;

        public AnylineEnergyScanViewResultController(string result, ALScanMode scanMode) : base()
        {
            this.Result = result;
            this.ScanMode = scanMode;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            InitViews();
            SetupMeterReadingView();
        }

        private void InitViews()
        {
            Title = "";
            View.BackgroundColor = new UIColor(88.5f / 255f, 144f / 255f, 163f / 255f, 1f);

            meterReadingView = new EnergyMeterReadingView(new CGRect(0, 100, View.Frame.Size.Width, 150));
            View.AddSubview(meterReadingView);

            meterImageView = new UIImageView(MeterImage);
            meterImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            meterImageView.Frame = new CGRect((View.Frame.Size.Width - 272)/2,280,272, 70);
            View.AddSubview(meterImageView);

        }

        private void SetupMeterReadingView()
        {
            meterReadingView.SetText(Result);
            Title = "Result";
            meterReadingView.SetScanMode(ScanMode);
        }
    }
}
