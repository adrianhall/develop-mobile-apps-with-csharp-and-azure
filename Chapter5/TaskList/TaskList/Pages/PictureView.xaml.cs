using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TaskList.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PictureView : ContentPage
    {
        public PictureView(string picture)
        {
            InitializeComponent();
            BindingContext = new ViewModels.PictureViewModel(picture);
        }
    }
}
