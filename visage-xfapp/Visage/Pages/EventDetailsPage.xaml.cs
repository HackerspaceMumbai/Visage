using System;
using System.Collections.Generic;
using Visage.Models;
using Visage.ViewModels;
using Xamarin.Forms;

namespace Visage.Pages
{
    public partial class EventDetailsPage : TabbedPage
    {
        public EventDetailsPage(Event item)
        {
            InitializeComponent();

            BindingContext = new EventDetailsPageViewModel(item.name)
            {
                Navigation = Navigation
            };

			//TODO: set children via viewmodel
			Children.Add(new EventSummaryPage(item));
            Children.Add(new EventSessionsPage(item.sessions));
        }
    }
}
