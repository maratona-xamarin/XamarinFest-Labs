using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;

namespace Core.Models
{
	public class AttendeeModel
	{
		[Version]
		public string Version { get; set; } // no change this, is important for Azure
	}
}
