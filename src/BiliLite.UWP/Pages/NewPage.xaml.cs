using BiliLite.Pages.Bangumi;
using FFmpegInteropX;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using BiliLite.Models.Common;
using BiliLite.Services;
using BiliLite.Extensions;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class NewPage : Page
    {
        public NewPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SetBackground();
        }
        private async void SetBackground()
        {
            var background = SettingService.GetValue(SettingConstants.UI.BACKGROUND_IMAGE, Constants.App.BACKGROUND_IAMGE_URL);
            if (background == Constants.App.BACKGROUND_IAMGE_URL)
            {
                backgroundImage.Source = new BitmapImage(new Uri(background));
            }
            else
            {
                var file = await StorageFile.GetFileFromPathAsync(background);
                var img = new BitmapImage();
                img.SetSource(await file.OpenReadAsync());
                backgroundImage.Source = img;
            }
        }
        private void BtnOpenRank_Click(object sender, RoutedEventArgs e)
        {
            ((this.Parent as Frame).Parent as TabViewItem).Header = "排行榜";
            ((this.Parent as Frame).Parent as TabViewItem).IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource() { Symbol = Symbol.FourBars };
            this.Frame.Navigate(typeof(RankPage));
            //MessageCenter.NavigateToPage(this,new NavigationInfo() { 
            //    icon= Symbol.FourBars,
            //    page=typeof(RankPage),
            //    title="排行榜"
            //});
        }

        private void BtnOpenBangumiIndex_Click(object sender, RoutedEventArgs e)
        {
            ((this.Parent as Frame).Parent as TabViewItem).Header = "番剧索引";
            ((this.Parent as Frame).Parent as TabViewItem).IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource() { Symbol = Symbol.Filter };
            this.Frame.Navigate(typeof(AnimeIndexPage));
        }

        private void BtnOpenBangumiTimeline_Click(object sender, RoutedEventArgs e)
        {
            ((this.Parent as Frame).Parent as TabViewItem).Header = "番剧时间表";
            ((this.Parent as Frame).Parent as TabViewItem).IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource() { Symbol = Symbol.Clock };
            this.Frame.Navigate(typeof(TimelinePage), AnimeType.Bangumi);
        }

        private async void BtnOpenMyFollow_Click(object sender, RoutedEventArgs e)
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录");
                return;
            }
            ((this.Parent as Frame).Parent as TabViewItem).Header = "我的收藏";
            ((this.Parent as Frame).Parent as TabViewItem).IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource() { Symbol = Symbol.OutlineStar };
            this.Frame.Navigate(typeof(User.FavoritePage), User.OpenFavoriteType.Video);
        }

        private void BtnOpenSetting_Click(object sender, RoutedEventArgs e)
        {
            ((this.Parent as Frame).Parent as TabViewItem).Header = "设置";
            ((this.Parent as Frame).Parent as TabViewItem).IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource() { Symbol = Symbol.Setting };
            this.Frame.Navigate(typeof(SettingPage));
        }

        private async void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (string.IsNullOrEmpty(SearchBox.Text))
            {
                Notify.ShowMessageToast("关键字不能为空");
                return;
            }
            if (await MessageCenter.HandelUrl(SearchBox.Text))
            {
                return;
            }
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Find,
                page = typeof(SearchPage),
                title = "搜索:" + SearchBox.Text,
                parameters = new SearchParameter()
                {
                    keyword = SearchBox.Text,
                    searchType = SearchType.Video
                }
            });
        }

        private async void btnSetBackground_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.FileTypeFilter.Add(".png");
            fileOpenPicker.FileTypeFilter.Add(".jpg");
            StorageFile file = await fileOpenPicker.PickSingleFileAsync();
            if (file != null)
            {
                SettingService.SetValue(SettingConstants.UI.BACKGROUND_IMAGE, file.Path);
                SetBackground();
            }
        }

        private void btnSetDefaultBackground_Click(object sender, RoutedEventArgs e)
        {
            SettingService.SetValue(SettingConstants.UI.BACKGROUND_IMAGE, Constants.App.BACKGROUND_IAMGE_URL);
            SetBackground();
        }

        private async void BtnOpenHistory_Click(object sender, RoutedEventArgs e)
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录");
                return;
            }
            ((this.Parent as Frame).Parent as TabViewItem).Header = "历史记录";
            ((this.Parent as Frame).Parent as TabViewItem).IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource() { Symbol = Symbol.Clock };
            this.Frame.Navigate(typeof(User.HistoryPage));
        }

        private void BtnOpenDownload_Click(object sender, RoutedEventArgs e)
        {
            ((this.Parent as Frame).Parent as TabViewItem).Header = "离线下载";
            ((this.Parent as Frame).Parent as TabViewItem).IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource() { Symbol = Symbol.Download };
            this.Frame.Navigate(typeof(DownloadPage));
        }

        private void BtnOpenLive_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Document,
                page = typeof(Live.LiveRecommendPage),
                title = "全部直播"
            });
        }
    }
}
