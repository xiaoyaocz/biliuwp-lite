using BiliLite.Helpers;
using BiliLite.Modules;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingPage : Page
    {
        SettingVM settingVM;
        public SettingPage()
        {
            this.InitializeComponent();
            settingVM = new SettingVM();
            LoadUI();
            LoadPlayer();
            LoadDanmu();
        }
        private void LoadUI()
        {
            //主题
            cbTheme.SelectedIndex = SettingHelper.GetValue<int>(SettingHelper.UI.THEME, 0);
            cbTheme.Loaded += new RoutedEventHandler((sender, e) => {
                cbTheme.SelectionChanged += new SelectionChangedEventHandler((obj, args) => {
                    SettingHelper.SetValue(SettingHelper.UI.THEME, cbTheme.SelectedIndex);
                    Frame rootFrame = Window.Current.Content as Frame;
                    switch (cbTheme.SelectedIndex)
                    {
                        case 1:
                            rootFrame.RequestedTheme = ElementTheme.Light;
                            break;
                        case 2:
                            rootFrame.RequestedTheme = ElementTheme.Dark;
                            break;
                        default:
                            rootFrame.RequestedTheme = ElementTheme.Default;
                            break;
                    }
                });
            });

            cbColor.SelectedIndex = SettingHelper.GetValue<int>(SettingHelper.UI.THEME_COLOR, 0);
            cbColor.Loaded += new RoutedEventHandler((sender, e) => {
                cbColor.SelectionChanged += new SelectionChangedEventHandler((obj, args) => {
                    SettingHelper.SetValue(SettingHelper.UI.THEME_COLOR, cbColor.SelectedIndex);
                    Color color = new Color();
                    if (cbColor.SelectedIndex==0)
                    {
                        var uiSettings = new Windows.UI.ViewManagement.UISettings();
                        color = uiSettings.GetColorValue(Windows.UI.ViewManagement.UIColorType.Accent);
                    }
                    else
                    {
                        color = Utils.ToColor((cbColor.SelectedItem as AppThemeColor).color);
                        
                    }
                    (Application.Current.Resources["SystemControlHighlightAltAccentBrush"] as SolidColorBrush).Color = color;
                    (Application.Current.Resources["SystemControlHighlightAccentBrush"] as SolidColorBrush).Color = color;
                  //(App.Current.Resources.ThemeDictionaries["Light"] as ResourceDictionary)["SystemAccentColor"] = Utils.ToColor(item.color);

                });
            });


            //显示模式
            cbDisplayMode.SelectedIndex = SettingHelper.GetValue<int>(SettingHelper.UI.DISPLAY_MODE, 0);
            cbDisplayMode.Loaded += new RoutedEventHandler((sender, e) => {
                cbDisplayMode.SelectionChanged += new SelectionChangedEventHandler((obj, args) => {
                    SettingHelper.SetValue(SettingHelper.UI.DISPLAY_MODE, cbDisplayMode.SelectedIndex);
                    Utils.ShowMessageToast("重启生效");
                });
            });
            //加载原图
            swPictureQuality.IsOn = SettingHelper.GetValue<bool>(SettingHelper.UI.ORTGINAL_IMAGE, false);
            swPictureQuality.Loaded += new RoutedEventHandler((sender, e) => {
                swPictureQuality.Toggled += new RoutedEventHandler((obj, args) => { 
                    SettingHelper.SetValue(SettingHelper.UI.ORTGINAL_IMAGE, swPictureQuality.IsOn);
                    SettingHelper.UI._loadOriginalImage = null;
                });
            });
            //缓存页面
            swHomeCache.IsOn = SettingHelper.GetValue<bool>(SettingHelper.UI.CACHE_HOME, true);
            swHomeCache.Loaded += new RoutedEventHandler((sender, e) => {
                swHomeCache.Toggled += new RoutedEventHandler((obj, args) => {
                    SettingHelper.SetValue(SettingHelper.UI.CACHE_HOME, swHomeCache.IsOn);
                   
                });
            });

            gridHomeCustom.ItemsSource= SettingHelper.GetValue<ObservableCollection<HomeNavItem>>(SettingHelper.UI.HOEM_ORDER, HomeVM.GetAllNavItems());
            ExceptHomeNavItems();



        }
        private void LoadPlayer()
        {
            //播放类型
            cbVideoType.SelectedIndex = SettingHelper.GetValue<int>(SettingHelper.Player.DEFAULT_VIDEO_TYPE, 1);
            cbVideoType.Loaded += new RoutedEventHandler((sender, e) => {
                cbVideoType.SelectionChanged += new SelectionChangedEventHandler((obj, args) => {
                    SettingHelper.SetValue(SettingHelper.Player.DEFAULT_VIDEO_TYPE, cbVideoType.SelectedIndex);
                });
            });

            //硬解视频
            swHardwareDecode.IsOn = SettingHelper.GetValue<bool>(SettingHelper.Player.HARDWARE_DECODING, false);
            swHardwareDecode.Loaded += new RoutedEventHandler((sender, e) => {
                swHardwareDecode.Toggled += new RoutedEventHandler((obj, args) => {
                    SettingHelper.SetValue(SettingHelper.Player.HARDWARE_DECODING, swHardwareDecode.IsOn);
                });
            });
            //自动播放
            swAutoPlay.IsOn = SettingHelper.GetValue<bool>(SettingHelper.Player.AUTO_PLAY, false);
            swAutoPlay.Loaded += new RoutedEventHandler((sender, e) => {
                swAutoPlay.Toggled += new RoutedEventHandler((obj, args) => {
                    SettingHelper.SetValue(SettingHelper.Player.AUTO_PLAY, swAutoPlay.IsOn);
                });
            });

            //使用其他网站
            swPlayerSettingUseOtherSite.IsOn = SettingHelper.GetValue<bool>(SettingHelper.Player.USE_OTHER_SITEVIDEO, true);
            swPlayerSettingUseOtherSite.Loaded += new RoutedEventHandler((sender, e) => {
                swPlayerSettingUseOtherSite.Toggled += new RoutedEventHandler((obj, args) => {
                    SettingHelper.SetValue(SettingHelper.Player.USE_OTHER_SITEVIDEO, swPlayerSettingUseOtherSite.IsOn);
                });
            });
        }
       
        private void LoadDanmu()
        {
            //弹幕开关
            var state = SettingHelper.GetValue<Visibility>(SettingHelper.VideoDanmaku.SHOW, Visibility.Visible) == Visibility.Visible;
            DanmuSettingState.IsOn = state;
            DanmuSettingState.Toggled += new RoutedEventHandler((e,args)=> {
                SettingHelper.SetValue(SettingHelper.VideoDanmaku.SHOW, DanmuSettingState.IsOn?Visibility.Visible:Visibility.Collapsed);
            });
            //弹幕关键词
            DanmuSettingListWords.ItemsSource = settingVM.ShieldWords;

            //正则关键词
            DanmuSettingListRegulars.ItemsSource = settingVM.ShieldRegulars;

            //用户
            DanmuSettingListUsers.ItemsSource = settingVM.ShieldUsers;
        }

       
        private void ExceptHomeNavItems()
        {
            List<HomeNavItem> list = new List<HomeNavItem>();
            var all = HomeVM.GetAllNavItems();
            foreach (var item in all)
            {
                if((gridHomeCustom.ItemsSource as ObservableCollection<HomeNavItem>).FirstOrDefault(x=>x.Title==item.Title)==null)
                {
                    list.Add(item);
                }
            }
            gridHomeNavItem.ItemsSource = list;
        }
        private void gridHomeCustom_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            SettingHelper.SetValue(SettingHelper.UI.HOEM_ORDER, gridHomeCustom.ItemsSource as ObservableCollection<HomeNavItem>);
            Utils.ShowMessageToast("更改成功,重启生效");
        }

        private void gridHomeNavItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item =e.ClickedItem as HomeNavItem;
            (gridHomeCustom.ItemsSource as ObservableCollection<HomeNavItem>).Add(item);
            SettingHelper.SetValue(SettingHelper.UI.HOEM_ORDER, gridHomeCustom.ItemsSource as ObservableCollection<HomeNavItem>);
            ExceptHomeNavItems();
            Utils.ShowMessageToast("更改成功,重启生效");
        }

        private void menuRemoveHomeItem_Click(object sender, RoutedEventArgs e)
        {
           var item= (sender as MenuFlyoutItem).DataContext as HomeNavItem;
            if (gridHomeCustom.Items.Count==1)
            {
                Utils.ShowMessageToast("至少要留一个页面");
                return;
            }
           (gridHomeCustom.ItemsSource as ObservableCollection<HomeNavItem>).Remove(item);
            SettingHelper.SetValue(SettingHelper.UI.HOEM_ORDER, gridHomeCustom.ItemsSource as ObservableCollection<HomeNavItem>);
            ExceptHomeNavItems();
            Utils.ShowMessageToast("更改成功,重启生效");
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            version.Text = $"版本 {SystemInformation.ApplicationVersion.Major}.{SystemInformation.ApplicationVersion.Minor}.{SystemInformation.ApplicationVersion.Build}.{SystemInformation.ApplicationVersion.Revision}";
        }

        private async void DanmuSettingAddWord_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(DanmuSettingTxtWord.Text))
            {
                Utils.ShowMessageToast("关键词不能为空");
                return;
            }
            settingVM.ShieldWords.Add(DanmuSettingTxtWord.Text);
            SettingHelper.SetValue(SettingHelper.VideoDanmaku.SHIELD_WORD, settingVM.ShieldWords);
            var result = await settingVM.AddDanmuFilterItem(DanmuSettingTxtWord.Text, 0);
            DanmuSettingTxtWord.Text = "";
            if (!result)
            {
                Utils.ShowMessageToast("已经添加到本地，但远程同步失败");
            }
        }

        private async void DanmuSettingSyncWords_Click(object sender, RoutedEventArgs e)
        {
            await settingVM.SyncDanmuFilter();
        }

        private void RemoveDanmuWord_Click(object sender, RoutedEventArgs e)
        {
            var word= (sender as AppBarButton).DataContext as string;
            settingVM.ShieldWords.Remove(word);
            SettingHelper.SetValue(SettingHelper.VideoDanmaku.SHIELD_WORD, settingVM.ShieldWords);
        }

        private void RemoveDanmuRegular_Click(object sender, RoutedEventArgs e)
        {
            var word = (sender as AppBarButton).DataContext as string;
            settingVM.ShieldRegulars.Remove(word);
            SettingHelper.SetValue(SettingHelper.VideoDanmaku.SHIELD_REGULAR, settingVM.ShieldRegulars);
        }

        private void RemoveDanmuUser_Click(object sender, RoutedEventArgs e)
        {
            var word = (sender as AppBarButton).DataContext as string;
            settingVM.ShieldUsers.Remove(word);
            SettingHelper.SetValue(SettingHelper.VideoDanmaku.SHIELD_USER, settingVM.ShieldUsers);
        }

        private async void DanmuSettingAddRegex_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(DanmuSettingTxtRegex.Text))
            {
                Utils.ShowMessageToast("正则表达式不能为空");
                return;
            }
            var txt = DanmuSettingTxtRegex.Text.Trim('/');
            settingVM.ShieldRegulars.Add(txt);
            SettingHelper.SetValue(SettingHelper.VideoDanmaku.SHIELD_REGULAR, settingVM.ShieldRegulars);
            var result = await settingVM.AddDanmuFilterItem(txt, 1);
            DanmuSettingTxtRegex.Text = "";
            if (!result)
            {
                Utils.ShowMessageToast("已经添加到本地，但远程同步失败");
            }
        }

        private async void DanmuSettingAddUser_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(DanmuSettingTxtUser.Text))
            {
                Utils.ShowMessageToast("用户ID不能为空");
                return;
            }
            settingVM.ShieldUsers.Add(DanmuSettingTxtUser.Text);
            SettingHelper.SetValue(SettingHelper.VideoDanmaku.SHIELD_WORD, settingVM.ShieldUsers);
            var result = await settingVM.AddDanmuFilterItem(DanmuSettingTxtUser.Text, 2);
            DanmuSettingTxtUser.Text = "";
            if (!result)
            {
                Utils.ShowMessageToast("已经添加到本地，但远程同步失败");
            }
        }
    }
}
