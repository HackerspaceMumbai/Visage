using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Visage.Services;
using Xamarin.Forms;

namespace Visage.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
		bool isBusy;

		public bool IsBusy
		{
			get { return isBusy; }
			set { isBusy = value; OnPropertyChanged(); }
		}
        
        public INavigation Navigation { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}

        internal IDialogService _dialogService;

        protected BaseViewModel()
        {
            IsBusy = false;
            
            _dialogService = new DialogService();
        }

		protected BaseViewModel(INavigation Navigation)
		{
			this.Navigation = Navigation;
		}
    }
}
