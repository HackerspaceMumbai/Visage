using Android.App;
using Android.OS;

namespace Visage.Droid
{
    [Activity(Label = "@string/app_name",
			  Theme = "@style/SplashTheme",
			  MainLauncher = true,
			  NoHistory = true)]
	public class SplashActivity : Activity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			StartActivity(typeof(MainActivity));

			Finish();
		}
	}
}
