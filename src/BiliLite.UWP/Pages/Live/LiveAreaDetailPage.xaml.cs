using BiliLite.Helpers;
using BiliLite.Modules.Live;
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
    public class LiveAreaPar
    {
        public int area_id { get; set; } = 0;
        public int parent_id { get; set; } = 0;
    }
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LiveAreaDetailPage : BasePage
    {
        LiveAreaDetailVM liveAreaDetailVM;
        public LiveAreaDetailPage()
        {
            this.InitializeComponent();
            Title = "分区详情";
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New && liveAreaDetailVM == null)
            {
                var data = e.Parameter as LiveAreaPar;
                liveAreaDetailVM = new LiveAreaDetailVM(data.area_id, data.parent_id);
                await liveAreaDetailVM.GetItems();
            }
        }

        private void RefreshContainer_RefreshRequested(Microsoft.UI.Xaml.Controls.RefreshContainer sender, Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs args)
        {
            liveAreaDetailVM.Refresh();
        }

        private void AdaptiveGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as LiveRecommendItemModel;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Video,
                page = typeof(LiveDetailPage),
                title = data.uname + "的直播间",
                parameters = data.roomid
            });
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as ToggleButton).DataContext as LiveTagItemModel;
            if (data.Select) return;
            var select=liveAreaDetailVM.Tags.FirstOrDefault(x => x.Select);
            select.Select = false;
            data.Select = true;
            liveAreaDetailVM.SelectTag = data;
            liveAreaDetailVM.Refresh();
        }
    }
}
