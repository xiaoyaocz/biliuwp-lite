using BiliLite.Helpers;
using BiliLite.Modules.User;
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

namespace BiliLite.Pages.User
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class WatchlaterPage : BasePage
    {
        WatchLaterVM watchLaterVM;
        public WatchlaterPage()
        {
            this.InitializeComponent();
            Title = "稍后再看";
            watchLaterVM = new WatchLaterVM();
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if(e.NavigationMode== NavigationMode.New)
            {
               await watchLaterVM.LoadData();
            }
        }

        private void Video_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (watchLaterVM.Videos == null) return;
            var data = e.ClickedItem as WatchlaterItemModel;
            List<VideoPlaylistItem> items = new List<VideoPlaylistItem>();
            foreach (var item in watchLaterVM.Videos)
            {
                if (item.title != "已失效视频")
                {
                    items.Add(new VideoPlaylistItem()
                    {
                        Cover = item.pic,
                        Author = item.owner.name,
                        ID = item.aid,
                        Title = item.title
                    });
                }

            }
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Play,
                page = typeof(VideoDetailPage),
                title = "视频播放",
                parameters = new VideoPlaylist()
                {
                    Index = watchLaterVM.Videos.IndexOf(data),
                    Playlist = items
                }
            });
        }
    }
}
