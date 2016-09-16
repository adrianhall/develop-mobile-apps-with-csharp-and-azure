using Xamarin.Forms;

namespace TaskList.Pages
{
    public partial class TaskDetail : ContentPage
    {
        public TaskDetail(Models.TodoItem item = null)
        {
            InitializeComponent();
            var vm = new ViewModels.TaskDetailViewModel(item);
            BindingContext = vm;
        }
    }
}
