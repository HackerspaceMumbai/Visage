using System;
using System.Collections.Generic;
using System.Linq;
using Auth0.OidcClient;
using Foundation;
using UIKit;

namespace Visage.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
		public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
		{
			ActivityMediator.Instance.Send(url.AbsoluteString);

			return true;
		}

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();

            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }
    }
}
