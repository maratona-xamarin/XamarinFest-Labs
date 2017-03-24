using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;

namespace Core.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			global::Xamarin.Forms.Forms.Init();
			Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();
			BuildStyles();

			LoadApplication(new App());

			return base.FinishedLaunching(app, options);
		}

		private void BuildStyles()
		{
			UINavigationBar.Appearance.BarTintColor = UIColor.FromRGB(75, 65, 185);
			UINavigationBar.Appearance.TintColor = UIColor.White;
			UINavigationBar.Appearance.SetTitleTextAttributes(
				new UITextAttributes() { TextColor = UIColor.White }
			);
		}
	}
}
