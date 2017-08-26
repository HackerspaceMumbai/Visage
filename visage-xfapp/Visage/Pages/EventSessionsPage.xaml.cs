using System;
using System.Collections.Generic;
using Visage.Models;
using Visage.ViewModels;
using Xamarin.Forms;

namespace Visage.Pages
{
    public partial class EventSessionsPage : ContentPage
    {
        public EventSessionsPage(List<Session> sessions)
        {
            InitializeComponent();

            BindingContext = new EventSessionsPageViewModel(sessions)
            {
                Navigation = Navigation
            };
        }
    }
}
