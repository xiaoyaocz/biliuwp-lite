using BiliLite.Helpers;
using BiliLite.Pages.Bangumi;
using FFmpegInterop;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
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
    public sealed partial class BlankPage : Page
    {
        public BlankPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private void BtnOpenRank_Click(object sender, RoutedEventArgs e)
        {
            ((this.Parent as Frame).Parent as TabViewItem).Header = "排行榜";
            ((this.Parent as Frame).Parent as TabViewItem).IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource() { Symbol= Symbol.FourBars };
            this.Frame.Navigate(typeof(RankPage));
            //MessageCenter.OpenNewWindow(this,new NavigationInfo() { 
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
            this.Frame.Navigate(typeof(TimelinePage), Modules.AnimeType.bangumi);
        }

        private async void BtnOpenMyFollow_Click(object sender, RoutedEventArgs e)
        {
            if (!SettingHelper.Account.Logined && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录");
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
                Utils.ShowMessageToast("关键字不能为空");
                return;
            }
            if (await MessageCenter.HandelUrl(SearchBox.Text))
            {
                return;
            }
            MessageCenter.OpenNewWindow(this, new NavigationInfo()
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
    }
}
