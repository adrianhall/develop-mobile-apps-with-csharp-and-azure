using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VideoApp.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class VideoList : ContentPage
	{
		public VideoList ()
		{
			InitializeComponent ();
			BindingContext = new ViewModels.VideoListViewModel();
		}
	}
}
