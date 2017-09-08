using Visage.iOS.Services;
using Visage.Services;
using Wevedo.XFApp.iOS.Services;

[assembly: Xamarin.Forms.Dependency(typeof(NetworkService))]
namespace Wevedo.XFApp.iOS.Services
{
	public class NetworkService : INetworkService
	{
		public bool IsConnected()
		{
			var status = Reachability.InternetConnectionStatus();

			if (status.Equals(NetworkStatus.NotReachable))
				return false;

			return true;
		}
	}
}
