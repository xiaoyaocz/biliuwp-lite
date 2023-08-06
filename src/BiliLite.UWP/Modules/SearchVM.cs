using BiliLite.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using Newtonsoft.Json;
using BiliLite.Pages;
using BiliLite.Models.Requests.Api;
using BiliLite.Services;

namespace BiliLite.Modules
{
    public class ISearchVM : IModules
    {
        public SearchType SearchType { get; set; }
        public SearchAPI searchAPI;
        public ISearchVM()
        {
            searchAPI = new SearchAPI();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
        }
        public string Title { get; set; }

        private string _keyword;
        public string Keyword
        {
            get { return _keyword; }
            set { _keyword = value; }
        }
        public ICommand LoadMoreCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }

        public string Area { get; set; } = "";

        public int Page { get; set; } = 1;
        private bool _loading = false;

        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }
        private bool _Nothing = false;
        public bool Nothing
        {
            get { return _Nothing; }
            set { _Nothing = value; DoPropertyChanged("Nothing"); }
        }

        private bool _ShowLoadMore = false;
        public bool ShowLoadMore
        {
            get { return _ShowLoadMore; }
            set { _ShowLoadMore = value; DoPropertyChanged("ShowLoadMore"); }
        }

        public async virtual void Refresh()
        {
            HasData = false;
            Page = 1;
            await LoadData();
        }
        public async virtual void LoadMore()
        {
            await LoadData();
        }
        public bool HasData { get; set; } = false;

        public virtual Task LoadData()
        {
            throw new NotImplementedException();
        }
    }

    public class SearchVM : IModules
    {
        /// <summary>
        /// 搜索请求需要cookie
        /// </summary>
        public static string cookie = "";
        public SearchVM()
        {
            Area = Areas[0];
            SearchItems = new ObservableCollection<ISearchVM>() {
                new SearchVideoVM()
                {
                    Title="视频",
                    SearchType= SearchType.Video,
                    Area= Area.area
                },
                new SearchAnimeVM()
                {
                    Title="番剧",
                    SearchType= SearchType.Anime,
                    Area= Area.area
                },
                new SearchLiveRoomVM()
                {
                    Title="直播",
                    SearchType= SearchType.Live,
                    Area= Area.area
                },
                //new SearchLiveRoomVM()
                //{
                //    Title="主播",
                //    SearchType= SearchType.Anchor 
                //},
                new SearchUserVM()
                {
                    Title="用户",
                    SearchType= SearchType.User,
                    Area= Area.area
                },
                new SearchAnimeVM()
                {
                    Title="影视",
                    SearchType= SearchType.Movie,
                    Area= Area.area
                },
                new SearchArticleVM()
                {
                    Title="专栏",
                    SearchType= SearchType.Article,
                    Area= Area.area
                },
                new SearchTopicVM()
                {
                    Title="话题",
                    SearchType= SearchType.Topic,
                    Area= Area.area
                }
            };
            SelectItem = SearchItems[0];

        }
        private ObservableCollection<ISearchVM> _items;
        public ObservableCollection<ISearchVM> SearchItems
        {
            get { return _items; }
            set { _items = value; DoPropertyChanged("SearchItems"); }
        }

        public List<SearchArea> Areas { get; set; } = new List<SearchArea>()
        {
            new SearchArea("默认地区",""),
            new SearchArea("大陆地区","cn"),
            new SearchArea("香港地区","hk"),
            new SearchArea("台湾地区","tw"),
        };
        public SearchArea Area { get; set; }

        private ISearchVM _SelectItem;
        public ISearchVM SelectItem
        {
            get { return _SelectItem; }
            set { _SelectItem = value; }
        }

        private ObservableCollection<string> m_suggestSearchContents;

        public ObservableCollection<string> SuggestSearchContents
        {
            get => m_suggestSearchContents;
            set
            {
                m_suggestSearchContents = value;
                DoPropertyChanged("SuggestSearchContents");
            }
        }
    }
    public class SearchVideoVM : ISearchVM
    {
        ILogger _logger = GlobalLogger.FromCurrentType();

        public SearchVideoVM()
        {
            OrderFilters = new List<SearchFilterItem>() {
                new SearchFilterItem("综合排序",""),
                new SearchFilterItem("最多点击","click"),
                new SearchFilterItem("最新发布","pubdate"),
                new SearchFilterItem("最多弹幕","dm"),
                new SearchFilterItem("最多收藏","stow")
            };
            SelectOrder = OrderFilters[0];
            DurationFilters = new List<SearchFilterItem>() {
                new SearchFilterItem("全部时长",""),
                new SearchFilterItem("10分钟以下","1"),
                new SearchFilterItem("10-30分钟","2"),
                new SearchFilterItem("30-60分钟","3"),
                new SearchFilterItem("60分钟以上","4")
            };
            SelectDuration = DurationFilters[0];
            RegionFilters = new List<SearchFilterItem>() {
                new SearchFilterItem("全部分区","0"),
            };
            foreach (var item in AppHelper.Regions.Where(x => x.children != null && x.children.Count != 0))
            {
                RegionFilters.Add(new SearchFilterItem(item.name, item.tid.ToString()));
            }
            SelectRegion = RegionFilters[0];
        }
        public List<SearchFilterItem> OrderFilters { get; set; }

        private SearchFilterItem _SelectOrder;
        public SearchFilterItem SelectOrder
        {
            get { return _SelectOrder; }
            set { _SelectOrder = value; }
        }

        public List<SearchFilterItem> DurationFilters { get; set; }

        private SearchFilterItem _SelectDuration;
        public SearchFilterItem SelectDuration
        {
            get { return _SelectDuration; }
            set { _SelectDuration = value; }
        }
        public List<SearchFilterItem> RegionFilters { get; set; }
        private SearchFilterItem _SelectRegion;
        public SearchFilterItem SelectRegion
        {
            get { return _SelectRegion; }
            set { _SelectRegion = value; }
        }


        private ObservableCollection<SearchVideoItem> _videos;
        public ObservableCollection<SearchVideoItem> Videos
        {
            get { return _videos; }
            set { _videos = value; DoPropertyChanged("Videos"); }
        }

        public async override Task LoadData()
        {
            try
            {
                if (Loading)
                {
                    return;
                }
                ShowLoadMore = false;
                Loading = true;
                Nothing = false;
                var results = await searchAPI.WebSearchVideo(Keyword, Page, SelectOrder.value, SelectDuration.value, SelectRegion.value, Area).Request();
                if (!results.status)
                {
                    throw new CustomizedErrorException(results.message);
                }
                var data = await results.GetJson<ApiDataModel<JObject>>();
                if (!data.success)
                {
                    throw new CustomizedErrorException(data.message);
                }
                var result = JsonConvert.DeserializeObject<ObservableCollection<SearchVideoItem>>(data.data["result"]?.ToString() ?? "[]");
                if (Page == 1)
                {
                    if (result == null || result.Count == 0)
                    {
                        Nothing = true;
                        Videos?.Clear();
                        return;
                    }
                    Videos = result;
                }
                else if (data.data != null)
                {
                    foreach (var item in result)
                    {
                        Videos.Add(item);
                    }
                }
                if (Page < data.data["numPages"].ToInt32())
                {
                    ShowLoadMore = true;
                    Page++;
                }
                HasData = true;

            }
            catch (Exception ex)
            {
                if (ex is CustomizedErrorException customizedErrorException)
                {
                    Notify.ShowMessageToast(ex.Message);
                    _logger.Error("搜索失败", ex);
                }

                var handel = HandelError<SearchVM>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }
    }

    public class SearchArticleVM : ISearchVM
    {
        public SearchArticleVM()
        {
            OrderFilters = new List<SearchFilterItem>() {
                new SearchFilterItem("默认排序","totalrank"),
                new SearchFilterItem("最多阅读","click"),
                new SearchFilterItem("最新发布","pubdate"),
                new SearchFilterItem("最多喜欢","attention"),
                new SearchFilterItem("最多评论","scores")
            };
            SelectOrder = OrderFilters[0];

            RegionFilters = new List<SearchFilterItem>() {
                new SearchFilterItem("全部分区","0"),
                new SearchFilterItem("动画","2"),
                new SearchFilterItem("游戏","1"),
                new SearchFilterItem("影视","28"),
                new SearchFilterItem("生活","3"),
                new SearchFilterItem("兴趣","29"),
                new SearchFilterItem("轻小说","16"),
                new SearchFilterItem("科技","17"),
            };

            SelectRegion = RegionFilters[0];
        }
        public List<SearchFilterItem> OrderFilters { get; set; }

        private SearchFilterItem _SelectOrder;
        public SearchFilterItem SelectOrder
        {
            get { return _SelectOrder; }
            set { _SelectOrder = value; }
        }

        public List<SearchFilterItem> DurationFilters { get; set; }


        public List<SearchFilterItem> RegionFilters { get; set; }
        private SearchFilterItem _SelectRegion;
        public SearchFilterItem SelectRegion
        {
            get { return _SelectRegion; }
            set { _SelectRegion = value; }
        }


        private ObservableCollection<SearchArticleItem> _Articles;
        public ObservableCollection<SearchArticleItem> Articles
        {
            get { return _Articles; }
            set { _Articles = value; DoPropertyChanged("Articles"); }
        }

        public async override Task LoadData()
        {
            try
            {
                if (Loading)
                {
                    return;
                }
                ShowLoadMore = false;
                Loading = true;
                Nothing = false;
                var results = await searchAPI.WebSearchArticle(Keyword, Page, SelectOrder.value, SelectRegion.value, Area).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        var result = JsonConvert.DeserializeObject<ObservableCollection<SearchArticleItem>>(data.data["result"]?.ToString() ?? "[]");
                        if (Page == 1)
                        {
                            if (result == null || result.Count == 0)
                            {
                                Nothing = true;
                                Articles?.Clear();
                                return;
                            }
                            Articles = result;
                        }
                        else
                        {
                            if (data.data != null)
                            {
                                foreach (var item in result)
                                {
                                    Articles.Add(item);
                                }
                            }
                        }
                        if (Page < data.data["numPages"].ToInt32())
                        {
                            ShowLoadMore = true;
                            Page++;
                        }
                        HasData = true;
                    }
                    else
                    {
                        Notify.ShowMessageToast(data.message);
                    }
                }
                else
                {
                    Notify.ShowMessageToast(results.message);
                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<SearchVM>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }


    }
    public class SearchAnimeVM : ISearchVM
    {
        public SearchAnimeVM()
        {
        }

        private ObservableCollection<SearchAnimeItem> _Animes;
        public ObservableCollection<SearchAnimeItem> Animes
        {
            get { return _Animes; }
            set { _Animes = value; DoPropertyChanged("Animes"); }
        }

        public async override Task LoadData()
        {
            try
            {
                if (Loading)
                {
                    return;
                }
                ShowLoadMore = false;
                Loading = true;
                Nothing = false;
                var api = searchAPI.WebSearchAnime(Keyword, Page, Area);
                if (this.SearchType == SearchType.Movie)
                {
                    api = searchAPI.WebSearchMovie(Keyword, Page, Area);
                }
                var results = await api.Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {

                        var result = JsonConvert.DeserializeObject<ObservableCollection<SearchAnimeItem>>(data.data["result"]?.ToString() ?? "[]");
                        if (Page == 1)
                        {
                            if (result == null || result.Count == 0)
                            {
                                Nothing = true;
                                Animes?.Clear();
                                return;
                            }
                            Animes = result;
                        }
                        else
                        {
                            if (data.data != null)
                            {
                                foreach (var item in result)
                                {
                                    Animes.Add(item);
                                }
                            }
                        }
                        if (Page < data.data["numPages"].ToInt32())
                        {
                            ShowLoadMore = true;
                            Page++;
                        }
                        HasData = true;
                    }
                    else
                    {
                        Notify.ShowMessageToast(data.message);
                    }
                }
                else
                {
                    Notify.ShowMessageToast(results.message);
                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<SearchVM>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }


    }
    public class SearchUserVM : ISearchVM
    {
        public SearchUserVM()
        {
            OrderFilters = new List<SearchFilterItem>() {
                new SearchFilterItem("默认排序","&order=&order_sort="),
                new SearchFilterItem("粉丝数由高到低","&order=fans&order_sort=0"),
                new SearchFilterItem("粉丝数由低到高","&order=fans&order_sort=1"),
                new SearchFilterItem("LV等级由高到低","&order=level&order_sort=0"),
                new SearchFilterItem("LV等级由低到高","&order=level&order_sort=1"),
            };
            SelectOrder = OrderFilters[0];
            TypeFilters = new List<SearchFilterItem>() {
                new SearchFilterItem("全部用户","&user_type=0"),
                new SearchFilterItem("UP主","&user_type=1"),
                new SearchFilterItem("普通用户","&user_type=2"),
                new SearchFilterItem("认证用户","&user_type=3")
            };
            SelectType = TypeFilters[0];

        }
        public List<SearchFilterItem> OrderFilters { get; set; }

        private SearchFilterItem _SelectOrder;
        public SearchFilterItem SelectOrder
        {
            get { return _SelectOrder; }
            set { _SelectOrder = value; }
        }

        public List<SearchFilterItem> TypeFilters { get; set; }

        private SearchFilterItem _SelectType;
        public SearchFilterItem SelectType
        {
            get { return _SelectType; }
            set { _SelectType = value; }
        }


        private ObservableCollection<SearchUserItem> _users;
        public ObservableCollection<SearchUserItem> Users
        {
            get { return _users; }
            set { _users = value; DoPropertyChanged("Users"); }
        }

        public async override Task LoadData()
        {
            try
            {
                if (Loading)
                {
                    return;
                }
                ShowLoadMore = false;
                Loading = true;
                Nothing = false;
                var results = await searchAPI.WebSearchUser(Keyword, Page, SelectOrder.value, SelectType.value, Area).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        var result = JsonConvert.DeserializeObject<ObservableCollection<SearchUserItem>>(data.data["result"]?.ToString() ?? "[]");
                        if (Page == 1)
                        {
                            if (result == null || result.Count == 0)
                            {
                                Nothing = true;
                                Users?.Clear();
                                return;
                            }
                            Users = result;
                        }
                        else
                        {
                            if (data.data != null)
                            {
                                foreach (var item in result)
                                {
                                    Users.Add(item);
                                }
                            }
                        }
                        if (Page < data.data["numPages"].ToInt32())
                        {
                            ShowLoadMore = true;
                            Page++;
                        }
                        HasData = true;
                    }
                    else
                    {
                        Notify.ShowMessageToast(data.message);
                    }
                }
                else
                {
                    Notify.ShowMessageToast(results.message);
                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<SearchVM>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }


    }
    public class SearchLiveRoomVM : ISearchVM
    {
        public SearchLiveRoomVM()
        {
        }

        private ObservableCollection<SearchLiveRoomItem> _Rooms;
        public ObservableCollection<SearchLiveRoomItem> Rooms
        {
            get { return _Rooms; }
            set { _Rooms = value; DoPropertyChanged("Rooms"); }
        }

        public async override Task LoadData()
        {
            try
            {
                if (Loading)
                {
                    return;
                }
                ShowLoadMore = false;
                Loading = true;
                Nothing = false;

                var results = await searchAPI.WebSearchLive(Keyword, Page, Area).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {

                        var result = JsonConvert.DeserializeObject<ObservableCollection<SearchLiveRoomItem>>(data.data["result"]["live_room"]?.ToString() ?? "[]");
                        if (Page == 1)
                        {
                            if (result == null || result.Count == 0)
                            {
                                Nothing = true;
                                Rooms?.Clear();
                                return;
                            }
                            Rooms = result;
                        }
                        else
                        {
                            if (data.data != null)
                            {
                                foreach (var item in result)
                                {
                                    Rooms.Add(item);
                                }
                            }
                        }
                        if (Page < data.data["pageinfo"]["live_room"]["numPages"].ToInt32())
                        {
                            ShowLoadMore = true;
                            Page++;
                        }
                        HasData = true;
                    }
                    else
                    {
                        Notify.ShowMessageToast(data.message);
                    }
                }
                else
                {
                    Notify.ShowMessageToast(results.message);
                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<SearchVM>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }


    }
    public class SearchTopicVM : ISearchVM
    {
        public SearchTopicVM()
        {
        }

        private ObservableCollection<SearchTopicItem> _Topics;
        public ObservableCollection<SearchTopicItem> Topics
        {
            get { return _Topics; }
            set { _Topics = value; DoPropertyChanged("Topics"); }
        }

        public async override Task LoadData()
        {
            try
            {
                if (Loading)
                {
                    return;
                }
                ShowLoadMore = false;
                Loading = true;
                Nothing = false;
                var results = await searchAPI.WebSearchTopic(Keyword, Page, Area).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {

                        var result = JsonConvert.DeserializeObject<ObservableCollection<SearchTopicItem>>(data.data["result"]?.ToString() ?? "[]");
                        if (Page == 1)
                        {
                            if (result == null || result.Count == 0)
                            {
                                Nothing = true;
                                Topics?.Clear();
                                return;
                            }
                            Topics = result;
                        }
                        else
                        {
                            if (data.data != null)
                            {
                                foreach (var item in result)
                                {
                                    Topics.Add(item);
                                }
                            }
                        }
                        if (Page < data.data["numPages"].ToInt32())
                        {
                            ShowLoadMore = true;
                            Page++;
                        }
                        HasData = true;
                    }
                    else
                    {
                        Notify.ShowMessageToast(data.message);
                    }
                }
                else
                {
                    Notify.ShowMessageToast(results.message);
                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<SearchVM>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }


    }

    public class SearchFilterItem
    {
        public SearchFilterItem(string name, string value)
        {
            this.name = name;
            this.value = value;
        }
        public string name { get; set; }
        public string value { get; set; }
    }

    public class SearchArea
    {
        public SearchArea(string name, string area)
        {
            this.name = name;
            this.area = area;
        }
        public string name { get; set; }
        public string area { get; set; }
    }
    public class SearchVideoItem
    {
        public string type { get; set; }
        public string typename { get; set; }
        public string author { get; set; }
        public string id { get; set; }
        public string aid { get; set; }
        private string _title;

        public string title
        {
            get { return _title; }
            set
            {

                _title = System.Web.HttpUtility.HtmlDecode(value.Replace("<em class=\"keyword\">", "").Replace("</em>", ""));
            }
        }
        public string tag { get; set; }
        public int play { get; set; }
        public int video_review { get; set; }
        public int review { get; set; }
        public int favorites { get; set; }
        public string duration { get; set; }
        private string _pic;
        public string pic
        {
            get { return _pic; }
            set { _pic = "https:" + value; }
        }

    }
    public class SearchAnimeItem
    {
        public string type { get; set; }
        public string season_id { get; set; }
        public string media_id { get; set; }
        private string _title;

        public string title
        {
            get { return _title; }
            set
            {

                _title = System.Web.HttpUtility.HtmlDecode(value.Replace("<em class=\"keyword\">", "").Replace("</em>", ""));
            }
        }
        public string areas { get; set; }
        public string cv { get; set; }
        public string styles { get; set; }
        public string desc { get; set; }
        public long pubtime { get; set; }
        public string season_type_name { get; set; }

        private string _pic;
        public string cover
        {
            get { return _pic; }
            set { _pic = value; }
        }

        public string angle_title { get; set; }
        public bool showBadge
        {
            get
            {
                return !string.IsNullOrEmpty(angle_title);
            }
        }
    }

    public class SearchUserItem
    {
        public string mid { get; set; }
        public string uname { get; set; }
        private string _pic;
        public string upic
        {
            get { return _pic; }
            set { _pic = "https:" + value; }
        }

        public int level { get; set; }
        public int videos { get; set; }
        public int fans { get; set; }
        public int is_upuser { get; set; }
        public string lv
        {
            get
            {
                return $"ms-appx:///Assets/Icon/lv{level}.png";
            }
        }
        public SearchUserOfficialVerifyItem official_verify { get; set; }
        public string Verify
        {
            get
            {
                if (official_verify == null)
                {
                    return "";
                }
                switch (official_verify.type)
                {
                    case 0:
                        return Constants.App.VERIFY_PERSONAL_IMAGE;
                    case 1:
                        return Constants.App.VERIFY_OGANIZATION_IMAGE;
                    default:
                        return Constants.App.TRANSPARENT_IMAGE;
                }
            }
        }
        public string usign { get; set; }
        public string sign
        {
            get
            {
                if (official_verify != null && !string.IsNullOrEmpty(official_verify.desc))
                {
                    return official_verify.desc;
                }
                return usign;
            }
        }
    }
    public class SearchLiveRoomItem
    {

        public string roomid { get; set; }

        private string _title;

        public string title
        {
            get { return _title; }
            set
            {

                _title = System.Web.HttpUtility.HtmlDecode(value.Replace("<em class=\"keyword\">", "").Replace("</em>", ""));
            }
        }
        public string uname { get; set; }
        public string tags { get; set; }
        public string cate_name { get; set; }
        public int online { get; set; }


        private string _user_cover;
        public string user_cover
        {
            get { return _user_cover; }
            set { _user_cover = "https:" + value; }
        }
        private string _uface;
        public string uface
        {
            get { return _uface; }
            set { _uface = "https:" + value; }
        }
        private string _cover;
        public string cover
        {
            get { return _cover; }
            set { _cover = "https:" + value; }
        }

    }
    public class SearchUserOfficialVerifyItem
    {
        public string desc { get; set; }
        public int type { get; set; }
    }
    public class SearchArticleItem
    {

        public string mid { get; set; }

        private string _title;

        public string title
        {
            get { return _title; }
            set
            {
                _title = System.Web.HttpUtility.HtmlDecode(value.Replace("<em class=\"keyword\">", "").Replace("</em>", ""));
            }
        }
        public string category_name { get; set; }
        public string type { get; set; }
        public string desc { get; set; }
        public int like { get; set; }
        public int view { get; set; }
        public int reply { get; set; }
        public string id { get; set; }
        public List<string> image_urls { get; set; }
        public string cover
        {
            get
            {
                if (image_urls != null && image_urls.Count != 0)
                {
                    return "https:" + image_urls[0];
                }
                return null;
            }
        }
    }
    public class SearchTopicItem
    {
        public string arcurl { get; set; }

        private string _title;

        public string title
        {
            get { return _title; }
            set
            {
                _title = System.Web.HttpUtility.HtmlDecode(value.Replace("<em class=\"keyword\">", "").Replace("</em>", ""));
            }
        }

        private string _description;

        public string description
        {
            get { return _description; }
            set
            {
                _description = value.Replace("<em class=\"keyword\">", "").Replace("</em>", "");
            }
        }


        public long pubdate { get; set; }


        private string _pic;
        public string cover
        {
            get { return _pic; }
            set { _pic = "https:" + value; }
        }


    }
}
