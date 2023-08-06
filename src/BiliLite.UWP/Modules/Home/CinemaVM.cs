using BiliLite.Models;
using BiliLite.Models.Requests.Api.Home;
using BiliLite.Models.Requests.Api.User;
using BiliLite.Pages.Bangumi;
using BiliLite.Pages.User;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using BiliLite.Extensions;
using BiliLite.Services;
using BiliLite.Models.Common;

namespace BiliLite.Modules
{
    public class CinemaVM : IModules
    {
        readonly FollowAPI followAPI;
        readonly CinemaAPI cinemaAPI;
        public CinemaVM()
        {
            cinemaAPI = new CinemaAPI();
            followAPI = new FollowAPI();
            Entrances = new List<PageEntranceModel>() {
                new PageEntranceModel(){
                    Logo="ms-appx:///Assets/Icon/榜单.png",
                    Name="热门榜单",
                      NavigationInfo=new NavigationInfo(){
                            icon= Symbol.FourBars,
                            page=typeof(SeasonRankPage),
                            title="热门榜单",
                            parameters=2
                    }
                },
                new PageEntranceModel(){
                    Logo="ms-appx:///Assets/Icon/电影.png",
                    Name="电影索引",
                    NavigationInfo=new NavigationInfo(){
                            icon= Symbol.Filter,
                            page=typeof(AnimeIndexPage),
                            title="电影索引",
                            parameters=new SeasonIndexParameter()
                            {
                                type= IndexSeasonType.Movie
                            }
                    }
                },
                 new PageEntranceModel(){
                    Logo="ms-appx:///Assets/Icon/电视剧.png",
                    Name="电视剧索引",
                    NavigationInfo=new NavigationInfo(){
                            icon= Symbol.Filter,
                            page=typeof(AnimeIndexPage),
                            title="电视剧索引",
                            parameters=new SeasonIndexParameter()
                            {
                                type= IndexSeasonType.TV
                            }
                    }
                },
                 new PageEntranceModel(){
                    Logo="ms-appx:///Assets/Icon/纪录片.png",
                    Name="纪录片索引",
                    NavigationInfo=new NavigationInfo(){
                            icon= Symbol.Filter,
                            page=typeof(AnimeIndexPage),
                            title="纪录片索引",
                            parameters=new SeasonIndexParameter()
                            {
                                type= IndexSeasonType.Documentary
                            }
                    }
                },
                  new PageEntranceModel(){
                    Logo="ms-appx:///Assets/Icon/综艺.png",
                    Name="综艺索引",
                    NavigationInfo=new NavigationInfo(){
                            icon= Symbol.Filter,
                            page=typeof(AnimeIndexPage),
                            title="综艺索引",
                            parameters=new SeasonIndexParameter()
                            {
                                type= IndexSeasonType.Variety
                            }
                    }
                },
                  new PageEntranceModel(){
                    Logo="ms-appx:///Assets/Icon/我的.png",
                    Name="我的追剧",
                    NavigationInfo=new NavigationInfo(){
                            icon= Symbol.OutlineStar,
                            page=typeof(FavoritePage),
                            title="我的追剧",
                            parameters=OpenFavoriteType.Cinema
                    }
                },
            };
        }



        private bool _showFollows = false;
        public bool ShowFollows
        {
            get { return _showFollows; }
            set { _showFollows = value; DoPropertyChanged("ShowFollows"); }
        }

        private bool _loading = true;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }
        private bool _loadingFollow = true;
        public bool LoadingFollow
        {
            get { return _loadingFollow; }
            set { _loadingFollow = value; DoPropertyChanged("LoadingFollow"); }
        }
        private ObservableCollection<FollowSeasonModel> _follows;

        public ObservableCollection<FollowSeasonModel> Follows
        {
            get { return _follows; }
            set { _follows = value; DoPropertyChanged("Follows"); }
        }

        private CinemaHomeModel _homeData;
        public CinemaHomeModel HomeData
        {
            get { return _homeData; }
            set { _homeData = value; DoPropertyChanged("HomeData"); }
        }

