using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TaskList.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class EntryPage : ContentPage
	{
		public EntryPage ()
		{
			InitializeComponent ();
			BindingContext = new ViewModels.EntryPageViewModel();
		}
	}
}
