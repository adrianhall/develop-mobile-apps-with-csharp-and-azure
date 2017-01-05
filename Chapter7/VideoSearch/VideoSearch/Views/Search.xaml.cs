using Xamarin.Forms;

namespace VideoSearch.Views
{
    public partial class Search : ContentPage
    {
        public Search()
        {
            InitializeComponent();
            BindingContext = new ViewModels.Search();
        }
    }
}
