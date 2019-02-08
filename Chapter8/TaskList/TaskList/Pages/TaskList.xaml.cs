using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TaskList.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class TaskList : ContentPage
	{
		public TaskList ()
		{
			InitializeComponent ();
			BindingContext = new ViewModels.TaskListViewModel();
		}
	}
}
