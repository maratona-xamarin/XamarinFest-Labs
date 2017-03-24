using Core.Models;
using Core.ViewModels;
using Xamarin.Forms;

namespace Core.Views
{
	public partial class DetailsView : ContentPage
	{
		public DetailsView(AttendeeModel attendeeModel)
		{
			InitializeComponent();
			BindingContext = new DetailsViewModel(attendeeModel);
		}
	}
}
