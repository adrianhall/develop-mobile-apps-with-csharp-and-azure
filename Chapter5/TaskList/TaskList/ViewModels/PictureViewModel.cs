using TaskList.Abstractions;
using Xamarin.Forms;

namespace TaskList.ViewModels
{
    public class PictureViewModel : BaseViewModel
    {
        public PictureViewModel(string picture = null)
        {
            if (picture != null)
            {
                PictureSource = picture;
            }
            Title = "A Picture for you";
        }

        public string PictureSource { get; }
    }
}
