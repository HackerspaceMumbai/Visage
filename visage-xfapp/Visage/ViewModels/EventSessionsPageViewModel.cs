using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using Visage.Helpers;
using Visage.Models;
using Xamarin.Forms;

namespace Visage.ViewModels
{
    public class EventSessionsPageViewModel : BaseViewModel
    {
        public List<Session> Sessions
        {
            get;
            set;
        }

        public ICommand SessionSelectedCommand
        {
            get;
            set;
        }

        public EventSessionsPageViewModel(List<Session> sessions)
        {
            Sessions = sessions;
            
            SessionSelectedCommand = new Command<Session>(ExecuteSessionSelected); 
        }

        async void ExecuteSessionSelected(Session selectedSession)
        {
            try
            {
                IsBusy = true;

                await _dialogService.ShowMessage(AppHelper.ApplicationName, "You selected: " + selectedSession.name, Actions.Close);
            }
            catch(Exception ex)
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
