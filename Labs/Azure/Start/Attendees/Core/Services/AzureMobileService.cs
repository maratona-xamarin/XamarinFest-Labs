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
				
			}
			catch (Exception e)
			{
				LogHelper.Instance.AddLog(e);
			}
		}

		public async Task<IList<AttendeeModel>> GetAttendees()
		{
			return new List<AttendeeModel>();
		}

		public async Task SaveAttendee(AttendeeModel attendeeModel)
		{
			
		}

		public async Task DeleteAttendee(AttendeeModel attendeeModel)
		{
			
		}
	}
}
