using System;
using System.Diagnostics;
using System.Text;
using Auth0.OidcClient;
using UIKit;
using Visage.Models;
using Visage.Pages;

namespace Visage.iOS
{
    public partial class Auth0ViewController : UIViewController
    {
        Auth0Client _client;

        string auth0Domain = "indcoder.auth0.com";
        string auth0ClientId = "LmyPloWc6iTqUqZ0iOsXEc2XjcGAulen";
        
		//public Auth0ViewController(IntPtr handle) : base(handle)
		//{

		//}
        
        public Auth0ViewController() : base("Auth0ViewController", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            LoginButton.TouchUpInside += LoginButton_TouchUpInside;
            CancelButton.TouchUpInside += CancelButton_TouchUpInside;
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        async void LoginButton_TouchUpInside(object sender, EventArgs e)
        {

			try
		    {
        		_client = new Auth0Client(new Auth0ClientOptions
        		{
        		    Domain = auth0Domain,
        		    ClientId = auth0ClientId,
        		    Controller = this
        		});

    			var loginResult = await _client.LoginAsync(null);

    			var sb = new StringBuilder();

    			if (loginResult.IsError)
    			{
    			  sb.AppendLine("An error occurred during login:");
    			  sb.AppendLine(loginResult.Error);
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
    			}
	        }
		    catch(Exception ex)
		    {
		        Debug.WriteLine(ex.Message);
		    }
	    }

        void CancelButton_TouchUpInside(object sender, EventArgs e)
        {
            NavigationController.PopViewController(true);
        }
    }
}

