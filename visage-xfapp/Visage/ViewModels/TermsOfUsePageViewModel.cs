using System;
using System.Diagnostics;
using System.Windows.Input;
using Visage.Helpers;
using Visage.Pages;
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
            Application.Current.MainPage = new MainPage();
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

				// process the terms of use here
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);

                await _dialogService.ShowMessage(AppHelper.ApplicationName, "Unable to read terms of use", Actions.Close);

				await Navigation.PopAsync();
			}
			finally
			{
				IsBusy = false;
			}
		}
    }
}
