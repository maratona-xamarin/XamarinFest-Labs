using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Core.Helpers;
using Core.Models;
using Core.Services;
using Xamarin.Forms;

namespace Core.ViewModels
{
	public class AttendeesViewModel : BaseViewModel
	{
		public ObservableCollection<AttendeeModel> Attendees { get; set; }

		public ICommand GetAttendeesCommand 
			=> new Command(async () => await GetAttendees());

		public ICommand AddAttendeeCommand
			=> new Command(async () => await AddAttendee());

		public AttendeesViewModel()
		{
			Attendees = new ObservableCollection<AttendeeModel>();
		}

		public async Task GetAttendees()
		{
			if (IsBusy)
				return;
			
			try
			{
				IsBusy = true;
				Attendees.Clear();

				var items = await AzureMobileService.Instance.GetAttendees();
				foreach (var item in items)
					Attendees.Add(item);
			}
			catch (Exception e)
			{
				LogHelper.Instance.AddLog(e);
			}
			finally
			{
				IsBusy = false;
			}
		}

		private async Task AddAttendee()
		{
			await NavigationHelper.Instance.GotoDetails(new AttendeeModel());
		}
	}
}
