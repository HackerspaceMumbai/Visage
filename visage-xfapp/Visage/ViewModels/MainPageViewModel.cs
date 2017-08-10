using System;
using System.Windows.Input;
using Visage.Pages;
using Xamarin.Forms;

namespace Visage.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        public bool IsBusy
        {
            get;
            set;
        }
        
        public ICommand NavigateCommand
        {
            get;
            set;
        }

        public NavigationPage DetailPage
        {
            get;
            set;
        }

        public MainPageViewModel()
        {
            // set default detail page as events page
            DetailPage = new NavigationPage((Page)Activator.CreateInstance(typeof(EventsPage)));
            
            NavigateCommand = new Command<Type>(ExecuteNavigateCommand);
        }

        void ExecuteNavigateCommand(Type target)
        {
            // update the detail page via command
            DetailPage = new NavigationPage((Page)Activator.CreateInstance(target));
        }
    }
}
