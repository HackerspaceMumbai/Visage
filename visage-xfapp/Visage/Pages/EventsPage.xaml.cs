using System;
using System.Collections.Generic;
using Visage.ViewModels;
using Xamarin.Forms;

namespace Visage.Pages
{
    public partial class EventsPage : TabbedPage
    {
        public EventsPage()
        {
            InitializeComponent();

            BindingContext = new EventsPageViewModel
            {
                Navigation = Navigation
            };

            //TODO: set children via viewmodel
            Children.Add(new EventsTabItemPage { Title="Upcoming" });
            Children.Add(new EventsTabItemPage { Title="Completed" });
        }
    }
}
