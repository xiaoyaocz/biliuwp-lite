using BiliLite.Helpers;
using BiliLite.Modules.Live.LiveCenter;
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

namespace BiliLite.Pages.Live
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LiveCenterPage : BasePage
    {
        readonly LiveAttentionVM liveAttentionVM;
        readonly LiveAttentionUnLiveVM liveAttentionUnLiveVM;
        readonly LiveCenterHistoryVM  liveCenterHistoryVM;
        readonly LiveCenterVM liveCenterVM;
        public LiveCenterPage()
        {
            this.InitializeComponent();
            Title = "直播中心";
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            liveAttentionVM = new LiveAttentionVM();
            liveAttentionUnLiveVM = new LiveAttentionUnLiveVM();
            liveCenterHistoryVM = new LiveCenterHistoryVM();
            liveCenterVM = new LiveCenterVM();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if(e.NavigationMode== NavigationMode.New)
            {
                liveCenterVM.GetUserInfo();
                await liveAttentionVM.GetFollows();
            }
        }

        private void AttentionlList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as LiveFollowAnchorModel;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Video,
                page = typeof(LiveDetailPage),
                title = data.uname + "的直播间",
                parameters = data.roomid
            });
        }

        private void UnLiveList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as LiveFollowUnliveAnchorModel;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Video,
                page = typeof(LiveDetailPage),
                title = data.uname + "的直播间",
                parameters = data.roomid
            });
        }

        private void HistoryList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as LiveHistoryItemModel;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Video,
                page = typeof(LiveDetailPage),
                title = data.name + "的直播间",
                parameters = data.history.oid
            }) ;
        }

        private async void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(pivot.SelectedIndex==1&& liveAttentionUnLiveVM.Items == null)
            {
                await liveAttentionUnLiveVM.Get();
            }
            if (pivot.SelectedIndex == 2 && liveCenterHistoryVM.Items == null)
            {
                await liveCenterHistoryVM.Get();
            }
        }
    }
}
