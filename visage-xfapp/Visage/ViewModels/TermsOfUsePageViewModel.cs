using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using Visage.Helpers;
using Visage.Pages;
using Visage.Services;
using Xamarin.Forms;

namespace Visage.ViewModels
{
    public class TermsOfUsePageViewModel : BaseViewModel
    {
		HtmlWebViewSource eulaText;

		public HtmlWebViewSource EulaText
		{
			get { return eulaText; }
			set { eulaText = value; OnPropertyChanged(); }
		}

		public ICommand AcceptCommand { get; set; }

		public ICommand CancelCommand { get; set; }

        public TermsOfUsePageViewModel()
        {
            LoadEula();

			AcceptCommand = new Command(ExecuteAcceptCommand);

			CancelCommand = new Command(ExecuteCancelCommand);
        }

		void ExecuteAcceptCommand()
		{
            //Application.Current.MainPage = new LoginWithEventbritePage();

            DependencyService.Get<IAuth0Service>().LoginViaAuth0();
		}

		async void ExecuteCancelCommand()
		{
            await _dialogService.ShowMessage(AppHelper.ApplicationName, "Acceptance is required to proceed with use of app", Actions.Close);
		}

		async void LoadEula()
		{
			try
			{
				IsBusy = true;

                var isLoggedIn = await App.VisageDatabase.ProfileExists();

                if (isLoggedIn)
                    Application.Current.MainPage = new MainPage();

                var termsOfUserHtmlFile = DependencyService.Get<IPlatformFileManager>().GetHtmlContentAsString("terms_of_use.html");

                if (!string.IsNullOrEmpty(termsOfUserHtmlFile))
                {
                    EulaText = new HtmlWebViewSource { Html = termsOfUserHtmlFile };
                }
                else
                {
                    await _dialogService.ShowMessage(AppHelper.ApplicationName, "Unable to read terms of use", Actions.Close);
                }
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);

                await _dialogService.ShowMessage(AppHelper.ApplicationName, "Unable to read terms of use", Actions.Close);
			}
			finally
			{
				IsBusy = false;
			}
		}
    }
}
