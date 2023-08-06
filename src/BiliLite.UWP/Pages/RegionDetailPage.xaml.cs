using BiliLite.Models.Common;
using BiliLite.Modules;
using BiliLite.Pages.Bangumi;
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

namespace BiliLite.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class RegionDetailPage : BasePage
    {
        RegionDetailVM regionDetailVM;
        OpenRegionInfo regionInfo;
        public RegionDetailPage()
        {
            this.InitializeComponent();
            Title = "分区详情";
            regionDetailVM = new RegionDetailVM();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New||regionDetailVM.Regions==null)
            {
                if (e.Parameter!=null)
                {
                    regionInfo = e.Parameter as OpenRegionInfo;
                }
                else
                {
                    regionInfo = new OpenRegionInfo();
                }
                regionDetailVM.InitRegion(regionInfo.id, regionInfo.tid);
            }
        }

        private async void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot.SelectedItem==null)
            {
                return;
            }
            if(pivot.SelectedItem is RegionDetailHomeVM)
            {
                GridOrder.Visibility = Visibility.Collapsed;
                var data = pivot.SelectedItem as RegionDetailHomeVM;
                if (!data.Loading&&data.Banners==null)
                {
                    await data.LoadHome();
                }
            }
            else
            {
                var data = pivot.SelectedItem as RegionDetailChildVM;
                if (!data.Loading && data.Tasgs == null)
                {
                    await data.LoadHome();
                }
                GridOrder.Visibility = Visibility.Visible;
                GridOrder.DataContext = data;
               
            }
        }

        private void btnOpenRank_Click(object sender, RoutedEventArgs e)
        {
            if (regionInfo.id == 13)
            {
                //打开番剧排行榜
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.FourBars,
                    page = typeof(SeasonRankPage),
                    title = "热门榜单",
                    parameters = AnimeType.Bangumi
                });
                return;
            }
            if (regionInfo.id == 167)
            {
                //打开国创排行榜
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.FourBars,
                    page = typeof(SeasonRankPage),
                    title = "热门榜单",
                    parameters = AnimeType.GuoChuang
                });
                return;
            } 
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.FourBars,
                page = typeof(RankPage),
                title = "排行榜",
                parameters= regionInfo.id
            });
        }

        private  void cbTags_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTags.SelectedItem==null)
            {
                return;
            }
          (pivot.SelectedItem as RegionDetailChildVM).Refresh();

        }

        private void cbOrder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbOrder.SelectedItem == null)
            {
                return;
            }
            (pivot.SelectedItem as RegionDetailChildVM).Refresh();
        }

        private void AdaptiveGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as RegionVideoItemModel;
            MessageCenter.NavigateToPage(this,new NavigationInfo() { 
                icon= Symbol.Play,
                page=typeof(VideoDetailPage),
                parameters=data.param,
                title=data.title
            });
        }

        private async void BtnOpenBanner_Click(object sender, RoutedEventArgs e)
        {
           await MessageCenter.HandelUrl(((sender as HyperlinkButton).DataContext as RegionHomeBannerItemModel).uri);
        }

        private void AddToWatchLater_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as MenuFlyoutItem).DataContext as RegionVideoItemModel;

            Modules.User.WatchLaterVM.Instance.AddToWatchlater(data.param);
        }
    }
    public class RegionDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate HomeTemplate { get; set; }

        public DataTemplate ChildTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is RegionDetailHomeVM)
            {
                return HomeTemplate;
            }
            else
            {
                return ChildTemplate;
            }
            

        }
    }

    public class OpenRegionInfo
    {
        /// <summary>
        /// 分区ID
        /// </summary>
        public int id { get; set; } = 1;
        /// <summary>
        /// 子分区ID
        /// </summary>
        public int tid { get; set; } = 0;
    }
}
