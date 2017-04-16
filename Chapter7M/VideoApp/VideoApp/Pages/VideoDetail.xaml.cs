using Plugin.MediaManager;
using Plugin.MediaManager.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VideoApp.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class VideoDetail : ContentPage
	{
		private IPlaybackController PlaybackController => CrossMediaManager.Current.PlaybackController;

		public VideoDetail(Models.Video video)
		{
			InitializeComponent();

			//CrossMediaManager.Current.PlayingChanged += (sender, e) =>
			//{
			//	ProgressBar.Progress = e.Progress;
			//	Duration.Text = $"{e.Duration.TotalSeconds} seconds";
			//};

			//VideoPlayer.Source = video.VideoUri;
			Title = video.Filename;
		}

		private void PlayClicked(object sender, System.EventArgs e)
		{
			//PlaybackController.Play();
		}

		private void PauseClicked(object sender, System.EventArgs e)
		{
			//PlaybackController.Pause();
		}

		private void StopClicked(object sender, System.EventArgs e)
		{
			//PlaybackController.Stop();
		}
	}
}
