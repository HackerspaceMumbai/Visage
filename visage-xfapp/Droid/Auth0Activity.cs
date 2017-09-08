
using System;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using Auth0.OidcClient;
using IdentityModel.OidcClient;
using Visage.Models;
using Visage.Pages;

namespace Visage.Droid
{
    [Activity(Label = "Auth0Activity",
              LaunchMode = LaunchMode.SingleTask,
              Theme = "@style/MyTheme", 
              ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	[IntentFilter(
		new[] { Intent.ActionView },
		Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
		DataScheme = "com.visage",
		DataHost = "@string/auth0_domain",
		DataPathPrefix = "/android/com.visage/callback")]
    public class Auth0Activity : Activity
    {
		Auth0Client client;
        AuthorizeState authorizeState;
		
        Button loginButton, cancelButton;

        ProgressDialog progress;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.Auth0Layout);

			loginButton = FindViewById<Button>(Resource.Id.LoginButton);
			loginButton.Click += LoginButtonOnClick;

            cancelButton = FindViewById<Button>(Resource.Id.CancelButton);
            cancelButton.Click += CancelButton_Click;

			client = new Auth0Client(new Auth0ClientOptions
			{
				Domain = Resources.GetString(Resource.String.auth0_domain),
				ClientId = Resources.GetString(Resource.String.auth0_client_id),
				Activity = this
			});
        }

		protected override void OnResume()
		{
			base.OnResume();

			if (progress != null)
			{
				progress.Dismiss();

				progress.Dispose();
				progress = null;
			}
		}

		protected override async void OnNewIntent(Intent intent)
		{
			base.OnNewIntent(intent);

            try
            {
				progress = new ProgressDialog(this);
				progress.SetTitle("Redirecting");
				progress.SetMessage("Please wait...");
				progress.SetCancelable(false);
				progress.Show();
                
				var loginResult = await client.ProcessResponseAsync(intent.DataString, authorizeState);

				var sb = new StringBuilder();
				if (loginResult.IsError)
				{
					sb.AppendLine($"An error occurred during login: {loginResult.Error}");
				}
				else
				{
					var fullname = string.Empty;
					var thumbnail = string.Empty;

					sb.AppendLine($"ID Token: {loginResult.IdentityToken}");
					sb.AppendLine($"Access Token: {loginResult.AccessToken}");
					sb.AppendLine($"Refresh Token: {loginResult.RefreshToken}");

					sb.AppendLine();

					sb.AppendLine("-- Claims --");
					foreach (var claim in loginResult.User.Claims)
					{
						if (claim.Type.Equals("name"))
							fullname = claim.Value;

						if (claim.Type.Equals("picture"))
							thumbnail = claim.Value;

						sb.AppendLine($"{claim.Type} = {claim.Value}");
					}

					var profile = new Profile
					{
						FullName = fullname,
						Thumbnail = thumbnail,
						Email = "N/A",
						Rating = 0
					};

					await App.VisageDatabase.SaveItemAsync(profile);

					App.Current.MainPage = new MainPage();

                    Finish();
				}
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
		}

		private async void LoginButtonOnClick(object sender, EventArgs eventArgs)
		{
			progress = new ProgressDialog(this);
			progress.SetTitle("Redirecting");
			progress.SetMessage("Please wait...");
			progress.SetCancelable(false);
			progress.Show();

			// Prepare for the login
			authorizeState = await client.PrepareLoginAsync();

			// Send the user off to the authorization endpoint
			var uri = Android.Net.Uri.Parse(authorizeState.StartUrl);
			var intent = new Intent(Intent.ActionView, uri);
			intent.AddFlags(ActivityFlags.NoHistory);
			StartActivity(intent);
		}

        void CancelButton_Click(object sender, EventArgs e)
        {
            Finish();
        }
    }
}
