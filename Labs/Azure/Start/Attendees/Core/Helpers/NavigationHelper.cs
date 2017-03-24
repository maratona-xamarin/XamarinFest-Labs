using System.Threading.Tasks;
using Core.Models;
using Core.Views;
using Xamarin.Forms;

namespace Core.Helpers
{
	public class NavigationHelper
	{
		private static NavigationHelper _instance;
		public static NavigationHelper Instance => _instance ?? (_instance = new NavigationHelper());

		private NavigationHelper() {}

		private NavigationPage GetMainPage()
		{
			return Application.Current.MainPage as NavigationPage;
		}

		public async Task GotoDetails(AttendeeModel attendeeModel)
		{
			await GetMainPage().PushAsync(new DetailsView(attendeeModel));
		}

		public async Task GoBack()
		{
			await GetMainPage().PopAsync();
		}
	}
}
