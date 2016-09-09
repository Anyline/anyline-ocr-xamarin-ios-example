using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using AnylineXamarinSDK.iOS;

namespace AnylineXamarinApp.iOS
{
	partial class MainController : UINavigationController
	{
		public MainController (IntPtr handle) : base (handle)
		{
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            return UIInterfaceOrientationMask.Portrait;
        }

        public override bool ShouldAutorotate()
        {
            return false;
        }
	}
}
