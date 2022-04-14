using BiliLite.Helpers;
using BiliLite.Modules;
using BiliLite.Modules.User;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.System;
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
    public sealed partial class RecommendPage : Page
    {
        private bool IsGrid { get; set; } = true;
        readonly RecommendVM recommendVM;
        public RecommendPage()
        {
            this.InitializeComponent();
            if (SettingHelper.GetValue<bool>(SettingHelper.UI.CACHE_HOME, true))
            {
                this.NavigationCacheMode = NavigationCacheMode.Enabled;
            }
            else
            {
                this.NavigationCacheMode = NavigationCacheMode.Disabled;
            }
            recommendVM = new RecommendVM();
            this.DataContext = recommendVM;
        }
        
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
           
            if (e.NavigationMode== NavigationMode.New)
            {
                if (IsGrid)
                {
                    RecommendGridView.ItemTemplate = (DataTemplate)this.Resources["Grid"];
                }
               
                SetListDisplay();
                await recommendVM.GetRecommend();
                if (SettingHelper.GetValue<bool>("推荐右键提示", true))
                {
                    SettingHelper.SetValue("推荐右键提示", false);
                    Utils.ShowMessageToast("右键或长按项目可以进行更多操作哦~", 5);
                }
            }

           
        }
        private void SetListDisplay()
        {
            var grid = SettingHelper.GetValue<int>(SettingHelper.UI.RECMEND_DISPLAY_MODE, 0) == 0;
            if (grid != IsGrid)
            {
                IsGrid = grid;
                if (grid)
                {
                    btnGrid_Click(this, null);
                }
                else
                {
                    btnList_Click(this, null);
                }
            }
            
        }
        

        private async void RecommendGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as Modules.RecommendItemModel;
            if (data.uri == null&&data.ad_info!=null)
            {
                var url = data.ad_info.creative_content.url;
                if (!url.Contains("http://")&& !url.Contains("https://"))
                {
                    url = data.ad_info.creative_content.click_url ?? data.ad_info.creative_content.url;
                }
                await MessageCenter.HandelUrl(url);
              
                //MessageCenter.NavigateToPage(this, new NavigationInfo()
                //{
                //    icon = Symbol.World,
                //    page = typeof(WebPage),
                //    parameters = url,
                //    title = "广告"
                //});
                return;
            }
            if (data.card_goto== "new_tunnel")
            { 
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Favorite,
                    page = typeof(User.FavoritePage),
                    parameters = User.OpenFavoriteType.Bangumi,
                    title = "我的收藏"
                });
                return;
            }
            if (await MessageCenter.HandelUrl(data.uri))
            {
                return;
            }
            var browserUri = data.three_point_v2.FirstOrDefault(x => x.type == "browser")?.url ?? "";
            if (!string.IsNullOrEmpty(browserUri)&& await MessageCenter.HandelUrl(browserUri))
            {
                return;
            }
        }

   
        private void RefreshContainer_RefreshRequested(Microsoft.UI.Xaml.Controls.RefreshContainer sender, Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs args)
        {
             recommendVM.Refresh();
        }

     
        private async void BannerItem_Click(object sender, RoutedEventArgs e)
        {
            await MessageCenter.HandelUrl(((sender as HyperlinkButton).DataContext as RecommendBannerItemModel).uri);
           
        }

        private async void ListMenu_ItemClick(object sender, ItemClickEventArgs e)
        {
            var threePoint = e.ClickedItem as RecommendThreePointV2ItemModel;
            if (threePoint.type== "watch_later")
            {
                var item= (sender as ListView).DataContext as RecommendItemModel;
                WatchLaterVM.Instance.AddToWatchlater(item.param);
                return;
            }
            if (threePoint.type == "dislike")
            {
                await recommendVM.Dislike(threePoint.idx, threePoint, null);
                return;
            }
            if (threePoint.type == "browser")
            {
                await Launcher.LaunchUriAsync(new Uri(threePoint.url));
                return;
            }

        }

        private async void ListDislike_ItemClick(object sender, ItemClickEventArgs e)
        {
           var reasons = e.ClickedItem as RecommendThreePointV2ItemReasonsModel;
           var threePoint=  (sender as GridView).DataContext as RecommendThreePointV2ItemModel;
            await recommendVM.Dislike(threePoint.idx, threePoint, reasons);
        }

        private void btnTop_Click(object sender, RoutedEventArgs e)
        {
            RecommendGridView.ScrollIntoView(RecommendGridView.Items[0]);
        }

        private void btnList_Click(object sender, RoutedEventArgs e)
        {
            IsGrid = false;
            //右下角按钮
            btnGrid.Visibility = Visibility.Visible;
            btnList.Visibility = Visibility.Collapsed;
            //设置
            SettingHelper.SetValue<int>(SettingHelper.UI.RECMEND_DISPLAY_MODE, 1);
            RecommendGridView.ItemHeight = 100;
            RecommendGridView.DesiredWidth =500;
            RecommendGridView.ItemTemplate = (DataTemplate)this.Resources["List"];
        }

        private void btnGrid_Click(object sender, RoutedEventArgs e)
        {
            IsGrid = true;
            //右下角按钮
            btnGrid.Visibility = Visibility.Collapsed;
            btnList.Visibility = Visibility.Visible;
            //设置
            SettingHelper.SetValue<int>(SettingHelper.UI.RECMEND_DISPLAY_MODE, 0);
            RecommendGridView.ItemHeight = 240;
            RecommendGridView.DesiredWidth = 260;
            RecommendGridView.ItemTemplate = (DataTemplate)this.Resources["Grid"];
        }
    }
  
}
