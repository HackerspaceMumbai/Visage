using System;
using Visage.Models;

namespace Visage.ViewModels
{
    public class ProfilePageViewModel : BaseViewModel
    {
        public Profile Profile
        {
            get;
            set;
        }

        public ProfilePageViewModel()
        {
            GetProfile();
        }

		void GetProfile()
		{
			Profile = new Profile
			{
				Thumbnail = "https://appmatticresourcegrou959.blob.core.windows.net/images/ic_account_circle.png",
				Email = "gorilla@zoo.com",
				FullName = "Gorilla",
				Rating = 3.5
			};
		}
    }
}
