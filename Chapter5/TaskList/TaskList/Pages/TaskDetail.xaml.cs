using TaskList.ViewModels;
using Xamarin.Forms;

namespace TaskList.Pages
{
    public partial class TaskDetail : ContentPage
    {
        public TaskDetail(Models.TodoItem item = null)
        {
            InitializeComponent();
            var context = new TaskDetailViewModel(item);
            BindingContext = context;
        }
    }
}
