using TaskList.ViewModels;
using Xamarin.Forms;

namespace TaskList.Pages
{
    public partial class EntryPage : ContentPage
    {
        public EntryPage()
        {
            InitializeComponent();
            BindingContext = new EntryPageViewModel();
        }
    }
}
