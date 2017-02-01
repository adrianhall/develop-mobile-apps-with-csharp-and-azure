using VideoSearch.Models;
using Xamarin.Forms;

namespace VideoSearch.Views
{
    public partial class Details : ContentPage
    {
        public Details(Movie item = null)
        {
            InitializeComponent();
            var context = new ViewModels.Details(item);
            BindingContext = context;
        }
    }
}
