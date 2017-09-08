using System;
using System.Diagnostics;
using Visage.Data;
using Visage.Pages;
using Visage.Services;
using Xamarin.Forms;

namespace Visage
{
    public partial class App : Application
    {
        static string KEY_VISAGE_DATABASE_NAME = "visage.db3";

        static VisageDatabase visageDatabase;

        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new TermsOfUsePage());
        }

        public static VisageDatabase VisageDatabase
		{
			get
			{
				if (visageDatabase == null)
				{
                    visageDatabase = new VisageDatabase(DependencyService.Get<IFileHelper>().GetLocalFilePath(KEY_VISAGE_DATABASE_NAME));
				}
				return visageDatabase;
			}
		}

		public static bool isConnected()
		{
			try
			{
				return DependencyService
								 .Get<INetworkService>()
								 .IsConnected();
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);

				return false;
			}
		}

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
