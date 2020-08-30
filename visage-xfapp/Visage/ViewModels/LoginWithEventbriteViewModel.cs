using System;
using System.Windows.Input;
using Visage.Helpers;
using Visage.Pages;
using Xamarin.Forms;

namespace Visage.ViewModels
{
    public class LoginWithEventbriteViewModel : BaseViewModel
    {
        public String Email
        {
            get;
            set;
        }

        public string Password
        {
            get;
            set;
        }
        
        public ICommand LoginCommand { get; set; }

		public ICommand CancelCommand { get; set; }

		public LoginWithEventbriteViewModel()
		{
			LoginCommand = new Command(ExecuteLoginCommand);

			CancelCommand = new Command(ExecuteCancelCommand);
		}

        void ExecuteLoginCommand()
		{
            Application.Current.MainPage = new MainPage();
		}

		void ExecuteCancelCommand()
		{
            //TODO: Handle cancel click
            //await Navigation.PopAsync();
		}
    }
}
