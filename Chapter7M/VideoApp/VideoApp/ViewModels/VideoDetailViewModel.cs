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

        public VideoDetailViewModel(VideoView videoView, Video video)
        {
            Video = video;
            VideoView = videoView;
        }

        #region AutoPlay
        private bool _autoPlay = false;
        public bool AutoPlay
        {
            get { return _autoPlay; }
            set { SetProperty(ref _autoPlay, value, "AutoPlay"); }
        }
        #endregion

        #region FullScreen
        private bool _fullscreen = false;
        public bool FullScreen
        {
            get { return _fullscreen; }
            set { SetProperty(ref _fullscreen, value, "FullScreen"); }
        }
        #endregion

        #region Volume
        private double _volume = 1;
        public double Volume
        {
            get { return _volume; }
            set {
                SetProperty(ref _volume, value, "Volume");
                OnPropertyChanged(nameof(SliderVolume));
            }
        }
        #endregion

        #region SliderVolume
        public double SliderVolume
        {
            get { return _volume * 100; }
            set {
                try
                {
                    SetProperty(ref _volume, value / 100, "Volume");
                }
                catch
                {
                    SetProperty(ref _volume, 0L, "Volume");
                }
                OnPropertyChanged(nameof(SliderVolume));
            }
        }
        #endregion

        #region LoopPlay
        private bool _loopPlay = false;
        public bool LoopPlay
        {
            get { return _loopPlay; }
            set { SetProperty(ref _loopPlay, value, "LoopPlay"); }
        }
        #endregion

        #region ShowController
        private bool _showController = false;
        public bool ShowController
        {
            get { return _showController; }
            set { SetProperty(ref _showController, value, "ShowController"); }
        }
        #endregion

        #region Muted
        private bool _muted = false;
        public bool Muted
        {
            get { return _muted; }
            set { SetProperty(ref _muted, value, "Muted"); }
        }
        #endregion

        #region Duration
        private TimeSpan _duration;
        public TimeSpan Duration
        {
            get { return _duration; }
        }

        public double SliderDuration
        {
            get
            {
                double totalMilliseconds = _duration.TotalMilliseconds;
                return (totalMilliseconds <= 0) ? 1 : totalMilliseconds;
            }
        }
        #endregion Duration

        #region LabelVideoStatus
        private string _labelVideoStatus;
        public string LabelVideoStatus
        {
            get { return _labelVideoStatus; }
        }
        #endregion

        #region Position
        private TimeSpan _position;
        public TimeSpan Position
        {
            get { return _position; }
            set
            {
                SetProperty(ref _position, value, "Position");
                OnPropertyChanged(nameof(SliderPosition));
            }
        }

        public double SliderPosition
        {
            get { return _position.TotalMilliseconds; }
            set
            {
                try
                {
                    SetProperty(ref _position, TimeSpan.FromMilliseconds(value), "Position");
                }
                catch
                {
                    SetProperty(ref _position, TimeSpan.Zero, "Position");
                }
                OnPropertyChanged(nameof(SliderPosition));
            }
        }
        #endregion

        #region PositionInterval
        private TimeSpan _PositionInterval = TimeSpan.FromMilliseconds(500);
        public TimeSpan PositionInterval
        {
            get { return _PositionInterval; }
            set
            {
                SetProperty(ref _PositionInterval, value, "PositionInterval");
                OnPropertyChanged(nameof(EntryPositionInterval));
            }
        }
        #endregion

        #region EntryPositionInterval
        public string EntryPositionInterval
        {
            get { return _PositionInterval.TotalMilliseconds.ToString(); }
            set
            {
                int positionIntervalMilliseconds;
                if (int.TryParse(value, out positionIntervalMilliseconds))
                {
                    _PositionInterval = TimeSpan.FromMilliseconds(positionIntervalMilliseconds);
                }
                else
                {
                    _PositionInterval = TimeSpan.Zero;
                }

                OnPropertyChanged(nameof(PositionInterval));
                OnPropertyChanged(nameof(EntryPositionInterval));
            }
        }
        #endregion

        #region Video
        private Video _video;
        public Video Video
        {
            get { return _video; }
            set
            {
                SetProperty(ref _video, value, "Video");
                OnPropertyChanged(nameof(EntrySource));
                OnPropertyChanged(nameof(VideoSource));
            }
        }

        public string EntrySource
        {
            get
            {
                return _video.VideoUri;
            }
        }

        public ImageSource VideoSource
        {
            get
            {
                ImageSource videoSource = null;
                try
                {
                    ImageSourceConverter converter = new ImageSourceConverter();
                    videoSource = (ImageSource)converter.ConvertFromInvariantString(EntrySource);
                }
                catch
                {

                }
                return videoSource;
            }
        }
        #endregion

        public ICommand PropertyChangedCommand
        {
            get
            {
                return new Command<string>((propertyName) =>
                {
                    switch (propertyName)
                    {
                        case nameof(VideoView.VideoState):
                            {
                                SetProperty(ref _labelVideoStatus, VideoView.VideoState.ToString(), "LabelViewStatus");
                                break;
                            }
                        case nameof(VideoView.Duration):
                            {
                                SetProperty(ref _duration, VideoView.Duration, "Duration");
                                OnPropertyChanged(nameof(SliderDuration));
                                break;
                            }
                    }
                });
            }
        }

        public ICommand PlayCommand => new Command(async () => await VideoView.Start());
        public ICommand PauseCommand => new Command(async () => await VideoView.Pause());
        public ICommand StopCommand => new Command(async () => await VideoView.Stop());
    }
}
