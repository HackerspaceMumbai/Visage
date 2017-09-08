using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Visage.Models;
using Visage.Pages;
using Xamarin.Forms;

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
            var profile = App.VisageDatabase.GetProfile().Result;

            Profile = new Profile
            {
                Thumbnail = profile.Thumbnail,
                Email = profile.Email,
                FullName = profile.FullName,
                Rating = profile.Rating
            };
        }
    }
}
