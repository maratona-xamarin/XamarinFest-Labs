using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Helpers;
using Core.Models;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;

namespace Core.Services
{
	public class AzureMobileService
	{
		private static AzureMobileService _instance;
		public static AzureMobileService Instance => _instance ?? (_instance = new AzureMobileService());

		private MobileServiceClient _client;
		private IMobileServiceSyncTable<AttendeeModel> _attendee;

		private AzureMobileService() {}

		private async Task Initialize()
		{
			if (_client?.SyncContext?.IsInitialized ?? false)
				return;
			
			var store = new MobileServiceSQLiteStore(AppConfig.DatabaseName);
			store.DefineTable<AttendeeModel>();

			_client = new MobileServiceClient(AppConfig.MobileAppUri);
			await _client.SyncContext.InitializeAsync(store);

			_attendee = _client.GetSyncTable<AttendeeModel>();
		}

		private async Task SyncAttendees()
		{
			try
			{
				await _client.SyncContext.PushAsync();
				await _attendee.PullAsync("attendees", _attendee.CreateQuery());
			}
			catch (Exception e)
			{
				LogHelper.Instance.AddLog(e);
			}
		}

		public async Task<IList<AttendeeModel>> GetAttendees()
		{
			await Initialize();
			await SyncAttendees();
			return await _attendee.OrderBy(a => a.Name).ToListAsync();
		}

		public async Task SaveAttendee(AttendeeModel attendeeModel)
		{
			await Initialize();

			if (string.IsNullOrEmpty(attendeeModel.Id))
				await _attendee.InsertAsync(attendeeModel);
			else
				await _attendee.UpdateAsync(attendeeModel);

			await SyncAttendees();
		}

		public async Task DeleteAttendee(AttendeeModel attendeeModel)
		{
			await Initialize();
			await _attendee.DeleteAsync(attendeeModel);
			await SyncAttendees();
		}
	}
}
