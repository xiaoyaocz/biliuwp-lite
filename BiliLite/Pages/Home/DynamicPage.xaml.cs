using BiliLite.Helpers;
using BiliLite.Modules;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
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
    public sealed partial class DynamicPage : Page
    {
        DynamicVM dynamicVM;
        public DynamicPage()
        {
            this.InitializeComponent();
            dynamicVM = new DynamicVM();
            dynamicVM.dynamicItemDataTemplateSelector.resource = this.Resources;
            this.DataContext = dynamicVM;
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
            if (e.NavigationMode == NavigationMode.New && dynamicVM.Items == null)
            {
                await dynamicVM.GetDynamicItems();
            }
        }

        private void RefreshContainer_RefreshRequested(Microsoft.UI.Xaml.Controls.RefreshContainer sender, Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs args)
        {
            dynamicVM.Refresh();
        }

        private void AdaptiveGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
           var item= e.ClickedItem as DynamicItemModel;
            if (item.desc.type==8)
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo() { 
                    icon= Symbol.Play,
                    page=typeof(VideoDetailPage),
                    parameters=item.video.aid,
                    title=item.video.title
                });
            }else if (item.desc.type == 512)
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Play,
                    page = typeof(SeasonDetailPage),
                    parameters = item.season.season.season_id,
                    title = item.season.season.title
                });
            }
        }

        private void AddToWatchLater_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as MenuFlyoutItem).DataContext as DynamicItemModel;
            Modules.User.WatchLaterVM.Instance.AddToWatchlater(data.video.aid);
        }
    }
}
