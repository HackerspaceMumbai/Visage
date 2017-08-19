using System;
using System.Collections.Generic;
using Visage.Models;
using Visage.ViewModels;
using Xamarin.Forms;

namespace Visage.Pages
{
    public partial class EventSummaryPage : ContentPage
    {
        public EventSummaryPage(Event item)
        {
            InitializeComponent();

			BindingContext = new EventSummaryPageViewModel(item)
			{
				Navigation = Navigation
			};
        }
    }
}
