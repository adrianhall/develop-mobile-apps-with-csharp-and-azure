using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TaskList.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class TaskDetail : ContentPage
	{
		public TaskDetail (Models.TodoItem item = null)
		{
			InitializeComponent ();
			BindingContext = new ViewModels.TaskDetailViewModel(item);
		}
	}
}
