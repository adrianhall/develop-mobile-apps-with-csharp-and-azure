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

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            TaskDetailViewModel vm = BindingContext as TaskDetailViewModel;
            if (vm != null)
            {
                vm.TagPicker = tagPicker;
                vm.RefreshCommand.Execute(null);
            }
        }
    }
}
