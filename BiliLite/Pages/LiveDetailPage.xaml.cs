using BiliLite.Controls;
using BiliLite.Helpers;
using BiliLite.Modules;
using BiliLite.Modules.LiveRoomDetailModels;
using FFmpegInterop;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
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
    public sealed partial class LiveDetailPage : Page
    {
        readonly FFmpegInteropConfig _config;
        FFmpegInterop.FFmpegInteropMSS interopMSS;
        LiveRoomVM liveRoomVM;
        readonly MediaPlayer mediaPlayer;
        public LiveDetailPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
            _config = new FFmpegInteropConfig();
            _config.FFmpegOptions.Add("rtsp_transport", "tcp");
            _config.FFmpegOptions.Add("user_agent", "Mozilla/5.0 BiliDroid/1.12.0 (bbcallen@gmail.com)");
            _config.FFmpegOptions.Add("referer", "https://live.bilibili.com/");
           
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

        }
        
        private async void LiveRoomVM_LotteryEnd(object sender, LiveRoomEndAnchorLotteryInfoModel e)
        {
            var str = "";
            foreach (var item in e.award_users)
            {
                str += item.uname + "、";
            }
            str=str.TrimEnd('、');
            await new MessageDialog($"奖品:{e.award_name}\r\n中奖用户:{str}","开奖信息").ShowAsync();
        }

        private void LiveRoomVM_AddNewDanmu(object sender, string e)
        {
            if (DanmuControl.Visibility== Visibility.Visible)
            {
                DanmuControl.AddLiveDanmu(e, false,Colors.White);
            }
           
        }
        #region 播放器事件
        private async void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                liveRoomVM.Liveing = false;
                url = "";
                player.SetMediaPlayer(null);
            });
        }

        private async void PlaybackSession_BufferingEnded(MediaPlaybackSession sender, object args)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
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
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,async () =>
            {
                LogHelper.Log("直播加载失败", LogType.ERROR, new Exception(args.ErrorMessage));
                await new MessageDialog($"啊，直播加载失败了\r\n错误信息:{args.ErrorMessage}\r\n请尝试在直播设置中打开/关闭硬解试试", "播放失败").ShowAsync();
            });

        }

        private async void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
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
                str += $"Video Codec: {interopMSS.VideoStream.CodecName}\r\nAudio Codec:{interopMSS.AudioStreams[0].CodecName}\r\n";
                str += $"Resolution: {interopMSS.VideoStream.PixelWidth} x {interopMSS.VideoStream.PixelHeight}\r\n";
                str += $"Video Bitrate: {interopMSS.VideoStream.Bitrate/1024} Kbps\r\n";
                str += $"Audio Bitrate: {interopMSS.AudioStreams[0].Bitrate/1024} Kbps\r\n";
                str += $"Decoder Engine: {interopMSS.VideoStream.DecoderEngine.ToString()}";
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

        private void LiveDetailPage_Loaded(object sender, RoutedEventArgs e)
        {
            DanmuControl.ClearAll();
            if (this.Parent is MyFrame)
            {
                (this.Parent as MyFrame).ClosedPage -= LiveDetailPage_ClosedPage;
                (this.Parent as MyFrame).ClosedPage += LiveDetailPage_ClosedPage;
            }
        }

        private void LiveDetailPage_ClosedPage(object sender, EventArgs e)
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
            liveRoomVM.Dispose();
            liveRoomVM = null;
        }

        string roomid;
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if(e.NavigationMode== NavigationMode.New)
            {
                LoadSetting();
                roomid = e.Parameter.ToString();
                await liveRoomVM.LoadLiveRoomDetail(roomid);
                ChangeTitle(liveRoomVM.LiveInfo.anchor_info.base_info.uname + "的直播间");
            }
        }

        private void LoadSetting()
        {
            //弹幕大小
            DanmuControl.sizeZoom = SettingHelper.GetValue<double>(SettingHelper.Live.FONT_ZOOM, 1);
            DanmuSettingFontZoom.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                SettingHelper.SetValue<double>(SettingHelper.Live.FONT_ZOOM, DanmuSettingFontZoom.Value);
            });
            //弹幕速度
            DanmuControl.speed = SettingHelper.GetValue<int>(SettingHelper.Live.SPEED, 10);
            DanmuSettingSpeed.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                SettingHelper.SetValue<double>(SettingHelper.Live.SPEED, DanmuSettingSpeed.Value);
            });
            //弹幕透明度
            DanmuControl.Opacity = SettingHelper.GetValue<double>(SettingHelper.Live.OPACITY, 1.0);
            DanmuSettingOpacity.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                SettingHelper.SetValue<double>(SettingHelper.Live.OPACITY, DanmuSettingOpacity.Value);
            });
            //弹幕加粗
            DanmuControl.bold = SettingHelper.GetValue<bool>(SettingHelper.Live.BOLD, false);
            DanmuSettingBold.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingHelper.SetValue<bool>(SettingHelper.Live.BOLD, DanmuSettingBold.IsOn);
            });
            //弹幕样式
            DanmuControl.BorderStyle = SettingHelper.GetValue<int>(SettingHelper.Live.BORDER_STYLE, 2);
            DanmuSettingStyle.SelectionChanged += new SelectionChangedEventHandler((e, args) =>
            {
                if (DanmuSettingStyle.SelectedIndex != -1)
                {
                    SettingHelper.SetValue<int>(SettingHelper.Live.BORDER_STYLE, DanmuSettingStyle.SelectedIndex);
                }
            });
           
            //半屏显示
            DanmuSettingDotHideSubtitle.IsOn = SettingHelper.GetValue<bool>(SettingHelper.Live.DOTNET_HIDE_SUBTITLE, false);
            DanmuSettingDotHideSubtitle.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingHelper.SetValue<bool>(SettingHelper.Live.DOTNET_HIDE_SUBTITLE, DanmuSettingDotHideSubtitle.IsOn);
            });

            //弹幕开关
            DanmuControl.Visibility = SettingHelper.GetValue<Visibility>(SettingHelper.Live.SHOW, Visibility.Visible);
            //弹幕延迟
            LiveSettingDelay.Value = SettingHelper.GetValue<int>(SettingHelper.Live.DELAY, 20);
            liveRoomVM.SetDelay(LiveSettingDelay.Value.ToInt32());
            LiveSettingDelay.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                SettingHelper.SetValue(SettingHelper.Live.DELAY, LiveSettingDelay.Value);
                liveRoomVM.SetDelay(LiveSettingDelay.Value.ToInt32());
            });

            //互动清理数量
            LiveSettingCount.Value = SettingHelper.GetValue<int>(SettingHelper.Live.DANMU_CLEAN_COUNT, 200);
            liveRoomVM.CleanCount= LiveSettingCount.Value.ToInt32();
            LiveSettingCount.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                SettingHelper.SetValue(SettingHelper.Live.DANMU_CLEAN_COUNT, LiveSettingCount.Value);
                liveRoomVM.CleanCount = LiveSettingCount.Value.ToInt32();
            });

            //硬解视频
            LiveSettingHardwareDecode.IsOn = SettingHelper.GetValue<bool>(SettingHelper.Live.HARDWARE_DECODING, true);
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
                SettingHelper.SetValue<bool>(SettingHelper.Live.HARDWARE_DECODING, LiveSettingHardwareDecode.IsOn);
                if (LiveSettingHardwareDecode.IsOn)
                {
                    _config.VideoDecoderMode = VideoDecoderMode.ForceSystemDecoder;
                }
                else
                {
                    _config.VideoDecoderMode = VideoDecoderMode.ForceFFmpegSoftwareDecoder;
                }
                Utils.ShowMessageToast("刷新后生效");
            });
            //自动打开宝箱
            LiveSettingAutoOpenBox.IsOn = SettingHelper.GetValue<bool>(SettingHelper.Live.AUTO_OPEN_BOX, true);
            liveRoomVM.AutoReceiveFreeSilver = LiveSettingAutoOpenBox.IsOn;
            LiveSettingAutoOpenBox.Toggled += new RoutedEventHandler((e, args) =>
            {
                liveRoomVM.AutoReceiveFreeSilver = LiveSettingAutoOpenBox.IsOn;
                SettingHelper.SetValue<bool>(SettingHelper.Live.AUTO_OPEN_BOX, LiveSettingAutoOpenBox.IsOn);
            });

            //屏蔽礼物信息
            LiveSettingDotReceiveGiftMsg.IsOn = SettingHelper.GetValue<bool>(SettingHelper.Live.HIDE_GIFT, false);
            liveRoomVM.ReceiveGiftMsg = !LiveSettingDotReceiveGiftMsg.IsOn;
            LiveSettingDotReceiveGiftMsg.Toggled += new RoutedEventHandler((e, args) =>
            {
                liveRoomVM.ReceiveGiftMsg = !LiveSettingDotReceiveGiftMsg.IsOn;
                if (LiveSettingAutoOpenBox.IsOn)
                {
                    liveRoomVM.ShowGiftMessage = false;
                }
                SettingHelper.SetValue<bool>(SettingHelper.Live.HIDE_GIFT, LiveSettingDotReceiveGiftMsg.IsOn);
            });

            //屏蔽进场信息
            LiveSettingDotReceiveWelcomeMsg.IsOn = SettingHelper.GetValue<bool>(SettingHelper.Live.HIDE_WELCOME, false);
            liveRoomVM.ReceiveWelcomeMsg = !LiveSettingDotReceiveWelcomeMsg.IsOn;
            LiveSettingDotReceiveWelcomeMsg.Toggled += new RoutedEventHandler((e, args) =>
            {
                liveRoomVM.ReceiveWelcomeMsg = !LiveSettingDotReceiveWelcomeMsg.IsOn;
                SettingHelper.SetValue<bool>(SettingHelper.Live.HIDE_WELCOME, LiveSettingDotReceiveWelcomeMsg.IsOn);
            });

            //屏蔽抽奖信息
            LiveSettingDotReceiveLotteryMsg.IsOn = SettingHelper.GetValue<bool>(SettingHelper.Live.HIDE_LOTTERY, false);
            liveRoomVM.ReceiveLotteryMsg = !LiveSettingDotReceiveLotteryMsg.IsOn;
            LiveSettingDotReceiveWelcomeMsg.Toggled += new RoutedEventHandler((e, args) =>
            {
                liveRoomVM.ReceiveLotteryMsg = !LiveSettingDotReceiveLotteryMsg.IsOn;
                SettingHelper.SetValue<bool>(SettingHelper.Live.HIDE_LOTTERY, LiveSettingDotReceiveLotteryMsg.IsOn);
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
                MessageCenter.ChangeTitle(title);
            }
        }

        private async Task SetPlayer(string url)
        {
            try
            {
                PlayerLoading.Visibility = Visibility.Visible;
                PlayerLoadText.Text = "加载中";
                if (interopMSS != null)
                {
                    interopMSS = null;
                }
                if (mediaPlayer != null)
                {
                    mediaPlayer.Source = null;
                }
                interopMSS = await FFmpegInteropMSS.CreateFromUriAsync(url, _config);
                mediaPlayer.AutoPlay = true;
                mediaPlayer.Source = interopMSS.CreateMediaPlaybackItem();
                player.SetMediaPlayer(mediaPlayer);
            }
            catch (Exception ex)
            {
                Utils.ShowMessageToast("播放失败" + ex.Message);
            }
           
        }

        private async void BottomCBQuality_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BottomCBQuality.SelectedItem == null|| flag)
            {
                return;
            }
            var item = BottomCBQuality.SelectedItem as LiveRoomWebUrlQualityDescriptionItemModel;
            await liveRoomVM.GetPlayUrl(liveRoomVM.RoomID, item.qn);
        }

        private async void BottomCBLine_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BottomCBLine.SelectedIndex==-1)
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

            BottomBtnFullWindows.Visibility = Visibility.Collapsed;
            BottomBtnExitFullWindows.Visibility = Visibility.Visible;
            SetFullWindow(true);
        }

        private void BottomBtnExitFullWindows_Click(object sender, RoutedEventArgs e)
        {
            BottomBtnFullWindows.Visibility = Visibility.Visible;
            BottomBtnExitFullWindows.Visibility = Visibility.Collapsed;
            SetFullWindow(false);
        }

        private void BottomBtnFull_Click(object sender, RoutedEventArgs e)
        {
            BottomBtnFull.Visibility = Visibility.Collapsed;
            BottomBtnExitFull.Visibility = Visibility.Visible;
            SetFullScreen(true);
        }

        private void BottomBtnExitFull_Click(object sender, RoutedEventArgs e)
        {
            BottomBtnFull.Visibility = Visibility.Visible;
            BottomBtnExitFull.Visibility = Visibility.Collapsed;
            SetFullScreen(false);
        }
        
        private void SetFullWindow(bool e)
        {
            if (e)
            {
                RightInfo.Width = new GridLength(0, GridUnitType.Pixel);
                BottomInfo.Height = new GridLength(0, GridUnitType.Pixel);
            }
            else
            {
                RightInfo.Width = new GridLength(280, GridUnitType.Pixel);
                BottomInfo.Height = GridLength.Auto;
            }
        }
        private void SetFullScreen(bool e)
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            if (e)
            {
                this.Margin = new Thickness(0, -40, 0, 0);
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

        private async void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (control.Visibility== Visibility.Visible)
            {
                await control.FadeOutAsync(280);
                control.Visibility = Visibility.Collapsed;
            }
            else
            {
                control.Visibility = Visibility.Visible;
                await control.FadeInAsync(280);
            }
         
        }

        private void Grid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if(BottomBtnFull.Visibility== Visibility.Visible)
            {
                BottomBtnFull_Click(sender, null);
            }
            else
            {
                BottomBtnExitFull_Click(sender, null);
            }
        }

        private async void BottomBtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await liveRoomVM.LoadLiveRoomDetail(roomid);
        }

        private void btnSendGift_Click(object sender, RoutedEventArgs e)
        {
            var giftInfo= (sender as Button).DataContext as LiveGiftItem;
            liveRoomVM.SendGift(giftInfo);
            
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
                Utils.ShowMessageToast("截图已经保存至图片库");
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("截图失败");
            }
        }

        private void TopBtnCloseDanmaku_Click(object sender, RoutedEventArgs e)
        {
            DanmuControl.Visibility = Visibility.Collapsed;
            SettingHelper.SetValue(SettingHelper.Live.SHOW, Visibility.Collapsed);
            DanmuControl.ClearAll();
        }

        private void TopBtnOpenDanmaku_Click(object sender, RoutedEventArgs e)
        {
            DanmuControl.Visibility = Visibility.Visible;
            SettingHelper.SetValue(SettingHelper.Live.SHOW, Visibility.Visible);
        }

        private async void BtnOpenBox_Click(object sender, RoutedEventArgs e)
        {
           await liveRoomVM.GetFreeSilver();
        }

        private void BtnOpenUser_Click(object sender, RoutedEventArgs e)
        {
            if (liveRoomVM.LiveInfo==null)
            {
                return;
            }
            MessageCenter.OpenNewWindow(this, new NavigationInfo()
            {
                icon = Symbol.Account,
                title = "用户信息",
                page = typeof(UserInfoPage),
                parameters = liveRoomVM.LiveInfo.room_info.uid
            });
        }

        private async void DanmuText_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (string.IsNullOrEmpty( DanmuText.Text))
            {
                Utils.ShowMessageToast("弹幕内容不能为空");
                return;
            }
           var result=await liveRoomVM.SendDanmu(DanmuText.Text);
            if (result)
            {
                DanmuText.Text = "";
            }

        }

        private async void BtnAttention_Click(object sender, RoutedEventArgs e)
        {
            if (liveRoomVM.LiveInfo!=null)
            {
                var result=await new VideoDetailVM().AttentionUP(liveRoomVM.LiveInfo.room_info.uid.ToString(),1);
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
                var result = await new VideoDetailVM().AttentionUP(liveRoomVM.LiveInfo.room_info.uid.ToString(), 2);
                if (result)
                {
                    liveRoomVM.Attention = false;
                }
            }
        }

        private async void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot.SelectedIndex==3&&liveRoomVM.Guards.Count==0)
            {
                await liveRoomVM.GetGuardList();
            }
        }

        private void list_Guard_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as LiveGuardRankItem;
            MessageCenter.OpenNewWindow(this, new NavigationInfo()
            {
                icon = Symbol.Account,
                title = item.username,
                page = typeof(UserInfoPage),
                parameters = item.uid
            });
        }

        private async void cb_Rank_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_Rank.SelectedItem==null)
            {
                return;
            }
            liveRoomVM.DoPropertyChanged("SelectRank");
            var data= cb_Rank.SelectedItem as LiveRoomRankVM;
            if (!data.Loading&&data.Items.Count==0)
            {
                
                await data.LoadData();
            }
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as LiveRoomRankItemModel;
            MessageCenter.OpenNewWindow(this, new NavigationInfo()
            {
                icon = Symbol.Account,
                title = item.uname,
                page = typeof(UserInfoPage),
                parameters = item.uid
            });
        }

        private async void BtnSendLotteryDanmu_Click(object sender, RoutedEventArgs e)
        {
            if(liveRoomVM.anchorLotteryVM!=null&& liveRoomVM.anchorLotteryVM.LotteryInfo != null &&!string.IsNullOrEmpty( liveRoomVM.anchorLotteryVM.LotteryInfo.danmu))
            {
                var result = await liveRoomVM.SendDanmu(liveRoomVM.anchorLotteryVM.LotteryInfo.danmu);
                if (result)
                {
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
        private async void MiniWidnows(bool mini)
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            if (mini)
            {
                BottomBtnFullWindows_Click(this, null);
                StandardControl.Visibility = Visibility.Collapsed;
                MiniControl.Visibility = Visibility.Visible;
                DanmuControl.Visibility = Visibility.Collapsed;
                if (ApplicationView.GetForCurrentView().IsViewModeSupported(ApplicationViewMode.CompactOverlay))
                {
                    this.Margin = new Thickness(0, -40, 0, 0);
                    await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay);
                }
            }
            else
            {
                BottomBtnExitFullWindows_Click(this, null);
                this.Margin = new Thickness(0, 0, 0, 0);
                StandardControl.Visibility = Visibility.Visible;
                MiniControl.Visibility = Visibility.Collapsed;
                await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);
                DanmuControl.Visibility = SettingHelper.GetValue<Visibility>(SettingHelper.Live.SHOW, Visibility.Visible);
            }
        }
    }
}
