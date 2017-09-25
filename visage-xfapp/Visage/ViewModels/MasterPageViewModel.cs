using System;
using System.Collections.Generic;
using System.Windows.Input;
using Visage.Models;
using Visage.Pages;
using Visage.Services;
using Xamarin.Forms;

namespace Visage.ViewModels
{
    public class MasterPageViewModel : BaseViewModel
    {
        public List<MasterPageItem> MasterPageItems
        {
            get;
            set;
        }

        public Profile Profile
        {
            get;
            set;
        }

        public ICommand LogoutCommand
        {
            get;
            set;
        }

        public MasterPageViewModel()
        {
            LoadMasterPageItems();

            GetProfile();

            LogoutCommand = new Command(ExecuteLogoutCommand);
        }

        void LoadMasterPageItems()
        {
            var masterPageItems = new List<MasterPageItem>();

			masterPageItems.Add(new MasterPageItem
			{
				Title = "Profile",
				TargetType = typeof(ProfilePage)
			});
			masterPageItems.Add(new MasterPageItem
			{
				Title = "Events",
                TargetType = typeof(EventsPage)
			});

            MasterPageItems = masterPageItems;
        }

        void GetProfile()
        {
            var profile = App.VisageDatabase.GetProfile().Result;

			Profile = new Profile
			{
				Thumbnail = profile.Thumbnail,
				Email = profile.Email,
				FullName = profile.FullName,
				Rating = profile.Rating
			};
        }

        async void ExecuteLogoutCommand()
        {
            (Application.Current.MainPage as MainPage).IsPresented = false;

            var result = await _dialogService.ShowMessage("Visage", "Sure, you want to logout?", "Yes", "Cancel");

            if(result) // if user selected yes
            {
                var profile = await App.VisageDatabase.GetProfile();
                await App.VisageDatabase.DeleteItemAsync(profile);

                DependencyService.Get<IAuth0Service>().LoginViaAuth0();
            }
        }

    }
}
