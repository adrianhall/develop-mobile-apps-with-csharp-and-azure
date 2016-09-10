using System.Threading.Tasks;
using TaskList.Abstractions;
using TaskList.Helpers;
using TaskList.Models;
using Xamarin.Forms;

namespace TaskList.Pages
{
    public partial class TaskDetail : ContentPage
    {
        public ICloudService CloudService => ServiceLocator.Get<ICloudService>();

        public TaskDetail(Models.TodoItem item = null)
        {
            InitializeComponent();
            BindingContext = new ViewModels.TaskDetailViewModel(item);

            var RefreshCommand = new Command(async () => await RefreshAsync());
            RefreshCommand.Execute(null);
        }

        public async Task RefreshAsync()
        {
            var table = await CloudService.GetTableAsync<Tag>();
            var items = await table.ReadAllItemsAsync();

            tagPicker.Items.AddRange(items);


        }
    }
}
