using System.Threading.Tasks;
using Plugin.Media;
using Plugin.Media.Abstractions;

namespace Core.Plugins
{
	public class MediaService
	{
		private static MediaService _instance;
		public static MediaService Instance => _instance ?? (_instance = new MediaService());

		private MediaService() {}

		private async Task<bool> Initialize()
		{
			return await CrossMedia.Current.Initialize();
		}

		public async Task<bool> IsCameraAvailable()
		{
			await Initialize();
			return CrossMedia.Current.IsCameraAvailable;
		}

		public async Task<bool> IsPickPhotoSupported()
		{
			await Initialize();
			return CrossMedia.Current.IsPickPhotoSupported;
		}

		public async Task<MediaFile> PickPhotoAsync(PickMediaOptions options = null)
		{
			await Initialize();

			if (options == null)
			{
				options = new PickMediaOptions()
				{
					CompressionQuality = 5
				};
			}

			return await CrossMedia.Current.PickPhotoAsync(options);
		}

		public async Task<MediaFile> TakePhotoAsync(StoreCameraMediaOptions options = null)
		{
			await Initialize();

			if (options == null)
			{
				options = new StoreCameraMediaOptions()
				{
					CompressionQuality = 5
				};
			}

			return await CrossMedia.Current.TakePhotoAsync(options);
		}
	}
}
