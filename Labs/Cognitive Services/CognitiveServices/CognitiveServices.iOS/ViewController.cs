using System;
using System.Threading.Tasks;
using BigTed;
using Plugin.Media;
using UIKit;

namespace CognitiveServices.iOS
{
	public partial class ViewController : UIViewController
	{
		async partial void BtnAnalyzeFace_Action(UIBarButtonItem sender)
		{
			if (CrossMedia.Current.IsPickPhotoSupported)
			{
				try
				{
					var result = await CrossMedia.Current.PickPhotoAsync();
					if (result == null)
						return;
					BTProgressHUD.Show("Analyzing");
					var face = await Service.Instance.AnalyzeFace(result.GetStream());
					if (face == null)
					{
						BTProgressHUD.Dismiss();
						return;
					}
					BTProgressHUD.Dismiss();
					await Task.Delay(1000);
					var stringData = $"Gender: {face.FaceAttributes.Gender}" + Environment.NewLine;
					stringData += $"Age: {face.FaceAttributes.Age}" + Environment.NewLine;
					stringData += $"Glasses: {face.FaceAttributes.Glasses.ToString()}" + Environment.NewLine;
					stringData += $"Beard: {face.FaceAttributes.FacialHair.Beard}" + Environment.NewLine;
					stringData += $"Moustache: {face.FaceAttributes.FacialHair.Moustache}" + Environment.NewLine;
					stringData += $"Sideburns: {face.FaceAttributes.FacialHair.Sideburns}" + Environment.NewLine;
					stringData += $"Smile: {face.FaceAttributes.Smile}";
					new UIAlertView("Microsoft Cognitive Services", stringData, null, "Ok").Show();
				}
				catch
				{
					BTProgressHUD.Dismiss();
				}
			}
		}

		async partial void BtnVerifyFace_Action(UIBarButtonItem sender)
		{
			if (CrossMedia.Current.IsPickPhotoSupported)
			{
				try
				{
					var result = await CrossMedia.Current.PickPhotoAsync();
					if (result == null)
						return;
					BTProgressHUD.Show("Searching");
					var person = await Service.Instance.FindSimilarFace(result.GetStream());
					BTProgressHUD.Dismiss();
					if (person == null)
					{
						BTProgressHUD.Dismiss();
						return;
					}
					await Task.Delay(1000);
					BTProgressHUD.ShowSuccessWithStatus(person.Name);
				}
				catch {
					BTProgressHUD.Dismiss();
				}
			}
		}

		protected ViewController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}

		public async override void ViewDidLoad()
		{
			base.ViewDidLoad();
			// Perform any additional setup after loading the view, typically from a nib.
			var dataSource = new MVPDataSource();
			_mainTableView.Delegate = dataSource;
			_mainTableView.DataSource = dataSource;
			_mainTableView.ReloadData();
			BTProgressHUD.Show("Initializing");
			await Service.Instance.RegisterEmployees();
			BTProgressHUD.Dismiss();
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}
	}
}
