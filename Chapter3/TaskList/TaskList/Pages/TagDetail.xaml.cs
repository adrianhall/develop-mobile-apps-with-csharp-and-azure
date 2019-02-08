using Xamarin.Forms;

namespace TaskList.Pages
{
    public partial class TagDetail : ContentPage
    {
        public TagDetail(Models.Tag item = null)
        {
            InitializeComponent();
            BindingContext = new ViewModels.TagDetailViewModel(item);
        }
    }
}
