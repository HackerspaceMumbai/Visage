using System;
using System.Collections.Generic;
using Visage.ViewModels;
using Xamarin.Forms;

namespace Visage.Pages
{
    public partial class EventsTabItemPage : ContentPage
    {
        public EventsTabItemPage(string title)
        {
            InitializeComponent();

            BindingContext = new EventsTabItemPageViewModel(title)
            {
                Navigation = Navigation
            };
        }
    }
}