        public List<PageEntranceModel> Entrances { get; set; }
        public async void SeasonItemClick(object sender, ItemClickEventArgs e)
        {
            var seasonId = e.ClickedItem.GetType().GetProperty("season_id").GetValue(e.ClickedItem, null);
            var title = e.ClickedItem.GetType().GetProperty("title").GetValue(e.ClickedItem, null) ?? "";
            if (seasonId != null && seasonId.ToInt32() != 0)
            {
                MessageCenter.NavigateToPage(sender, new NavigationInfo()
                {
                    icon = Symbol.Play,
                    page = typeof(Pages.SeasonDetailPage),
                    parameters = seasonId,
                    title = title.ToString()
                });
            }
            else
            {
                var weblink = e.ClickedItem.GetType().GetProperty("link").GetValue(e.ClickedItem, null) ?? "";
                var result = await MessageCenter.HandelUrl(weblink.ToString());
                if (!result) Notify.ShowMessageToast("无法打开此链接");
            }
        }
        public void LinkItemClick(object sender, ItemClickEventArgs e)
        {
            var weblink = e.ClickedItem.GetType().GetProperty("link").GetValue(e.ClickedItem, null);
            var title = e.ClickedItem.GetType().GetProperty("title").GetValue(e.ClickedItem, null) ?? "";
            MessageCenter.NavigateToPage(sender, new NavigationInfo()
            {
                icon = Symbol.World,
                page = typeof(Pages.WebPage),
                parameters = weblink,
                title = title.ToString()
            });
        }

        public async Task GetCinemaHome()
        {
            try
            {
                Loading = true;
                var api = cinemaAPI.CinemaHome();

                var results = await api.Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<CinemaHomeModel>>();
                    if (data.success)
                    {
                        HomeData = data.data;
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
                var handel = HandelError<CinemaVM>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }

        public async Task GetFollows()
        {
            try
            {
                LoadingFollow = true;
                var results = await followAPI.MyFollowCinema().Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiResultModel<JObject>>();
                    if (data.success)
                    {
                        Follows = await data.result["follow_list"].ToString().DeserializeJson<ObservableCollection<FollowSeasonModel>>();
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
                var handel = HandelError<CinemaVM>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                LoadingFollow = false;
            }
        }


        public async Task GetFallMore(CinemaHomeFallModel AnimeFallModel)
        {
            try
            {
                AnimeFallModel.ShowMore = false;
                var results = await cinemaAPI.CinemaFallMore(AnimeFallModel.wid, AnimeFallModel.items.LastOrDefault().cursor).Request();
                if (results.status)
                {
                    var data = await results.GetJson<List<CinemaHomeFallItemModel>>();
                    foreach (var item in data)
                    {
                        AnimeFallModel.items.Add(item);
                    }

                }
                else
                {
                    Notify.ShowMessageToast(results.message);
                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<List<CinemaHomeFallItemModel>>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                AnimeFallModel.ShowMore = true;
            }
        }


    }


    public class CinemaHomeModel
    {
        public List<CinemaHomeBannerModel> banners { get; set; }
        public List<CinemaHomeFallModel> falls { get; set; }
        public List<CinemaHomeHotItem> update { get; set; }
        /// <summary>
        /// 记录片 87
        /// </summary>
        public List<CinemaHomeHotItem> documentary { get; set; }
        /// <summary>
        /// 电影 88
        /// </summary>
        public List<CinemaHomeHotItem> movie { get; set; }
        /// <summary>
        /// 电视剧 89
        /// </summary>
        public List<CinemaHomeHotItem> tv { get; set; }
        /// <summary>
        /// 综艺 173
        /// </summary>
        public List<CinemaHomeHotItem> variety { get; set; }
    }
    public class CinemaHomeFallModel : IModules
    {
        public int wid { get; set; }
        public string title { get; set; }

        private bool _showMore = true;
        public bool ShowMore
        {
            get { return _showMore; }
            set { _showMore = value; DoPropertyChanged("ShowMore"); DoPropertyChanged("ShowLoading"); }
        }

        public ObservableCollection<CinemaHomeFallItemModel> items { get; set; }
    }
    public class CinemaHomeFallItemModel
    {
        public string cover { get; set; }
        public string desc { get; set; }
        public string title { get; set; }
        public string link { get; set; }
        public long cursor { get; set; }
        public int wid { get; set; }
    }
    public class CinemaHomeBannerModel
    {
        public string title { get; set; }
        public string img { get; set; }
        public string url { get; set; }
    }
    public class CinemaHomeHotItem
    {
        public string hat { get; set; }
        public string cover { get; set; }
        public string badge { get; set; }
        public int badge_type { get; set; }
        public bool show_badge
        {
            get
            {
                return !string.IsNullOrEmpty(badge);
            }
        }
        public string desc { get; set; }
        public string title { get; set; }
        public string link { get; set; }
        public int season_id { get; set; }
        public int season_type { get; set; }
        public string type { get; set; }
        public int wid { get; set; }
        public CinemaHomeStatModel stat { get; set; }
    }
    public class CinemaHomeStatModel
    {
        public int view { get; set; }
        public string follow_view { get; set; }
        public int follow { get; set; }
        public int danmaku { get; set; }
    }
}
