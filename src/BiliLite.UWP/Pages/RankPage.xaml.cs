using BiliLite.Helpers;
using BiliLite.Modules;
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
    public sealed partial class RankPage : BasePage
    {
        readonly RankVM rankVM;
        public RankPage()
        {
            this.InitializeComponent();
            Title = "排行榜";
            rankVM = new RankVM();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if(e.NavigationMode== NavigationMode.New)
            {
                var rid = 0;
                if (e.Parameter != null)
                {
                    rid = e.Parameter.ToInt32();
                }
                 rankVM.LoadRankRegion(rid);
            }
        }

        private async void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot.SelectedItem == null)
            {
                return;
            }
            var data = pivot.SelectedItem as RankRegionModel;
            if (data.Items==null||data.Items.Count==0)
            {
                await rankVM.LoadRankDetail(data);
            }
        }

        private void AdaptiveGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
           var item= e.ClickedItem as RankItemModel;
            MessageCenter.NavigateToPage(this,new NavigationInfo() { 
                icon= Symbol.Play,
                page=typeof(VideoDetailPage),
                title=item.title,
                parameters=item.aid
            });
        }

        private void AddToWatchLater_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as MenuFlyoutItem).DataContext as RankItemModel;
            Modules.User.WatchLaterVM.Instance.AddToWatchlater(data.aid);
        }
    }
}
