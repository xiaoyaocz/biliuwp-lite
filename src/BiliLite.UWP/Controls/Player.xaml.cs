using BiliLite.Helpers;
using BiliLite.Modules;
using BiliLite.Modules.Player.Playurl;
using FFmpegInteropX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Editing;
using Windows.Media.Playback;
using Windows.Media.Streaming.Adaptive;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls
{
    public enum PlayState
    {
        Loading,
        Playing,
        Pause,
        End,
        Error
    }
    public enum PlayEngine
    {
        Native = 1,
        FFmpegInteropMSS = 2,
        SYEngine = 3,
        FFmpegInteropMSSH265 = 4,
        VLC = 5
    }
    public enum PlayMediaType
    {
        Single,
        MultiFlv,
        Dash
    }
    //TODO 写得太复杂了，需要重写
    public sealed partial class Player : UserControl, IDisposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void DoPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public PlayState PlayState { get; set; }
        public PlayMediaType PlayMediaType { get; set; }
        public VideoPlayHistoryHelper.ABPlayHistoryEntry ABPlay { get; set; }
        private BiliDashPlayUrlInfo _dashInfo;
        private PlayEngine current_engine;

        private FFmpegMediaSource _ffmpegMSSVideo;
        private MediaPlayer _playerVideo;
        //音视频分离
        private FFmpegMediaSource _ffmpegMSSAudio;
        private MediaPlayer _playerAudio;
        private MediaTimelineController _mediaTimelineController;

        //多段FLV
        private List<FFmpegMediaSource> _ffmpegMSSItems;
        private MediaPlaybackList _mediaPlaybackList;




        /// <summary>
        /// 播放状态变更
        /// </summary>
        public event EventHandler<PlayState> PlayStateChanged;
        /// <summary>
        /// 媒体加载完成
        /// </summary>
        public event EventHandler PlayMediaOpened;
        /// <summary>
        /// 播放完成
        /// </summary>
        public event EventHandler PlayMediaEnded;

        /// <summary>
        /// 播放错误
        /// </summary>
        public event EventHandler<string> PlayMediaError;
        /// <summary>
        /// 更改播放引擎
        /// </summary>
        public event EventHandler<ChangePlayerEngine> ChangeEngine;

        /// <summary>
        /// 进度
        /// </summary>
        public double Position
        {
            get { return (double)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(double), typeof(Player), new PropertyMetadata(0.0, OnPositionChanged));
        private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = d as Player;
            if (sender.ABPlay != null && sender.ABPlay.PointB != 0 && (double)e.NewValue > sender.ABPlay.PointB)
            {
                sender.Position = sender.ABPlay.PointA;
                return;
            }
            if (Math.Abs((double)e.NewValue - (double)e.OldValue) > 1)
            {
                if (sender.PlayState == PlayState.Playing || sender.PlayState == PlayState.Pause)
                {
                    sender.SetPosition((double)e.NewValue);
                }
            }
        }

        /// <summary>
        /// 时长
        /// </summary>
        public double Duration
        {
            get { return (double)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(double), typeof(Player), new PropertyMetadata(0.0));

        /// <summary>
        /// 音量0-1
        /// </summary>
        public double Volume
        {
            get { return (double)GetValue(VolumeProperty); }
            set
            {
                if (value > 1)
                {
                    value = 1;
                }
                if (value < 0)
                {
                    value = 0;
                }
                SetValue(VolumeProperty, value);
            }
        }
        public static readonly DependencyProperty VolumeProperty =
            DependencyProperty.Register("Volume", typeof(double), typeof(Player), new PropertyMetadata(1.0, OnVolumeChanged));
        private static void OnVolumeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = d as Player;
            var value = (double)e.NewValue;
            if (value > 1)
            {
                value = 1;
            }
            else if (value < 0)
            {
                value = 0;
            }
            sender.SetVolume(value);
        }


        /// <summary>
        /// 是否缓冲中
        /// </summary>
        public bool Buffering
        {
            get { return (bool)GetValue(BufferingProperty); }
            set { SetValue(BufferingProperty, value); }
        }
        public static readonly DependencyProperty BufferingProperty =
            DependencyProperty.Register("Buffering", typeof(bool), typeof(Player), new PropertyMetadata(false));


        /// <summary>
        /// 缓冲进度,0-100
        /// </summary>
        public double BufferCache
        {
            get { return (double)GetValue(BufferCacheProperty); }
            set { SetValue(BufferCacheProperty, value); }
        }


        public static readonly DependencyProperty BufferCacheProperty =
            DependencyProperty.Register("BufferCache", typeof(double), typeof(Player), new PropertyMetadata(1));




        /// <summary>
        /// 播放速度
        /// </summary>
        public double Rate { get; set; } = 1.0;


        /// <summary>
        /// 媒体信息
        /// </summary>
        public string MediaInfo
        {
            get { return (string)GetValue(MediaInfoProperty); }
            set { SetValue(MediaInfoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MediaInfo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MediaInfoProperty =
            DependencyProperty.Register("MediaInfo", typeof(string), typeof(Player), new PropertyMetadata(""));


        public bool Opening { get; set; }
        public Player()
        {
            this.InitializeComponent();

            // We don't have ARM64 support of SYEngine.
            if (RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
            {
                SYEngine.Core.ForceNetworkMode = true;
                SYEngine.Core.ForceSoftwareDecode = !SettingHelper.GetValue<bool>(SettingHelper.Player.HARDWARE_DECODING, false);
            }
            //_ffmpegConfig.StreamBufferSize = 655360;//1024 * 30;

        }
        /// <summary>
        /// 使用AdaptiveMediaSource播放视频
        /// </summary>
        /// <param name="videoUrl"></param>
        /// <param name="audioUrl"></param>
        /// <param name="positon"></param>
        /// <param name="needConfig"></param>
        /// <returns></returns>
        public async Task<PlayerOpenResult> PlayerDashUseNative(BiliDashPlayUrlInfo dashInfo, string userAgent, string referer, double positon = 0)
        {
            try
            {
                mediaPlayerVideo.Visibility = Visibility.Visible;
                //vlcVideoView.Visibility = Visibility.Collapsed;
                _dashInfo = dashInfo;

                Opening = true;
                current_engine = PlayEngine.Native;
                PlayMediaType = PlayMediaType.Dash;
                //加载中
                PlayState = PlayState.Loading;
                PlayStateChanged?.Invoke(this, PlayState);
                ClosePlay();


                //设置播放器
                _playerVideo = new MediaPlayer();
                //_playerVideo.Source = MediaSource.CreateFromUri(new Uri(videoUrl.baseUrl));
                var mediaSource = await CreateAdaptiveMediaSource(dashInfo, userAgent, referer);
                if (mediaSource == null)
                {
                    return new PlayerOpenResult()
                    {
                        result = false,
                        message = "创建MediaSource失败"
                    };
                }

                _playerVideo.Source = MediaSource.CreateFromAdaptiveMediaSource(mediaSource);
                Buffering = true;
              
                //设置时长
                _playerVideo.MediaOpened += new TypedEventHandler<MediaPlayer, object>(async (e, arg) =>
                {
                    Opening = false;
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Buffering = false;
                        Duration = _playerVideo.PlaybackSession.NaturalDuration.TotalSeconds;
                        PlayMediaOpened?.Invoke(this, new EventArgs());

                        ////设置进度
                        //if (positon != 0)
                        //{
                        //    _playerVideo.PlaybackSession.Position = TimeSpan.FromSeconds(positon);
                        //}
                    });
                });

                //播放完成
                _playerVideo.MediaEnded += new TypedEventHandler<MediaPlayer, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        //加个判断，是否真的播放完成了
                        if (Position.ToInt32() >= Duration.ToInt32())
                        {
                            PlayState = PlayState.End;
                            Position = 0;
                            PlayStateChanged?.Invoke(this, PlayState);
                            PlayMediaEnded?.Invoke(this, new EventArgs());
                        }
                    });
                });
                //播放错误
                _playerVideo.MediaFailed += new TypedEventHandler<MediaPlayer, MediaPlayerFailedEventArgs>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        PlayState = PlayState.Error;
                        PlayStateChanged?.Invoke(this, PlayState);
                        ChangeEngine?.Invoke(this, new ChangePlayerEngine()
                        {
                            change_engine = PlayEngine.FFmpegInteropMSS,
                            current_mode = PlayEngine.Native,
                            need_change = true,
                            play_type = PlayMediaType.Dash
                        });
                    });
                });
                //缓冲开始
                _playerVideo.PlaybackSession.BufferingStarted += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Buffering = true;
                    });
                });
                //缓冲进行中
                _playerVideo.PlaybackSession.BufferingProgressChanged += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Buffering = true;
                        BufferCache = e.BufferingProgress;
                    });


                });
                //缓冲进行中
                _playerVideo.PlaybackSession.BufferingEnded += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Buffering = false;
                    });
                });
                //进度变更
                _playerVideo.PlaybackSession.PositionChanged += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        try
                        {
                            Position = e.Position.TotalSeconds;
                        }
                        catch (Exception)
                        {
                        }
                    });
                });

                PlayState = PlayState.Pause;
                PlayStateChanged?.Invoke(this, PlayState);
                //设置音量
                _playerVideo.Volume = Volume;
                //设置速率
                _playerVideo.PlaybackSession.PlaybackRate = Rate;

                //绑定MediaPlayer
                mediaPlayerVideo.SetMediaPlayer(_playerVideo);

                return new PlayerOpenResult()
                {
                    result = true
                };
            }
            catch (Exception ex)
            {
                //PlayMediaError?.Invoke(this, "视频加载时出错:" + ex.Message);
                return new PlayerOpenResult()
                {
                    result = false,
                    message = ex.Message,
                    detail_message = ex.StackTrace
                };
            }
        }
        /// <summary>
        /// 使用eMediaSource播放视频
        /// </summary>
        /// <param name="videoUrl"></param>
        /// <param name="audioUrl"></param>
        /// <param name="positon"></param>
        /// <param name="needConfig"></param>
        /// <returns></returns>
        public async Task<PlayerOpenResult> PlayerSingleMp4UseNativeAsync(string url, double positon = 0, bool needConfig = true, bool isLocal = false)
        {
            try
            {
                mediaPlayerVideo.Visibility = Visibility.Visible;
                //vlcVideoView.Visibility = Visibility.Collapsed;

                Opening = true;
                current_engine = PlayEngine.Native;
                PlayMediaType = PlayMediaType.Dash;
                //加载中
                PlayState = PlayState.Loading;
                PlayStateChanged?.Invoke(this, PlayState);
                ClosePlay();

                //设置播放器
                _playerVideo = new MediaPlayer();
                if (isLocal)
                {
                    _playerVideo.Source = MediaSource.CreateFromStorageFile(await StorageFile.GetFileFromPathAsync(url));
                }
                else
                {
                    _playerVideo.Source = MediaSource.CreateFromUri(new Uri(url));
                }


                //设置时长
                _playerVideo.MediaOpened += new TypedEventHandler<MediaPlayer, object>(async (e, arg) =>
                {
                    Opening = false;
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Duration = _playerVideo.PlaybackSession.NaturalDuration.TotalSeconds;
                        PlayMediaOpened?.Invoke(this, new EventArgs());

                        ////设置进度
                        //if (positon != 0)
                        //{
                        //    _playerVideo.PlaybackSession.Position = TimeSpan.FromSeconds(positon);
                        //}
                    });
                });

                //播放完成
                _playerVideo.MediaEnded += new TypedEventHandler<MediaPlayer, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        //加个判断，是否真的播放完成了
                        if (Position.ToInt32() >= Duration.ToInt32())
                        {
                            PlayState = PlayState.End;
                            Position = 0;
                            PlayStateChanged?.Invoke(this, PlayState);
                            PlayMediaEnded?.Invoke(this, new EventArgs());
                        }
                    });
                });
                //播放错误
                _playerVideo.MediaFailed += new TypedEventHandler<MediaPlayer, MediaPlayerFailedEventArgs>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        PlayState = PlayState.Error;
                        PlayStateChanged?.Invoke(this, PlayState);
                        ChangeEngine?.Invoke(this, new ChangePlayerEngine()
                        {
                            change_engine = PlayEngine.FFmpegInteropMSS,
                            current_mode = PlayEngine.Native,
                            need_change = true,
                            play_type = PlayMediaType.Dash
                        });
                    });
                });
                //缓冲开始
                _playerVideo.PlaybackSession.BufferingStarted += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Buffering = true;
                    });
                });
                //缓冲进行中
                _playerVideo.PlaybackSession.BufferingProgressChanged += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Buffering = true;
                        BufferCache = e.BufferingProgress;
                    });


                });
                //缓冲进行中
                _playerVideo.PlaybackSession.BufferingEnded += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Buffering = false;
                    });
                });
                //进度变更
                _playerVideo.PlaybackSession.PositionChanged += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        try
                        {
                            Position = e.Position.TotalSeconds;
                        }
                        catch (Exception)
                        {
                        }
                    });
                });

                PlayState = PlayState.Pause;
                PlayStateChanged?.Invoke(this, PlayState);
                //设置音量
                _playerVideo.Volume = Volume;
                //设置速率
                _playerVideo.PlaybackSession.PlaybackRate = Rate;

                //绑定MediaPlayer
                mediaPlayerVideo.SetMediaPlayer(_playerVideo);

                return new PlayerOpenResult()
                {
                    result = true
                };
            }
            catch (Exception ex)
            {
                //PlayMediaError?.Invoke(this, "视频加载时出错:" + ex.Message);
                return new PlayerOpenResult()
                {
                    result = false,
                    message = ex.Message,
                    detail_message = ex.StackTrace
                };
            }
        }

        /// <summary>
        /// 使用FFmpegInterop解码播放音视频分离视频
        /// </summary>
        /// <param name="videoUrl"></param>
        /// <param name="audioUrl"></param>
        /// <param name="positon"></param>
        /// <param name="needConfig"></param>
        /// <returns></returns>
        public async Task<PlayerOpenResult> PlayDashUseFFmpegInterop(BiliDashPlayUrlInfo dashPlayUrlInfo, string userAgent, string referer, double positon = 0, bool needConfig = true, bool isLocal = false)
        {
            try
            {
                mediaPlayerVideo.Visibility = Visibility.Visible;
                //vlcVideoView.Visibility = Visibility.Collapsed;
                Opening = true;
                _dashInfo = dashPlayUrlInfo;

                current_engine = PlayEngine.FFmpegInteropMSS;

                PlayMediaType = PlayMediaType.Dash;
                //加载中
                PlayState = PlayState.Loading;
                PlayStateChanged?.Invoke(this, PlayState);
                //关闭正在播放的视频
                ClosePlay();
                var _ffmpegConfig = CreateFFmpegInteropConfig(userAgent, referer);
                if (isLocal)
                {

                    var videoFile = await StorageFile.GetFileFromPathAsync(dashPlayUrlInfo.Video.Url);
                    _ffmpegMSSVideo = await FFmpegMediaSource.CreateFromStreamAsync(await videoFile.OpenAsync(FileAccessMode.Read), _ffmpegConfig);
                    if (dashPlayUrlInfo.Audio != null)
                    {
                        var audioFile = await StorageFile.GetFileFromPathAsync(dashPlayUrlInfo.Audio.Url);
                        _ffmpegMSSAudio = await FFmpegMediaSource.CreateFromStreamAsync(await audioFile.OpenAsync(FileAccessMode.Read), _ffmpegConfig);
                    }
                }
                else
                {
                    _ffmpegMSSVideo = await FFmpegMediaSource.CreateFromUriAsync(dashPlayUrlInfo.Video.Url, _ffmpegConfig);
                    if (dashPlayUrlInfo.Audio != null)
                    {
                        _ffmpegMSSAudio = await FFmpegMediaSource.CreateFromUriAsync(dashPlayUrlInfo.Audio.Url, _ffmpegConfig);
                    }

                }


                //设置时长
                Duration = _ffmpegMSSVideo.Duration.TotalSeconds;
                //设置视频
                _playerVideo = new MediaPlayer();
                _playerVideo.Source = _ffmpegMSSVideo.CreateMediaPlaybackItem();
                //设置音频
                if (dashPlayUrlInfo.Audio != null)
                {
                    _playerAudio = new MediaPlayer();
                    _playerAudio.Source = _ffmpegMSSAudio.CreateMediaPlaybackItem();
                }

                //设置时间线控制器
                _mediaTimelineController = new MediaTimelineController();
                _playerVideo.CommandManager.IsEnabled = false;
                _playerVideo.TimelineController = _mediaTimelineController;
                if (dashPlayUrlInfo.Audio != null)
                {
                    _playerAudio.CommandManager.IsEnabled = true;
                    _playerAudio.TimelineController = _mediaTimelineController;
                }

                _playerVideo.MediaOpened += new TypedEventHandler<MediaPlayer, object>(async (e, arg) =>
                {
                    Opening = false;
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        PlayMediaOpened?.Invoke(this, new EventArgs());
                    });
                });
                //播放完成
                _playerVideo.MediaEnded += new TypedEventHandler<MediaPlayer, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        //加个判断，是否真的播放完成了
                        if (Position.ToInt32() >= Duration.ToInt32())
                        {
                            PlayState = PlayState.End;
                            Position = 0;
                            PlayStateChanged?.Invoke(this, PlayState);
                            PlayMediaEnded?.Invoke(this, new EventArgs());
                        }
                    });
                });
                //播放错误
                _playerVideo.MediaFailed += new TypedEventHandler<MediaPlayer, MediaPlayerFailedEventArgs>(async (e, arg) =>
                {
                    if (_playerVideo == null || _playerVideo.Source == null) return;
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        PlayState = PlayState.Error;
                        PlayStateChanged?.Invoke(this, PlayState);
                        ChangeEngine?.Invoke(this, new ChangePlayerEngine()
                        {
                            need_change = false,
                            message = arg.ErrorMessage
                        });
                    });

                });
                //缓冲开始
                _playerVideo.PlaybackSession.BufferingStarted += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Buffering = true;
                    });


                });



                //缓冲进行中
                _playerVideo.PlaybackSession.BufferingProgressChanged += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Buffering = true;
                        BufferCache = e.BufferingProgress;
                    });

                });

                //缓冲进行中
                _playerVideo.PlaybackSession.BufferingEnded += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Buffering = false;
                    });
                });

                //进度变更
                _playerVideo.PlaybackSession.PositionChanged += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        try
                        {
                            Position = e.Position.TotalSeconds;
                        }
                        catch (Exception)
                        {
                        }
                    });
                });
                if (dashPlayUrlInfo.Audio != null)
                {
                    _playerAudio.PlaybackSession.BufferingStarted += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                    {
                        await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            Buffering = true;
                        });
                    });
                    _playerAudio.PlaybackSession.BufferingProgressChanged += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                    {
                        await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            Buffering = true;
                            BufferCache = e.BufferingProgress;
                        });
                    });
                    _playerAudio.PlaybackSession.BufferingEnded += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                    {
                        await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            Buffering = false;
                        });
                    });
                    //设置音量
                    _playerAudio.Volume = Volume;
                    mediaPlayerAudio.SetMediaPlayer(_playerAudio);
                }
                else
                {
                    _playerVideo.Volume = Volume;
                }
                PlayState = PlayState.Pause;
                PlayStateChanged?.Invoke(this, PlayState);

                //绑定MediaPlayer
                mediaPlayerVideo.SetMediaPlayer(_playerVideo);

                //设置速率
                _mediaTimelineController.ClockRate = Rate;

                return new PlayerOpenResult()
                {
                    result = true
                };
            }
            catch (Exception ex)
            {
                //PlayMediaError?.Invoke(this, "视频加载时出错:" + ex.Message);
                return new PlayerOpenResult()
                {
                    result = false,
                    message = ex.Message,
                    detail_message = ex.StackTrace
                };
            }
        }
        /// <summary>
        /// 使用FFmpeg解码播放单FLV视频
        /// </summary>
        /// <param name="url"></param>
        /// <param name="positon"></param>
        /// <param name="needConfig"></param>
        /// <returns></returns>
        public async Task<PlayerOpenResult> PlayDashUrlUseFFmpegInterop(string url, string userAgent, string referer, double positon = 0, bool needConfig = true)
        {

            try
            {
                mediaPlayerVideo.Visibility = Visibility.Visible;
                Opening = true;
                current_engine = PlayEngine.FFmpegInteropMSS;

                PlayMediaType = PlayMediaType.Single;
                //加载中
                PlayState = PlayState.Loading;
                PlayStateChanged?.Invoke(this, PlayState);
                //关闭正在播放的视频
                ClosePlay();

                var _ffmpegConfig = CreateFFmpegInteropConfig(userAgent, referer);
                _ffmpegMSSVideo = await FFmpegMediaSource.CreateFromUriAsync(url, _ffmpegConfig);


                //设置时长
                Duration = _ffmpegMSSVideo.Duration.TotalSeconds;
                //设置播放器
                _playerVideo = new MediaPlayer();
                var mediaSource = _ffmpegMSSVideo.CreateMediaPlaybackItem();
                _playerVideo.Source = mediaSource;

                _playerVideo.MediaOpened += new TypedEventHandler<MediaPlayer, object>(async (e, arg) =>
                {
                    Opening = false;
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        PlayMediaOpened?.Invoke(this, new EventArgs());
                    });
                });
                //播放完成
                _playerVideo.MediaEnded += new TypedEventHandler<MediaPlayer, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        //加个判断，是否真的播放完成了
                        if (Position.ToInt32() >= Duration.ToInt32())
                        {
                            PlayState = PlayState.End;
                            Position = 0;
                            PlayStateChanged?.Invoke(this, PlayState);
                            PlayMediaEnded?.Invoke(this, new EventArgs());
                        }
                    });
                });
                //播放错误
                _playerVideo.MediaFailed += new TypedEventHandler<MediaPlayer, MediaPlayerFailedEventArgs>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        PlayState = PlayState.Error;
                        PlayStateChanged?.Invoke(this, PlayState);
                        ChangeEngine?.Invoke(this, new ChangePlayerEngine()
                        {
                            change_engine = PlayEngine.SYEngine,
                            current_mode = PlayEngine.FFmpegInteropMSS,
                            need_change = true,
                            play_type = PlayMediaType.Single
                        });
                    });
                });
                //缓冲开始
                _playerVideo.PlaybackSession.BufferingStarted += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Buffering = true;
                    });
                });
                //缓冲进行中
                _playerVideo.PlaybackSession.BufferingProgressChanged += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Buffering = true;
                        BufferCache = e.BufferingProgress;
                    });
                });
                //缓冲进行中
                _playerVideo.PlaybackSession.BufferingEnded += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Buffering = false;
                    });
                });
                //进度变更
                _playerVideo.PlaybackSession.PositionChanged += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        try
                        {
                            Position = e.Position.TotalSeconds;
                        }
                        catch (Exception)
                        {
                        }
                    });
                });

                PlayState = PlayState.Pause;
                PlayStateChanged?.Invoke(this, PlayState);
                //设置音量
                _playerVideo.Volume = Volume;
                //设置速率
                _playerVideo.PlaybackSession.PlaybackRate = Rate;
                ////设置进度
                //if (positon != 0)
                //{
                //    _playerVideo.PlaybackSession.Position = TimeSpan.FromSeconds(positon);
                //}
                //绑定MediaPlayer
                mediaPlayerVideo.SetMediaPlayer(_playerVideo);
                return new PlayerOpenResult()
                {
                    result = true
                };
            }
            catch (Exception ex)
            {
                //PlayMediaError?.Invoke(this, "视频加载时出错:" + ex.Message);
                return new PlayerOpenResult()
                {
                    result = false,
                    message = ex.Message,
                    detail_message = ex.StackTrace
                };
            }
        }
        /// <summary>
        /// 使用FFmpeg解码播放单FLV视频
        /// </summary>
        /// <param name="url"></param>
        /// <param name="positon"></param>
        /// <param name="needConfig"></param>
        /// <returns></returns>
        public async Task<PlayerOpenResult> PlaySingleFlvUseFFmpegInterop(string url, string userAgent, string referer, double positon = 0, bool needConfig = true)
        {

            try
            {
                mediaPlayerVideo.Visibility = Visibility.Visible;
                //vlcVideoView.Visibility = Visibility.Collapsed;
                Opening = true;
                current_engine = PlayEngine.FFmpegInteropMSS;

                PlayMediaType = PlayMediaType.Single;
                //加载中
                PlayState = PlayState.Loading;
                PlayStateChanged?.Invoke(this, PlayState);
                //关闭正在播放的视频
                ClosePlay();

                var _ffmpegConfig = CreateFFmpegInteropConfig(userAgent, referer);
                _ffmpegMSSVideo = await FFmpegMediaSource.CreateFromUriAsync(url, _ffmpegConfig);


                //设置时长
                Duration = _ffmpegMSSVideo.Duration.TotalSeconds;
                //设置播放器
                _playerVideo = new MediaPlayer();
                var mediaSource = _ffmpegMSSVideo.CreateMediaPlaybackItem();
                _playerVideo.Source = mediaSource;

                _playerVideo.MediaOpened += new TypedEventHandler<MediaPlayer, object>(async (e, arg) =>
                {
                    Opening = false;
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        PlayMediaOpened?.Invoke(this, new EventArgs());
                    });
                });
                //播放完成
                _playerVideo.MediaEnded += new TypedEventHandler<MediaPlayer, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        //加个判断，是否真的播放完成了
                        if (Position.ToInt32() >= Duration.ToInt32())
                        {
                            PlayState = PlayState.End;
                            Position = 0;
                            PlayStateChanged?.Invoke(this, PlayState);
                            PlayMediaEnded?.Invoke(this, new EventArgs());
                        }
                    });
                });
                //播放错误
                _playerVideo.MediaFailed += new TypedEventHandler<MediaPlayer, MediaPlayerFailedEventArgs>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        PlayState = PlayState.Error;
                        PlayStateChanged?.Invoke(this, PlayState);
                        ChangeEngine?.Invoke(this, new ChangePlayerEngine()
                        {
                            change_engine = PlayEngine.SYEngine,
                            current_mode = PlayEngine.FFmpegInteropMSS,
                            need_change = true,
                            play_type = PlayMediaType.Single
                        });
                    });
                });
                //缓冲开始
                _playerVideo.PlaybackSession.BufferingStarted += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Buffering = true;
                    });
                });
                //缓冲进行中
                _playerVideo.PlaybackSession.BufferingProgressChanged += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Buffering = true;
                        BufferCache = e.BufferingProgress;
                    });
                });
                //缓冲进行中
                _playerVideo.PlaybackSession.BufferingEnded += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Buffering = false;
                    });
                });
                //进度变更
                _playerVideo.PlaybackSession.PositionChanged += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        try
                        {
                            Position = e.Position.TotalSeconds;
                        }
                        catch (Exception)
                        {
                        }
                    });
                });

                PlayState = PlayState.Pause;
                PlayStateChanged?.Invoke(this, PlayState);
                //设置音量
                _playerVideo.Volume = Volume;
                //设置速率
                _playerVideo.PlaybackSession.PlaybackRate = Rate;
                ////设置进度
                //if (positon != 0)
                //{
                //    _playerVideo.PlaybackSession.Position = TimeSpan.FromSeconds(positon);
                //}
                //绑定MediaPlayer
                mediaPlayerVideo.SetMediaPlayer(_playerVideo);
                return new PlayerOpenResult()
                {
                    result = true
                };
            }
            catch (Exception ex)
            {
                //PlayMediaError?.Invoke(this, "视频加载时出错:" + ex.Message);
                return new PlayerOpenResult()
                {
                    result = false,
                    message = ex.Message,
                    detail_message = ex.StackTrace
                };
            }
        }
        /// <summary>
        /// 使用SYEngine解码播放FLV视频
        /// </summary>
        /// <param name="url"></param>
        /// <param name="positon"></param>
        /// <param name="needConfig"></param>
        /// <param name="epId"></param>
        /// <returns></returns>
        public async Task<PlayerOpenResult> PlaySingleFlvUseSYEngine(string url, string userAgent, string referer, double positon = 0, bool needConfig = true, string epId = "")
        {

            try
            {
                mediaPlayerVideo.Visibility = Visibility.Visible;
                //vlcVideoView.Visibility = Visibility.Collapsed;
                Opening = true;
                current_engine = PlayEngine.SYEngine;
                PlayMediaType = PlayMediaType.Single;
                //加载中
                PlayState = PlayState.Loading;
                PlayStateChanged?.Invoke(this, PlayState);
                //关闭正在播放的视频
                ClosePlay();
                var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp);
                if (needConfig)
                {
                    playList.NetworkConfigs = CreatePlaylistNetworkConfigs(userAgent, referer, epId);
                }
                playList.Append(url, 0, 0);
                //设置播放器
                _playerVideo = new MediaPlayer();
                _playerVideo.Source = null;
                var mediaSource = await playList.SaveAndGetFileUriAsync();
                _playerVideo.Source = MediaSource.CreateFromUri(mediaSource);
                //设置时长
                _playerVideo.MediaOpened += new TypedEventHandler<MediaPlayer, object>(async (e, arg) =>
                {
                    Opening = false;
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Duration = _playerVideo.PlaybackSession.NaturalDuration.TotalSeconds;
                        PlayMediaOpened?.Invoke(this, new EventArgs());

                    });
                });
                //播放完成
                _playerVideo.MediaEnded += new TypedEventHandler<MediaPlayer, object>(async (e, arg) =>
                {
                    var source = (e as MediaPlayer).Source;
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        //加个判断，是否真的播放完成了
                        if (Position.ToInt32() >= Duration.ToInt32())
                        {
                            PlayState = PlayState.End;
                            Position = 0;
                            PlayStateChanged?.Invoke(this, PlayState);
                            PlayMediaEnded?.Invoke(this, new EventArgs());
                        }
                    });
                });
                //播放错误
                _playerVideo.MediaFailed += new TypedEventHandler<MediaPlayer, MediaPlayerFailedEventArgs>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        PlayState = PlayState.Error;
                        PlayStateChanged?.Invoke(this, PlayState);
                        ChangeEngine?.Invoke(this, new ChangePlayerEngine()
                        {
                            need_change = false,
                            play_type = PlayMediaType.Single,
                            message = arg.ErrorMessage
                        });

                    });
                });
                //缓冲开始
                _playerVideo.PlaybackSession.BufferingStarted += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Buffering = true;
                    });

                });
                //缓冲进行中
                _playerVideo.PlaybackSession.BufferingProgressChanged += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Buffering = true;
                        BufferCache = e.BufferingProgress;
                    });
                });
                //缓冲进行中
                _playerVideo.PlaybackSession.BufferingEnded += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Buffering = false;
                    });
                });
                //进度变更
                _playerVideo.PlaybackSession.PositionChanged += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        try
                        {
                            Position = e.Position.TotalSeconds;
                        }
                        catch (Exception)
                        {
                        }
                    });
                });

                PlayState = PlayState.Pause;
                PlayStateChanged?.Invoke(this, PlayState);
                //设置音量
                _playerVideo.Volume = Volume;
                //设置速率
                _playerVideo.PlaybackSession.PlaybackRate = Rate;
                //设置进度
                //if (positon != 0)
                //{
                //    _playerVideo.PlaybackSession.Position = TimeSpan.FromSeconds(positon);
                //}
                //绑定MediaPlayer
                mediaPlayerVideo.SetMediaPlayer(_playerVideo);
                return new PlayerOpenResult()
                {
                    result = true
                };
            }
            catch (Exception ex)
            {
                //PlayMediaError?.Invoke(this, "视频加载时出错:" + ex.Message);
                return new PlayerOpenResult()
                {
                    result = false,
                    message = ex.Message,
                    detail_message = ex.StackTrace
                };
            }
        }
        /// <summary>
        /// 使用SYEngine解码播放多段FLV视频
        /// </summary>
        /// <param name="url"></param>
        /// <param name="positon"></param>
        /// <param name="needConfig"></param>
        /// <param name="epId"></param>
        /// <returns></returns>
        public async Task<PlayerOpenResult> PlayVideoUseSYEngine(List<BiliFlvPlayUrlInfo> urls, string userAgent, string referer, double positon = 0, bool needConfig = true, string epId = "", bool isLocal = false)
        {
            current_engine = PlayEngine.SYEngine;
            PlayMediaType = PlayMediaType.MultiFlv;
            try
            {
                mediaPlayerVideo.Visibility = Visibility.Visible;
                //vlcVideoView.Visibility = Visibility.Collapsed;
                Opening = false;
                //加载中
                PlayState = PlayState.Loading;
                PlayStateChanged?.Invoke(this, PlayState);
                ClosePlay();
                var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp);
                if (needConfig)
                {
                    playList.NetworkConfigs = CreatePlaylistNetworkConfigs(userAgent, referer, epId);
                }
                foreach (var item in urls)
                {
                    playList.Append(item.Url, 0, item.Length / 1000);
                }
                //设置时长
                Duration = urls.Sum(x => x.Length / 1000);
                //设置播放器
                _playerVideo = new MediaPlayer();
                _playerVideo.Source = null;

                if (isLocal)
                {
                    MediaComposition composition = new MediaComposition();
                    foreach (var item in urls)
                    {
                        var file = await StorageFile.GetFileFromPathAsync(item.Url);
                        var clip = await MediaClip.CreateFromFileAsync(file);
                        composition.Clips.Add(clip);
                    }
                    _playerVideo.Source = MediaSource.CreateFromMediaStreamSource(composition.GenerateMediaStreamSource());
                }
                else
                {
                    var mediaSource = await playList.SaveAndGetFileUriAsync();

                    _playerVideo.Source = MediaSource.CreateFromUri(mediaSource);

                }
                _playerVideo.MediaOpened += new TypedEventHandler<MediaPlayer, object>(async (e, arg) =>
                {
                    Opening = false;
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        PlayMediaOpened?.Invoke(this, new EventArgs());
                    });
                });
                //播放完成
                _playerVideo.MediaEnded += new TypedEventHandler<MediaPlayer, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        //加个判断，是否真的播放完成了
                        if (Position.ToInt32() >= Duration.ToInt32())
                        {
                            PlayState = PlayState.End;
                            Position = 0;
                            PlayStateChanged?.Invoke(this, PlayState);
                            PlayMediaEnded?.Invoke(this, new EventArgs());
                        }
                    });
                });
                //播放错误
                _playerVideo.MediaFailed += new TypedEventHandler<MediaPlayer, MediaPlayerFailedEventArgs>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        PlayState = PlayState.Error;
                        PlayStateChanged?.Invoke(this, PlayState);
                        ChangeEngine?.Invoke(this, new ChangePlayerEngine()
                        {
                            need_change = false,
                            play_type = PlayMediaType.MultiFlv,
                            message = arg.ErrorMessage
                        });
                        //PlayMediaError?.Invoke(this, arg.ErrorMessage);
                    });
                });
                //缓冲开始
                _playerVideo.PlaybackSession.BufferingStarted += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Buffering = true;
                    });
                });
                //缓冲进行中
                _playerVideo.PlaybackSession.BufferingProgressChanged += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Buffering = true;
                        BufferCache = e.BufferingProgress;
                    });
                });
                //缓冲进行中
                _playerVideo.PlaybackSession.BufferingEnded += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Buffering = false;
                    });
                });
                //进度变更
                _playerVideo.PlaybackSession.PositionChanged += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        try
                        {
                            Position = e.Position.TotalSeconds;
                        }
                        catch (Exception)
                        {
                        }
                    });
                });

                PlayState = PlayState.Pause;
                PlayStateChanged?.Invoke(this, PlayState);
                //设置音量
                _playerVideo.Volume = Volume;
                //设置速率
                _playerVideo.PlaybackSession.PlaybackRate = Rate;
                //设置进度
                //if (positon != 0)
                //{
                //    _playerVideo.PlaybackSession.Position = TimeSpan.FromSeconds(positon);
                //}
                //绑定MediaPlayer
                mediaPlayerVideo.SetMediaPlayer(_playerVideo);

                return new PlayerOpenResult()
                {
                    result = true
                };
            }
            catch (Exception ex)
            {
                return new PlayerOpenResult()
                {
                    result = false,
                    message = ex.Message,
                    detail_message = ex.StackTrace
                };
            }
        }

        public void SetRatioMode(int mode)
        {

            switch (mode)
            {
                case 0:
                    mediaPlayerVideo.Width = double.NaN;
                    mediaPlayerVideo.Height = double.NaN;
                    mediaPlayerVideo.Stretch = Stretch.Uniform;
                    break;
                case 1:
                    mediaPlayerVideo.Width = double.NaN;
                    mediaPlayerVideo.Height = double.NaN;
                    mediaPlayerVideo.Stretch = Stretch.UniformToFill;
                    break;
                case 2:
                    mediaPlayerVideo.Stretch = Stretch.Fill;
                    mediaPlayerVideo.Height = this.ActualHeight;
                    mediaPlayerVideo.Width = this.ActualHeight * 16 / 9;
                    break;
                case 3:
                    mediaPlayerVideo.Stretch = Stretch.Fill;
                    mediaPlayerVideo.Height = this.ActualHeight;
                    mediaPlayerVideo.Width = this.ActualHeight * 4 / 3;
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 设置进度
        /// </summary>
        public void SetPosition(double position)
        {
            if (_mediaTimelineController != null)
            {
                _mediaTimelineController.Position = TimeSpan.FromSeconds(position);
            }
            else
            {
                _playerVideo.PlaybackSession.Position = TimeSpan.FromSeconds(position);
            }

        }
        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            try
            {
                if (_mediaTimelineController != null)
                {
                    if (_mediaTimelineController.State == MediaTimelineControllerState.Running)
                    {
                        _mediaTimelineController.Pause();
                        PlayState = PlayState.Pause;
                    }
                }
                else
                {
                    if (_playerVideo.PlaybackSession.CanPause)
                    {
                        _playerVideo.Pause();
                        PlayState = PlayState.Pause;
                    }
                }
                PlayStateChanged?.Invoke(this, PlayState);
            }
            catch (Exception ex)
            {
                LogHelper.Log("暂停出现错误", LogType.ERROR, ex);
            }

        }

        /// <summary>
        /// 播放
        /// </summary>
        public void Play()
        {
            if (Position == 0 && Duration == 0) return;
            if (_mediaTimelineController != null)
            {
                if (_mediaTimelineController.State == MediaTimelineControllerState.Paused)
                {
                    _mediaTimelineController.Resume();
                    PlayState = PlayState.Playing;
                }
                else
                {
                    _mediaTimelineController.Start();
                    PlayState = PlayState.Playing;
                }
            }

            else
            {
                _playerVideo.Play();
                PlayState = PlayState.Playing;
            }

            PlayStateChanged?.Invoke(this, PlayState);
        }
        /// <summary>
        /// 设置播放速度
        /// </summary>
        /// <param name="value"></param>
        public void SetRate(double value)
        {
            Rate = value;
            if (_mediaTimelineController != null)
            {
                _mediaTimelineController.ClockRate = value;
            }
            else
            {
                if (_playerVideo != null)
                {
                    _playerVideo.PlaybackSession.PlaybackRate = value;
                }
            }

        }
        /// <summary>
        /// 停止播放
        /// </summary>
        public void ClosePlay()
        {
            //全部设置为NULL
            if (mediaPlayerVideo.MediaPlayer != null)
            {
                mediaPlayerVideo.SetMediaPlayer(null);

            }
            if (mediaPlayerAudio.MediaPlayer != null)
            {
                mediaPlayerVideo.SetMediaPlayer(null);
            }
            if (_ffmpegMSSVideo != null)
            {
                _ffmpegMSSVideo.Dispose();
                _ffmpegMSSVideo = null;
            }
            if (_ffmpegMSSAudio != null)
            {
                _ffmpegMSSAudio.Dispose();
                _ffmpegMSSAudio = null;
            }
            if (_playerVideo != null)
            {
                _playerVideo.Source = null;
                _playerVideo.Dispose();
                _playerVideo = null;
            }
            if (_playerAudio != null)
            {
                _playerAudio.Source = null;
                _playerAudio.Dispose();
                _playerAudio = null;
            }
            if (_mediaPlaybackList != null)
            {
                _mediaPlaybackList.Items.Clear();
                _mediaPlaybackList = null;
            }
            if (_mediaTimelineController != null)
            {
                _mediaTimelineController = null;
            }
            if (_ffmpegMSSItems != null)
            {
                _ffmpegMSSItems.Clear();
                _ffmpegMSSItems = null;
            }


            PlayState = PlayState.End;
            //进度设置为0
            Position = 0;
            Duration = 0;
        }
        /// <summary>
        /// 设置音量
        /// </summary>
        /// <param name="volume"></param>
        public void SetVolume(double volume)
        {

            if (mediaPlayerAudio.MediaPlayer != null)
            {
                mediaPlayerAudio.MediaPlayer.Volume = volume;
            }
            if (mediaPlayerVideo.MediaPlayer != null)
            {
                mediaPlayerVideo.MediaPlayer.Volume = volume;
            }
        }

        public string GetMediaInfo()
        {
            try
            {
                var info = "";
                switch (PlayMediaType)
                {
                    case PlayMediaType.Single:
                        info += $"Type: single_video\r\n";
                        break;
                    case PlayMediaType.MultiFlv:
                        info += $"Type: multi_video\r\n";
                        break;
                    case PlayMediaType.Dash:
                        info += $"Type: dash\r\n";
                        break;
                    default:
                        break;
                }
                info += $"Engine: {current_engine.ToString()}\r\n";
                if (_ffmpegMSSVideo != null)
                {
                    info += $"Resolution: {_ffmpegMSSVideo.CurrentVideoStream.PixelHeight} x {_ffmpegMSSVideo.CurrentVideoStream.PixelWidth}\r\n";
                    info += $"Video Codec: {_ffmpegMSSVideo.CurrentVideoStream.CodecName}\r\n";
                    info += $"Video Bitrate: {_ffmpegMSSVideo.CurrentVideoStream.Bitrate}\r\n";
                    info += $"Average Frame: {((double)_ffmpegMSSVideo.CurrentVideoStream.FramesPerSecond).ToString("0.0")}\r\n";
                    if (PlayMediaType == PlayMediaType.Dash)
                    {
                        info += $"Audio Codec: {_ffmpegMSSAudio.AudioStreams[0].CodecName}\r\n";
                        info += $"Audio Bitrate: {_ffmpegMSSAudio.AudioStreams[0].Bitrate}";
                    }
                    else
                    {
                        info += $"Audio Codec: {_ffmpegMSSVideo.AudioStreams[0].CodecName}\r\n";
                        info += $"Audio Bitrate: {_ffmpegMSSVideo.AudioStreams[0].Bitrate}";
                    }
                }
                else
                {
                    //info += $"Resolution: {_playerVideo.PlaybackSession.NaturalVideoHeight} x {_playerVideo.PlaybackSession.NaturalVideoWidth}\r\n";
                    if (_dashInfo != null && _dashInfo.Audio != null)
                    {
                        info += $"Resolution: {_dashInfo.Video.Width} x {_dashInfo.Video.Height}\r\n";
                        info += $"Video Codec: {_dashInfo.Video.Codecs}\r\n";
                        info += $"Video DataRate: {(_dashInfo.Video.BandWidth / 1024).ToString("0.0")}Kbps\r\n";
                        info += $"Average Frame: {_dashInfo.Video.FrameRate}\r\n";
                        info += $"Audio Codec: {_dashInfo.Audio.Codecs}\r\n";
                        info += $"Audio DataRate: {(_dashInfo.Audio.BandWidth / 1024).ToString("0.0")}Kbps\r\n";
                        info += $"Video Host: { _dashInfo.Video.Host}\r\n";
                        info += $"Audio Host: {_dashInfo.Audio.Host}\r\n";
                    }
                    else
                    {
                        info += $"Resolution: {_playerVideo.PlaybackSession.NaturalVideoWidth} x {_playerVideo.PlaybackSession.NaturalVideoHeight}\r\n";
                    }
                }
                //MediaInfo = info;
                return info;
            }
            catch (Exception)
            {
                //MediaInfo = "读取失败";
                return "读取视频信息失败";
            }

        }

        public void Dispose()
        {
            this.ClosePlay();
            //try
            //{
            //    _vlcMediaPlayer?.Media?.Dispose();

            //    _vlcMediaPlayer?.Dispose();

            //    _vlcMediaPlayer=null;
            //    _libVLC?.Dispose();
            //    _libVLC = null;
            //}
            //catch (Exception)
            //{
            //}

        }
        private async Task<AdaptiveMediaSource> CreateAdaptiveMediaSource(BiliDashPlayUrlInfo dashInfo, string userAgent, string referer)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                if (userAgent != null && userAgent.Length > 0)
                {
                    httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
                }
                if (referer != null && referer.Length > 0)
                {
                    httpClient.DefaultRequestHeaders.Add("Referer", referer);
                }
                var mpdStr = "";
                if (dashInfo.Audio != null)
                {
                    mpdStr = $@"<MPD xmlns=""urn:mpeg:DASH:schema:MPD:2011""  profiles=""urn:mpeg:dash:profile:isoff-on-demand:2011"" type=""static"">
                  <Period  start=""PT0S"">
                    <AdaptationSet>
                      <ContentComponent contentType=""video"" id=""1"" />
                      <Representation bandwidth=""{dashInfo.Video.BandWidth}"" codecs=""{dashInfo.Video.Codecs}"" height=""{dashInfo.Video.Height}"" id=""{dashInfo.Video.ID}"" mimeType=""{dashInfo.Video.MimeType}"" width=""{dashInfo.Video.Width}"" startWithSap=""{dashInfo.Video.StartWithSap}"">
                        <BaseURL></BaseURL>
                        <SegmentBase indexRange=""{dashInfo.Video.SegmentBaseIndexRange}"">
                          <Initialization range=""{dashInfo.Video.SegmentBaseInitialization}"" />
                        </SegmentBase>
                      </Representation>
                    </AdaptationSet>
                    <AdaptationSet>
                      <ContentComponent contentType=""audio"" id=""2"" />
                      <Representation bandwidth=""{dashInfo.Audio.BandWidth}"" codecs=""{dashInfo.Audio.Codecs}"" id=""{dashInfo.Audio.ID}"" mimeType=""{dashInfo.Audio.MimeType}"" >
                        <BaseURL></BaseURL>
                        <SegmentBase indexRange=""{dashInfo.Audio.SegmentBaseIndexRange}"">
                          <Initialization range=""{dashInfo.Audio.SegmentBaseInitialization}"" />
                        </SegmentBase>
                      </Representation>
                    </AdaptationSet>
                  </Period>
                </MPD>";
                }
                else
                {
                    mpdStr = $@"<MPD xmlns=""urn:mpeg:DASH:schema:MPD:2011""  profiles=""urn:mpeg:dash:profile:isoff-on-demand:2011"" type=""static"">
                  <Period  start=""PT0S"">
                    <AdaptationSet>
                      <ContentComponent contentType=""video"" id=""1"" />
                      <Representation bandwidth=""{dashInfo.Video.BandWidth}"" codecs=""{dashInfo.Video.Codecs}"" height=""{dashInfo.Video.Height}"" id=""{dashInfo.Video.ID}"" mimeType=""{dashInfo.Video.MimeType}"" width=""{dashInfo.Video.Width}"" startWithSap=""{dashInfo.Video.StartWithSap}"">
                        <BaseURL></BaseURL>
                        <SegmentBase indexRange=""{dashInfo.Video.SegmentBaseIndexRange}"">
                          <Initialization range=""{dashInfo.Video.SegmentBaseInitialization}"" />
                        </SegmentBase>
                      </Representation>
                    </AdaptationSet>
                  </Period>
                </MPD>";
                }



                var stream = new MemoryStream(Encoding.UTF8.GetBytes(mpdStr)).AsInputStream();
                var soure = await AdaptiveMediaSource.CreateFromStreamAsync(stream, new Uri(dashInfo.Video.Url), "application/dash+xml", httpClient);
                soure.MediaSource.AdvancedSettings.AllSegmentsIndependent = true;

                var s = soure.Status;
                soure.MediaSource.DownloadRequested += (sender, args) =>
                {
                    if (args.ResourceContentType == "audio/mp4")
                    {
                        args.Result.ResourceUri = new Uri(dashInfo.Audio.Url);
                    }
                };
                return soure.MediaSource;
            }
            catch (Exception)
            {
                return null;
            }

        }
        private MediaSourceConfig CreateFFmpegInteropConfig(string userAgent, string referer)
        {

            var passthrough = SettingHelper.GetValue<bool>(SettingHelper.Player.HARDWARE_DECODING, true);
            var _ffmpegConfig = new MediaSourceConfig();
            if (userAgent != null && userAgent.Length > 0)
            {
                _ffmpegConfig.FFmpegOptions.Add("user_agent", userAgent);
            }
            if (referer != null && referer.Length > 0)
            {
                _ffmpegConfig.FFmpegOptions.Add("referer", referer);
            }

            _ffmpegConfig.VideoDecoderMode = passthrough ? VideoDecoderMode.ForceSystemDecoder : VideoDecoderMode.ForceFFmpegSoftwareDecoder;
            return _ffmpegConfig;
        }

        private SYEngine.PlaylistNetworkConfigs CreatePlaylistNetworkConfigs(string userAgent, string referer, string epId = "")
        {

            SYEngine.PlaylistNetworkConfigs config = new SYEngine.PlaylistNetworkConfigs();
            config.DownloadRetryOnFail = true;
            config.HttpCookie = string.Empty;
            config.UniqueId = string.Empty;
            config.HttpReferer = string.Empty;
            config.HttpUserAgent = string.Empty;
            if (userAgent != null && userAgent.Length > 0)
            {
                config.HttpUserAgent = userAgent;
            }
            if (referer != null && referer.Length > 0)
            {
                config.HttpReferer = referer;
            }
            return config;
        }

        //private void vlcVideoView_Initialized(object sender, LibVLCSharp.Platforms.UWP.InitializedEventArgs e)
        //{
        //    //_libVLC = new LibVLCSharp.Shared.LibVLC(enableDebugLogs: true, e.SwapChainOptions);
        //    ////LibVLC.SetUserAgent("Mozilla/5.0", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.198 Safari/537.36");

        //    //_libVLC.Log += LibVLC_Log;
        //    //_vlcMediaPlayer = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
        //    //vlcVideoView.MediaPlayer = _vlcMediaPlayer;
        //}

        //private void LibVLC_Log(object sender, LibVLCSharp.Shared.LogEventArgs e)
        //{
        //    Debug.WriteLine(e.FormattedLog);
        //}
    }

    public class PlayerOpenResult
    {
        public bool result { get; set; }
        public string message { get; set; }
        public string detail_message { get; set; }
    }
    public class ChangePlayerEngine
    {
        public bool need_change { get; set; }
        /// <summary>
        /// 当前引擎
        /// </summary>
        public PlayEngine current_mode { get; set; }
        /// <summary>
        /// 更换引擎
        /// </summary>
        public PlayEngine change_engine { get; set; }

        public PlayMediaType play_type { get; set; }
        public string message { get; set; }
    }

}
