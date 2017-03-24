using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Core.Services
{
	public class AzureStorageService
	{
		private static AzureStorageService _instance;
		public static AzureStorageService Instance => _instance ?? (_instance = new AzureStorageService());

		public CloudBlobContainer ImageContainer { get; private set; }

		private AzureStorageService() 
		{
			var account = CloudStorageAccount.Parse(AppConfig.StorageAppConfig);
			var client = account.CreateCloudBlobClient();
			ImageContainer = client.GetContainerReference("images");
		}

		public async Task UploadFile(Stream stream, string name)
		{
			
		}

		public async Task<byte[]> DownloadFile(string name)
		{
			return new byte[0];
		}

		public async Task<bool> DeleteFile(string name)
		{
			return false;
		}
	}
}
