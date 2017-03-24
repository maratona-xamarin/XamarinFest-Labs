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
			await ImageContainer.CreateIfNotExistsAsync();
			var blob = ImageContainer.GetBlockBlobReference(name);
			await blob.UploadFromStreamAsync(stream);
		}

		public async Task<byte[]> DownloadFile(string name)
		{
			await ImageContainer.CreateIfNotExistsAsync();
			var blob = ImageContainer.GetBlobReference(name);
			if (await blob.ExistsAsync())
			{
				await blob.FetchAttributesAsync();
				byte[] bytes = new byte[blob.Properties.Length];
				await blob.DownloadToByteArrayAsync(bytes, 0);
				return bytes;
			}

			return null;
		}

		public async Task<bool> DeleteFile(string name)
		{
			await ImageContainer.CreateIfNotExistsAsync();
			var blob = ImageContainer.GetBlobReference(name);
			return await blob.DeleteIfExistsAsync();
		}
	}
}
