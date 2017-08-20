using System;
using System.Collections.Generic;
using Visage.ViewModels;
using Xamarin.Forms;

namespace Visage.Pages
{
    public partial class LoginWithEventbritePage : ContentPage
    {
        public LoginWithEventbritePage()
        {
            InitializeComponent();

            BindingContext = new LoginWithEventbriteViewModel
            {
                Navigation = Navigation
            };
        }
    }
}
