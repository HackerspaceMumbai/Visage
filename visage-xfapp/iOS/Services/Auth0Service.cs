using System;
using System.Diagnostics;
using System.Linq;
using UIKit;
using Visage.iOS.Services;
using Visage.Services;

[assembly: Xamarin.Forms.Dependency(typeof(Auth0Service))]
namespace Visage.iOS.Services
{
    public class Auth0Service : IAuth0Service
    {
		public void LoginViaAuth0()
		{
			try
			{
				var auth0ViewController = new Auth0ViewController();
				auth0ViewController.Title = "Login with Eventbrite";

				var rootController = UIApplication.SharedApplication.KeyWindow.RootViewController.ChildViewControllers.First().ChildViewControllers.Last().ChildViewControllers.First();
				var navcontroller = rootController as UINavigationController;
				if (navcontroller != null)
					rootController = navcontroller.VisibleViewController;
                
                rootController.NavigationController.PushViewController(auth0ViewController, true);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
		}
    }
}
