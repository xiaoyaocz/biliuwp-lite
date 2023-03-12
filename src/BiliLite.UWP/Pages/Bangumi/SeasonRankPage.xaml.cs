using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Modules.Season;
using BiliLite.Services;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
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

        private void SeasonRankItemOpen(object sender, SeasonRankItemModel item, bool dontGoTo = false)
        {
            if (item == null) return;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Play,
                page = typeof(SeasonDetailPage),
                title = item.title,
                parameters = item.season_id,
                dontGoTo = dontGoTo
            });
        }

        private void AdaptiveGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as SeasonRankItemModel;
            SeasonRankItemOpen(sender, item);
        }

        private void AdaptiveGridView_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!e.IsMiddleButtonNewTap(sender)) return;
            var element = e.OriginalSource as FrameworkElement;
            var item = element.DataContext as SeasonRankItemModel;
            SeasonRankItemOpen(sender, item, true);
        }
    }
}
