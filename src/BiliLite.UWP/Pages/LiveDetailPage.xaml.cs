using BiliLite.Controls;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Modules;
using BiliLite.Modules.LiveRoomDetailModels;
using BiliLite.Services;
using FFmpegInteropX;
using Microsoft.UI.Xaml.Controls;
using NSDanmaku.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.System.Display;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LiveDetailPage : BasePage
    {
        private static readonly ILogger logger = GlobalLogger.FromCurrentType();

        DisplayRequest dispRequest;
        readonly MediaSourceConfig _config;
        FFmpegInteropX.FFmpegMediaSource interopMSS;
        LiveRoomVM liveRoomVM;
        SettingVM settingVM;
        readonly MediaPlayer mediaPlayer;
        DispatcherTimer timer_focus;
        DispatcherTimer controlTimer;
        public LiveDetailPage()
        {
            this.InitializeComponent();
            Title = "直播间";
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            dispRequest = new DisplayRequest();
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            _config = new MediaSourceConfig();
            _config.FFmpegOptions.Add("rtsp_transport", "tcp");
            _config.FFmpegOptions.Add("user_agent", "Mozilla/5.0 BiliDroid/1.12.0 (bbcallen@gmail.com)");
            _config.FFmpegOptions.Add("referer", "https://live.bilibili.com/");
            //每过2秒就设置焦点
            timer_focus = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(2) };

            timer_focus.Tick += Timer_focus_Tick;
            controlTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
            controlTimer.Tick += ControlTimer_Tick;
            settingVM = new SettingVM();

            liveRoomVM = new LiveRoomVM();
            mediaPlayer = new MediaPlayer();
            mediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
            mediaPlayer.PlaybackSession.BufferingStarted += PlaybackSession_BufferingStarted;
            mediaPlayer.PlaybackSession.BufferingProgressChanged += PlaybackSession_BufferingProgressChanged;
            mediaPlayer.PlaybackSession.BufferingEnded += PlaybackSession_BufferingEnded;
            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded; ;
            mediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
            liveRoomVM.ChangedPlayUrl += LiveRoomVM_ChangedPlayUrl;
            liveRoomVM.AddNewDanmu += LiveRoomVM_AddNewDanmu;
            liveRoomVM.LotteryEnd += LiveRoomVM_LotteryEnd;
            this.Loaded += LiveDetailPage_Loaded;
            this.Unloaded += LiveDetailPage_Unloaded;
        }



        private void ControlTimer_Tick(object sender, object e)
        {
            if (showControlsFlag != -1)
            {
                if (showControlsFlag >= 5)
                {
                    var elent = FocusManager.GetFocusedElement();
                    if (!(elent is TextBox) && !(elent is AutoSuggestBox))
                    {
                        ShowControl(false);
                        showControlsFlag = -1;
                    }
                }
                else
                {
                    showControlsFlag++;
                }
            }
        }

        private void Timer_focus_Tick(object sender, object e)
        {
            var elent = FocusManager.GetFocusedElement();
            if (elent is Button || elent is AppBarButton || elent is HyperlinkButton || elent is MenuFlyoutItem)
            {
                BtnFoucs.Focus(FocusState.Programmatic);
            }

        }
        private void LiveRoomVM_LotteryEnd(object sender, LiveRoomEndAnchorLotteryInfoModel e)
        {
            var str = "";
            foreach (var item in e.award_users)
            {
                str += item.uname + "、";
            }
            str = str.TrimEnd('、');

            Notify.ShowMessageToast($"开奖信息:\r\n奖品:{e.award_name}\r\n中奖用户:{str}", new List<MyUICommand>() { }, 10);

        }

        private void LiveRoomVM_AddNewDanmu(object sender, string e)
        {
            if (DanmuControl.Visibility == Visibility.Visible)
            {
                if (settingVM.LiveWords != null && settingVM.LiveWords.Count > 0)
                {
                    if (settingVM.LiveWords.FirstOrDefault(x => e.Contains(x)) != null) return;
                }
                try
                {
                    DanmuControl.AddLiveDanmu(e, false, Colors.White);
                }
                catch (Exception ex)
                {
                    //记录错误，不弹出通知
                    logger.Log(ex.Message, LogType.ERROR, ex);
                }

            }

        }
        #region 播放器事件
        private async void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                liveRoomVM.Liveing = false;
                url = "";
                player.SetMediaPlayer(null);
            });
        }

        private async void PlaybackSession_BufferingEnded(MediaPlaybackSession sender, object args)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                PlayerLoading.Visibility = Visibility.Collapsed;
            });

        }

        private async void PlaybackSession_BufferingProgressChanged(MediaPlaybackSession sender, object args)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                PlayerLoadText.Text = sender.BufferingProgress.ToString("p");
            });
        }

        private async void PlaybackSession_BufferingStarted(MediaPlaybackSession sender, object args)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                PlayerLoading.Visibility = Visibility.Visible;
                PlayerLoadText.Text = "缓冲中";
            });
        }

        private async void MediaPlayer_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
             {
                 logger.Log("直播加载失败", LogType.ERROR, new Exception(args.ErrorMessage));
                 await new MessageDialog($"啊，直播加载失败了\r\n错误信息:{args.ErrorMessage}\r\n请尝试在直播设置中打开/关闭硬解试试", "播放失败").ShowAsync();
             });

        }

        private async void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                //保持屏幕常亮
                dispRequest.RequestActive();
                PlayerLoading.Visibility = Visibility.Collapsed;
                SetMediaInfo();
            });
        }
        private void SetMediaInfo()
        {
            try
            {
                var str = $"Url: {url}\r\n";
                str += $"Quality: {liveRoomVM.current_qn.desc}({liveRoomVM.current_qn.qn})\r\n";
                str += $"Video Codec: {interopMSS.CurrentVideoStream.CodecName}\r\nAudio Codec:{interopMSS.AudioStreams[0].CodecName}\r\n";
                str += $"Resolution: {interopMSS.CurrentVideoStream.PixelWidth} x {interopMSS.CurrentVideoStream.PixelHeight}\r\n";
                str += $"Video Bitrate: {interopMSS.CurrentVideoStream.Bitrate / 1024} Kbps\r\n";
                str += $"Audio Bitrate: {interopMSS.AudioStreams[0].Bitrate / 1024} Kbps\r\n";
                str += $"Decoder Engine: {interopMSS.CurrentVideoStream.DecoderEngine.ToString()}";
                txtInfo.Text = str;
            }
            catch (Exception)
            {
                txtInfo.Text = "Url";
            }



        }
        private async void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                switch (sender.PlaybackState)
                {
                    case MediaPlaybackState.None:
                        break;
                    case MediaPlaybackState.Opening:
                        PlayerLoading.Visibility = Visibility.Visible;
                        PlayerLoadText.Text = "加载中";
                        break;
                    case MediaPlaybackState.Buffering:
                        PlayerLoading.Visibility = Visibility.Visible;
                        break;
                    case MediaPlaybackState.Playing:
                        BottomBtnPlay.Visibility = Visibility.Collapsed;
                        BottomBtnPause.Visibility = Visibility.Visible;
                        break;
                    case MediaPlaybackState.Paused:
                        BottomBtnPlay.Visibility = Visibility.Visible;
                        BottomBtnPause.Visibility = Visibility.Collapsed;
                        break;
                    default:
                        break;
                }
            });
        }
        #endregion

        string url = "";
        bool flag = false;
        private void LiveRoomVM_ChangedPlayUrl(object sender, LiveRoomPlayUrlModel e)
        {
            flag = true;
            BottomCBLine.ItemsSource = liveRoomVM.urls;
            BottomCBLine.SelectedIndex = 0;
            BottomCBQuality.SelectedItem = liveRoomVM.current_qn;
            flag = false;
        }
        private void LiveDetailPage_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyDown;
            timer_focus.Stop();
            controlTimer.Stop();
        }

        private void LiveDetailPage_Loaded(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            BtnFoucs.Focus(FocusState.Programmatic);
            DanmuControl.ClearAll();
            if (this.Parent is MyFrame)
            {
                (this.Parent as MyFrame).ClosedPage -= LiveDetailPage_ClosedPage;
                (this.Parent as MyFrame).ClosedPage += LiveDetailPage_ClosedPage;
            }
            timer_focus.Start();
            controlTimer.Start();
        }

        private async void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            var elent = FocusManager.GetFocusedElement();
            if (elent is TextBox || elent is AutoSuggestBox)
            {
                args.Handled = false;
                return;
            }
            args.Handled = true;
            switch (args.VirtualKey)
            {
                //case Windows.System.VirtualKey.Space:
                //    if (mediaPlayer.PlaybackSession.CanPause)
                //    {
                //        mediaPlayer.Pause();
                //    }
                //    else
                //    {
                //        mediaPlayer.Play();
                //    }
                //    break;

                case Windows.System.VirtualKey.Up:
                    if (SliderVolume.Value + 0.1 > 1)
                    {
                        SliderVolume.Value = 1;
                    }
                    else
                    {
                        SliderVolume.Value += 0.1;
                    }

                    TxtToolTip.Text = "音量:" + SliderVolume.Value.ToString("P");
                    ToolTip.Visibility = Visibility.Visible;
                    await Task.Delay(2000);
                    ToolTip.Visibility = Visibility.Collapsed;
                    break;

                case Windows.System.VirtualKey.Down:

                    if (SliderVolume.Value - 0.1 < 0)
                    {
                        SliderVolume.Value = 0;
                    }
                    else
                    {
                        SliderVolume.Value -= 0.1;
                    }
                    if (SliderVolume.Value == 0)
                    {
                        TxtToolTip.Text = "静音";
                    }
                    else
                    {
                        TxtToolTip.Text = "音量:" + SliderVolume.Value.ToString("P");
                    }
                    ToolTip.Visibility = Visibility.Visible;
                    await Task.Delay(2000);
                    ToolTip.Visibility = Visibility.Collapsed;
                    break;
                case Windows.System.VirtualKey.Escape:
                    SetFullScreen(false);

                    break;
                case Windows.System.VirtualKey.F8:
                case Windows.System.VirtualKey.T:
                    //小窗播放
                    MiniWidnows(BottomBtnMiniWindows.Visibility == Visibility.Visible);

                    break;
                case Windows.System.VirtualKey.F12:
                case Windows.System.VirtualKey.W:
                    SetFullWindow(BottomBtnFullWindows.Visibility == Visibility.Visible);
                    break;
                case Windows.System.VirtualKey.F11:
                case Windows.System.VirtualKey.F:
                case Windows.System.VirtualKey.Enter:
                    SetFullScreen(BottomBtnFull.Visibility == Visibility.Visible);
                    break;
                case Windows.System.VirtualKey.F10:
                    await CaptureVideo();
                    break;
                case Windows.System.VirtualKey.F9:
                case Windows.System.VirtualKey.D:
                    if (DanmuControl.Visibility == Visibility.Visible)
                    {
                        DanmuControl.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        DanmuControl.Visibility = Visibility.Visible;
                    }
                    break;

                default:
                    break;
            }
        }


        private void LiveDetailPage_ClosedPage(object sender, EventArgs e)
        {
            StopPlay();
        }
        private void StopPlay()
        {
            if (mediaPlayer != null)
            {
                mediaPlayer.Pause();
                mediaPlayer.Source = null;
            }
            if (interopMSS != null)
            {
                interopMSS.Dispose();
                interopMSS = null;
            }
            liveRoomVM?.Dispose();
            //取消屏幕常亮
            if (dispRequest != null)
            {
                dispRequest = null;
            }
            liveRoomVM = null;
            SetFullScreen(false);
            MiniWidnows(false);
        }
        string roomid;
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                LoadSetting();
                roomid = e.Parameter.ToString();
                await liveRoomVM.LoadLiveRoomDetail(roomid);
                Title = liveRoomVM.LiveInfo.anchor_info.base_info.uname + "的直播间";
                ChangeTitle(liveRoomVM.LiveInfo.anchor_info.base_info.uname + "的直播间");
            }
            else
            {
                Title = (liveRoomVM.LiveInfo?.anchor_info?.base_info?.uname ?? "") + "直播间";
                MessageCenter.ChangeTitle(this, Title);
            }
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
                StopPlay();
            base.OnNavigatingFrom(e);
        }

        private void LoadSetting()
        {
            //音量
            mediaPlayer.Volume = SettingService.GetValue<double>(SettingConstants.Player.PLAYER_VOLUME, 1.0);
            SliderVolume.Value = mediaPlayer.Volume;
            SliderVolume.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                mediaPlayer.Volume = SliderVolume.Value;
                SettingService.SetValue<double>(SettingConstants.Player.PLAYER_VOLUME, SliderVolume.Value);
            });
            //亮度
            _brightness = SettingService.GetValue<double>(SettingConstants.Player.PLAYER_BRIGHTNESS, 0);
            BrightnessShield.Opacity = _brightness;

            //弹幕顶部距离
            DanmuControl.Margin = new Thickness(0, SettingService.GetValue<int>(SettingConstants.VideoDanmaku.TOP_MARGIN, 0), 0, 0);
            DanmuTopMargin.Value = DanmuControl.Margin.Top;
            DanmuTopMargin.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                SettingService.SetValue<double>(SettingConstants.VideoDanmaku.TOP_MARGIN, DanmuTopMargin.Value);
                DanmuControl.Margin = new Thickness(0, DanmuTopMargin.Value, 0, 0);
            });
            //弹幕大小
            DanmuControl.DanmakuSizeZoom = SettingService.GetValue<double>(SettingConstants.Live.FONT_ZOOM, 1);
            DanmuSettingFontZoom.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                if (isMini) return;
                SettingService.SetValue<double>(SettingConstants.Live.FONT_ZOOM, DanmuSettingFontZoom.Value);
            });
            //弹幕速度
            DanmuControl.DanmakuDuration = SettingService.GetValue<int>(SettingConstants.Live.SPEED, 10);
            DanmuSettingSpeed.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                if (isMini) return;
                SettingService.SetValue<double>(SettingConstants.Live.SPEED, DanmuSettingSpeed.Value);
            });
            //弹幕透明度
            DanmuControl.Opacity = SettingService.GetValue<double>(SettingConstants.Live.OPACITY, 1.0);
            DanmuSettingOpacity.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                SettingService.SetValue<double>(SettingConstants.Live.OPACITY, DanmuSettingOpacity.Value);
            });
            //弹幕加粗
            DanmuControl.DanmakuBold = SettingService.GetValue<bool>(SettingConstants.Live.BOLD, false);
            DanmuSettingBold.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingService.SetValue<bool>(SettingConstants.Live.BOLD, DanmuSettingBold.IsOn);
            });
            //弹幕样式
            DanmuControl.DanmakuStyle = (DanmakuBorderStyle)SettingService.GetValue<int>(SettingConstants.Live.BORDER_STYLE, 2);
            DanmuSettingStyle.SelectionChanged += new SelectionChangedEventHandler((e, args) =>
            {
                if (DanmuSettingStyle.SelectedIndex != -1)
                {
                    SettingService.SetValue<int>(SettingConstants.Live.BORDER_STYLE, DanmuSettingStyle.SelectedIndex);
                }
            });


            //弹幕显示区域
            DanmuControl.DanmakuArea = SettingService.GetValue<double>(SettingConstants.Live.AREA, 1);
            DanmuSettingArea.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                SettingService.SetValue<double>(SettingConstants.Live.AREA, DanmuSettingArea.Value);
            });

            //弹幕开关
            DanmuControl.Visibility = SettingService.GetValue<Visibility>(SettingConstants.Live.SHOW, Visibility.Visible);
            //弹幕延迟
            //LiveSettingDelay.Value = SettingService.GetValue<int>(SettingConstants.Live.DELAY, 20);
            //liveRoomVM.SetDelay(LiveSettingDelay.Value.ToInt32());
            //LiveSettingDelay.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            //{
            //    SettingService.SetValue(SettingConstants.Live.DELAY, LiveSettingDelay.Value);
            //    liveRoomVM.SetDelay(LiveSettingDelay.Value.ToInt32());
            //});

            //互动清理数量
            LiveSettingCount.Value = SettingService.GetValue<int>(SettingConstants.Live.DANMU_CLEAN_COUNT, 200);
            liveRoomVM.CleanCount = LiveSettingCount.Value.ToInt32();
            LiveSettingCount.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                SettingService.SetValue(SettingConstants.Live.DANMU_CLEAN_COUNT, LiveSettingCount.Value);
                liveRoomVM.CleanCount = LiveSettingCount.Value.ToInt32();
            });

            //硬解视频
            LiveSettingHardwareDecode.IsOn = SettingService.GetValue<bool>(SettingConstants.Live.HARDWARE_DECODING, true);
            if (LiveSettingHardwareDecode.IsOn)
            {
                _config.VideoDecoderMode = VideoDecoderMode.ForceSystemDecoder;
            }
            else
            {
                _config.VideoDecoderMode = VideoDecoderMode.ForceFFmpegSoftwareDecoder;
            }
            LiveSettingHardwareDecode.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingService.SetValue<bool>(SettingConstants.Live.HARDWARE_DECODING, LiveSettingHardwareDecode.IsOn);
                if (LiveSettingHardwareDecode.IsOn)
                {
                    _config.VideoDecoderMode = VideoDecoderMode.ForceSystemDecoder;
                }
                else
                {
                    _config.VideoDecoderMode = VideoDecoderMode.ForceFFmpegSoftwareDecoder;
                }
                Notify.ShowMessageToast("刷新后生效");
            });
            //自动打开宝箱
            LiveSettingAutoOpenBox.IsOn = SettingService.GetValue<bool>(SettingConstants.Live.AUTO_OPEN_BOX, true);
            liveRoomVM.AutoReceiveFreeSilver = LiveSettingAutoOpenBox.IsOn;
            LiveSettingAutoOpenBox.Toggled += new RoutedEventHandler((e, args) =>
            {
                liveRoomVM.AutoReceiveFreeSilver = LiveSettingAutoOpenBox.IsOn;
                SettingService.SetValue<bool>(SettingConstants.Live.AUTO_OPEN_BOX, LiveSettingAutoOpenBox.IsOn);
            });

            //屏蔽礼物信息
            LiveSettingDotReceiveGiftMsg.IsOn = SettingService.GetValue<bool>(SettingConstants.Live.HIDE_GIFT, false);
            liveRoomVM.ReceiveGiftMsg = !LiveSettingDotReceiveGiftMsg.IsOn;
            LiveSettingDotReceiveGiftMsg.Toggled += new RoutedEventHandler((e, args) =>
            {
                liveRoomVM.ReceiveGiftMsg = !LiveSettingDotReceiveGiftMsg.IsOn;
                if (LiveSettingAutoOpenBox.IsOn)
                {
                    liveRoomVM.ShowGiftMessage = false;
                }
                SettingService.SetValue<bool>(SettingConstants.Live.HIDE_GIFT, LiveSettingDotReceiveGiftMsg.IsOn);
            });

            //屏蔽进场信息
            LiveSettingDotReceiveWelcomeMsg.IsOn = SettingService.GetValue<bool>(SettingConstants.Live.HIDE_WELCOME, false);
            liveRoomVM.ReceiveWelcomeMsg = !LiveSettingDotReceiveWelcomeMsg.IsOn;
            LiveSettingDotReceiveWelcomeMsg.Toggled += new RoutedEventHandler((e, args) =>
            {
                liveRoomVM.ReceiveWelcomeMsg = !LiveSettingDotReceiveWelcomeMsg.IsOn;
                SettingService.SetValue<bool>(SettingConstants.Live.HIDE_WELCOME, LiveSettingDotReceiveWelcomeMsg.IsOn);
            });

            //屏蔽抽奖信息
            LiveSettingDotReceiveLotteryMsg.IsOn = SettingService.GetValue<bool>(SettingConstants.Live.HIDE_LOTTERY, false);
            liveRoomVM.ReceiveLotteryMsg = !LiveSettingDotReceiveLotteryMsg.IsOn;
            LiveSettingDotReceiveWelcomeMsg.Toggled += new RoutedEventHandler((e, args) =>
            {
                liveRoomVM.ReceiveLotteryMsg = !LiveSettingDotReceiveLotteryMsg.IsOn;
                SettingService.SetValue<bool>(SettingConstants.Live.HIDE_LOTTERY, LiveSettingDotReceiveLotteryMsg.IsOn);
            });

        }


        public void ChangeTitle(string title)
        {
            if ((this.Parent as Frame).Parent is TabViewItem)
            {
                if (this.Parent != null)
                {
                    ((this.Parent as Frame).Parent as TabViewItem).Header = title;
                }
            }
            else
            {
                MessageCenter.ChangeTitle(this, title);
            }
        }

        private async Task SetPlayer(string url)
        {
            try
            {
                PlayerLoading.Visibility = Visibility.Visible;
                PlayerLoadText.Text = "加载中";
                if (mediaPlayer != null)
                {
                    mediaPlayer.Pause();
                    mediaPlayer.Source = null;
                }
                if (interopMSS != null)
                {
                    interopMSS.Dispose();
                    interopMSS = null;
                }
                interopMSS = await FFmpegMediaSource.CreateFromUriAsync(url, _config);
                mediaPlayer.AutoPlay = true;
                mediaPlayer.Source = interopMSS.CreateMediaPlaybackItem();
                player.SetMediaPlayer(mediaPlayer);
            }
            catch (Exception ex)
            {
                Notify.ShowMessageToast("播放失败" + ex.Message);
            }

        }

        private async void BottomCBQuality_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BottomCBQuality.SelectedItem == null || flag)
            {
                return;
            }
            var item = BottomCBQuality.SelectedItem as LiveRoomWebUrlQualityDescriptionItemModel;
            SettingService.SetValue(SettingConstants.Live.DEFAULT_QUALITY, item.qn);
            await liveRoomVM.GetPlayUrl(liveRoomVM.RoomID, item.qn);
        }

        private async void BottomCBLine_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BottomCBLine.SelectedIndex == -1)
            {
                return;
            }
            url = liveRoomVM.urls[BottomCBLine.SelectedIndex].url;
            await SetPlayer(url);
        }

        private void BottomBtnPause_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
        }

        private void BottomBtnPlay_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Play();
        }

        private void BottomBtnFullWindows_Click(object sender, RoutedEventArgs e)
        {


            SetFullWindow(true);
        }

        private void BottomBtnExitFullWindows_Click(object sender, RoutedEventArgs e)
        {

            SetFullWindow(false);
        }

        private void BottomBtnFull_Click(object sender, RoutedEventArgs e)
        {

            SetFullScreen(true);
        }

        private void BottomBtnExitFull_Click(object sender, RoutedEventArgs e)
        {

            SetFullScreen(false);
        }

        private void SetFullWindow(bool e)
        {

            if (e)
            {
                BottomBtnFullWindows.Visibility = Visibility.Collapsed;
                BottomBtnExitFullWindows.Visibility = Visibility.Visible;
                RightInfo.Width = new GridLength(0, GridUnitType.Pixel);
                BottomInfo.Height = new GridLength(0, GridUnitType.Pixel);
            }
            else
            {
                BottomBtnFullWindows.Visibility = Visibility.Visible;
                BottomBtnExitFullWindows.Visibility = Visibility.Collapsed;
                RightInfo.Width = new GridLength(280, GridUnitType.Pixel);
                BottomInfo.Height = GridLength.Auto;
            }
        }
        private void SetFullScreen(bool e)
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            if (e)
            {
                BottomBtnFull.Visibility = Visibility.Collapsed;
                BottomBtnExitFull.Visibility = Visibility.Visible;
                this.Margin = new Thickness(0, SettingService.GetValue<int>(SettingConstants.UI.DISPLAY_MODE, 0) == 0 ? -48 : -48, 0, 0);
                RightInfo.Width = new GridLength(0, GridUnitType.Pixel);
                BottomInfo.Height = new GridLength(0, GridUnitType.Pixel);
                //全屏
                if (!view.IsFullScreenMode)
                {
                    view.TryEnterFullScreenMode();
                }
            }
            else
            {
                BottomBtnFull.Visibility = Visibility.Visible;
                BottomBtnExitFull.Visibility = Visibility.Collapsed;
                this.Margin = new Thickness(0);
                RightInfo.Width = new GridLength(280, GridUnitType.Pixel);
                BottomInfo.Height = GridLength.Auto;
                //退出全屏
                if (view.IsFullScreenMode)
                {
                    view.ExitFullScreenMode();
                }
            }
        }



        private async void BottomBtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await liveRoomVM.LoadLiveRoomDetail(roomid);
        }

        private void btnSendGift_Click(object sender, RoutedEventArgs e)
        {
            var giftInfo = (sender as Button).DataContext as LiveGiftItem;
            liveRoomVM.SendGift(giftInfo).RunWithoutAwait();
        }

        private async void TopBtnScreenshot_Click(object sender, RoutedEventArgs e)
        {
            await CaptureVideo();
        }
        private async Task CaptureVideo()
        {
            try
            {
                string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg";
                StorageFolder applicationFolder = KnownFolders.PicturesLibrary;
                StorageFolder folder = await applicationFolder.CreateFolderAsync("哔哩哔哩截图", CreationCollisionOption.OpenIfExists);
                StorageFile saveFile = await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
                RenderTargetBitmap bitmap = new RenderTargetBitmap();
                await bitmap.RenderAsync(player);
                var pixelBuffer = await bitmap.GetPixelsAsync();
                using (var fileStream = await saveFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, fileStream);
                    encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                        BitmapAlphaMode.Ignore,
                         (uint)bitmap.PixelWidth,
                         (uint)bitmap.PixelHeight,
                         DisplayInformation.GetForCurrentView().LogicalDpi,
                         DisplayInformation.GetForCurrentView().LogicalDpi,
                         pixelBuffer.ToArray());
                    await encoder.FlushAsync();
                }
                Notify.ShowMessageToast("截图已经保存至图片库");
            }
            catch (Exception)
            {
                Notify.ShowMessageToast("截图失败");
            }
        }

        private void TopBtnCloseDanmaku_Click(object sender, RoutedEventArgs e)
        {
            DanmuControl.Visibility = Visibility.Collapsed;
            SettingService.SetValue(SettingConstants.Live.SHOW, Visibility.Collapsed);
            DanmuControl.ClearAll();
        }

        private void TopBtnOpenDanmaku_Click(object sender, RoutedEventArgs e)
        {
            DanmuControl.Visibility = Visibility.Visible;
            SettingService.SetValue(SettingConstants.Live.SHOW, Visibility.Visible);
        }

        private async void BtnOpenBox_Click(object sender, RoutedEventArgs e)
        {
            await liveRoomVM.GetFreeSilver();
        }

        private void BtnOpenUser_Click(object sender, RoutedEventArgs e)
        {
            if (liveRoomVM.LiveInfo == null)
            {
                return;
            }
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Account,
                title = "用户信息",
                page = typeof(UserInfoPage),
                parameters = liveRoomVM.LiveInfo.room_info.uid
            });
        }

        private async void DanmuText_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (string.IsNullOrEmpty(DanmuText.Text))
            {
                Notify.ShowMessageToast("弹幕内容不能为空");
                return;
            }
            var result = await liveRoomVM.SendDanmu(DanmuText.Text);
            if (result)
            {
                DanmuText.Text = "";
            }

        }

        private async void BtnAttention_Click(object sender, RoutedEventArgs e)
        {
            if (liveRoomVM.LiveInfo != null)
            {
                var result = await new VideoDetailPageViewModel().AttentionUP(liveRoomVM.LiveInfo.room_info.uid.ToString(), 1);
                if (result)
                {
                    liveRoomVM.Attention = true;
                }
            }

        }

        private async void BtnCacnelAttention_Click(object sender, RoutedEventArgs e)
        {
            if (liveRoomVM.LiveInfo != null)
            {
                var result = await new VideoDetailPageViewModel().AttentionUP(liveRoomVM.LiveInfo.room_info.uid.ToString(), 2);
                if (result)
                {
                    liveRoomVM.Attention = false;
                }
            }
        }

        private async void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot.SelectedIndex == 3 && liveRoomVM.Guards.Count == 0)
            {
                await liveRoomVM.GetGuardList();
            }
        }

        private void list_Guard_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as LiveGuardRankItem;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Account,
                title = item.username,
                page = typeof(UserInfoPage),
                parameters = item.uid
            });
        }

        private async void cb_Rank_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_Rank.SelectedItem == null)
            {
                return;
            }
            liveRoomVM.DoPropertyChanged("SelectRank");
            var data = cb_Rank.SelectedItem as LiveRoomRankVM;
            if (!data.Loading && data.Items.Count == 0)
            {

                await data.LoadData();
            }
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as LiveRoomRankItemModel;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Account,
                title = item.uname,
                page = typeof(UserInfoPage),
                parameters = item.uid
            });
        }

        private async void BtnSendLotteryDanmu_Click(object sender, RoutedEventArgs e)
        {
            if (liveRoomVM.anchorLotteryVM != null && liveRoomVM.anchorLotteryVM.LotteryInfo != null && !string.IsNullOrEmpty(liveRoomVM.anchorLotteryVM.LotteryInfo.danmu))
            {
                var result = await liveRoomVM.SendDanmu(liveRoomVM.anchorLotteryVM.LotteryInfo.danmu);
                if (result)
                {
                    Notify.ShowMessageToast("弹幕发送成功");
                    FlyoutLottery.Hide();
                }

            }
        }

        private void BottomBtnMiniWindows_Click(object sender, RoutedEventArgs e)
        {
            MiniWidnows(true);

        }

        private void BottomBtnExitMiniWindows_Click(object sender, RoutedEventArgs e)
        {
            MiniWidnows(false);
        }
        bool isMini = false;
        private async void MiniWidnows(bool mini)
        {
            isMini = mini;
            ApplicationView view = ApplicationView.GetForCurrentView();
            if (mini)
            {
                BottomBtnFullWindows_Click(this, null);
                StandardControl.Visibility = Visibility.Collapsed;
                MiniControl.Visibility = Visibility.Visible;

                if (ApplicationView.GetForCurrentView().IsViewModeSupported(ApplicationViewMode.CompactOverlay))
                {
                    //隐藏标题栏
                    this.Margin = new Thickness(0, -40, 0, 0);
                    await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay);
                    DanmuControl.DanmakuSizeZoom = 0.5;
                    DanmuControl.DanmakuDuration = 6;
                    DanmuControl.ClearAll();
                }
            }
            else
            {
                BottomBtnExitFullWindows_Click(this, null);
                this.Margin = new Thickness(0, 0, 0, 0);
                StandardControl.Visibility = Visibility.Visible;
                MiniControl.Visibility = Visibility.Collapsed;
                await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);
                DanmuControl.DanmakuSizeZoom = SettingService.GetValue<double>(SettingConstants.Live.FONT_ZOOM, 1);
                DanmuControl.DanmakuDuration = SettingService.GetValue<int>(SettingConstants.Live.SPEED, 10);
                DanmuControl.ClearAll();
                DanmuControl.Visibility = SettingService.GetValue<Visibility>(SettingConstants.Live.SHOW, Visibility.Visible);
            }
            MessageCenter.SetMiniWindow(mini);
        }

        private void btnRemoveWords_Click(object sender, RoutedEventArgs e)
        {
            var word = (sender as HyperlinkButton).DataContext as string;
            settingVM.LiveWords.Remove(word);
            SettingService.SetValue(SettingConstants.Live.SHIELD_WORD, settingVM.LiveWords);

        }

        private void DanmuSettingAddWord_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(DanmuSettingTxtWord.Text))
            {
                Notify.ShowMessageToast("关键字不能为空");
                return;
            }
            if (!settingVM.LiveWords.Contains(DanmuSettingTxtWord.Text))
            {
                settingVM.LiveWords.Add(DanmuSettingTxtWord.Text);
                SettingService.SetValue(SettingConstants.Live.SHIELD_WORD, settingVM.LiveWords);
            }

            DanmuSettingTxtWord.Text = "";
        }

        #region 播放器手势
        int showControlsFlag = 0;
        bool pointer_in_player = false;

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ShowControl(control.Visibility == Visibility.Collapsed);

        }
        bool runing = false;
        private async void ShowControl(bool show)
        {
            if (runing) return;
            runing = true;
            if (show)
            {
                showControlsFlag = 0;
                control.Visibility = Visibility.Visible;
                await control.FadeInAsync(280);

            }
            else
            {
                if (pointer_in_player)
                {
                    Window.Current.CoreWindow.PointerCursor = null;
                }
                await control.FadeOutAsync(280);
                control.Visibility = Visibility.Collapsed;
            }
            runing = false;
        }
        private void Grid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (BottomBtnFull.Visibility == Visibility.Visible)
            {
                BottomBtnFull_Click(sender, null);
            }
            else
            {
                BottomBtnExitFull_Click(sender, null);
            }
        }
        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            pointer_in_player = true;
        }

        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            pointer_in_player = false;
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
        }

        private void Grid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (Window.Current.CoreWindow.PointerCursor == null)
            {
                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            }

        }

        bool ManipulatingBrightness = false;
        private void Grid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            e.Handled = true;
            //progress.Visibility = Visibility.Visible;
            if (ManipulatingBrightness)
                HandleSlideBrightnessDelta(e.Delta.Translation.Y);
            else
                HandleSlideVolumeDelta(e.Delta.Translation.Y);
        }


        private void HandleSlideVolumeDelta(double delta)
        {
            if (delta > 0)
            {
                double dd = delta / (this.ActualHeight * 0.8);

                //slider_V.Value -= d;
                var volume = mediaPlayer.Volume - dd;
                if (volume < 0) volume = 0;
                SliderVolume.Value = volume;

            }
            else
            {
                double dd = Math.Abs(delta) / (this.ActualHeight * 0.8);
                var volume = mediaPlayer.Volume + dd;
                if (volume > 1) volume = 1;
                SliderVolume.Value = volume;
                //slider_V.Value += d;
            }
            TxtToolTip.Text = "音量:" + mediaPlayer.Volume.ToString("P");

            //Notify.ShowMessageToast("音量:" +  mediaElement.MediaPlayer.Volume.ToString("P"), 3000);
        }
        private void HandleSlideBrightnessDelta(double delta)
        {
            double dd = Math.Abs(delta) / (this.ActualHeight * 0.8);
            if (delta > 0)
            {
                Brightness = Math.Min(Brightness + dd, 1);
            }
            else
            {
                Brightness = Math.Max(Brightness - dd, 0);
            }
            TxtToolTip.Text = "亮度:" + Math.Abs(Brightness - 1).ToString("P");
        }
        private void Grid_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            e.Handled = true;
            TxtToolTip.Text = "";
            ToolTip.Visibility = Visibility.Visible;

            if (e.Position.X < this.ActualWidth / 2)
                ManipulatingBrightness = true;
            else
                ManipulatingBrightness = false;

        }

        double _brightness;
        double Brightness
        {
            get => _brightness;
            set
            {
                _brightness = value;
                BrightnessShield.Opacity = value;
                SettingService.SetValue<double>(SettingConstants.Player.PLAYER_BRIGHTNESS, _brightness);
            }
        }

        private void Grid_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            e.Handled = true;
            ToolTip.Visibility = Visibility.Collapsed;
        }
        #endregion

        private async void btnSendBagGift_Click(object sender, RoutedEventArgs e)
        {
            var giftInfo = (sender as Button).DataContext as LiveGiftItem;
            await Task.Run(() => liveRoomVM.SendBagGift(giftInfo)).ConfigureAwait(false);
        }
        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            request.Data.Properties.Title = liveRoomVM.LiveInfo.room_info.title;
            request.Data.SetWebLink(new Uri("https://live.bilibili.com/" + liveRoomVM.RoomID));
        }
        private void btnShare_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }
        private void btnShareCopy_Click(object sender, RoutedEventArgs e)
        {
            $"{liveRoomVM.LiveInfo.room_info.title} - {liveRoomVM.LiveInfo.anchor_info.base_info.uname}的直播间\r\nhttps://live.bilibili.com/{liveRoomVM.RoomID}".SetClipboard();
            Notify.ShowMessageToast("已复制内容到剪切板");
        }

        private void btnShareCopyUrl_Click(object sender, RoutedEventArgs e)
        {
            ("https://live.bilibili.com/" + liveRoomVM.RoomID).SetClipboard();
            Notify.ShowMessageToast("已复制链接到剪切板");
        }

        private void Player_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var rectangle = new RectangleGeometry();
            rectangle.Rect = new Rect(0, 0, PlayerView.ActualWidth, PlayerView.ActualHeight);
            DanmuControl.Clip = rectangle;
        }
    }
}
