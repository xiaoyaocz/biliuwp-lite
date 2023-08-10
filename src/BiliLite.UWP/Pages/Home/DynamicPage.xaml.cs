using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Services;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using BiliLite.Models.Common.Dynamic;
using BiliLite.ViewModels.Home;
using Microsoft.Extensions.DependencyInjection;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages.Home
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DynamicPage : Page
    {
        private readonly DynamicPageViewModel m_viewModel;

        public DynamicPage()
        {
            this.InitializeComponent();
            m_viewModel = App.ServiceProvider.GetService<DynamicPageViewModel>();
            m_viewModel.DynamicItemDataTemplateSelector.Resource = this.Resources;
            this.DataContext = m_viewModel;
            this.NavigationCacheMode = SettingService.GetValue<bool>(SettingConstants.UI.CACHE_HOME, true) ? NavigationCacheMode.Enabled : NavigationCacheMode.Disabled;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New && m_viewModel.Items == null)
            {
                await m_viewModel.GetDynamicItems();
            }
        }

        private void RefreshContainer_RefreshRequested(Microsoft.UI.Xaml.Controls.RefreshContainer sender, Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs args)
        {
            m_viewModel.Refresh();
        }

        private void AdaptiveGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as DynamicItemModel;
            DynamicItemModelOpen(sender, item);
        }

        private void DynamicItemModelOpen(object sender, DynamicItemModel item, bool dontGoTo = false)
        {
            if (item == null) return;

            if (item.Desc.Type == 512)
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Play,
                    page = typeof(SeasonDetailPage),
                    parameters = item.Season.Season.SeasonId,
                    title = item.Season.Season.Title,
                    dontGoTo = dontGoTo
                });
            }
            else if (item.Video.SeasonId != 0)
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Play,
                    page = typeof(SeasonDetailPage),
                    parameters = item.Video.SeasonId,
                    title = item.Video.Title,
                    dontGoTo = dontGoTo
                });
            }
            else
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Play,
                    page = typeof(VideoDetailPage),
                    parameters = item.Video.Aid,
                    title = item.Video.Title,
                    dontGoTo = dontGoTo
                });
            }
        }

        private void AdaptiveGridView_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!e.IsMiddleButtonNewTap(sender)) return;
            var element = e.OriginalSource as FrameworkElement;
            var item = element.DataContext as DynamicItemModel;
            DynamicItemModelOpen(sender, item, true);
        }

        private void AddToWatchLater_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as MenuFlyoutItem).DataContext as DynamicItemModel;
            Modules.User.WatchLaterVM.Instance.AddToWatchlater(data.Video.Aid);
        }
    }
}
