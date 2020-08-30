using System;
using System.Collections.Generic;
using Visage.ViewModels;
using Xamarin.Forms;

namespace Visage.Pages
{
    public partial class ProfilePage : ContentPage
    {
        public ProfilePage()
        {
            InitializeComponent();

            BindingContext = new ProfilePageViewModel
            {
                Navigation = Navigation
            };
        }
    }
}
