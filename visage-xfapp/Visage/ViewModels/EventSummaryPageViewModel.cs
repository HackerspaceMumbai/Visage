using System;
using System.Diagnostics;
using System.Windows.Input;
using Visage.Helpers;
using Visage.Models;
using Xamarin.Forms;

namespace Visage.ViewModels
{
    public class EventSummaryPageViewModel : BaseViewModel
    {
		public Event EventItem
		{
			get;
			set;
		}

		public ICommand CheckInCommand
		{
			get;
			set;
		}

		public EventSummaryPageViewModel(Event item)
		{
			EventItem = item;

			CheckInCommand = new Command(ExecuteCheckInCommand);
		}

		async void ExecuteCheckInCommand()
		{
			try
			{
				IsBusy = true;

				var startDateTime = EventItem.starts;
				var endDateTime = EventItem.ends;

				if (startDateTime == null || endDateTime == null)
				{
					await _dialogService.ShowMessage(AppHelper.ApplicationName, "Invalid request, logout and retry", Actions.Close);
				}

				if (!DateTime.Now.Date.Equals(startDateTime.Date))
				{
					await _dialogService.ShowMessage(AppHelper.ApplicationName, "Cannot check in until the date of the event", Actions.Close);
				}

				var systemTime = DateTime.Now.TimeOfDay;
				var hourBeforeStartTime = startDateTime.AddHours(-1).TimeOfDay;

				// check in before an hour of actual start time of event and on or before end time of event
				if (hourBeforeStartTime > systemTime && systemTime <= endDateTime.TimeOfDay)
				{

				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);

				await _dialogService.ShowMessage(AppHelper.ApplicationName, ex.Message, Actions.Close);
			}
			finally
			{
				IsBusy = false;
			}
		}
    }
}
