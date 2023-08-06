using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Modules.User;
using BiliLite.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using BiliLite.Models.Common.Recommend;
using BiliLite.ViewModels.Home;
using Microsoft.Extensions.DependencyInjection;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages.Home
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class RecommendPage : Page
    {
        #region Fields

        private bool m_isGrid = true;
        private readonly RecommendPageViewModel m_viewModel;

        #endregion

        #region Constructors

        public RecommendPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = SettingService.GetValue<bool>(SettingConstants.UI.CACHE_HOME, true) ? NavigationCacheMode.Enabled : NavigationCacheMode.Disabled;
            m_viewModel = App.ServiceProvider.GetRequiredService<RecommendPageViewModel>();
            this.DataContext = m_viewModel;
        }

        #endregion

        #region Protected Methods
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode != NavigationMode.New) return;
            if (m_isGrid)
            {
                RecommendGridView.ItemTemplate = (DataTemplate)this.Resources["Grid"];
            }

            SetListDisplay();
            await m_viewModel.GetRecommend();
            if (!SettingService.GetValue<bool>("推荐右键提示", true)) return;
            SettingService.SetValue("推荐右键提示", false);
            Notify.ShowMessageToast("右键或长按项目可以进行更多操作哦~", 5);
        }

        #endregion

        #region Private Methods

        private void SetListDisplay()
        {
            var grid = SettingService.GetValue<int>(SettingConstants.UI.RECMEND_DISPLAY_MODE, 0) == 0;
            if (grid == m_isGrid) return;
            m_isGrid = grid;
            if (grid)
            {
                BtnGrid_Click(this, null);
            }
            else
            {
                BtnList_Click(this, null);
            }
        }

        private async void RecommendGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as RecommendItemModel;
            await VideoItemClicked(data);
        }

        private void RefreshContainer_RefreshRequested(Microsoft.UI.Xaml.Controls.RefreshContainer sender, Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs args)
        {
            m_viewModel.Refresh();
        }

        private async void BannerItem_Click(object sender, RoutedEventArgs e)
        {
            await MessageCenter.HandelUrl(((sender as HyperlinkButton).DataContext as RecommendBannerItemModel).Uri);
        }

        private async void ListMenu_ItemClick(object sender, ItemClickEventArgs e)
        {
            var threePoint = e.ClickedItem as RecommendThreePointV2ItemModel;
            switch (threePoint.Type)
            {
                case "watch_later":
                {
                    var item = (sender as ListView).DataContext as RecommendItemModel;
                    WatchLaterVM.Instance.AddToWatchlater(item.Param);
                    return;
                }
                case "dislike":
                    await m_viewModel.Dislike(threePoint.Idx, threePoint, null);
                    return;
                case "browser":
                    await Launcher.LaunchUriAsync(new Uri(threePoint.Url));
                    return;
            }
        }

        private async void ListDislike_ItemClick(object sender, ItemClickEventArgs e)
        {
            var reasons = e.ClickedItem as RecommendThreePointV2ItemReasonsModel;
            var threePoint = (sender as GridView).DataContext as RecommendThreePointV2ItemModel;
            await m_viewModel.Dislike(threePoint.Idx, threePoint, reasons);
        }

        private async void RecommendItemGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!e.IsMiddleButtonNewTap(sender)) return;
            var element = e.OriginalSource as FrameworkElement;
            var data = element.DataContext as RecommendItemModel;
            await VideoItemClicked(data, true);
        }

        private async Task VideoItemClicked(RecommendItemModel data, bool dontGoTo = false)
        {
            if (data == null)
                if (data.Uri == null && data.AdInfo != null)
                {
                    var url = data.AdInfo.CreativeContent.Url;
                    if (!url.Contains("http://") && !url.Contains("https://"))
                    {
                        url = data.AdInfo.CreativeContent.ClickUrl ?? data.AdInfo.CreativeContent.Url;
                    }
                    await MessageCenter.HandelUrl(url, dontGoTo);
                    
                    return;
                }
            if (data.CardGoto == "new_tunnel")
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Favorite,
                    page = typeof(User.FavoritePage),
                    parameters = User.OpenFavoriteType.Bangumi,
                    title = "我的收藏",
                    dontGoTo = dontGoTo
                });
                return;
            }
            if (await MessageCenter.HandelUrl(data.Uri, dontGoTo))
            {
                return;
            }
            var browserUri = data.ThreePointV2.FirstOrDefault(x => x.Type == "browser")?.Url ?? "";
            if (!string.IsNullOrEmpty(browserUri) && await MessageCenter.HandelUrl(browserUri, dontGoTo))
            {
                return;
            }
        }

        private void BtnTop_Click(object sender, RoutedEventArgs e)
        {
            RecommendGridView.ScrollIntoView(RecommendGridView.Items[0]);
        }

        private void BtnList_Click(object sender, RoutedEventArgs e)
        {
            m_isGrid = false;
            //右下角按钮
            BtnGrid.Visibility = Visibility.Visible;
            BtnList.Visibility = Visibility.Collapsed;
            //设置
            SettingService.SetValue<int>(SettingConstants.UI.RECMEND_DISPLAY_MODE, 1);
            RecommendGridView.ItemHeight = 100;
            RecommendGridView.DesiredWidth = 500;
            RecommendGridView.ItemTemplate = (DataTemplate)this.Resources["List"];
        }

        private void BtnGrid_Click(object sender, RoutedEventArgs e)
        {
            m_isGrid = true;
            //右下角按钮
            BtnGrid.Visibility = Visibility.Collapsed;
            BtnList.Visibility = Visibility.Visible;
            //设置
            SettingService.SetValue<int>(SettingConstants.UI.RECMEND_DISPLAY_MODE, 0);
            RecommendGridView.ItemHeight = 240;
            RecommendGridView.DesiredWidth = 260;
            RecommendGridView.ItemTemplate = (DataTemplate)this.Resources["Grid"];
        }

        #endregion
    }
}
