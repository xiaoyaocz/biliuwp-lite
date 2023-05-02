using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Modules;
using BiliLite.Services;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages
{
    public enum SearchType
    {
        /// <summary>
        /// 视频
        /// </summary>
        Video = 0,
        /// <summary>
        /// 番剧
        /// </summary>
        Anime = 1,
        /// <summary>
        /// 直播
        /// </summary>
        Live = 2,
        /// <summary>
        /// 主播
        /// </summary>
        Anchor = 3,
        /// <summary>
        /// 用户
        /// </summary>
        User = 4,
        /// <summary>
        /// 影视
        /// </summary>
        Movie = 5,
        /// <summary>
        /// 专栏
        /// </summary>
        Article = 6,
        /// <summary>
        /// 话题
        /// </summary>
        Topic = 7
    }
    public class SearchParameter
    {
        public string keyword { get; set; }
        public SearchType searchType { get; set; } = SearchType.Video;
    }
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SearchPage : BasePage
    {
        SearchVM searchVM;
        public SearchPage()
        {
            this.InitializeComponent();
            Title = "搜索";
            searchVM = new SearchVM();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SearchParameter par = new SearchParameter();
            if (e.Parameter is SearchParameter)
            {
                par = e.Parameter as SearchParameter;
            }
            else
            {
                par.keyword = e.Parameter.ToString();
            }
            par.keyword = par.keyword.TrimStart('@');
            txtKeyword.Text = par.keyword;
            foreach (var item in searchVM.SearchItems)
            {
                item.Keyword = par.keyword;
                item.Area = searchVM.Area.area;
            }
            pivot.SelectedIndex = (int)par.searchType;
        }

        private async void txtKeyword_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var queryText = args.QueryText;
            if (string.IsNullOrEmpty(queryText))
            {
                Notify.ShowMessageToast("关键字不能为空啊，喂(#`O′)");
                return;
            }

            if (await MessageCenter.HandelUrl(queryText))
            {
                return;
            }
            queryText = queryText.TrimStart('@');
            foreach (var item in searchVM.SearchItems)
            {
                item.Keyword = queryText;
                item.Area = searchVM.Area.area;
                item.Page = 1;
                item.HasData = false;
            }
            searchVM.SelectItem.Refresh();
            ChangeTitle("搜索:" + queryText);
        }

        public void ChangeTitle(string title)
        {
            if ((this.Parent as Frame).Parent is TabViewItem)
            {
                if (this.Parent != null)
                {
                    ((this.Parent as Frame).Parent as TabViewItem).Header = title;
                }
            }
            else
            {
                MessageCenter.ChangeTitle(this, title);
            }
        }
        private async void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot.SelectedItem != null)
            {
                var item = pivot.SelectedItem as ISearchVM;
                if (!item.HasData && !item.Loading)
                {
                    await item.LoadData();
                }
            }

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var data = (sender as ComboBox).DataContext as ISearchVM;
            if (data.HasData && !data.Loading)
            {
                data.Refresh();
            }
        }
        private void Search_ItemClick(object sender, ItemClickEventArgs e)
        {
            SearchItemModelOpen(e.ClickedItem);
        }

        private void SearchItemModelOpen(object item, bool dontGoTo = false)
        {
            if (item is SearchVideoItem)
            {
                var data = item as SearchVideoItem;
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Play,
                    page = typeof(VideoDetailPage),
                    title = data.title,
                    parameters = data.aid,
                    dontGoTo = dontGoTo
                });
                return;
            }
            if (item is SearchAnimeItem)
            {
                var data = item as SearchAnimeItem;
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Play,
                    page = typeof(SeasonDetailPage),
                    title = data.title,
                    parameters = data.season_id,
                    dontGoTo = dontGoTo
                });
                return;
            }
            if (item is SearchUserItem)
            {
                var data = item as SearchUserItem;
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Contact,
                    title = data.uname,
                    page = typeof(UserInfoPage),
                    parameters = data.mid,
                    dontGoTo = dontGoTo
                });
                return;
            }
            if (item is SearchLiveRoomItem)
            {
                var data = item as SearchLiveRoomItem;
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Play,
                    title = data.title,
                    page = typeof(LiveDetailPage),
                    parameters = data.roomid,
                    dontGoTo = dontGoTo
                });
                return;
            }
            if (item is SearchArticleItem)
            {
                var data = item as SearchArticleItem;
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Document,
                    page = typeof(WebPage),
                    title = data.title,
                    parameters = "https://www.bilibili.com/read/cv" + data.id,
                    dontGoTo = dontGoTo
                });
                return;
            }
            if (item is SearchTopicItem)
            {
                var data = item as SearchTopicItem;
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Document,
                    page = typeof(WebPage),
                    title = data.title,
                    parameters = data.arcurl,
                    dontGoTo = dontGoTo
                });
                return;
            }
        }

        private void Search_ItemPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!e.IsMiddleButtonNewTap(sender)) return;
            var element = e.OriginalSource as FrameworkElement;
            var item = element.DataContext;
            SearchItemModelOpen(item, true);
        }

        private void cbArea_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbArea.SelectedItem != null)
            {
                foreach (var item in searchVM.SearchItems)
                {
                    item.Area = searchVM.Area.area;
                    item.Page = 1;
                    item.HasData = false;
                }
                searchVM.SelectItem.Refresh();
            }
        }

        private async void txtKeyword_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput) return;
            var text = sender.Text;
            var suggestSearchContents = await new SearchService().GetSearchSuggestContents(text);
            if (searchVM.SuggestSearchContents == null)
            {
                searchVM.SuggestSearchContents = new System.Collections.ObjectModel.ObservableCollection<string>(suggestSearchContents);
            }
            else
            {
                searchVM.SuggestSearchContents.ReplaceRange(suggestSearchContents);
            }
        }
    }
    public class SearchDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate VideoTemplate { get; set; }
        public DataTemplate AnimeTemplate { get; set; }
        public DataTemplate TestTemplate { get; set; }
        public DataTemplate LiveRoomTemplate { get; set; }
        public DataTemplate UserTemplate { get; set; }
        public DataTemplate ArticTemplate { get; set; }
        public DataTemplate TopicTemplate { get; set; }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var data = item as ISearchVM;
            switch (data.SearchType)
            {
                case SearchType.Video:
                    return VideoTemplate;
                case SearchType.Anime:
                case SearchType.Movie:
                    return AnimeTemplate;
                case SearchType.User:
                    return UserTemplate;
                case SearchType.Live:
                    return LiveRoomTemplate;
                case SearchType.Article:
                    return ArticTemplate;
                case SearchType.Topic:
                    return TopicTemplate;
                case SearchType.Anchor:
                    return TestTemplate;
                default:
                    return TestTemplate;
            }


        }
    }
}
