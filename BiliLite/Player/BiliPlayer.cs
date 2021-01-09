using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BiliLite.Player
{
    public class BiliPlayer: UserControl,IBiliPlayer
    {

        public PlayerState PlayState
        {
            get { return (PlayerState)GetValue(PlayStateProperty); }
            set { SetValue(PlayStateProperty, value); }
        }
        public static readonly DependencyProperty PlayStateProperty =
            DependencyProperty.Register("PlayState", typeof(PlayerState), typeof(BiliPlayer), new PropertyMetadata(PlayerState.Loading));

        public TimeSpan Duration
        {
            get { return (TimeSpan)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(TimeSpan), typeof(BiliPlayer), new PropertyMetadata(0));

        public TimeSpan Position
        {
            get { return (TimeSpan)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(TimeSpan), typeof(BiliPlayer), new PropertyMetadata(0));



        public string MediaInfo
        {
            get { return (string)GetValue(MediaInfoProperty); }
            set { SetValue(MediaInfoProperty, value); }
        }

        public static readonly DependencyProperty MediaInfoProperty =
            DependencyProperty.Register("MediaInfo", typeof(string), typeof(BiliPlayer), new PropertyMetadata(""));




        public double Rate
        {
            get { return (double)GetValue(RateProperty); }
            set { SetValue(RateProperty, value); }
        }
        public static readonly DependencyProperty RateProperty =
            DependencyProperty.Register("Rate", typeof(double), typeof(BiliPlayer), new PropertyMetadata(1.0d));


        public double Volume
        {
            get { return (double)GetValue(VolumeProperty); }
            set { SetValue(VolumeProperty, value); }
        }
        public static readonly DependencyProperty VolumeProperty =
            DependencyProperty.Register("Volume", typeof(double), typeof(BiliPlayer), new PropertyMetadata(1.0d));

        public PlayerAspectRatio AspectRatio
        {
            get { return (PlayerAspectRatio)GetValue(AspectRatioProperty); }
            set { SetValue(AspectRatioProperty, value); }
        }

        public static readonly DependencyProperty AspectRatioProperty =
            DependencyProperty.Register("AspectRatio", typeof(PlayerAspectRatio), typeof(BiliPlayer), new PropertyMetadata(PlayerAspectRatio.Uniform));





        public event EventHandler<double> PositionChanged;
        public event EventHandler MediaOpened;
        public event EventHandler<string> MediaError;
        public event EventHandler<double> PlayBuffering;
        public event EventHandler PlayBufferEnd;
        public event EventHandler<PlayerState> PlayStateChanged;

        public virtual void Pause()
        {
            PlayState = PlayerState.Pause;
        }
        

        public virtual void Play()
        {
            PlayState = PlayerState.Playing;
        }

        public virtual void Seek(double position_s)
        {
            Position = TimeSpan.FromSeconds(position_s);
        }

        public virtual void SetRate(double rate)
        {
            Rate = rate;
        }

        public virtual void SetRatio(PlayerAspectRatio stretch)
        {
            AspectRatio = stretch;
        }

        public virtual void SetVolume(double volume)
        {
            Volume = volume;
        }

        public virtual void Stop()
        {
            PlayState = PlayerState.End;
        }

        public virtual void TakeScreenshot()
        {
            
        }

        public virtual bool SetMedia(MediaItem media, bool autoPlay = false)
        {
            return false;
        }
    }
}
