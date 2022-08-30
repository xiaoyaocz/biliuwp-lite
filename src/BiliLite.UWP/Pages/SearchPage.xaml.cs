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
        Anchor=3,
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
            par.keyword= par.keyword.TrimStart('@');
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
            if (string.IsNullOrEmpty( txtKeyword.Text))
            {
                Utils.ShowMessageToast("关键字不能为空啊，喂(#`O′)");
                return;
            }

            if (await MessageCenter.HandelUrl(txtKeyword.Text))
            {
                return;
            }
            txtKeyword.Text = txtKeyword.Text.TrimStart('@');
            foreach (var item in searchVM.SearchItems)
            {
                item.Keyword = txtKeyword.Text;
                item.Area= searchVM.Area.area;
                item.Page = 1;
                item.HasData = false;
            }
            searchVM.SelectItem.Refresh();
            ChangeTitle("搜索:"+txtKeyword.Text);
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
                MessageCenter.ChangeTitle(title);
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
           
            if (e.ClickedItem is SearchVideoItem)
            {
                var data = e.ClickedItem as SearchVideoItem;
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Play,
                    page = typeof(VideoDetailPage),
                    title = data.title,
                    parameters = data.aid
                });
                return;
            }
            if (e.ClickedItem is SearchAnimeItem)
            {
                var data = e.ClickedItem as SearchAnimeItem;
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Play,
                    page = typeof(SeasonDetailPage),
                    title = data.title,
                    parameters = data.season_id
                });
                return;
            }
            if (e.ClickedItem is SearchUserItem)
            {
                var data = e.ClickedItem as SearchUserItem;
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Contact,
                    title = data.uname,
                    page = typeof(UserInfoPage),
                    parameters = data.mid
                });
                return;
            }
            if (e.ClickedItem is SearchLiveRoomItem)
            {
                var data = e.ClickedItem as SearchLiveRoomItem;
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Play,
                    title = data.title,
                    page = typeof(LiveDetailPage),
                    parameters = data.roomid
                });
                return;
            }
            if (e.ClickedItem is SearchArticleItem)
            {
                var data = e.ClickedItem as SearchArticleItem;
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Document,
                    page = typeof(WebPage),
                    title = data.title,
                    parameters = "https://www.bilibili.com/read/cv" + data.id
                });
                return;
            }
            if (e.ClickedItem is SearchTopicItem)
            {
                var data = e.ClickedItem as SearchTopicItem;
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Document,
                    page = typeof(WebPage),
                    title = data.title,
                    parameters = data.arcurl
                });
                return;
            }
        }

        private void cbArea_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbArea.SelectedItem!=null)
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
