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
    class NotificationView : UIView
    {

        public UILabel textLabel;
        private nfloat cornerRadius;
        
        public CGColor fillColor;
        public CGColor borderColor;
        
        public NotificationView() : base() { }

        public NotificationView(CGRect frame)
            : base(frame)
        {
            // Initialization code
            textLabel = new UILabel(new CGRect(0, 0, frame.Size.Width, frame.Size.Height));
            textLabel.BackgroundColor = UIColor.Clear;
            textLabel.TextAlignment = UITextAlignment.Center;
            textLabel.TextColor = UIColor.White;

            AddSubview(textLabel);

            BackgroundColor = UIColor.Clear;

            borderColor = UIColor.Clear.CGColor;
            fillColor = UIColor.Yellow.CGColor;
            
            cornerRadius = 15;
            Opaque = false;
        }

        public override void Draw(CGRect rect)
        {            
            using (var context = UIGraphics.GetCurrentContext())
            {
                context.SetLineWidth(4);
                context.SetFillColor(fillColor);
                context.SetStrokeColor(borderColor);
                
                UIBezierPath roundedRect = UIBezierPath.FromRoundedRect(rect, this.cornerRadius);

                context.AddPath(roundedRect.CGPath);

                context.DrawPath(CGPathDrawingMode.FillStroke);

                roundedRect.Dispose();
                roundedRect = null;
            }
        }
    }
}
