using Xamarin.Forms;

namespace TaskList.Pages
{
    public partial class TagsList : ContentPage
    {
        public TagsList()
        {
            InitializeComponent();
            BindingContext = new ViewModels.TagsListViewModel();
        }
    }
}
