using BiliLite.Helpers;
using BiliLite.Modules;
using BiliLite.Modules.Live.LiveCenter;
using BiliLite.Pages.Live;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
    public sealed partial class LivePage : Page
    {
        private Modules.LiveVM liveVM;
        public LivePage()
        {
            this.InitializeComponent();
            liveVM = new Modules.LiveVM();
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
            if (e.NavigationMode == NavigationMode.New&& liveVM.Banners==null)
            {
                await LoadData();
            }
        }
        private async Task LoadData()
        {
            await liveVM.GetLiveHome();
            if (SettingHelper.Account.Logined)
            {
                liveVM.ShowFollows = true;
                await liveVM.liveAttentionVM.GetFollows();
            }
        }

        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadData();
        }

        private async void BannerItem_Click(object sender, RoutedEventArgs e)
        {
            var result = await MessageCenter.HandelUrl(((sender as HyperlinkButton).DataContext as LiveHomeBannerModel).link);
            if (!result)
            {
                Utils.ShowMessageToast("不支持打开的链接");
            }
        }

        private async void RefreshContainer_RefreshRequested(Microsoft.UI.Xaml.Controls.RefreshContainer sender, Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs args)
        {
            await LoadData();
        }

        private void FollowLive_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as LiveFollowAnchorModel;
            MessageCenter.NavigateToPage(this,new NavigationInfo() {
                icon = Symbol.Video,
                page = typeof(LiveDetailPage),
                title = data.uname+"的直播间",
                parameters = data.roomid
            });
        }

        private void LiveItems_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data=e.ClickedItem as LiveHomeItemsItemModel;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Video,
                page = typeof(LiveDetailPage),
                title = data.uname + "的直播间",
                parameters = data.roomid
            });
        }

        private void loadMore_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as HyperlinkButton).DataContext as LiveHomeItemsModel;
            if (data.module_info.title== "推荐直播")
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo() { 
                    icon= Symbol.Document,
                    page=typeof(LiveRecommendPage),
                    title="全部直播"
                });
            }
            if (!string.IsNullOrEmpty(data.module_info.link))
            {
                try
                {
                    var match = Regex.Match(data.module_info.link, @"parentAreaId=(\d+)&areaId=(\d+)");
                    if (match.Groups.Count == 3)
                    {
                        MessageCenter.NavigateToPage(this, new NavigationInfo()
                        {
                            icon = Symbol.Document,
                            page = typeof(LiveAreaDetailPage),
                            title = data.module_info.title,
                            parameters=new LiveAreaPar()
                            {
                                parent_id= match.Groups[1].Value.ToInt32(),
                                area_id= match.Groups[2].Value.ToInt32()
                            }
                        });
                        
                    }
                }
                catch (Exception)
                {
                }
               
                
            }

        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var area = e.ClickedItem as LiveHomeAreaModel;
            if (area.id== 0)
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Document,
                    page = typeof(LiveAreaPage),
                    title = area.title
                });
                return;
            }
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Document,
                page = typeof(LiveAreaDetailPage),
                title = area.title,
                parameters = new LiveAreaPar()
                {
                    parent_id = area.area_v2_parent_id,
                    area_id = area.area_v2_id
                }
            });
        }

        private async void btnOpenLiveCenter_Click(object sender, RoutedEventArgs e)
        {
            if (!SettingHelper.Account.Logined && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录");
                return;
            }
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Contact,
                page = typeof(Live.LiveCenterPage),
                title = "直播中心",

            });
        }
    }
}
