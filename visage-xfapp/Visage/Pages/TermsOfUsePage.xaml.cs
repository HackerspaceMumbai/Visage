using System;
using System.Collections.Generic;
using Visage.ViewModels;
using Xamarin.Forms;

namespace Visage.Pages
{
    public partial class TermsOfUsePage : ContentPage
    {
        public TermsOfUsePage()
        {
            InitializeComponent();

            BindingContext = new TermsOfUsePageViewModel
            {
                Navigation = Navigation
            };
        }
    }
}
