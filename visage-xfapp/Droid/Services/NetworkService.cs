using System;
using Android.App;
using Android.Content;
using Android.Net;
using Android.Util;
using Visage.Droid.Services;
using Visage.Services;

[assembly: Xamarin.Forms.Dependency(typeof(NetworkService))]
namespace Visage.Droid.Services
{
	public class NetworkService : INetworkService
	{
		// 1. check if network is available
		// 2. check if device is connected to internet

		public bool IsConnected()
		{
			try
			{
				var result = IsNetworkConnected();

				return result;
			}
			catch (Exception ex)
			{
				Log.Error("IsConnected", ex.Message);

				return false;
			}
		}

		// TODO: Find better technqiue to ensure internet access
		bool IsNetworkConnected()
		{
			ConnectivityManager connectivityManager = (ConnectivityManager)
														Application
															.Context
															.GetSystemService(Context.ConnectivityService);

			var networks = connectivityManager.GetAllNetworks();

			if (networks.Length > 0)
			{
				foreach (var network in networks)
				{
					var networkInfo = connectivityManager.GetNetworkInfo(network);

					if (networkInfo != null)
						if (networkInfo.IsConnected)
							return true;
				}
			}

			return false;
		}
	}
}
