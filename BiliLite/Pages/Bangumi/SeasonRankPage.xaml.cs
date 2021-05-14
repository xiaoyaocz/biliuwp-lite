using BiliLite.Helpers;
using BiliLite.Modules.Season;
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

namespace BiliLite.Pages.Bangumi
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SeasonRankPage : BasePage
    {
        readonly SeasonRankVM seasonRankVM;
        public SeasonRankPage()
        {
            this.InitializeComponent();
            Title = "热门榜单";
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            seasonRankVM = new SeasonRankVM();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                 seasonRankVM.LoadRankRegion((int)e.Parameter);
            }
        }

        private async void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot.SelectedItem == null)
            {
                return;
            }
            var data = pivot.SelectedItem as SeasonRankModel;
            if (data.Items == null || data.Items.Count == 0)
            {
                await seasonRankVM.LoadRankDetail(data);
            }
        }

        private void AdaptiveGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as SeasonRankItemModel;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Play,
                page = typeof(SeasonDetailPage),
                title = item.title,
                parameters = item.season_id
            });
        }

    }
}
