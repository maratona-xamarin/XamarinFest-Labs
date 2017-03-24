using System.Threading.Tasks;
using Xamarin.Forms;

namespace Core.Helpers
{
	public class MessageHelper
	{
		private static MessageHelper _instance;
		public static MessageHelper Instance => _instance ?? (_instance = new MessageHelper());

		private MessageHelper() {}

		private NavigationPage GetNavigation()
		{
			return (Application.Current.MainPage as NavigationPage);
		}

		public async Task ShowMessage(string title, string message, string button)
		{
			await GetNavigation().DisplayAlert(title, message, button);
		}

		public async Task<bool> ShowAsk(string title, string message, string buttonYes, string buttonNo)
		{
			return await GetNavigation().DisplayAlert(title, message, buttonYes, buttonNo);
		}

		public async Task<string> ShowOptions(string title, string cancel, string destruction, string[] buttons)
		{
			return await GetNavigation().DisplayActionSheet(title, cancel, destruction, buttons);
		}
	}
}
