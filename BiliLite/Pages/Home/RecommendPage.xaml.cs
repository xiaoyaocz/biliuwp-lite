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

namespace BiliLite.Pages.Home
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class RecommendPage : Page
    {
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
                await recommendVM.GetRecommend();
            }
           

        }

   

        private async void RecommendGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as Modules.RecommendItemModel;
            if (data.uri == null&&data.ad_info!=null)
            {
                MessageCenter.OpenNewWindow(this, new NavigationInfo()
                {
                    icon = Symbol.World,
                    page = typeof(WebPage),
                    parameters = data.ad_info.creative_content.click_url?? data.ad_info.creative_content.url,
                    title = "广告"
                });
                return;
            }
            if (await MessageCenter.HandelUrl(data.uri))
            {
                return;
            }
           
        }

   
        private void RefreshContainer_RefreshRequested(Microsoft.UI.Xaml.Controls.RefreshContainer sender, Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs args)
        {
             recommendVM.Refresh();
        }

        private void scrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight-100)
            {
                recommendVM.LoadMore();
            }
        }

        private async void BannerItem_Click(object sender, RoutedEventArgs e)
        {
            await MessageCenter.HandelUrl(((sender as HyperlinkButton).DataContext as RecommendBannerItemModel).uri);
           
        }

        private void ListMenu_ItemClick(object sender, ItemClickEventArgs e)
        {
            if ((e.ClickedItem as RecommendThreePointV2ItemModel).type== "watch_later")
            {
                Utils.ShowMessageToast("添加到稍后再看");
                return;
            }
           
        }

        private async void ListDislike_ItemClick(object sender, ItemClickEventArgs e)
        {
           var reasons = e.ClickedItem as RecommendThreePointV2ItemReasonsModel;
           var threePoint=  (sender as GridView).DataContext as RecommendThreePointV2ItemModel;
            await recommendVM.Dislike(threePoint.idx, threePoint, reasons);
        }
    }
  
}
