using Android.App;
using Android.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Plugin.Permissions;
using Android.Content.PM;
using Plugin.Media;
using AndroidHUD;
using System.Threading.Tasks;
using System;

namespace CognitiveServices.Android
{
	[Activity(Label = "Cognitive Services", Theme="@style/Theme.AppCompat.Light.NoActionBar", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : AppCompatActivity
	{
		async void BtnVerify_Click(object sender, System.EventArgs e)
		{
			if (CrossMedia.Current.IsPickPhotoSupported)
			{
				try
				{
					var result = await CrossMedia.Current.PickPhotoAsync();
					if (result == null)
						return;
					AndHUD.Shared.Show(this, "Searching");
					var person = await Service.Instance.FindSimilarFace(result.GetStream());
					AndHUD.Shared.Dismiss(this);
					if (person == null)
					{
						AndHUD.Shared.Dismiss(this);
						return;
					}
					await Task.Delay(1000);
					AndHUD.Shared.ShowSuccessWithStatus(this, person.Name, MaskType.Black, TimeSpan.FromSeconds(1));
				}
				catch
				{
					AndHUD.Shared.Dismiss(this);
				}
			}
		}

		async void BtnAnalyze_Click(object sender, System.EventArgs e)
		{
			if (CrossMedia.Current.IsPickPhotoSupported)
			{
				try
				{
					var result = await CrossMedia.Current.PickPhotoAsync();
					if (result == null)
						return;
					AndHUD.Shared.Show(this, "Analyzing");
					var face = await Service.Instance.AnalyzeFace(result.GetStream());
					if (face == null)
					{
						AndHUD.Shared.Dismiss(this);
						return;
					}
					AndHUD.Shared.Dismiss(this);
					await Task.Delay(1000);
					var stringData = $"Gender: {face.FaceAttributes.Gender}" + Environment.NewLine;
					stringData += $"Age: {face.FaceAttributes.Age}" + Environment.NewLine;
					stringData += $"Glasses: {face.FaceAttributes.Glasses.ToString()}" + Environment.NewLine;
					stringData += $"Beard: {face.FaceAttributes.FacialHair.Beard}" + Environment.NewLine;
					stringData += $"Moustache: {face.FaceAttributes.FacialHair.Moustache}" + Environment.NewLine;
					stringData += $"Sideburns: {face.FaceAttributes.FacialHair.Sideburns}" + Environment.NewLine;
					stringData += $"Smile: {face.FaceAttributes.Smile}";

					global::Android.Support.V7.App.AlertDialog.Builder builder =
									   new global::Android.Support.V7.App.AlertDialog.Builder(this)
									   .SetTitle("Microsoft Cognitive Services")
									   .SetPositiveButton("Ok", (sender1, e1) => { })
									   .SetMessage(stringData);
					builder.Show();
				}
				catch
				{
					AndHUD.Shared.Dismiss(this);
				}
			}
		}

		protected async override void OnCreate(global::Android.OS.Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);
			var recycler = FindViewById<RecyclerView>(Resource.Id.recyclerView);
			recycler.SetAdapter(new MVPAdapter());
			recycler.SetLayoutManager(new LinearLayoutManager(this));

			var btnAnalyze = FindViewById<Button>(Resource.Id.btnAnalyze);
			btnAnalyze.Click += BtnAnalyze_Click;

			var btnVerify = FindViewById<Button>(Resource.Id.btnVerify);
			btnVerify.Click += BtnVerify_Click;

			AndHUD.Shared.Show(this, "Initializing");
			await Service.Instance.RegisterEmployees();
			AndHUD.Shared.Dismiss();
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
		{
			PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}
	}
}

