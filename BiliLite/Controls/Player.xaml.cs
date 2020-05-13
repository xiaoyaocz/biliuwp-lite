using BiliLite.Helpers;
using BiliLite.Modules;
using FFmpegInterop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.Streaming.Adaptive;
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
    }
    public enum PlayMediaType
    {
        Single,
        MultiFlv,
        Dash
    }
    public sealed partial class Player : UserControl, IDisposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void DoPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public PlayState PlayState { get; set; }
        public PlayMediaType PlayMediaType { get; set; }
        private DashItemModel _dash_video;
        private DashItemModel _dash_audio;
        private PlayEngine current_engine;
        public FFmpegInteropConfig _ffmpegConfig;
        private FFmpegInteropMSS _ffmpegMSSVideo;
        private MediaPlayer _playerVideo;
        //音视频分离
        private FFmpegInteropMSS _ffmpegMSSAudio;
        private MediaPlayer _playerAudio;
        private MediaTimelineController _mediaTimelineController;

        //多段FLV
        private List<FFmpegInteropMSS> _ffmpegMSSItems;
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
        /// 缓冲开始
        /// </summary>
        public event EventHandler PlayBufferStart;
        /// <summary>
        /// 缓冲中
        /// </summary>
        public event EventHandler<double> PlayBuffering;
        /// <summary>
        /// 缓冲完成
        /// </summary>
        public event EventHandler PlayBufferEnd;
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
            _ffmpegConfig = new FFmpegInteropConfig();
            _ffmpegConfig.FFmpegOptions.Add("user_agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.80 Safari/537.36");
            _ffmpegConfig.FFmpegOptions.Add("referer", "https://www.bilibili.com");
            var passthrough = SettingHelper.GetValue<bool>(SettingHelper.Player.HARDWARE_DECODING, false);
            _ffmpegConfig.PassthroughVideoHEVC = passthrough;
            _ffmpegConfig.PassthroughVideoH264 = passthrough;
            SYEngine.Core.ForceNetworkMode = true;
            SYEngine.Core.ForceSoftwareDecode = !SettingHelper.GetValue<bool>(SettingHelper.Player.HARDWARE_DECODING, false);
            //_ffmpegConfig.StreamBufferSize = 655360;//1024 * 30;

        }
        public async Task<PlayerOpenResult> PlayerDashUseNative(DashItemModel videoUrl, DashItemModel audioUrl, double positon = 0, bool needConfig = true)
        {
            try
            {
                _dash_video = videoUrl;
                _dash_audio = audioUrl;
                Opening = true;
                current_engine = PlayEngine.Native;
                PlayMediaType = PlayMediaType.Dash;
                //加载中
                PlayState = PlayState.Loading;
                PlayStateChanged?.Invoke(this, PlayState);
                ClosePlay();
                var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp);

                //设置播放器
                _playerVideo = new MediaPlayer();
                _playerVideo.Source = MediaSource.CreateFromAdaptiveMediaSource(await CreateAdaptiveMediaSource(videoUrl, audioUrl));
                //设置时长
                _playerVideo.MediaOpened += new TypedEventHandler<MediaPlayer, object>(async (e, arg) =>
                {
                    Opening = false;
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        PlayMediaOpened?.Invoke(this, new EventArgs());
                        Duration = _playerVideo.PlaybackSession.NaturalDuration.TotalSeconds;
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
                        PlayBufferStart?.Invoke(this, new EventArgs());
                    });
                });
                //缓冲进行中
                _playerVideo.PlaybackSession.BufferingProgressChanged += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        PlayBuffering?.Invoke(this, e.BufferingProgress);
                    });

                });
                //缓冲进行中
                _playerVideo.PlaybackSession.BufferingEnded += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        PlayBufferEnd?.Invoke(this, new EventArgs());
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
                if (positon != 0)
                {
                    _playerVideo.PlaybackSession.Position = TimeSpan.FromSeconds(positon);
                }
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
        public async Task<PlayerOpenResult> PlayDashUseFFmpegInterop(DashItemModel videoUrl, DashItemModel audioUrl, double positon = 0, bool needConfig = true)
        {
            try
            {
                Opening = true;
                _dash_video = videoUrl;
                _dash_audio = audioUrl;
                current_engine = PlayEngine.FFmpegInteropMSS;

                PlayMediaType = PlayMediaType.Dash;
                //加载中
                PlayState = PlayState.Loading;
                PlayStateChanged?.Invoke(this, PlayState);
                //关闭正在播放的视频
                ClosePlay();
                if (needConfig)
                {
                    _ffmpegMSSVideo = await FFmpegInteropMSS.CreateFromUriAsync(videoUrl.base_url, _ffmpegConfig);
                    _ffmpegMSSAudio = await FFmpegInteropMSS.CreateFromUriAsync(audioUrl.base_url, _ffmpegConfig);
                }
                else
                {
                    _ffmpegMSSVideo = await FFmpegInteropMSS.CreateFromUriAsync(videoUrl.base_url);
                    _ffmpegMSSAudio = await FFmpegInteropMSS.CreateFromUriAsync(audioUrl.base_url);
                }

                //设置时长
                Duration = _ffmpegMSSVideo.Duration.TotalSeconds;
                //设置视频
                _playerVideo = new MediaPlayer();
                _playerVideo.Source = _ffmpegMSSVideo.CreateMediaPlaybackItem();
                //设置音频
                _playerAudio = new MediaPlayer();
                _playerAudio.Source = _ffmpegMSSAudio.CreateMediaPlaybackItem();
                //设置时间线控制器
                _mediaTimelineController = new MediaTimelineController();
                _playerVideo.CommandManager.IsEnabled = false;
                _playerVideo.TimelineController = _mediaTimelineController;
                _playerAudio.CommandManager.IsEnabled = true;
                _playerAudio.TimelineController = _mediaTimelineController;
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
                            message = arg.ErrorMessage
                        });
                    });

                });
                //缓冲开始
                _playerVideo.PlaybackSession.BufferingStarted += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        PlayBufferStart?.Invoke(this, new EventArgs());
                    });
                });
                _playerAudio.PlaybackSession.BufferingStarted += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        PlayBufferStart?.Invoke(this, new EventArgs());
                    });
                });

                //缓冲进行中
                _playerVideo.PlaybackSession.BufferingProgressChanged += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        PlayBuffering?.Invoke(this, e.BufferingProgress);
                    });

                });
                _playerAudio.PlaybackSession.BufferingProgressChanged += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        PlayBuffering?.Invoke(this, e.BufferingProgress);
                    });

                });
                //缓冲进行中
                _playerVideo.PlaybackSession.BufferingEnded += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        PlayBufferEnd?.Invoke(this, new EventArgs());
                    });
                });
                _playerAudio.PlaybackSession.BufferingEnded += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        PlayBufferEnd?.Invoke(this, new EventArgs());
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
                _playerAudio.Volume = Volume;
                //绑定MediaPlayer
                mediaPlayerVideo.SetMediaPlayer(_playerVideo);
                mediaPlayerAudio.SetMediaPlayer(_playerAudio);
                //设置速率
                _mediaTimelineController.ClockRate = Rate;
                //设置进度
                if (positon != 0)
                {
                    _mediaTimelineController.Position = TimeSpan.FromSeconds(positon);
                }
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
        
        public async Task<PlayerOpenResult> PlaySingleFlvUseFFmpegInterop(string url, double positon = 0, bool needConfig = true)
        {

            try
            {
                Opening = true;
                current_engine = PlayEngine.FFmpegInteropMSS;

                PlayMediaType = PlayMediaType.Single;
                //加载中
                PlayState = PlayState.Loading;
                PlayStateChanged?.Invoke(this, PlayState);
                //关闭正在播放的视频
                ClosePlay();
                if (needConfig)
                {
                    _ffmpegMSSVideo = await FFmpegInteropMSS.CreateFromUriAsync(url, _ffmpegConfig);
                }
                else
                {
                    _ffmpegMSSVideo = await FFmpegInteropMSS.CreateFromUriAsync(url);
                }
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
                        PlayBufferStart?.Invoke(this, new EventArgs());
                    });
                });
                //缓冲进行中
                _playerVideo.PlaybackSession.BufferingProgressChanged += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        PlayBuffering?.Invoke(this, e.BufferingProgress);
                    });

                });
                //缓冲进行中
                _playerVideo.PlaybackSession.BufferingEnded += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        PlayBufferEnd?.Invoke(this, new EventArgs());
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
                if (positon != 0)
                {
                    _playerVideo.PlaybackSession.Position = TimeSpan.FromSeconds(positon);
                }
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
        public async Task<PlayerOpenResult> PlaySingleFlvUseSYEngine(string url, double positon = 0, bool needConfig = true, string epId = "")
        {

            try
            {
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
                    playList.NetworkConfigs = CreatePlaylistNetworkConfigs(epId);
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
                        PlayMediaOpened?.Invoke(this, new EventArgs());
                        Duration = _playerVideo.PlaybackSession.NaturalDuration.TotalSeconds;
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
                        PlayBufferStart?.Invoke(this, new EventArgs());
                    });
                });
                //缓冲进行中
                _playerVideo.PlaybackSession.BufferingProgressChanged += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        PlayBuffering?.Invoke(this, e.BufferingProgress);
                    });

                });
                //缓冲进行中
                _playerVideo.PlaybackSession.BufferingEnded += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        PlayBufferEnd?.Invoke(this, new EventArgs());
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
                if (positon != 0)
                {
                    _playerVideo.PlaybackSession.Position = TimeSpan.FromSeconds(positon);
                }
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

        public async Task<PlayerOpenResult> PlayVideoUseSYEngine(List<FlvDurlModel> url, double positon = 0, bool needConfig = true,string epId="")
        {
            current_engine = PlayEngine.SYEngine;
            PlayMediaType = PlayMediaType.MultiFlv;
            try
            {
                Opening = false;
                //加载中
                PlayState = PlayState.Loading;
                PlayStateChanged?.Invoke(this, PlayState);
                ClosePlay();
                var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp);
                if (needConfig)
                {
                    playList.NetworkConfigs = CreatePlaylistNetworkConfigs(epId);
                }
                foreach (var item in url)
                {
                    playList.Append(item.url, 0, item.length / 1000);
                }
                //设置时长
                Duration = url.Sum(x => x.length / 1000);
                //设置播放器
                _playerVideo = new MediaPlayer();
                var mediaSource = await playList.SaveAndGetFileUriAsync();
                _playerVideo.Source = MediaSource.CreateFromUri(mediaSource);
                _playerVideo.MediaOpened += new TypedEventHandler<MediaPlayer, object>(async (e, arg) =>
                {
                    Opening = true;
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
                        PlayBufferStart?.Invoke(this, new EventArgs());
                    });
                });
                //缓冲进行中
                _playerVideo.PlaybackSession.BufferingProgressChanged += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        PlayBuffering?.Invoke(this, e.BufferingProgress);
                    });

                });
                //缓冲进行中
                _playerVideo.PlaybackSession.BufferingEnded += new TypedEventHandler<MediaPlaybackSession, object>(async (e, arg) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        PlayBufferEnd?.Invoke(this, new EventArgs());
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
                if (positon != 0)
                {
                    _playerVideo.PlaybackSession.Position = TimeSpan.FromSeconds(positon);
                }
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

        /// <summary>
        /// 播放
        /// </summary>
        public void Play()
        {
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
                mediaPlayerAudio.SetMediaPlayer(null);
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
                    info += $"Resolution: {_ffmpegMSSVideo.VideoStream.PixelHeight} x {_ffmpegMSSVideo.VideoStream.PixelWidth}\r\n";
                    info += $"Video Codec: {_ffmpegMSSVideo.VideoStream.CodecName}\r\n";
                    info += $"Video Bitrate: {_ffmpegMSSVideo.VideoStream.Bitrate}\r\n";
                    info += $"Average Frame: {((double)_ffmpegMSSVideo.VideoDescriptor.EncodingProperties.FrameRate.Numerator / _ffmpegMSSVideo.VideoDescriptor.EncodingProperties.FrameRate.Denominator).ToString("0.0")}\r\n";
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
                    if (_dash_video != null && _dash_audio != null)
                    {
                        info += $"Resolution: {_dash_video.width} x {_dash_video.height}\r\n";
                        info += $"Video Codec: {_dash_video.codecs}\r\n";
                        info += $"Video DataRate: {(_dash_video.bandwidth/1024).ToString("0.0")}Kbps\r\n";
                        info += $"Average Frame: {_dash_video.fps}\r\n";
                        info += $"Audio Codec: {_dash_audio.codecs}\r\n";
                        info += $"Audio DataRate: {(_dash_audio.bandwidth / 1024).ToString("0.0")}Kbps\r\n";
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
        }
        private async Task<AdaptiveMediaSource> CreateAdaptiveMediaSource(DashItemModel video, DashItemModel audio)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Referer = new Uri("https://www.bilibili.com");
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36");
                var mpdStr = $@"<MPD xmlns=""urn:mpeg:DASH:schema:MPD:2011""  profiles=""urn:mpeg:dash:profile:isoff-on-demand:2011"" type=""static"">
                  <Period  start=""PT0S"">
                    <AdaptationSet>
                      <ContentComponent contentType=""video"" id=""1"" />
                      <Representation bandwidth=""{video.bandwidth}"" codecs=""{video.codecs}"" height=""{video.height}"" id=""{video.id}"" mimeType=""{video.mimeType}"" width=""{video.width}"">
                        <BaseURL></BaseURL>
                        <SegmentBase indexRange=""{video.SegmentBase.indexRange}"">
                          <Initialization range=""{video.SegmentBase.Initialization}"" />
                        </SegmentBase>
                      </Representation>
                    </AdaptationSet>
                    <AdaptationSet>
                      <ContentComponent contentType=""audio"" id=""2"" />
                      <Representation bandwidth=""{audio.bandwidth}"" codecs=""{audio.codecs}"" id=""{audio.id}"" mimeType=""{audio.mimeType}"" >
                        <BaseURL></BaseURL>
                        <SegmentBase indexRange=""{audio.SegmentBase.indexRange}"">
                          <Initialization range=""{audio.SegmentBase.Initialization}"" />
                        </SegmentBase>
                      </Representation>
                    </AdaptationSet>
                  </Period>
                </MPD>";
                var stream = new MemoryStream(Encoding.UTF8.GetBytes(mpdStr)).AsInputStream();
                var soure = await AdaptiveMediaSource.CreateFromStreamAsync(stream, new Uri(video.baseUrl), "application/dash+xml", httpClient);
                var s = soure.Status;
                soure.MediaSource.DownloadRequested += (sender, args) =>
                {
                    if (args.ResourceContentType == "audio/mp4")
                    {
                        args.Result.ResourceUri = new Uri(audio.baseUrl);
                    }
                };
                return soure.MediaSource;
            }
            catch (Exception)
            {
                return null;
            }

        }
        private SYEngine.PlaylistNetworkConfigs CreatePlaylistNetworkConfigs(string epId="")
        {

            SYEngine.PlaylistNetworkConfigs config = new SYEngine.PlaylistNetworkConfigs();
            config.DownloadRetryOnFail = true;
            config.HttpCookie = string.Empty;
            config.UniqueId = string.Empty;
            config.HttpUserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36";
            config.HttpReferer =string.IsNullOrEmpty(epId)? "https://www.bilibili.com": "https://www.bilibili.com/bangumi/play/ep"+ epId;
            return config;
        }
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
