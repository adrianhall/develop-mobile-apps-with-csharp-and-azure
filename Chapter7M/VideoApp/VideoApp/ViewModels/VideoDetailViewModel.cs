using Rox;
using System;
using System.Windows.Input;
using VideoApp.Abstractions;
using VideoApp.Models;
using Xamarin.Forms;

namespace VideoApp.ViewModels
{
    public class VideoDetailViewModel : BaseViewModel
    {
        private readonly VideoView VideoView;

        public VideoDetailViewModel(Video video)
        {
            Video = video;
        }
    }
}
