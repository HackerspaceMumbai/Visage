using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Visage.Helpers;
using Xamarin.Forms;

namespace Visage.Services
{
	public interface IDialogService
	{
		Task<bool> ShowMessage(string title, string message, string buttonConfirmText, string buttonCancelText);

		Task<string> ShowMultipleSelection(string title, string[] options);

		Task ShowMessage(string title, string message, string buttonCloseText);

		Task ShowServerUnreachableMessage();
	}
    
    public class DialogService : IDialogService
    {
		public async Task<bool> ShowMessage(string title, string message, string buttonConfirmText, string buttonCancelText)
		{
			try
			{
				var result = await Application.Current.MainPage.DisplayAlert(title, message, buttonConfirmText, buttonCancelText);

				return result;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);

				return false;
			}
		}

		public async Task ShowMessage(string title, string message, string buttonCloseText)
		{
			try
			{
				await Application.Current.MainPage.DisplayAlert(title, message, buttonCloseText);

			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}

		}

		public async Task<string> ShowMultipleSelection(string title, string[] options)
		{
			try
			{
				var result = await Application.Current.MainPage.DisplayActionSheet(title, Actions.Cancel, null, options);

				return result;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);

				return null;
			}
		}

		public async Task ShowServerUnreachableMessage()
		{
			try
			{
				await Application.Current.MainPage.DisplayAlert("Cannot connect to server", "Check network settings", Actions.Close);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
		}
    }
}
