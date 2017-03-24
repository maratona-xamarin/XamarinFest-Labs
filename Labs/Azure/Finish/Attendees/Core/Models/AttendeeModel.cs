using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;

namespace Core.Models
{
	[DataTable("attendees")]
	public class AttendeeModel
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("email")]
		public string Email { get; set; }

		[JsonProperty("photo_name")]
		public string PhotoName { get; set; }

		[Version]
		public string Version { get; set; }
	}
}
