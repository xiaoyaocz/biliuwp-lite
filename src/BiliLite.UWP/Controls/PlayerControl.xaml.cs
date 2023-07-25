using BiliLite.Modules;
using NSDanmaku.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.System.Display;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Graphics.Display;
using System.Text.RegularExpressions;
using Windows.UI.Core;
using BiliLite.Dialogs;
using BiliLite.Modules.Player;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Windows.UI;
using Windows.Storage.Streams;
using Windows.UI.Text;
using BiliLite.Models.Requests.Api;
using BiliLite.Services;
using BiliLite.Models.Common;
using BiliLite.Extensions;
using BiliLite.Models.Common.Video;
using Windows.UI.Input;
using BiliLite.Models.Common.Danmaku;
using BiliLite.Models.Common.Player;
using BiliLite.Models.Common.Video.PlayUrlInfos;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls
{
    public sealed partial class PlayerControl : UserControl, IDisposable
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        public event PropertyChangedEventHandler PropertyChanged;
        private GestureRecognizer gestureRecognizer;
        private void DoPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        InteractionVideoVM interactionVideoVM;
        /// <summary>
        /// 铺满窗口事件
        /// </summary>
        public event EventHandler<bool> FullWindowEvent;
        /// <summary>
        /// 全屏事件
        /// </summary>
        public event EventHandler<bool> FullScreenEvent;
        /// <summary>
        /// 全部播放完毕
        /// </summary>
        public event EventHandler AllMediaEndEvent;
        /// <summary>
        /// 切换剧集事件
        /// </summary>

        public event EventHandler<int> ChangeEpisodeEvent;
        /// <summary>
        /// 播放列表
        /// </summary>
        public List<PlayInfo> PlayInfos { get; set; }
        /// <summary>
        /// 当前播放
        /// </summary>
        public int CurrentPlayIndex { get; set; }
        /// <summary>
        /// 当前播放
        /// </summary>
        public PlayInfo CurrentPlayItem { get; set; }
        readonly PlayerVM playerHelper;
        readonly NSDanmaku.Helper.DanmakuParse danmakuParse;
        private BiliPlayUrlQualitesInfo _playUrlInfo;
        private PlayerKeyRightAction m_playerKeyRightAction;
        /// <summary>
        /// 播放地址信息
        /// </summary>
        public BiliPlayUrlQualitesInfo playUrlInfo
        {
            get { return _playUrlInfo; }
            set { _playUrlInfo = value; DoPropertyChanged("playUrlInfo"); }
        }

        DispatcherTimer danmuTimer;
        /// <summary>
        /// 弹幕信息
        /// </summary>
        IDictionary<int, List<NSDanmaku.Model.DanmakuModel>> danmakuPool = new Dictionary<int, List<NSDanmaku.Model.DanmakuModel>>();
        List<int> danmakuLoadedSegment;
        SettingVM settingVM;
        DisplayRequest dispRequest;
        SystemMediaTransportControls _systemMediaTransportControls;
        DispatcherTimer timer_focus;
        public Player PlayerInstance { get { return Player; } }
        /// <summary>
        /// 当前选中的字幕名称
        /// </summary>
        private string CurrentSubtitleName { get; set; } = "无";

        DisplayInformation displayInformation = DisplayInformation.GetForCurrentView();

        public PlayerControl()
        {
            this.InitializeComponent();
            dispRequest = new DisplayRequest();
            playerHelper = new PlayerVM();
            settingVM = new SettingVM();

            danmakuParse = new NSDanmaku.Helper.DanmakuParse();
            //每过2秒就设置焦点
            timer_focus = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(2) };
            timer_focus.Tick += Timer_focus_Tick;
            danmuTimer = new DispatcherTimer();
            danmuTimer.Interval = TimeSpan.FromSeconds(1);
            danmuTimer.Tick += DanmuTimer_Tick;
            this.Loaded += PlayerControl_Loaded;
            this.Unloaded += PlayerControl_Unloaded;
            m_playerKeyRightAction = (PlayerKeyRightAction)SettingService.GetValue(SettingConstants.Player.PLAYER_KEY_RIGHT_ACTION, (int)PlayerKeyRightAction.ControlProgress);

            gestureRecognizer = new GestureRecognizer();
            InitializeGesture();
        }

        private void Timer_focus_Tick(object sender, object e)
        {
            var elent = FocusManager.GetFocusedElement();
            if (elent is Button || elent is AppBarButton || elent is HyperlinkButton || elent is MenuFlyoutItem)
            {
                BtnFoucs.Focus(FocusState.Programmatic);
            }

        }

        bool runing = false;
        bool pointer_in_player = false;
        private async void ShowControl(bool show)
        {
            if (runing) return;
            runing = true;
            if (show)
            {
                BottomImageBtnPlay.Margin = new Thickness(24, 24, 24, 100);
                showControlsFlag = 0;
                control.Visibility = Visibility.Visible;
                await control.FadeInAsync(400);
            }
            else
            {
                if (pointer_in_player)
                {
                    Window.Current.CoreWindow.PointerCursor = null;
                }
                BottomImageBtnPlay.Margin = new Thickness(24, 24, 24, 24);
                await control.FadeOutAsync(400);
                control.Visibility = Visibility.Collapsed;

            }
            runing = false;
        }
        private void PlayerControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            Window.Current.CoreWindow.KeyDown -= PlayerControl_KeyDown;
            Window.Current.CoreWindow.KeyUp -= PlayerControl_KeyUp;
            if (_systemMediaTransportControls != null)
            {
                _systemMediaTransportControls.DisplayUpdater.ClearAll();
                _systemMediaTransportControls.IsEnabled = false;
                _systemMediaTransportControls = null;
            }
            timer_focus.Stop();
        }
        private void PlayerControl_Loaded(object sender, RoutedEventArgs e)
        {
            DanmuControl.ClearAll();
            Window.Current.CoreWindow.KeyDown += PlayerControl_KeyDown;
            Window.Current.CoreWindow.KeyUp += PlayerControl_KeyUp;
            BtnFoucs.Focus(FocusState.Programmatic);
            _systemMediaTransportControls = SystemMediaTransportControls.GetForCurrentView();
            _systemMediaTransportControls.IsPlayEnabled = true;
            _systemMediaTransportControls.IsPauseEnabled = true;
            if (CurrentPlayItem != null)
            {
                SystemMediaTransportControlsDisplayUpdater updater = _systemMediaTransportControls.DisplayUpdater;
                updater.Type = MediaPlaybackType.Video;
                updater.VideoProperties.Title = CurrentPlayItem.title;
                updater.Update();
            }
            _systemMediaTransportControls.ButtonPressed += _systemMediaTransportControls_ButtonPressed;

            LoadPlayerSetting();
            LoadDanmuSetting();
            LoadSutitleSetting();

            danmuTimer.Start();
            timer_focus.Start();
        }

        private async void _systemMediaTransportControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Player.Play();
                    });
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Pause();
                    });
                    break;
                default:
                    break;
            }
        }

        private void PlayerControl_KeyUp(CoreWindow sender, KeyEventArgs args)
        {
            switch (args.VirtualKey)
            {
                case Windows.System.VirtualKey.Right:
                    {
                        if (m_playerKeyRightAction == PlayerKeyRightAction.AcceleratePlay)
                        {
                            StopHighRateSpeedPlay();
                        }
                    }
                    break;
            }
        }

        private async void PlayerControl_KeyDown(CoreWindow sender, KeyEventArgs args)
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
                case Windows.System.VirtualKey.Space:
                    if (Player.PlayState == PlayState.Playing || Player.PlayState == PlayState.End)
                    {
                        Pause();
                    }
                    else
                    {
                        Player.Play();
                    }
                    break;
                case Windows.System.VirtualKey.Left:
                    {
                        if (Player.PlayState == PlayState.Playing || Player.PlayState == PlayState.Pause)
                        {
                            var _position = Player.Position - 3;
                            if (_position < 0)
                            {
                                _position = 0;
                            }
                            Player.Position = _position;
                            TxtToolTip.Text = "进度:" + TimeSpan.FromSeconds(Player.Position).ToString(@"hh\:mm\:ss");
                            ToolTip.Visibility = Visibility.Visible;
                            await Task.Delay(2000);
                            ToolTip.Visibility = Visibility.Collapsed;
                        }
                    }

                    break;
                case Windows.System.VirtualKey.Right:
                    {
                        if (m_playerKeyRightAction == PlayerKeyRightAction.AcceleratePlay)
                        {
                            StartHighRateSpeedPlay();
                        }
                        if (m_playerKeyRightAction == PlayerKeyRightAction.ControlProgress && (Player.PlayState == PlayState.Playing || Player.PlayState == PlayState.Pause))
                        {
                            var _position = Player.Position + 3;
                            if (_position > Player.Duration)
                            {
                                _position = Player.Duration;
                            }
                            Player.Position = _position;
                            TxtToolTip.Text = "进度:" + TimeSpan.FromSeconds(Player.Position).ToString(@"hh\:mm\:ss");
                            ToolTip.Visibility = Visibility.Visible;
                            await Task.Delay(2000);
                            ToolTip.Visibility = Visibility.Collapsed;
                        }
                    }
                    break;
                case Windows.System.VirtualKey.Up:
                    Player.Volume += 0.1;
                    TxtToolTip.Text = "音量:" + Player.Volume.ToString("P");
                    ToolTip.Visibility = Visibility.Visible;
                    await Task.Delay(2000);
                    ToolTip.Visibility = Visibility.Collapsed;
                    break;

                case Windows.System.VirtualKey.Down:
                    Player.Volume -= 0.1;
                    if (Player.Volume == 0)
                    {
                        TxtToolTip.Text = "静音";
                    }
                    else
                    {
                        TxtToolTip.Text = "音量:" + Player.Volume.ToString("P");
                    }
                    ToolTip.Visibility = Visibility.Visible;
                    await Task.Delay(2000);
                    ToolTip.Visibility = Visibility.Collapsed;
                    break;
                case Windows.System.VirtualKey.Escape:
                    IsFullScreen = false;
                    break;
                case Windows.System.VirtualKey.F8:
                case Windows.System.VirtualKey.T:
                    //小窗播放
                    MiniWidnows(!miniWin);
                    break;
                case Windows.System.VirtualKey.F12:
                case Windows.System.VirtualKey.W:
                    IsFullWindow = !IsFullWindow;
                    break;
                case Windows.System.VirtualKey.F11:
                case Windows.System.VirtualKey.F:
                case Windows.System.VirtualKey.Enter:
                    IsFullScreen = !IsFullScreen;
                    break;
                case Windows.System.VirtualKey.F10:
                    await CaptureVideo();
                    break;
                case Windows.System.VirtualKey.O:
                case Windows.System.VirtualKey.P:
                    {
                        if (Player.PlayState == PlayState.Playing || Player.PlayState == PlayState.Pause)
                        {
                            var _position = Player.Position + 90;
                            if (_position > Player.Duration)
                            {
                                _position = Player.Duration;
                            }
                            Player.Position = _position;
                            TxtToolTip.Text = "跳过OP(快进90秒)";
                            ToolTip.Visibility = Visibility.Visible;
                            await Task.Delay(2000);
                            ToolTip.Visibility = Visibility.Collapsed;
                        }
                    }
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
                case Windows.System.VirtualKey.Z:
                case Windows.System.VirtualKey.N:
                case (Windows.System.VirtualKey)188:
                    if (EpisodeList.SelectedIndex == 0)
                    {
                        Notify.ShowMessageToast("已经是第一P了");
                    }
                    else
                    {
                        EpisodeList.SelectedIndex = EpisodeList.SelectedIndex - 1;
                    }
                    break;
                case Windows.System.VirtualKey.X:
                case Windows.System.VirtualKey.M:
                case (Windows.System.VirtualKey)190:
                    if (EpisodeList.SelectedIndex == EpisodeList.Items.Count - 1)
                    {
                        Notify.ShowMessageToast("已经是最后一P了");
                    }
                    else
                    {
                        EpisodeList.SelectedIndex = EpisodeList.SelectedIndex + 1;
                    }
                    break;
                case Windows.System.VirtualKey.F1:
                case (Windows.System.VirtualKey)186:
                    //慢速播放
                    if (BottomCBSpeed.SelectedIndex == 5)
                    {
                        Notify.ShowMessageToast("不能再慢啦");
                        return;
                    }

                    BottomCBSpeed.SelectedIndex += 1;

                    break;
                case Windows.System.VirtualKey.F2:
                case (Windows.System.VirtualKey)222:
                    //加速播放
                    if (BottomCBSpeed.SelectedIndex == 0)
                    {
                        Notify.ShowMessageToast("不能再快啦");
                        return;
                    }
                    BottomCBSpeed.SelectedIndex -= 1;
                    break;
                case Windows.System.VirtualKey.F3:
                case Windows.System.VirtualKey.V:
                    //静音
                    if (Player.Volume >= 0)
                    {
                        Player.Volume = 0;
                    }
                    else
                    {
                        Player.Volume = 1;
                    }
                    break;
                default:
                    break;
            }
        }


        private void LoadDanmuSetting()
        {

            //顶部
            DanmuSettingHideTop.IsOn = SettingService.GetValue<bool>(SettingConstants.VideoDanmaku.HIDE_TOP, false);
            if (DanmuSettingHideTop.IsOn)
            {
                DanmuControl.HideDanmaku(DanmakuLocation.Top);
            }
            DanmuSettingHideTop.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingService.SetValue<bool>(SettingConstants.VideoDanmaku.HIDE_TOP, DanmuSettingHideTop.IsOn);
                if (DanmuSettingHideTop.IsOn)
                {
                    DanmuControl.HideDanmaku(DanmakuLocation.Top);
                }
                else
                {
                    DanmuControl.ShowDanmaku(DanmakuLocation.Top);
                }
            });
            //底部
            DanmuSettingHideBottom.IsOn = SettingService.GetValue<bool>(SettingConstants.VideoDanmaku.HIDE_BOTTOM, false);
            if (DanmuSettingHideBottom.IsOn)
            {
                DanmuControl.HideDanmaku(DanmakuLocation.Bottom);
            }
            DanmuSettingHideBottom.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingService.SetValue<bool>(SettingConstants.VideoDanmaku.HIDE_BOTTOM, DanmuSettingHideBottom.IsOn);
                if (DanmuSettingHideBottom.IsOn)
                {
                    DanmuControl.HideDanmaku(DanmakuLocation.Bottom);
                }
                else
                {
                    DanmuControl.ShowDanmaku(DanmakuLocation.Bottom);
                }
            });
            //滚动
            DanmuSettingHideRoll.IsOn = SettingService.GetValue<bool>(SettingConstants.VideoDanmaku.HIDE_ROLL, false);
            if (DanmuSettingHideRoll.IsOn)
            {
                DanmuControl.HideDanmaku(DanmakuLocation.Scroll);
            }
            DanmuSettingHideRoll.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingService.SetValue<bool>(SettingConstants.VideoDanmaku.HIDE_ROLL, DanmuSettingHideRoll.IsOn);
                if (DanmuSettingHideRoll.IsOn)
                {
                    DanmuControl.HideDanmaku(DanmakuLocation.Scroll);
                }
                else
                {
                    DanmuControl.ShowDanmaku(DanmakuLocation.Scroll);
                }
            });
            //弹幕大小
            DanmuControl.DanmakuSizeZoom = SettingService.GetValue<double>(SettingConstants.VideoDanmaku.FONT_ZOOM, 1);
            DanmuSettingFontZoom.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                if (miniWin) return;
                SettingService.SetValue<double>(SettingConstants.VideoDanmaku.FONT_ZOOM, DanmuSettingFontZoom.Value);
            });
            //弹幕显示区域
            DanmuControl.DanmakuArea = SettingService.GetValue<double>(SettingConstants.VideoDanmaku.AREA, 1);
            DanmuSettingArea.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                if (miniWin) return;
                SettingService.SetValue<double>(SettingConstants.VideoDanmaku.AREA, DanmuSettingArea.Value);
            });

            //弹幕速度
            DanmuControl.DanmakuDuration = SettingService.GetValue<int>(SettingConstants.VideoDanmaku.SPEED, 10);
            DanmuSettingSpeed.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                if (miniWin) return;
                SettingService.SetValue<double>(SettingConstants.VideoDanmaku.SPEED, DanmuSettingSpeed.Value);
            });
            //弹幕顶部距离
            DanmuControl.Margin = new Thickness(0, SettingService.GetValue<int>(SettingConstants.VideoDanmaku.TOP_MARGIN, 0), 0, 0);
            DanmuTopMargin.Value = DanmuControl.Margin.Top;
            DanmuTopMargin.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                SettingService.SetValue<double>(SettingConstants.VideoDanmaku.TOP_MARGIN, DanmuTopMargin.Value);
                DanmuControl.Margin = new Thickness(0, DanmuTopMargin.Value, 0, 0);
            });
            //弹幕透明度
            DanmuControl.Opacity = SettingService.GetValue<double>(SettingConstants.VideoDanmaku.OPACITY, 1.0);
            DanmuSettingOpacity.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                SettingService.SetValue<double>(SettingConstants.VideoDanmaku.OPACITY, DanmuSettingOpacity.Value);
            });
            //弹幕最大值
            DanmuSettingMaxNum.Value = SettingService.GetValue<double>(SettingConstants.VideoDanmaku.MAX_NUM, 0);
            DanmuSettingMaxNum.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                SettingService.SetValue<double>(SettingConstants.VideoDanmaku.MAX_NUM, DanmuSettingMaxNum.Value);
            });

            //弹幕云屏蔽等级
            DanmuSettingShieldLevel.Value = SettingService.GetValue<int>(SettingConstants.VideoDanmaku.SHIELD_LEVEL, 0);
            DanmuSettingShieldLevel.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                SettingService.SetValue<int>(SettingConstants.VideoDanmaku.SHIELD_LEVEL, Convert.ToInt32(DanmuSettingShieldLevel.Value));
            });

            //弹幕加粗
            DanmuControl.DanmakuBold = SettingService.GetValue<bool>(SettingConstants.VideoDanmaku.BOLD, false);
            DanmuSettingBold.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingService.SetValue<bool>(SettingConstants.VideoDanmaku.BOLD, DanmuSettingBold.IsOn);
            });
            //弹幕样式
            DanmuControl.DanmakuStyle = (DanmakuBorderStyle)SettingService.GetValue<int>(SettingConstants.VideoDanmaku.BORDER_STYLE, 2);
            DanmuSettingStyle.SelectionChanged += new SelectionChangedEventHandler((e, args) =>
            {
                if (DanmuSettingStyle.SelectedIndex != -1)
                {
                    SettingService.SetValue<int>(SettingConstants.VideoDanmaku.BORDER_STYLE, DanmuSettingStyle.SelectedIndex);
                }
            });
            //合并弹幕
            DanmuSettingMerge.IsOn = SettingService.GetValue<bool>(SettingConstants.VideoDanmaku.MERGE, false);
            DanmuSettingMerge.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingService.SetValue<bool>(SettingConstants.VideoDanmaku.MERGE, DanmuSettingMerge.IsOn);
            });
            //半屏显示
            //DanmuControl.DanmakuArea = SettingService.GetValue<bool>(SettingConstants.VideoDanmaku.DOTNET_HIDE_SUBTITLE, false)?1:.5;
            //DanmuSettingDotHideSubtitle.Toggled += new RoutedEventHandler((e, args) =>
            //{
            //    SettingService.SetValue<bool>(SettingConstants.VideoDanmaku.DOTNET_HIDE_SUBTITLE, DanmuSettingDotHideSubtitle.IsOn);
            //});

            //弹幕开关
            DanmuControl.Visibility = SettingService.GetValue<Visibility>(SettingConstants.VideoDanmaku.SHOW, Visibility.Visible);
            DanmuSettingWords.ItemsSource = settingVM.ShieldWords;
        }
        private void LoadPlayerSetting()
        {

            //音量
            Player.Volume = SettingService.GetValue<double>(SettingConstants.Player.PLAYER_VOLUME, 1.0);
            SliderVolume.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                SettingService.SetValue<double>(SettingConstants.Player.PLAYER_VOLUME, SliderVolume.Value);
            });
            //亮度
            //_brightness = SettingService.GetValue<double>(SettingConstants.Player.PLAYER_BRIGHTNESS, 0);
            //BrightnessShield.Opacity = _brightness;

            //播放模式
            PlayerSettingMode.SelectedIndex = SettingService.GetValue<int>(SettingConstants.Player.DEFAULT_VIDEO_TYPE, 1);
            PlayerSettingMode.SelectionChanged += new SelectionChangedEventHandler((e, args) =>
            {
                SettingService.SetValue<int>(SettingConstants.Player.DEFAULT_VIDEO_TYPE, PlayerSettingMode.SelectedIndex);
            });
            //播放列表
            PlayerSettingPlayMode.SelectedIndex = SettingService.GetValue<int>(SettingConstants.Player.DEFAULT_PLAY_MODE, 0);
            PlayerSettingPlayMode.SelectionChanged += new SelectionChangedEventHandler((e, args) =>
            {
                SettingService.SetValue<int>(SettingConstants.Player.DEFAULT_PLAY_MODE, PlayerSettingPlayMode.SelectedIndex);
            });
            //使用其他网站视频
            PlayerSettingUseOtherSite.IsOn = SettingService.GetValue<bool>(SettingConstants.Player.USE_OTHER_SITEVIDEO, false);
            PlayerSettingUseOtherSite.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingService.SetValue<bool>(SettingConstants.Player.USE_OTHER_SITEVIDEO, PlayerSettingUseOtherSite.IsOn);
            });
            //自动跳转
            PlayerSettingAutoToPosition.IsOn = SettingService.GetValue<bool>(SettingConstants.Player.AUTO_TO_POSITION, true);
            PlayerSettingAutoToPosition.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingService.SetValue<bool>(SettingConstants.Player.AUTO_TO_POSITION, PlayerSettingAutoToPosition.IsOn);
            });
            //自动跳转
            PlayerSettingAutoNext.IsOn = SettingService.GetValue<bool>(SettingConstants.Player.AUTO_NEXT, true);
            PlayerSettingAutoNext.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingService.SetValue<bool>(SettingConstants.Player.AUTO_NEXT, PlayerSettingAutoNext.IsOn);
            });
            //播放比例
            PlayerSettingRatio.SelectedIndex = SettingService.GetValue<int>(SettingConstants.Player.RATIO, 0);
            Player.SetRatioMode(PlayerSettingRatio.SelectedIndex);
            PlayerSettingRatio.SelectionChanged += new SelectionChangedEventHandler((e, args) =>
            {
                SettingService.SetValue<int>(SettingConstants.Player.RATIO, PlayerSettingRatio.SelectedIndex);
                Player.SetRatioMode(PlayerSettingRatio.SelectedIndex);
            });
            // 播放倍数
            BottomCBSpeed.SelectedIndex = SettingConstants.Player.VideoSpeed.IndexOf(SettingService.GetValue<double>(SettingConstants.Player.DEFAULT_VIDEO_SPEED, 1.0d));
            Player.SetRate(SettingService.GetValue<double>(SettingConstants.Player.DEFAULT_VIDEO_SPEED, 1.0d));
            BottomCBSpeed.SelectionChanged += new SelectionChangedEventHandler((e, args) =>
            {
                SettingService.SetValue<double>(SettingConstants.Player.DEFAULT_VIDEO_SPEED, SettingConstants.Player.VideoSpeed[BottomCBSpeed.SelectedIndex]);
                Player.SetRate(SettingConstants.Player.VideoSpeed[BottomCBSpeed.SelectedIndex]);
            });

            _autoPlay = SettingService.GetValue<bool>(SettingConstants.Player.AUTO_PLAY, false);
            //A-B播放
            PlayerSettingABPlayMode.Toggled += new RoutedEventHandler((e, args) =>
            {
                if (PlayerSettingABPlayMode.IsOn)
                {
                    PlayerSettingABPlaySetPointA.Visibility = Visibility.Visible;
                }
                else
                {
                    Player.ABPlay = null;
                    VideoPlayHistoryHelper.SetABPlayHistory(CurrentPlayItem, null);
                    PlayerSettingABPlaySetPointA.Visibility = Visibility.Collapsed;
                    PlayerSettingABPlaySetPointB.Visibility = Visibility.Collapsed;
                    PlayerSettingABPlaySetPointA.Content = "设置A点";
                    PlayerSettingABPlaySetPointB.Content = "设置B点";
                }
            });
        }
        private void LoadSutitleSetting()
        {
            //字幕加粗
            SubtitleSettingBold.IsOn = SettingService.GetValue<bool>(SettingConstants.Player.SUBTITLE_BOLD, true);
            SubtitleSettingBold.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingService.SetValue<bool>(SettingConstants.Player.SUBTITLE_BOLD, SubtitleSettingBold.IsOn);
                UpdateSubtitle();
            });

            //字幕大小
            SubtitleSettingSize.Value = SettingService.GetValue<double>(SettingConstants.Player.SUBTITLE_SIZE, 40);
            SubtitleSettingSize.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                if (miniWin) return;
                SettingService.SetValue<double>(SettingConstants.Player.SUBTITLE_SIZE, SubtitleSettingSize.Value);
                UpdateSubtitle();
            });
            //字幕边框颜色
            SubtitleSettingBorderColor.SelectedIndex = SettingService.GetValue<int>(SettingConstants.Player.SUBTITLE_BORDER_COLOR, 0);
            SubtitleSettingBorderColor.SelectionChanged += new SelectionChangedEventHandler((e, args) =>
            {
                SettingService.SetValue<int>(SettingConstants.Player.SUBTITLE_BORDER_COLOR, SubtitleSettingBorderColor.SelectedIndex);
                UpdateSubtitle();
            });
            //字幕颜色
            SubtitleSettingColor.SelectedIndex = SettingService.GetValue<int>(SettingConstants.Player.SUBTITLE_COLOR, 0);
            SubtitleSettingColor.SelectionChanged += new SelectionChangedEventHandler((e, args) =>
            {
                SettingService.SetValue<int>(SettingConstants.Player.SUBTITLE_COLOR, SubtitleSettingColor.SelectedIndex);
                UpdateSubtitle();
            });

            //字幕对齐
            SubtitleSettingAlign.SelectedIndex = SettingService.GetValue<int>(SettingConstants.Player.SUBTITLE_ALIGN, 0);
            SubtitleSettingAlign.SelectionChanged += new SelectionChangedEventHandler((e, args) =>
            {
                SettingService.SetValue<int>(SettingConstants.Player.SUBTITLE_ALIGN, SubtitleSettingAlign.SelectedIndex);
                UpdateSubtitle();
            });

            //字幕透明度
            SubtitleSettingOpacity.Value = SettingService.GetValue<double>(SettingConstants.Player.SUBTITLE_OPACITY, 1.0);
            SubtitleSettingOpacity.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                SettingService.SetValue<double>(SettingConstants.Player.SUBTITLE_OPACITY, SubtitleSettingOpacity.Value);
            });
            //字幕底部距离
            SubtitleSettingBottom.Value = SettingService.GetValue<double>(SettingConstants.Player.SUBTITLE_BOTTOM, 40);
            BorderSubtitle.Margin = new Thickness(0, 0, 0, SubtitleSettingBottom.Value);
            SubtitleSettingBottom.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                BorderSubtitle.Margin = new Thickness(0, 0, 0, SubtitleSettingBottom.Value);
                SettingService.SetValue<double>(SettingConstants.Player.SUBTITLE_BOTTOM, SubtitleSettingBottom.Value);
            });
            //字幕转换
            SubtitleSettingToSimplified.IsOn = SettingService.GetValue<bool>(SettingConstants.Roaming.TO_SIMPLIFIED, true);
            SubtitleSettingToSimplified.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingService.SetValue<bool>(SettingConstants.Roaming.TO_SIMPLIFIED, SubtitleSettingToSimplified.IsOn);
                if (SubtitleSettingToSimplified.IsOn)
                {
                    currentSubtitleText = currentSubtitleText.ToSimplifiedChinese();
                    UpdateSubtitle();
                }
            });
        }

        public void InitializePlayInfo(List<PlayInfo> playInfos, int index)
        {
            //保持屏幕常亮
            dispRequest.RequestActive();

            PlayInfos = playInfos;
            EpisodeList.ItemsSource = PlayInfos;
            if (PlayInfos.Count > 1)
            {
                ShowPlaylistButton = true;
            }
            else if (PlayInfos.Count == 1 && PlayInfos[0].is_interaction)
            {
                ShowPlaylistButton = true;
            }
            else
            {
                ShowPlaylistButton = false;
            }
            EpisodeList.SelectedIndex = index;


        }

        private async void DanmuTimer_Tick(object sender, object e)
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
                    //FadeOut.Begin();
                    //control.Visibility = Visibility.Collapsed;

                }
                else
                {
                    showControlsFlag++;
                }
            }
            var p = Convert.ToInt32(Player.Position);
            var segIndex = Convert.ToInt32(Math.Ceiling(Player.Position / (60 * 6d)));
            if (segIndex <= 0) segIndex = 1;
            if (danmakuLoadedSegment != null && !danmakuLoadedSegment.Contains(segIndex))
            {
                await LoadDanmaku(segIndex);
            }
            if (Buffering)
            {
                return;
            }
            if (Player.PlayState != PlayState.Playing || GridBuffering.Visibility == Visibility.Visible)
            {
                return;
            }

            if (DanmuControl.Visibility == Visibility.Collapsed)
            {
                return;
            }
            var needDistinct = DanmuSettingMerge.IsOn;
            var level = DanmuSettingShieldLevel.Value;
            var max = Convert.ToInt32(DanmuSettingMaxNum.Value);
            await Task.Run(async () =>
            {
                try
                {
                    if (danmakuPool != null && danmakuPool.ContainsKey(p))
                    {
                        var data = danmakuPool[p].Where(x => true);
                        //云屏蔽
                        data = data.Where(x => x.weight >= level);
                        //去重
                        if (needDistinct)
                        {
                            data = data.Distinct(new CompareDanmakuModel());
                        }
                        //关键词
                        foreach (var item in settingVM.ShieldWords)
                        {
                            data = data.Where(x => !x.text.Contains(item));
                        }
                        //用户
                        foreach (var item in settingVM.ShieldUsers)
                        {
                            data = data.Where(x => !x.sendID.Equals(item));
                        }
                        //正则
                        foreach (var item in settingVM.ShieldRegulars)
                        {
                            data = data.Where(x => !Regex.IsMatch(x.text, item));
                        }
                        if (max > 0)
                        {
                            data = data.Take(max);
                        }
                        //加载弹幕
                        foreach (var item in data)
                        {
                            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
                            {
                                DanmuControl.AddDanmu(item, false);
                            });
                        }
                        data = null;
                    }
                }
                catch (Exception)
                {
                }
            });
            if (Player.PlayState == PlayState.Pause)
            {
                DanmuControl.PauseDanmaku();
            }
        }

        private async Task SetPlayItem(int index)
        {
            if (PlayInfos == null || PlayInfos.Count == 0)
            {
                return;
            }
            //清空字幕
            subtitles = null;
            subtitleTimer?.Stop();
            subtitleTimer = null;
            Player.ClosePlay();
            if (index >= PlayInfos.Count)
            {
                index = PlayInfos.Count - 1;
            }

            CurrentPlayIndex = index;
            CurrentPlayItem = PlayInfos[index];
            if (CurrentPlayItem.is_interaction)
            {
                ShowPlaylistButton = false;
                ShowPlayNodeButton = true;
            }
            //设置标题
            TopTitle.Text = CurrentPlayItem.title;
            if (_systemMediaTransportControls != null)
            {
                SystemMediaTransportControlsDisplayUpdater updater = _systemMediaTransportControls.DisplayUpdater;
                updater.Type = MediaPlaybackType.Video;
                updater.VideoProperties.Title = CurrentPlayItem.title;
                updater.Update();
            }

            //设置下一集按钮的显示
            if (PlayInfos.Count >= 1 && index != PlayInfos.Count - 1)
            {
                BottomBtnNext.Visibility = Visibility.Visible;
            }
            else
            {
                BottomBtnNext.Visibility = Visibility.Collapsed;
            }
            ChangeEpisodeEvent?.Invoke(this, index);

            playUrlInfo = null;
            //if (CurrentPlayItem.play_mode == VideoPlayType.Season)
            //{
            //   // Player._ffmpegConfig.FFmpegOptions["referer"] = "https://www.bilibili.com/bangumi/play/ep" + CurrentPlayItem.ep_id;
            //}
            if (SettingService.GetValue<bool>(SettingConstants.Player.AUTO_TO_POSITION, true))
            {
                _postion = SettingService.GetValue<double>(CurrentPlayItem.season_id != 0 ? "ep" + CurrentPlayItem.ep_id : CurrentPlayItem.cid, 0);
                //减去两秒防止视频直接结束了
                if (_postion >= 2) _postion -= 2;
            }
            else
            {
                _postion = 0;
            }
            await playerHelper.ReportHistory(CurrentPlayItem, 0);
            await SetDanmaku();

            if (!await CheckDownloaded())
            {
                var info = await GetPlayUrlQualitesInfo();
                if (!info.Success)
                {
                    ShowDialog($"请求信息:\r\n{info.Message}", "读取视频播放地址失败");
                }
                else
                {
                    playUrlInfo = info;
                    SetSoundQuality();
                    SetQuality();
                }
            }

            await GetPlayerInfo();

            Player.ABPlay = VideoPlayHistoryHelper.FindABPlayHistory(CurrentPlayItem);
            if (Player.ABPlay == null)
            {
                PlayerSettingABPlayMode.IsOn = false;
            }
            else
            {
                PlayerSettingABPlayMode.IsOn = true;
                PlayerSettingABPlaySetPointA.Visibility = Visibility.Visible;
                PlayerSettingABPlaySetPointB.Visibility = Visibility.Visible;
                PlayerSettingABPlaySetPointA.Content = "A: " + TimeSpan.FromSeconds(Player.ABPlay.PointA).ToString(@"hh\:mm\:ss\.fff");
                if (Player.ABPlay.PointB != double.MaxValue)
                    PlayerSettingABPlaySetPointB.Content = "B: " + TimeSpan.FromSeconds(Player.ABPlay.PointB).ToString(@"hh\:mm\:ss\.fff");
            }
        }


        /// <summary>
        /// 字幕文件
        /// </summary>
        SubtitleModel subtitles;
        /// <summary>
        /// 字幕Timer
        /// </summary>
        DispatcherTimer subtitleTimer;
        /// <summary>
        /// 当前显示的字幕文本
        /// </summary>
        string currentSubtitleText = "";
        /// <summary>
        /// 选择字幕
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Menuitem_Click(object sender, RoutedEventArgs e)
        {

            foreach (ToggleMenuFlyoutItem item in (BottomBtnSelctSubtitle.Flyout as MenuFlyout).Items)
            {
                item.IsChecked = false;
            }
            var menuitem = (sender as ToggleMenuFlyoutItem);
            CurrentSubtitleName = menuitem.Text;
            if (menuitem.Text == "无")
            {
                ClearSubTitle();
            }
            else
            {
                SetSubTitle(menuitem.Tag.ToString());
            }
            menuitem.IsChecked = true;
        }
        /// <summary>
        /// 设置字幕文件
        /// </summary>
        /// <param name="url"></param>
        private async void SetSubTitle(string url)
        {
            try
            {
                subtitles = await playerHelper.GetSubtitle(url);
                if (subtitles != null)
                {
                    //转为简体
                    if (SettingService.GetValue<bool>(SettingConstants.Roaming.TO_SIMPLIFIED, true) && CurrentSubtitleName == "中文（繁体）")
                    {
                        foreach (var item in subtitles.body)
                        {
                            item.content = item.content.ToSimplifiedChinese();
                        }
                    }
                    subtitleTimer = new DispatcherTimer();
                    subtitleTimer.Interval = TimeSpan.FromMilliseconds(100);
                    subtitleTimer.Tick += SubtitleTimer_Tick;
                    subtitleTimer.Start();
                }
            }
            catch (Exception)
            {
                Notify.ShowMessageToast("加载字幕失败了");
            }


        }

        private async void SubtitleTimer_Tick(object sender, object e)
        {
            if (Player.PlayState != PlayState.Playing) return;
            if (subtitles == null)
            {
                return;
            }
            var time = Player.Position;
            if (subtitles.body == null) return;
            var first = subtitles.body.FirstOrDefault(x => x.from <= time && x.to >= time);
            if (first != null)
            {
                if (first.content == currentSubtitleText) return;
                BorderSubtitle.Visibility = Visibility.Visible;
                BorderSubtitle.Child = await GenerateSubtitleItem(first.content);
                currentSubtitleText = first.content;
            }
            else
            {
                BorderSubtitle.Visibility = Visibility.Collapsed;
                currentSubtitleText = "";
            }
        }

        private async void UpdateSubtitle()
        {
            if (BorderSubtitle.Visibility == Visibility.Visible && currentSubtitleText != "")
            {
                BorderSubtitle.Child = await GenerateSubtitleItem(currentSubtitleText);
            }


        }

        private async Task<Grid> GenerateSubtitleItem(string text)
        {
            //行首行尾加空格，防止字体描边超出
            text = " " + text.Replace("\n", " \n ") + " ";

            var fontSize = (float)SubtitleSettingSize.Value;
            var color = (SubtitleSettingColor.SelectedItem as ComboBoxItem).Tag.ToString().StrToColor();
            var borderColor = (SubtitleSettingBorderColor.SelectedItem as ComboBoxItem).Tag.ToString().StrToColor();

            CanvasHorizontalAlignment canvasHorizontalAlignment = CanvasHorizontalAlignment.Center;
            TextAlignment textAlignment = TextAlignment.Center;
            if (SubtitleSettingAlign.SelectedIndex == 1)
            {
                canvasHorizontalAlignment = CanvasHorizontalAlignment.Left;
                textAlignment = TextAlignment.Left;
            }
            else if (SubtitleSettingAlign.SelectedIndex == 2)
            {
                canvasHorizontalAlignment = CanvasHorizontalAlignment.Right;
                textAlignment = TextAlignment.Right;
            }
            CanvasDevice device = CanvasDevice.GetSharedDevice();
            CanvasTextFormat fmt = new CanvasTextFormat() { FontSize = fontSize, HorizontalAlignment = canvasHorizontalAlignment, };
            var tb = new TextBlock { Text = text, FontSize = fontSize, TextAlignment = textAlignment };
            if (SubtitleSettingBold.IsOn)
            {
                fmt.FontWeight = FontWeights.Bold;
                tb.FontWeight = FontWeights.Bold;
            }

            tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            var myBitmap = new CanvasRenderTarget(device, (float)tb.DesiredSize.Width + 4, (float)tb.DesiredSize.Height, displayInformation.LogicalDpi);

            CanvasTextLayout canvasTextLayout = new CanvasTextLayout(device, text, fmt, (float)tb.DesiredSize.Width + 4, (float)tb.DesiredSize.Height);

            CanvasGeometry combinedGeometry = CanvasGeometry.CreateText(canvasTextLayout);

            using (var ds = myBitmap.CreateDrawingSession())
            {
                ds.Clear(Colors.Transparent);
                ds.DrawGeometry(combinedGeometry, borderColor, 4f, new CanvasStrokeStyle()
                {
                    DashStyle = CanvasDashStyle.Solid
                });
                ds.FillGeometry(combinedGeometry, color);
            }
            Image image = new Image();
            BitmapImage im = new BitmapImage();
            using (InMemoryRandomAccessStream oStream = new InMemoryRandomAccessStream())
            {
                await myBitmap.SaveAsync(oStream, CanvasBitmapFileFormat.Png, 1.0f);
                await im.SetSourceAsync(oStream);
            }
            image.Width = tb.DesiredSize.Width;
            image.Source = im;
            image.Stretch = Stretch.Uniform;
            Grid grid = new Grid();

            grid.Tag = text;
            grid.Children.Add(image);

            return grid;
        }


        /// <summary>
        /// 清除字幕
        /// </summary>
        private void ClearSubTitle()
        {
            if (subtitles != null)
            {
                if (subtitleTimer != null)
                {
                    subtitleTimer.Stop();
                    subtitleTimer = null;
                }
                BorderSubtitle.Visibility = Visibility.Collapsed;
                subtitles = null;
            }
        }



        public void ChangePlayIndex(int index)
        {
            ClearSubTitle();
            DanmuControl.ClearAll();
            EpisodeList.SelectedIndex = index;
        }
        private async void EpisodeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EpisodeList.SelectedItem == null)
            {
                return;
            }
            DanmuControl.ClearAll();
            await SetPlayItem(EpisodeList.SelectedIndex);
        }
        private async void NodeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NodeList.SelectedItem == null || interactionVideoVM.Loading)
            {
                return;
            }
            var item = NodeList.SelectedItem as InteractionEdgeInfoStoryListModel;
            await ChangedNode(item.edge_id, item.cid.ToString());
        }
        private async void SelectChoice_ItemClick(object sender, ItemClickEventArgs e)
        {
            var choice = e.ClickedItem as InteractionEdgeInfoChoiceModel;
            await ChangedNode(choice.id, choice.cid.ToString());

        }
        private async Task ChangedNode(int node_id, string cid)
        {
            InteractionChoices.Visibility = Visibility.Collapsed;
            CurrentPlayItem.cid = cid;

            await interactionVideoVM.GetNodes(node_id);

            TopTitle.Text = interactionVideoVM.Select.title;
            //if ((interactionVideoVM.Info.edges?.questions?.Count ?? 0) <= 0)
            //{
            //    Notify.ShowMessageToast("播放完毕，请点击右下角节点，重新开始");
            //    return;
            //}
            _postion = 0;
            _autoPlay = true;
            DanmuControl.ClearAll();
            await SetDanmaku();


            if (!await CheckDownloaded())
            {
                var info = await GetPlayUrlQualitesInfo();
                if (!info.Success)
                {
                    ShowDialog($"请求信息:\r\n{info.Message}", "读取视频播放地址失败");
                }
                else
                {
                    playUrlInfo = info;
                    SetSoundQuality();
                    SetQuality();
                }
            }
        }


        double _postion = 0;
        bool _autoPlay = false;
        private async Task SetDanmaku(bool update = false)
        {
            try
            {

                if (CurrentPlayItem.play_mode == VideoPlayType.Download && !update)
                {
                    var danmakuFile = await StorageFile.GetFileFromPathAsync(CurrentPlayItem.LocalPlayInfo.DanmakuPath);
                    var danmuList = danmakuParse.ParseBiliBili(await FileIO.ReadTextAsync(danmakuFile));
                    danmakuPool = danmuList.GroupBy(x => x.time_s).ToDictionary(x => x.Key, x => x.ToList());
                    TxtDanmuCount.Text = danmuList.Count.ToString();
                    danmuList.Clear();
                    danmuList = null;
                }
                else
                {
                    var segIndex = Math.Ceiling(Player.Position / (60 * 6d));
                    if (update)
                    {
                        await LoadDanmaku(segIndex.ToInt32());
                        Notify.ShowMessageToast($"已更新弹幕");
                    }
                    await LoadDanmaku(1);
                    //var danmuList = (await danmakuParse.ParseBiliBili(Convert.ToInt64(CurrentPlayItem.cid)));
                    ////await playerHelper.GetDanmaku(CurrentPlayItem.cid, 1) ;
                    //danmakuPool = danmuList.GroupBy(x=>x.time_s).ToDictionary(x => x.Key, x => x.ToList());
                    //TxtDanmuCount.Text = danmuList.Count.ToString();

                    //danmuList.Clear();
                    //danmuList = null;
                }


            }
            catch (Exception)
            {
                Notify.ShowMessageToast("弹幕加载失败");
            }
        }
        bool loadingDanmaku = false;
        private async Task LoadDanmaku(int segmentIndex)
        {

            try
            {
                if (loadingDanmaku) return;
                loadingDanmaku = true;
                if (segmentIndex <= 1)
                {
                    danmakuPool.Clear();
                    segmentIndex = 1;
                }
                var danmuList = await playerHelper.GetDanmaku(CurrentPlayItem.cid, segmentIndex);
                foreach (var item in danmuList.GroupBy(x => x.time_s).ToDictionary(x => x.Key, x => x.ToList()))
                {
                    if (danmakuPool.ContainsKey(item.Key))
                    {
                        danmakuPool[item.Key] = item.Value;
                    }
                    else
                    {
                        danmakuPool.Add(item.Key, item.Value);
                    }
                }
                TxtDanmuCount.Text = danmuList.Count.ToString();
                danmuList.Clear();
                if (segmentIndex == 1)
                {
                    danmakuLoadedSegment = new List<int>() { 1 };
                }
                else
                {
                    danmakuLoadedSegment.Add(segmentIndex);
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                loadingDanmaku = false;
            }

        }

        private async Task<bool> CheckDownloaded()
        {
            if (CurrentPlayItem.play_mode != VideoPlayType.Download)
            {
                return false;
            }
            BottomCBQuality.Visibility = Visibility.Collapsed;

            await PlayLocalFile();
            return true;
        }

        private async Task<BiliPlayUrlQualitesInfo> GetPlayUrlQualitesInfo()
        {
            VideoLoading.Visibility = Visibility.Visible;
            if (playUrlInfo != null && playUrlInfo.CurrentQuality != null)
            {
                playUrlInfo.CurrentQuality = null;
            }

            var qn = SettingService.GetValue<int>(SettingConstants.Player.DEFAULT_QUALITY, 80);
            var soundQualityId = SettingService.GetValue<int>(SettingConstants.Player.DEFAULT_SOUND_QUALITY, 0);
            var info = await playerHelper.GetPlayUrls(CurrentPlayItem, qn, soundQualityId);
            return info;
        }

        private void SetSoundQuality()
        {
            BottomSoundQuality.ItemsSource = playUrlInfo.AudioQualites;
            BottomSoundQuality.SelectionChanged -= BottomSoundQuality_SelectionChanged;
            BottomSoundQuality.SelectedItem = playUrlInfo.CurrentAudioQuality;
            BottomSoundQuality.SelectionChanged += BottomSoundQuality_SelectionChanged;
            ChangeQuality(current_quality_info, playUrlInfo.CurrentAudioQuality).RunWithoutAwait();
        }

        private void SetQuality()
        {
            BottomCBQuality.ItemsSource = playUrlInfo.Qualites;
            BottomCBQuality.SelectionChanged -= BottomCBQuality_SelectionChanged;
            BottomCBQuality.SelectedItem = playUrlInfo.CurrentQuality;
            //SettingService.SetValue<int>(SettingConstants.Player.DEFAULT_QUALITY, info.data.current.quality);
            BottomCBQuality.SelectionChanged += BottomCBQuality_SelectionChanged;
            ChangeQuality(playUrlInfo.CurrentQuality, playUrlInfo.CurrentAudioQuality).RunWithoutAwait();
        }

        private async Task PlayLocalFile()
        {
            VideoLoading.Visibility = Visibility.Visible;
            PlayerOpenResult result = new PlayerOpenResult()
            {
                result = false
            };
            var info = CurrentPlayItem.LocalPlayInfo.Info;
            if (info.PlayUrlType == BiliPlayUrlType.DASH)
            {
                result = await Player.PlayDashUseFFmpegInterop(info.DashInfo, "", "", positon: _postion, isLocal: true);
            }
            else if (CurrentPlayItem.LocalPlayInfo.Info.PlayUrlType == BiliPlayUrlType.SingleFLV)
            {
                result = await Player.PlayerSingleMp4UseNativeAsync(info.FlvInfo.First().Url, positon: _postion, isLocal: true);
            }
            else if (CurrentPlayItem.LocalPlayInfo.Info.PlayUrlType == BiliPlayUrlType.MultiFLV)
            {
                //TODO 本地播放
            }
            if (result.result)
            {
                VideoLoading.Visibility = Visibility.Collapsed;
                Player.Play();
            }
            else
            {
                ShowErrorDialog(result.message + "[LocalFile]");
            }
        }

        private async Task GetPlayerInfo()
        {
            TopOnline.Text = "";
            var autoAISubtitle = SettingService.GetValue<bool>(SettingConstants.Player.AUTO_OPEN_AI_SUBTITLE, false);
            if (CurrentPlayItem.play_mode == VideoPlayType.Download)
            {
                if (CurrentPlayItem.LocalPlayInfo.Subtitles != null && CurrentPlayItem.LocalPlayInfo.Subtitles.Count > 0)
                {

                    var menu = new MenuFlyout();
                    foreach (var item in CurrentPlayItem.LocalPlayInfo.Subtitles)
                    {
                        ToggleMenuFlyoutItem menuitem = new ToggleMenuFlyoutItem() { Text = item.Key, Tag = item.Value };
                        menuitem.Click += Menuitem_Click;
                        menu.Items.Add(menuitem);
                    }
                    ToggleMenuFlyoutItem noneItem = new ToggleMenuFlyoutItem() { Text = "无" };
                    noneItem.Click += Menuitem_Click;
                    menu.Items.Add(noneItem);
                    var firstMenuItem = (menu.Items[0] as ToggleMenuFlyoutItem);
                    if ((firstMenuItem.Text.Contains("自动") || firstMenuItem.Text.Contains("AI")) && !autoAISubtitle)
                    {
                        noneItem.IsChecked = true;
                        CurrentSubtitleName = noneItem.Text;
                    }
                    else
                    {
                        firstMenuItem.IsChecked = true;
                        CurrentSubtitleName = firstMenuItem.Text;
                        SetSubTitle(firstMenuItem.Tag.ToString());
                    }
                    BottomBtnSelctSubtitle.Flyout = menu;
                    BottomBtnSelctSubtitle.Visibility = Visibility.Visible;
                    BorderSubtitle.Visibility = Visibility.Collapsed;
                }
                else
                {
                    var menu = new MenuFlyout();
                    menu.Items.Add(new ToggleMenuFlyoutItem() { Text = "无", IsChecked = true });
                    CurrentSubtitleName = "无";
                    BottomBtnSelctSubtitle.Flyout = menu;
                    BottomBtnSelctSubtitle.Visibility = Visibility.Collapsed;
                    BorderSubtitle.Visibility = Visibility.Collapsed;
                }
                return;
            }
            var player_info = await playerHelper.GetPlayInfo(CurrentPlayItem.avid, CurrentPlayItem.cid);
            if (player_info.subtitle != null && player_info.subtitle.subtitles != null && player_info.subtitle.subtitles.Count != 0)
            {
                var menu = new MenuFlyout();
                foreach (var item in player_info.subtitle.subtitles)
                {
                    ToggleMenuFlyoutItem menuitem = new ToggleMenuFlyoutItem() { Text = item.lan_doc, Tag = item.subtitle_url };
                    menuitem.Click += Menuitem_Click;
                    menu.Items.Add(menuitem);
                }
                ToggleMenuFlyoutItem noneItem = new ToggleMenuFlyoutItem() { Text = "无" };
                noneItem.Click += Menuitem_Click;
                menu.Items.Add(noneItem);
                var firstMenuItem = (menu.Items[0] as ToggleMenuFlyoutItem);
                if ((firstMenuItem.Text.Contains("自动") || firstMenuItem.Text.Contains("AI")) && !autoAISubtitle)
                {
                    noneItem.IsChecked = true;
                    CurrentSubtitleName = noneItem.Text;
                }
                else
                {
                    firstMenuItem.IsChecked = true;
                    CurrentSubtitleName = firstMenuItem.Text;
                    SetSubTitle(firstMenuItem.Tag.ToString());
                }

                BottomBtnSelctSubtitle.Flyout = menu;
                BottomBtnSelctSubtitle.Visibility = Visibility.Visible;
                BorderSubtitle.Visibility = Visibility.Collapsed;
            }
            else
            {
                var menu = new MenuFlyout();
                menu.Items.Add(new ToggleMenuFlyoutItem() { Text = "无", IsChecked = true });
                CurrentSubtitleName = "无";
                BottomBtnSelctSubtitle.Flyout = menu;
                BottomBtnSelctSubtitle.Visibility = Visibility.Collapsed;
                BorderSubtitle.Visibility = Visibility.Collapsed;
            }

            if (player_info.interaction != null)
            {
                //设置互动视频
                if (interactionVideoVM == null)
                {
                    interactionVideoVM = new InteractionVideoVM(CurrentPlayItem.avid, player_info.interaction.graph_version);
                    NodeList.DataContext = interactionVideoVM;
                    InteractionChoices.DataContext = interactionVideoVM;
                    ShowPlaylistButton = false;
                    await interactionVideoVM.GetNodes();
                    TopTitle.Text = interactionVideoVM.Select.title;
                }
            }

            TopOnline.Text = await playerHelper.GetOnline(CurrentPlayItem.avid, CurrentPlayItem.cid);

        }

        BiliPlayUrlInfo current_quality_info = null;
        BiliDashAudioPlayUrlInfo current_audio_quality_info = null;

        private async Task<bool> ChangeQualityGetPlayUrls(BiliPlayUrlInfo quality, BiliDashAudioPlayUrlInfo soundQuality = null)
        {
            if (quality.HasPlayUrl)
            {
                return true;
            }
            var soundQualityId = soundQuality?.QualityID;
            if (soundQualityId == null)
            {
                soundQualityId = 0;
            }
            var info = await playerHelper.GetPlayUrls(CurrentPlayItem, quality.QualityID, soundQualityId.Value);
            if (!info.Success)
            {
                ShowDialog(info.Message, "切换清晰度失败");
                return false;
            }
            if (!info.CurrentQuality.HasPlayUrl)
            {
                ShowDialog("无法读取到播放地址，试试换个清晰度?", "播放失败");
                return false;
            }
            quality = info.CurrentQuality;
            return true;
        }

        private async Task<PlayerOpenResult> ChangeQualityPlayVideo(BiliPlayUrlInfo quality, BiliDashAudioPlayUrlInfo audioQuality)
        {
            PlayerOpenResult result = new PlayerOpenResult()
            {
                result = false
            };
            if (quality.PlayUrlType == BiliPlayUrlType.DASH)
            {
                var audio = audioQuality == null ? quality.DashInfo.Audio : audioQuality.Audio;
                var video = quality.DashInfo.Video;

                result = await Player.PlayerDashUseNative(quality.DashInfo, quality.UserAgent, quality.Referer, positon: _postion);

                if (!result.result)
                {
                    var mpd_url = new PlayerAPI().GenerateMPD(new Models.GenerateMPDModel()
                    {
                        AudioBandwidth = audio.BandWidth.ToString(),
                        AudioCodec = audio.Codecs,
                        AudioID = audio.ID.ToString(),
                        AudioUrl = audio.Url,
                        Duration = quality.DashInfo.Duration,
                        DurationMS = quality.Timelength,
                        VideoBandwidth = video.BandWidth.ToString(),
                        VideoCodec = video.Codecs,
                        VideoID = video.ID.ToString(),
                        VideoFrameRate = video.FrameRate.ToString(),
                        VideoHeight = video.Height,
                        VideoWidth = video.Width,
                        VideoUrl = video.Url,
                    });
                    result = await Player.PlayDashUrlUseFFmpegInterop(mpd_url, quality.UserAgent, quality.Referer, positon: _postion);
                }
            }
            else if (quality.PlayUrlType == BiliPlayUrlType.SingleFLV)
            {
                result = await Player.PlaySingleFlvUseSYEngine(quality.FlvInfo.First().Url, quality.UserAgent, quality.Referer, positon: _postion, epId: CurrentPlayItem.ep_id);
                if (!result.result)
                {
                    result = await Player.PlaySingleFlvUseFFmpegInterop(quality.FlvInfo.First().Url, quality.UserAgent, quality.Referer, positon: _postion);
                }
            }
            else if (quality.PlayUrlType == BiliPlayUrlType.MultiFLV)
            {
                result = await Player.PlayVideoUseSYEngine(quality.FlvInfo, quality.UserAgent, quality.Referer, positon: _postion, epId: CurrentPlayItem.ep_id);
            }
            return result;
        }

        private async Task ChangeQuality(BiliPlayUrlInfo quality, BiliDashAudioPlayUrlInfo soundQuality = null)
        {
            VideoLoading.Visibility = Visibility.Visible;
            if (quality == null)
            {
                return;
            }
            quality.DashInfo.Audio = soundQuality.Audio;
            current_quality_info = quality;
            current_audio_quality_info = soundQuality;
            if (!await ChangeQualityGetPlayUrls(quality, soundQuality))
            {
                return;
            }
            var result = await ChangeQualityPlayVideo(quality, soundQuality);
            if (result.result)
            {
                VideoLoading.Visibility = Visibility.Collapsed;
            }
            else
            {
                ShowErrorDialog(result.message + "[ChangeQuality]");
            }
        }
        private void ShowErrorDialog(string message)
        {
            ShowDialog($@"播放失败:{message}
你可以进行以下尝试:
1、切换视频清晰度
2、到⌈设置⌋-⌈播放⌋中修改⌈优先视频编码⌋选项
3、到⌈设置⌋-⌈播放⌋中打开或关闭⌈替换PCDN链接⌋选项
4、到⌈设置⌋-⌈代理⌋中打开或关闭⌈尝试替换视频的CDN⌋选项
5、如果视频编码选择了HEVC，请检查是否安装了HEVC扩展
6、如果视频编码选择了AV1，请检查是否安装了AV1扩展
7、如果是付费视频，请在手机或网页端购买后观看
8、尝试更新您的显卡驱动或使用核显打开应用", "播放失败");
        }
        private async void ShowDialog(string content, string title)
        {
            MessageDialog dislog = new MessageDialog(content, title);
            await dislog.ShowAsync();
        }

        #region 全屏处理
        public void FullScreen(bool fullScreen)
        {

            ApplicationView view = ApplicationView.GetForCurrentView();
            FullScreenEvent?.Invoke(this, fullScreen);
            if (fullScreen)
            {
                BottomBtnExitFull.Visibility = Visibility.Visible;
                BottomBtnFull.Visibility = Visibility.Collapsed;
                BottomBtnFullWindows.Visibility = Visibility.Collapsed;
                BottomBtnExitFullWindows.Visibility = Visibility.Collapsed;
                //全屏
                if (!view.IsFullScreenMode)
                {
                    view.TryEnterFullScreenMode();
                }
            }
            else
            {
                BottomBtnExitFull.Visibility = Visibility.Collapsed;
                BottomBtnFull.Visibility = Visibility.Visible;
                if (IsFullWindow)
                {
                    FullWidnow(true);
                    BottomBtnFullWindows.Visibility = Visibility.Collapsed;
                    BottomBtnExitFullWindows.Visibility = Visibility.Visible;
                }
                else
                {
                    BottomBtnFullWindows.Visibility = Visibility.Visible;
                    BottomBtnExitFullWindows.Visibility = Visibility.Collapsed;
                }
                //退出全屏
                if (view.IsFullScreenMode)
                {
                    view.ExitFullScreenMode();
                }
            }
            BtnFoucs.Focus(FocusState.Programmatic);
        }
        public void FullWidnow(bool fullWindow)
        {

            if (fullWindow)
            {
                BottomBtnFullWindows.Visibility = Visibility.Collapsed;
                BottomBtnExitFullWindows.Visibility = Visibility.Visible;
            }
            else
            {
                BottomBtnFullWindows.Visibility = Visibility.Visible;
                BottomBtnExitFullWindows.Visibility = Visibility.Collapsed;
            }
            FullWindowEvent?.Invoke(this, fullWindow);
            this.Focus(FocusState.Programmatic);
        }
        private void BottomBtnExitFull_Click(object sender, RoutedEventArgs e)
        {
            IsFullScreen = false;
        }

        private void BottomBtnFull_Click(object sender, RoutedEventArgs e)
        {
            IsFullScreen = true;
        }

        private void BottomBtnExitFullWindows_Click(object sender, RoutedEventArgs e)
        {
            IsFullWindow = false;
        }

        private void BottomBtnFullWindows_Click(object sender, RoutedEventArgs e)
        {
            IsFullWindow = true;
        }
        public bool IsFullScreen
        {
            get { return (bool)GetValue(IsFullScreenProperty); }
            set { SetValue(IsFullScreenProperty, value); }
        }
        public static readonly DependencyProperty IsFullScreenProperty =
            DependencyProperty.Register("IsFullScreen", typeof(bool), typeof(PlayerControl), new PropertyMetadata(false, OnIsFullScreenChanged));
        private static void OnIsFullScreenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = d as PlayerControl;
            sender.FullScreen((bool)e.NewValue);
        }
        public bool IsFullWindow
        {
            get { return (bool)GetValue(IsFullWindowProperty); }
            set { SetValue(IsFullWindowProperty, value); }
        }
        public static readonly DependencyProperty IsFullWindowProperty =
            DependencyProperty.Register("IsFullWindow", typeof(bool), typeof(PlayerControl), new PropertyMetadata(false, OnIsFullWidnowChanged));
        private static void OnIsFullWidnowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = d as PlayerControl;
            sender.FullWidnow((bool)e.NewValue);
        }

        #endregion


        public bool ShowPlaylistButton
        {
            get { return (bool)GetValue(ShowPlaylistButtonProperty); }
            set { SetValue(ShowPlaylistButtonProperty, value); }
        }
        public static readonly DependencyProperty ShowPlaylistButtonProperty =
            DependencyProperty.Register("ShowPlaylistButton", typeof(bool), typeof(PlayerControl), new PropertyMetadata(false));


        public bool ShowPlayNodeButton
        {
            get { return (bool)GetValue(ShowPlayNodeButtonProperty); }
            set { SetValue(ShowPlayNodeButtonProperty, value); }
        }
        public static readonly DependencyProperty ShowPlayNodeButtonProperty =
            DependencyProperty.Register("ShowPlayNodeButton", typeof(bool), typeof(PlayerControl), new PropertyMetadata(false));


        private bool _buffering = false;
        public bool Buffering
        {
            get { return _buffering; }
            set { _buffering = value; DoPropertyChanged("_Buffering"); }
        }
        private double _BufferingProgress;
        public double BufferingProgress
        {
            get { return _BufferingProgress; }
            set { _BufferingProgress = value; DoPropertyChanged("BufferingProgress"); }
        }


        private void TopBtnOpenDanmaku_Click(object sender, RoutedEventArgs e)
        {
            DanmuControl.Visibility = Visibility.Visible;
            SettingService.SetValue<Visibility>(SettingConstants.VideoDanmaku.SHOW, DanmuControl.Visibility);
        }

        private void TopBtnCloseDanmaku_Click(object sender, RoutedEventArgs e)
        {
            DanmuControl.Visibility = Visibility.Collapsed;
            SettingService.SetValue<Visibility>(SettingConstants.VideoDanmaku.SHOW, DanmuControl.Visibility);
        }
        #region 播放器手势
        int showControlsFlag = 0;
        bool HandlingGesture = false;
        bool HandlingHolding = false;
        bool DirectionX = false;
        bool DirectionY = false;

        bool tapFlag;
        double ssValue = 0;
        bool ManipulatingBrightness = false;
        double _brightness = 0;
        PlayerHoldingAction m_playerHoldingAction;
        double Brightness
        {
            get => _brightness;
            set
            {
                _brightness = value;
                BrightnessShield.Opacity = value;
                //SettingHelper.SetValue<double>(SettingHelper.Player.PLAYER_BRIGHTNESS, _brightness);
                //}
            }
        }
        private void InitializeGesture()
        {
            m_playerHoldingAction = (PlayerHoldingAction)SettingService.GetValue(SettingConstants.Player.HOLDING_GESTURE_ACTION, (int)PlayerHoldingAction.None);
            gestureRecognizer.GestureSettings = GestureSettings.Hold | GestureSettings.HoldWithMouse | GestureSettings.ManipulationTranslateX | GestureSettings.ManipulationTranslateY;

            gestureRecognizer.Holding += OnHolding;
            gestureRecognizer.ManipulationStarted += OnManipulationStarted;
            gestureRecognizer.ManipulationUpdated += OnManipulationUpdated;
            gestureRecognizer.ManipulationCompleted += OnManipulationCompleted;
        }

        private void OnHolding(GestureRecognizer sender, HoldingEventArgs args)
        {
            if (Player.PlayState != PlayState.Playing || m_playerHoldingAction == PlayerHoldingAction.None)
                return;

            switch (args.HoldingState)
            {
                case HoldingState.Started:
                    {
                        StartHolding();
                        break;
                    }
                case HoldingState.Completed:
                    {
                        StopHolding();
                        break;
                    }
                case HoldingState.Canceled:
                    {
                        var canCancel = SettingService.GetValue(SettingConstants.Player.HOLDING_GESTURE_CAN_CANCEL, true);
                        if (!canCancel) break;
                        StopHolding();
                        break;
                    }
            }
        }

        private void StartHolding()
        {
            HandlingHolding = true;
            StartHighRateSpeedPlay();
        }

        private void StopHolding()
        {
            HandlingHolding = false;
            StopHighRateSpeedPlay();
        }

        private void StartHighRateSpeedPlay()
        {
            TxtToolTip.Text = "倍速播放中";
            ToolTip.Visibility = Visibility.Visible;
            var highRatePlaySpeed = SettingService.GetValue(SettingConstants.Player.HIGH_RATE_PLAY_SPEED, 2.0d);
            Player.SetRate(highRatePlaySpeed);
        }

        private void StopHighRateSpeedPlay()
        {
            ToolTip.Visibility = Visibility.Collapsed;
            Player.SetRate(SettingService.GetValue<double>(SettingConstants.Player.DEFAULT_VIDEO_SPEED, 1.0d));
        }

        private void OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            ssValue = 0;
            //TxtToolTip.Text = "";
            ToolTip.Visibility = Visibility.Visible;

            if (e.Position.X < this.ActualWidth / 2)
                ManipulatingBrightness = true;
            else
                ManipulatingBrightness = false;

        }

        private void OnManipulationUpdated(object sender, ManipulationUpdatedEventArgs e)
        {
            var x = e.Delta.Translation.X;
            var y = e.Delta.Translation.Y;

            if (HandlingHolding)
                return;
            if (HandlingGesture == false)
            {
                if (Math.Abs(x) > Math.Abs(y))
                {
                    HandlingGesture = true;
                    DirectionX = true;

                    HandleSlideProgressDelta(e.Delta.Translation.X);
                }
                else
                {
                    HandlingGesture = true;
                    DirectionY = true;

                    if (ManipulatingBrightness)
                        HandleSlideBrightnessDelta(e.Delta.Translation.Y);
                    else
                        HandleSlideVolumeDelta(e.Delta.Translation.Y);
                }
            }
            else
            {
                if (DirectionX)
                {
                    HandleSlideProgressDelta(x);
                }
                if (DirectionY)
                {
                    if (ManipulatingBrightness)
                        HandleSlideBrightnessDelta(y);
                    else
                        HandleSlideVolumeDelta(y);
                }
            }

        }

        private void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (HandlingHolding)
            {
                StopHolding();
            }
            HandlingGesture = false;
            DirectionX = false;
            DirectionY = false;
            if (ssValue != 0)
            {
                Player.Position = Player.Position + ssValue;
            }
            ToolTip.Visibility = Visibility.Collapsed;
        }

        private void Grid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                var par = e.GetCurrentPoint(sender as Frame).Properties.PointerUpdateKind;
                if (SettingService.GetValue(SettingConstants.UI.MOUSE_MIDDLE_ACTION, (int)MouseMiddleActions.Back) == (int)MouseMiddleActions.Back
                && par == Windows.UI.Input.PointerUpdateKind.XButton1Pressed || par == Windows.UI.Input.PointerUpdateKind.MiddleButtonPressed)
                {
                    MessageCenter.GoBack(this);
                    return;
                }
                var ps = e.GetIntermediatePoints(null);
                if (ps != null && ps.Count > 0 && HandlingGesture != true)
                {
                    gestureRecognizer.ProcessDownEvent(ps[0]);
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                // 重复获取鼠标指针导致异常
            }
        }

        private void Grid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var ps = e.GetIntermediatePoints(null);
            if (ps != null && ps.Count > 0)
            {
                gestureRecognizer.ProcessUpEvent(ps[0]);
                e.Handled = true;
                gestureRecognizer.CompleteGesture();
            }
        }

        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            //ShowControl(true);
            ////FadeIn.Begin();
            ////control.Visibility = Visibility.Visible;
            pointer_in_player = true;
            //showControlsFlag = 0;
        }

        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            //showControlsFlag = 3;
            pointer_in_player = false;
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
        }

        private void Grid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            //showControlsFlag = 0;
            //ShowControl(true);
            ////control.Visibility = Visibility.Visible;
            if (Window.Current.CoreWindow.PointerCursor == null)
            {
                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            }
            gestureRecognizer.ProcessMoveEvents(e.GetIntermediatePoints(null));
            e.Handled = true;
        }
        private async void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            tapFlag = true;
            await Task.Delay(200);
            //if (control.Visibility == Visibility.Visible)
            //{
            //    if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse&& !Player.Opening)
            //    {
            //        if (Player.PlayState == PlayState.Pause || Player.PlayState == PlayState.End)
            //        {
            //            Player.Play();
            //        }
            //        else if (Player.PlayState == PlayState.Playing)
            //        {
            //            Pause();
            //        }
            //    }

            //}
            if (!tapFlag) return;
            ShowControl(control.Visibility == Visibility.Collapsed);
        }
        private void Grid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            tapFlag = false;
            var fullScreen = SettingService.GetValue<bool>(SettingConstants.Player.DOUBLE_CLICK_FULL_SCREEN, false);
            if (!fullScreen)
            {
                if (Player.PlayState == PlayState.Pause || Player.PlayState == PlayState.End)
                {
                    Player.Play();
                }
                else if (Player.PlayState == PlayState.Playing)
                {
                    Pause();
                }
            }
            else
            {
                IsFullScreen = !IsFullScreen;
            }
        }
        private void HandleSlideProgressDelta(double delta)
        {
            if (Player.PlayState != PlayState.Playing && Player.PlayState != PlayState.Pause)
                return;

            if (delta > 0)
            {
                double dd = delta / this.ActualWidth;
                double d = dd * 90;
                ssValue += d;
                //slider.Value += d;
            }
            else
            {
                double dd = Math.Abs(delta) / this.ActualWidth;
                double d = dd * 90;
                ssValue -= d;
                //slider.Value -= d;
            }
            var pos = Player.Position;
            pos += ssValue;

            if (pos < 0)
                pos = 0;
            else if (pos > Player.Duration)
                pos = Player.Duration;
            //txt_Post.Text = ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00") + "/" + mediaElement.MediaPlayer.PlaybackSession.NaturalDuration.TimeSpan.Hours.ToString("00") + ":" + mediaElement.MediaPlayer.PlaybackSession.NaturalDuration.TimeSpan.Minutes.ToString("00") + ":" + mediaElement.MediaPlayer.PlaybackSession.NaturalDuration.TimeSpan.Seconds.ToString("00");

            TxtToolTip.Text = TimeSpan.FromSeconds(pos).ToString(@"hh\:mm\:ss");
            //Notify.ShowMessageToast(ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00"), 3000);
        }

        private void HandleSlideVolumeDelta(double delta)
        {
            if (delta > 0)
            {
                double dd = delta / (this.ActualHeight * 0.8);

                //slider_V.Value -= d;
                var volume = Player.Volume - dd;
                Player.Volume = volume;

            }
            else
            {
                double dd = Math.Abs(delta) / (this.ActualHeight * 0.8);
                var volume = Player.Volume + dd;
                Player.Volume = volume;
                //slider_V.Value += d;
            }
            TxtToolTip.Text = "音量:" + Player.Volume.ToString("P");
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
        #endregion
        private void BottomBtnList_Click(object sender, RoutedEventArgs e)
        {
            NodeList.Visibility = Visibility.Collapsed;
            EpisodeList.Visibility = Visibility.Visible;
            SettingPivot.SelectedIndex = 0;
            SplitView.IsPaneOpen = true;

        }
        private void BottomBtnNode_Click(object sender, RoutedEventArgs e)
        {
            NodeList.Visibility = Visibility.Visible;
            EpisodeList.Visibility = Visibility.Collapsed;
            SettingPivot.SelectedIndex = 0;
            SplitView.IsPaneOpen = true;
        }



        private void TopBtnSettingDanmaku_Click(object sender, RoutedEventArgs e)
        {
            SettingPivot.SelectedIndex = 1;
            SplitView.IsPaneOpen = true;
        }

        private void TopBtnMore_Click(object sender, RoutedEventArgs e)
        {
            SettingPivot.SelectedIndex = 2;
            SplitView.IsPaneOpen = true;
        }

        private async void BottomSoundQuality_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BottomSoundQuality.SelectedItem == null)
            {
                return;
            }

            _postion = Player.Position;
            var data = BottomSoundQuality.SelectedItem as BiliDashAudioPlayUrlInfo;
            SettingService.SetValue<int>(SettingConstants.Player.DEFAULT_SOUND_QUALITY, data.QualityID);
            _autoPlay = Player.PlayState == PlayState.Playing;
            await ChangeQuality(current_quality_info, data);
        }

        private async void BottomCBQuality_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BottomCBQuality.SelectedItem == null)
            {
                return;
            }

            _postion = Player.Position;
            var data = BottomCBQuality.SelectedItem as BiliPlayUrlInfo;
            SettingService.SetValue<int>(SettingConstants.Player.DEFAULT_QUALITY, data.QualityID);
            _autoPlay = Player.PlayState == PlayState.Playing;
            await ChangeQuality(data, current_audio_quality_info);
        }

        private void BottomBtnPause_Click(object sender, RoutedEventArgs e)
        {
            if (Player.Opening)
            {
                return;
            }
            Pause();
        }

        private void BottomBtnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (Player.Opening)
            {
                return;
            }
            if (Player.PlayState == PlayState.Pause || Player.PlayState == PlayState.End)
            {
                Player.Play();
                DanmuControl.ResumeDanmaku();
            }
        }



        private void BottomBtnNext_Click(object sender, RoutedEventArgs e)
        {
            EpisodeList.SelectedIndex = EpisodeList.SelectedIndex + 1;
        }

        private void KeepScreenOn(bool value = true)
        {
            try
            {
                if (dispRequest != null)
                {
                    if (value)
                    {
                        dispRequest.RequestActive();
                    }
                    else
                    {
                        dispRequest.RequestRelease();
                    }
                }
            }
            catch (Exception ex)
            {
                //Debug.WriteLine(ex.Message);
                //throws an error but it works;
                //A method was called at an unexpected time
            }
        }

        private void Player_PlayStateChanged(object sender, PlayState e)
        {
            BottomImageBtnPlay.Visibility = Visibility.Collapsed;
            switch (e)
            {
                case PlayState.Loading:
                    KeepScreenOn(false);
                    if (_systemMediaTransportControls != null)
                    {
                        _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Changing;
                    }
                    BottomBtnLoading.Visibility = Visibility.Visible;
                    BottomBtnPlay.Visibility = Visibility.Collapsed;
                    BottomBtnPause.Visibility = Visibility.Collapsed;
                    break;
                case PlayState.Playing:
                    KeepScreenOn(true);
                    if (_systemMediaTransportControls != null)
                    {
                        _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                    }
                    BottomBtnLoading.Visibility = Visibility.Collapsed;
                    BottomBtnPlay.Visibility = Visibility.Collapsed;
                    BottomBtnPause.Visibility = Visibility.Visible;
                    DanmuControl.ResumeDanmaku();
                    break;
                case PlayState.Pause:
                    KeepScreenOn(false);
                    if (_systemMediaTransportControls != null)
                    {
                        _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Paused;
                    }
                    BottomImageBtnPlay.Visibility = Visibility.Visible;
                    BottomBtnLoading.Visibility = Visibility.Collapsed;
                    BottomBtnPlay.Visibility = Visibility.Visible;
                    BottomBtnPause.Visibility = Visibility.Collapsed;
                    DanmuControl.PauseDanmaku();
                    break;
                case PlayState.End:
                    KeepScreenOn(false);
                    if (_systemMediaTransportControls != null)
                    {
                        _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
                    }
                    BottomBtnLoading.Visibility = Visibility.Collapsed;
                    BottomBtnPlay.Visibility = Visibility.Visible;
                    BottomBtnPause.Visibility = Visibility.Collapsed;
                    break;
                case PlayState.Error:
                    KeepScreenOn(false);
                    if (_systemMediaTransportControls != null)
                    {
                        _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Closed;
                    }
                    BottomBtnLoading.Visibility = Visibility.Visible;
                    BottomBtnPlay.Visibility = Visibility.Collapsed;
                    BottomBtnPause.Visibility = Visibility.Collapsed;
                    break;
                default:
                    break;
            }
        }

        private void Player_PlayBufferStart(object sender, EventArgs e)
        {
            Buffering = true;
            GridBuffering.Visibility = Visibility.Visible;
            TxtBuffering.Text = "正在缓冲...";
            BufferingProgress = 0;
            DanmuControl.PauseDanmaku();
        }

        private void Player_PlayBuffering(object sender, double e)
        {
            Buffering = true;
            GridBuffering.Visibility = Visibility.Visible;
            TxtBuffering.Text = "正在缓冲" + e.ToString("p");
            BufferingProgress = e;
        }

        private void Player_PlayBufferEnd(object sender, EventArgs e)
        {
            GridBuffering.Visibility = Visibility.Collapsed;
            Buffering = false;
            DanmuControl.ResumeDanmaku();
        }

        private void Player_PlayMediaEnded(object sender, EventArgs e)
        {
            if (CurrentPlayItem.is_interaction)
            {
                if (interactionVideoVM.Info.is_leaf == 1)
                {
                    Notify.ShowMessageToast("播放完毕，请点击右下角节点，重新开始");
                    return;
                }
                DanmuControl.PauseDanmaku();
                InteractionChoices.Visibility = Visibility.Visible;
                return;
            }
            playerHelper.ReportHistory(CurrentPlayItem, Player.Duration).RunWithoutAwait();
            //列表顺序播放
            if (PlayerSettingPlayMode.SelectedIndex == 0)
            {
                if (CurrentPlayIndex == PlayInfos.Count - 1)
                {
                    if (AllMediaEndEvent != null)
                    {
                        AllMediaEndEvent?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        Notify.ShowMessageToast("播放完毕");
                    }

                }
                else
                {
                    if (PlayerSettingAutoNext.IsOn)
                    {
                        _autoPlay = true;
                        ChangePlayIndex(CurrentPlayIndex + 1);
                    }
                    else
                    {
                        Notify.ShowMessageToast("本P播放完成");
                    }

                }
                return;
            }
            //单P循环
            if (PlayerSettingPlayMode.SelectedIndex == 1)
            {
                ClearSubTitle();
                DanmuControl.ClearAll();
                Player.Play();
                return;
            }
            //列表循环播放
            if (PlayerSettingPlayMode.SelectedIndex == 2)
            {
                if (!PlayerSettingAutoNext.IsOn)
                {
                    Notify.ShowMessageToast("本P播放完成");
                    return;
                }
                //只有一P,重新播放
                if (PlayInfos.Count == 1)
                {
                    ClearSubTitle();
                    DanmuControl.ClearAll();
                    Player.Play();
                    return;
                }
                _autoPlay = true;
                if (CurrentPlayIndex == PlayInfos.Count - 1)
                {
                    ChangePlayIndex(0);
                }
                else
                {
                    ChangePlayIndex(CurrentPlayIndex + 1);
                }
                return;
            }


        }
        private void Player_PlayMediaError(object sender, string e)
        {
            _logger.Error($"播放失败:{e}");
            ShowDialog(e, "播放失败");
        }

        private async void DanmuSettingUpdateDanmaku_Click(object sender, RoutedEventArgs e)
        {
            await SetDanmaku(true);
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
                await bitmap.RenderAsync(Player);
                var pixelBuffer = await bitmap.GetPixelsAsync();
                using (var fileStream = await saveFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, fileStream);
                    encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                        BitmapAlphaMode.Ignore,
                         (uint)bitmap.PixelWidth,
                         (uint)bitmap.PixelHeight,
                         displayInformation.LogicalDpi,
                         displayInformation.LogicalDpi,
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

        private async void Player_ChangeEngine(object sender, ChangePlayerEngine e)
        {
            if (!e.need_change)
            {
                ShowErrorDialog(e.message + "[ChangeEngine]");
                return;
            }
            VideoLoading.Visibility = Visibility.Visible;
            PlayerOpenResult result = new PlayerOpenResult()
            {
                result = false,
                message = ""
            };
            if (e.play_type == PlayMediaType.Dash && e.change_engine == PlayEngine.FFmpegInteropMSS)
            {
                result = await Player.PlayDashUseFFmpegInterop(current_quality_info.DashInfo, current_quality_info.UserAgent, current_quality_info.Referer, positon: _postion);
            }
            if (e.play_type == PlayMediaType.Single && e.change_engine == PlayEngine.SYEngine)
            {
                result = await Player.PlaySingleFlvUseSYEngine(current_quality_info.FlvInfo.First().Url, current_quality_info.UserAgent, current_quality_info.Referer, positon: _postion);
            }
            if (!result.result)
            {
                _logger.Error($"播放失败:{result.message}");
                ShowDialog(result.message, "播放失败");
                return;
            }

        }

        private void Player_PlayMediaOpened(object sender, EventArgs e)
        {
            txtInfo.Text = Player.GetMediaInfo();
            VideoLoading.Visibility = Visibility.Collapsed;
            if (_postion != 0)
            {
                Player.SetPosition(_postion);
            }
            if (_autoPlay)
            {
                Player.Play();
            }
        }

        private async void BottomBtnSendDanmakuWide_Click(object sender, RoutedEventArgs e)
        {
            Pause();
            SendDanmakuDialog sendDanmakuDialog = new SendDanmakuDialog(CurrentPlayItem.avid, CurrentPlayItem.cid, Player.Position);
            sendDanmakuDialog.DanmakuSended += new EventHandler<SendDanmakuModel>((obj, arg) =>
            {
                DanmuControl.AddDanmu(new DanmakuModel()
                {
                    color = NSDanmaku.Utils.ToColor(arg.color),
                    text = arg.text,
                    location = (DanmakuLocation)arg.location,
                    size = 25,
                    time = Player.Position
                }, true);
            });
            await sendDanmakuDialog.ShowAsync();
            Player.Play();
        }

        private async void DanmuSettingSyncWords_Click(object sender, RoutedEventArgs e)
        {
            await settingVM.SyncDanmuFilter();
        }

        private async void DanmuSettingAddWord_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(DanmuSettingTxtWord.Text))
            {
                Notify.ShowMessageToast("关键词不能为空");
                return;
            }
            settingVM.ShieldWords.Add(DanmuSettingTxtWord.Text);
            SettingService.SetValue(SettingConstants.VideoDanmaku.SHIELD_WORD, settingVM.ShieldWords);
            var result = await settingVM.AddDanmuFilterItem(DanmuSettingTxtWord.Text, 0);
            DanmuSettingTxtWord.Text = "";
            if (!result)
            {
                Notify.ShowMessageToast("已经添加到本地，但远程同步失败");
            }
        }

        public async void Dispose()
        {
            if (CurrentPlayItem != null)
            {
                SettingService.SetValue<double>(CurrentPlayItem.season_id != 0 ? "ep" + CurrentPlayItem.ep_id : CurrentPlayItem.cid, Player.Position);
                //当视频播放结束的话，Position为0
                if (Player.PlayState != PlayState.End)
                    await playerHelper.ReportHistory(CurrentPlayItem, Player.Position);
            }

            Player.PlayStateChanged -= Player_PlayStateChanged;
            Player.PlayMediaEnded -= Player_PlayMediaEnded;
            Player.PlayMediaError -= Player_PlayMediaError;
            Player.ChangeEngine -= Player_ChangeEngine;
            //Player.PlayBufferEnd -= Player_PlayBufferEnd;
            //Player.PlayBufferStart -= Player_PlayBufferStart;
            //Player.PlayBuffering -= Player_PlayBuffering;
            Player.Dispose();
            if (danmuTimer != null)
            {
                danmuTimer.Stop();
                danmuTimer = null;
            }
            danmakuPool = null;
            if (dispRequest != null)
            {
                dispRequest = null;
            }
        }


        private void GridViewSelectColor_ItemClick(object sender, ItemClickEventArgs e)
        {
            SendDanmakuColorText.Text = e.ClickedItem.ToString();
        }

        private void SendDanmakuTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                SendDanmaku();
            }
        }

        private void SendDanmakuButton_Click(object sender, RoutedEventArgs e)
        {
            SendDanmaku();
        }
        private async void SendDanmaku()
        {
            int modeInt = 1;
            var location = DanmakuLocation.Scroll;
            if (SendDanmakuMode.SelectedIndex == 2)
            {
                modeInt = 4;
                location = DanmakuLocation.Bottom;
            }
            if (SendDanmakuMode.SelectedIndex == 1)
            {
                modeInt = 5;
                location = DanmakuLocation.Top;
            }
            var color = "16777215";
            if (SendDanmakuColorBorder.Background != null)
            {
                color = Convert.ToInt32((SendDanmakuColorBorder.Background as SolidColorBrush).Color.ToString().Replace("#FF", ""), 16).ToString();
            }

            var result = await playerHelper.SendDanmaku(CurrentPlayItem.avid, CurrentPlayItem.cid, SendDanmakuTextBox.Text, Convert.ToInt32(Player.Position), modeInt, color);
            if (result)
            {
                DanmuControl.AddDanmu(new DanmakuModel()
                {
                    color = NSDanmaku.Utils.ToColor(color),
                    text = SendDanmakuTextBox.Text,
                    location = location,
                    size = 25,
                    time = Player.Position
                }, true).RunWithoutAwait();
                SendDanmakuTextBox.Text = "";
            }

        }

        bool miniWin = false;
        private void BottomBtnExitMiniWindows_Click(object sender, RoutedEventArgs e)
        {

            MiniWidnows(false);
        }

        private void BottomBtnMiniWindows_Click(object sender, RoutedEventArgs e)
        {
            MiniWidnows(true);
        }

        public async void MiniWidnows(bool mini)
        {
            miniWin = mini;
            ApplicationView view = ApplicationView.GetForCurrentView();
            FullWindowEvent?.Invoke(this, IsFullWindow);
            if (mini)
            {
                IsFullWindow = true;
                StandardControl.Visibility = Visibility.Collapsed;
                MiniControl.Visibility = Visibility.Visible;
                //处理CC字幕
                if (ApplicationView.GetForCurrentView().IsViewModeSupported(ApplicationViewMode.CompactOverlay))
                {
                    this.Margin = new Thickness(0, -40, 0, 0);
                    await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay);
                    SubtitleSettingSize.Value = 14;

                    DanmuControl.DanmakuSizeZoom = 0.5;
                    DanmuControl.DanmakuDuration = 6;
                    DanmuControl.ClearAll();
                }
            }
            else
            {
                this.Margin = new Thickness(0, 0, 0, 0);
                StandardControl.Visibility = Visibility.Visible;
                MiniControl.Visibility = Visibility.Collapsed;
                await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);
                DanmuControl.DanmakuSizeZoom = SettingService.GetValue<double>(SettingConstants.VideoDanmaku.FONT_ZOOM, 1);
                DanmuControl.DanmakuDuration = SettingService.GetValue<int>(SettingConstants.VideoDanmaku.SPEED, 10);
                DanmuControl.ClearAll();
                DanmuControl.Visibility = SettingService.GetValue<Visibility>(SettingConstants.VideoDanmaku.SHOW, Visibility.Visible);
                SubtitleSettingSize.Value = SettingService.GetValue<double>(SettingConstants.Player.SUBTITLE_SIZE, 40);
            }
            BtnFoucs.Focus(FocusState.Programmatic);
            MessageCenter.SetMiniWindow(mini);
        }

        public void Pause()
        {
            DanmuControl.PauseDanmaku();
            Player.Pause();

        }

        public void PlayerSettingABPlaySetPointA_Click(object sender, RoutedEventArgs e)
        {
            if (Player.ABPlay != null)
            {
                Player.ABPlay = null;
                VideoPlayHistoryHelper.SetABPlayHistory(CurrentPlayItem, null);
                PlayerSettingABPlaySetPointA.Content = "设置A点";
                PlayerSettingABPlaySetPointB.Content = "设置B点";
                PlayerSettingABPlaySetPointB.Visibility = Visibility.Collapsed;

                Notify.ShowMessageToast("已取消设置A点");
            }
            else
            {
                Player.ABPlay = new VideoPlayHistoryHelper.ABPlayHistoryEntry()
                {
                    PointA = Player.Position
                };
                PlayerSettingABPlaySetPointA.Content = "A: " + TimeSpan.FromSeconds(Player.ABPlay.PointA).ToString(@"hh\:mm\:ss\.fff");
                PlayerSettingABPlaySetPointB.Visibility = Visibility.Visible;

                Notify.ShowMessageToast("已设置A点, 再次点击可取消设置");
            }
        }

        public void PlayerSettingABPlaySetPointB_Click(object sender, RoutedEventArgs e)
        {
            if (Player.ABPlay.PointB > 0 && Player.ABPlay.PointB != Double.MaxValue)
            {
                Player.ABPlay.PointB = double.MaxValue;
                PlayerSettingABPlaySetPointB.Content = "设置B点";

                Notify.ShowMessageToast("已取消设置B点");
            }
            else
            {
                if (Player.Position <= Player.ABPlay.PointA)
                {
                    Notify.ShowMessageToast("B点必须在A点之后");
                }
                else
                {
                    Player.ABPlay.PointB = Player.Position;
                    VideoPlayHistoryHelper.SetABPlayHistory(CurrentPlayItem, Player.ABPlay);
                    PlayerSettingABPlaySetPointB.Content = "B: " + TimeSpan.FromSeconds(Player.ABPlay.PointB).ToString(@"hh\:mm\:ss\.fff");

                    Notify.ShowMessageToast("已设置B点, 再次点击可取消设置");
                }
            }
        }

        private void Player_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var rectangle = new RectangleGeometry();
            rectangle.Rect = new Rect(0, 0, SplitView.ActualWidth, SplitView.ActualHeight);
            DanmuControl.Clip = rectangle;
        }
    }
}
