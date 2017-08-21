using System;
using System.Diagnostics;
using System.Windows.Input;
using Visage.Helpers;
using Visage.Models;
using Xamarin.Forms;

namespace Visage.ViewModels
{
    public class EventDetailsPageViewModel : BaseViewModel
    {
        public EventDetailsPageViewModel(string title)
        {
            Title = title;
        }
    }
}
