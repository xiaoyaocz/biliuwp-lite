using BiliLite.Helpers;
using BiliLite.Modules;
using BiliLite.Modules.User;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages.Home
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HotPage : Page
    {
        HotVM hotVM;
        public HotPage()
        {
            this.InitializeComponent();
            hotVM = new HotVM();
            this.DataContext = hotVM;
            if (SettingHelper.GetValue<bool>(SettingHelper.UI.CACHE_HOME, true))
            {
                this.NavigationCacheMode = NavigationCacheMode.Enabled;
            }
            else
            {
                this.NavigationCacheMode = NavigationCacheMode.Disabled;
            }

        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New&&hotVM.HotItems==null)
            {
                await hotVM.GetPopular();
            }
        }


       

        private async void gridHot_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = (e.ClickedItem as Modules.HotDataItemModel);
            if (data.card_goto=="av")
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo() {
                    icon= Symbol.Play,
                    page=typeof(VideoDetailPage),
                    title= data.title,
                    parameters= data.param
                });
            }
            else
            {
                await MessageCenter.HandelUrl(data.uri);
            }
        }

        private async void gridTop_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = (e.ClickedItem as Modules.HotTopItemModel);
            if (data.module_id == "rank")
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.FourBars,
                    page = typeof(RankPage),
                    title = "排行榜"
                });
            }
            else
            {
                await MessageCenter.HandelUrl(data.uri);
            }
        }

        private void AddToWatchLater_Click(object sender, RoutedEventArgs e)
        {
           var data=( sender as MenuFlyoutItem).DataContext as HotDataItemModel;
            WatchLaterVM.Instance.AddToWatchlater(data.param);
        }
    }
}
