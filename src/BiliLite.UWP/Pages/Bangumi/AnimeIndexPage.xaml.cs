using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Modules;
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
    public sealed partial class AnimeIndexPage : BasePage
    {
        private SeasonIndexParameter indexParameter;
        readonly SeasonIndexVM seasonIndexVM;
        public AnimeIndexPage()
        {
            this.InitializeComponent();
            Title = "剧集索引";
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            seasonIndexVM = new SeasonIndexVM();
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                if (e.Parameter == null)
                {
                    indexParameter = new SeasonIndexParameter();
                }
                else
                {
                    indexParameter=e.Parameter as SeasonIndexParameter;
                }
              
                seasonIndexVM.Parameter = indexParameter;
                await seasonIndexVM.LoadConditions();
                if (seasonIndexVM.Conditions != null)
                {
                    await seasonIndexVM.LoadResult();
                }
            }
        }

        private void SeasonIndexResultItemOpen(object sender, SeasonIndexResultItemModel item, bool dontGoTo = false)
        {
            if (item == null) return;
            MessageCenter.NavigateToPage(sender, new NavigationInfo()
            {
                icon = Symbol.Play,
                page = typeof(Pages.SeasonDetailPage),
                parameters = item.season_id,
                title = item.title,
                dontGoTo = dontGoTo
            });
        }

        private void ListResult_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as SeasonIndexResultItemModel;
            SeasonIndexResultItemOpen(sender, item);
        }

        private void MyAdaptiveGridView_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!e.IsMiddleButtonNewTap(sender)) return;
            var element = e.OriginalSource as FrameworkElement;
            var item = element.DataContext as SeasonIndexResultItemModel;
            SeasonIndexResultItemOpen(sender, item, true);
        }

        private async void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combox = sender as ComboBox;
            if (combox.SelectedItem == null || seasonIndexVM.ConditionsLoading|| seasonIndexVM.Loading)
            {
                return;
            }
            seasonIndexVM.Page = 1;
            await seasonIndexVM.LoadResult();
        }
    }

    
}
