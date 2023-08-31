using BiliLite.Models.Common;
using BiliLite.Modules;
using BiliLite.Modules.Home;
using BiliLite.Services;
using System;
using System.Collections.Generic;
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
    public sealed partial class RegionsPage : Page
    {
        RegionVM channelVM;
        public RegionsPage()
        {
            this.InitializeComponent();
            channelVM = new RegionVM();
            if (SettingService.GetValue<bool>(SettingConstants.UI.CACHE_HOME, true))
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
            if (e.NavigationMode == NavigationMode.New && channelVM.Regions == null)
            {
                await channelVM.GetRegions();
            }

        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as RegionItem;
            if (item.uri.Contains("http"))
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.World,
                    page = typeof(WebPage),
                    title = item.name,
                    parameters = item.uri
                });
                return;
            }
            if (item.children != null)
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Document,
                    page = typeof(Pages.RegionDetailPage),
                    title = item.name,
                    parameters = new OpenRegionInfo()
                    {
                        id = item.tid
                    }
                });
                return;
            }
            if (item.name == "番剧")
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Home,
                    page = typeof(Pages.Home.AnimePage),
                    title = item.name,
                    parameters = AnimeType.Bangumi
                });
                return;
            }
            if (item.name == "国创")
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Home,
                    page = typeof(Pages.Home.AnimePage),
                    title = item.name,
                    parameters = AnimeType.Bangumi
                });
                return;
            }
            if (item.name == "放映厅")
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Home,
                    page = typeof(Pages.Home.MoviePage),
                    title = item.name
                });
                return;
            }
            if (item.name == "直播")
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Home,
                    page = typeof(Pages.Home.LivePage),
                    title = item.name
                });
                return;
            }
            if (item.name == "全区排行榜")
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.FourBars,
                    page = typeof(RankPage),
                    title = "排行榜"
                });
                return;
            }
        }
    }
}
