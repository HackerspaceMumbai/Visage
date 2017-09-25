using System;
using Android.App;
using Android.Content;
using Visage.Droid.Services;
using Visage.Services;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(Auth0Service))]
namespace Visage.Droid.Services
{
    public class Auth0Service : IAuth0Service
    {
		public void LoginViaAuth0()
		{
			var activity = Forms.Context as Activity;

			activity.StartActivity(new Intent(activity, typeof(Auth0Activity)));
		}
    }
}
