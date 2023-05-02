using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Modules;
using BiliLite.Services;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
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
            if (e.NavigationMode == NavigationMode.New)
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
            if (data.Items == null || data.Items.Count == 0)
            {
                await rankVM.LoadRankDetail(data);
            }
        }

        private void AdaptiveGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as RankItemModel;
            RankItemModelOpen(sender, item);
        }

        private void RankItemModelOpen(object sender, RankItemModel item, bool dontGoTo = false)
        {
            if (item == null) return;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Play,
                page = typeof(VideoDetailPage),
                title = item.title,
                parameters = item.aid,
                dontGoTo = dontGoTo
            });
        }

        private void AdaptiveGridView_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!e.IsMiddleButtonNewTap(sender)) return;
            var element = e.OriginalSource as FrameworkElement;
            var item = element.DataContext as RankItemModel;
            RankItemModelOpen(sender, item, true);
        }

        private void AddToWatchLater_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as MenuFlyoutItem).DataContext as RankItemModel;
            Modules.User.WatchLaterVM.Instance.AddToWatchlater(data.aid);
        }
    }
}
