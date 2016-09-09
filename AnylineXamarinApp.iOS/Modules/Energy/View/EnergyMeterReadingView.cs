using AnylineXamarinSDK.iOS;
using CoreGraphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UIKit;

namespace AnylineXamarinApp.iOS
{
    /*
     * Custom Label
     */
    class AnylineMeterLabel : UILabel
    {
        private int cornerRadius;
        public AnylineMeterLabel() : base()
        {
            CustomInit();
        }
        public AnylineMeterLabel(CGRect frame) : base(frame)
        {
            CustomInit();
        }
        private void CustomInit()
        {
            cornerRadius = 3;
            TextColor = new UIColor(85.0f / 255f, 144f / 255f, 163f / 255f, 1f);
            BackgroundColor = UIColor.White;
            Font = UIFont.SystemFontOfSize(28);
            TextAlignment = UITextAlignment.Center;
        }

        public override void Draw(CGRect rect)
        {
 	        base.Draw(rect);
            Layer.CornerRadius = cornerRadius;
        }     
    }

    /*
     * Energy Meter Reading View
     */
    class EnergyMeterReadingView : UIView
    {
        const int kMeterLabelWidth = 30;
        const int kMeterLabelHeight = 45;

        const int kMeterLabelGap = 3;

        const int kCommaWidth = 5;

        const int kUnitKWHWidth = 56;
        const int kUnitM3Width = 28;

        List<AnylineMeterLabel> digits;
        
        UILabel unit;

        UIImageView meterIcon;

        public EnergyMeterReadingView(CGRect frame) : base(frame)
        {
            if (Self != null)
            {
                Debug.Assert(frame.Size.Width > 300, @"View width should be 300px min");
                Debug.Assert(frame.Size.Height > 80, @"View height should be 80px min");
            }

            //InitSubViews();
        }

        private void InitSubViews(int digitsCount)
        {
            meterIcon = new UIImageView(UIImage.FromBundle("flamme"));
            meterIcon.Center = new CGPoint(Frame.Size.Width / 2, Frame.Size.Height / 4);

            AddSubview(meterIcon);

            nfloat digitYPosition = Frame.Size.Height / 2 + (Frame.Size.Height / 2 - kMeterLabelHeight) / 2;

            unit = new UILabel(new CGRect(0, digitYPosition, kUnitKWHWidth, kMeterLabelHeight));
            unit.TextColor = UIColor.White;
            unit.Font = UIFont.SystemFontOfSize(28);

            digits = new List<AnylineMeterLabel>();

            for (int i = 0; i < digitsCount; i++)
                digits.Add(new AnylineMeterLabel(new CGRect(0, digitYPosition, kMeterLabelWidth, kMeterLabelHeight)));
            
            foreach (var d in digits)
                AddSubview(d);
            
            AddSubview(unit);
        }

        private void LayoutViewForDigits(int d)
        {
            nfloat offset = (Frame.Size.Width - (d * kMeterLabelWidth + kUnitKWHWidth + d * kMeterLabelGap)) / 2;
            
            for (int i = 0; i < digits.Count; i++)
            {
                digits[i].Frame = new CGRect(offset + (kMeterLabelWidth + kMeterLabelGap) * (i+1),
                    digits[i].Frame.Y, digits[i].Frame.Size.Width, digits[i].Frame.Size.Height);
            }
            if (unit.Text != "")
            {
                unit.Frame = new CGRect(offset + (kMeterLabelWidth + kMeterLabelGap) * (digits.Count + 1), unit.Frame.Y,
                    unit.Frame.Size.Width, unit.Frame.Size.Height);
            }
        }
        
        public void SetScanMode(ALScanMode scanMode)
        {
            switch (scanMode)
            {
                case ALScanMode.ALGasMeter:
                case ALScanMode.ALGasMeter6:
                case ALScanMode.ALHeatMeter4:
                case ALScanMode.ALHeatMeter5:
                case ALScanMode.ALHeatMeter6:
                    meterIcon.Image = UIImage.FromBundle("flamme");
                    //unit.Text = "m³";
                    break;
                case ALScanMode.ALElectricMeter:
                case ALScanMode.ALElectricMeter5_1:
                case ALScanMode.ALElectricMeter6_1:
                case ALScanMode.ALDigitalMeter:
                    meterIcon.Image = UIImage.FromBundle("blitz");
                    //unit.Text = "kWh";
                    break;
                case ALScanMode.ALWaterMeterBlackBackground:
                case ALScanMode.ALWaterMeterWhiteBackground:
                    meterIcon.Image = null;
                    //unit.Text = "m³";
                    break;
            }
        }

        public void SetText(String text)
        {

            InitSubViews(text.Length);

            for(int i = 0; i < text.Length; i++)
                digits[i].Text = text.Substring(i, 1);
            
            LayoutViewForDigits(text.Length);
        }
    }
}
