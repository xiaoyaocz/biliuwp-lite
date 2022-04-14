using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiliLite.Models;
using BiliLite.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;
using Newtonsoft.Json.Linq;
using BiliLite.Pages.User;
using BiliLite.Pages.Bangumi;
using BiliLite.Pages;

namespace BiliLite.Modules
{
    public enum AnimeType
    {
        bangumi = 1,
        guochuang = 4
    }

    public class AnimeVM : IModules
    {
        readonly Api.User.FollowAPI followAPI;
        readonly Api.Home.AnimeAPI bangumiApi;
        readonly AnimeType animeType;
        public AnimeVM(AnimeType type)
        {
            bangumiApi = new Api.Home.AnimeAPI();
            followAPI = new Api.User.FollowAPI();
            animeType = type;
            Entrances = new List<PageEntranceModel>() {
                new PageEntranceModel(){
                    Logo="ms-appx:///Assets/Icon/榜单.png",
                    Name="热门榜单",
                    NavigationInfo=new NavigationInfo(){
                            icon= Symbol.FourBars,
                            page=typeof(SeasonRankPage),
                            title="热门榜单",
                            parameters=(int)type
                    }
                },
                new PageEntranceModel(){
                    Logo="ms-appx:///Assets/Icon/索引.png",
                    Name="索引",
                    NavigationInfo=new NavigationInfo(){
                            icon= Symbol.Filter,
                            page=typeof(AnimeIndexPage),
                            title="索引",
                            parameters=new SeasonIndexParameter()
                            {
                                type=  IndexSeasonType.Anime,
                                area=animeType== AnimeType.bangumi?"-1":"1,6,7"
                            }
                    }
                },
                  new PageEntranceModel(){
                    Logo="ms-appx:///Assets/Icon/时间表.png",
                    Name="时间表",
                    NavigationInfo=new NavigationInfo(){
                            icon= Symbol.Filter,
                            page=typeof(TimelinePage),
                            title="时间表",
                            parameters=animeType
                    }
                },
                  new PageEntranceModel(){
                    Logo="ms-appx:///Assets/Icon/我的.png",
                    Name="我的追番",
                    NavigationInfo=new NavigationInfo(){
                            icon= Symbol.OutlineStar,
                            page=typeof(FavoritePage),
                            title="我的追番",
                            parameters=OpenFavoriteType.Bangumi
                    }
                },
            };
        }
        public List<PageEntranceModel> Entrances { get; set; }
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

        private AnimeHomeModel _homeData;
        public AnimeHomeModel HomeData
        {
            get { return _homeData; }
            set { _homeData = value; DoPropertyChanged("HomeData"); }
        }
      
        public void SeasonItemClick(object sender,ItemClickEventArgs e)
        {
            var seasonId = e.ClickedItem.GetType().GetProperty("season_id").GetValue(e.ClickedItem, null);
            var title = e.ClickedItem.GetType().GetProperty("title").GetValue(e.ClickedItem, null)??"";
            MessageCenter.NavigateToPage(sender,new NavigationInfo() {
                icon= Symbol.Play,
                page=typeof(Pages.SeasonDetailPage),
                parameters= seasonId,
                title= title.ToString()
            });
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

        public async Task GetBangumiHome()
        {
            try
            {
                Loading = true;
                var api = bangumiApi.BangumiHome();
                if (animeType== AnimeType.guochuang)
                {
                    api = bangumiApi.GuochuangHome();
                }
                var results = await api.Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<AnimeHomeModel>>();
                    if (data.success)
                    {
                        HomeData = data.data;
                    }
                    else
                    {
                        Utils.ShowMessageToast(data.message);
                    }
                }
                else
                {
                    Utils.ShowMessageToast(results.message);

                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<AnimeHomeModel>(ex);
                Utils.ShowMessageToast(handel.message);
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
                var results = await followAPI.MyFollowBangumi().Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiResultModel<JObject>>();
                    if (data.success)
                    {
                        Follows = await data.result["follow_list"].ToString().DeserializeJson<ObservableCollection<FollowSeasonModel>>();
                    }
                    else
                    {
                        Utils.ShowMessageToast(data.message);
                    }
                }
                else
                {
                    Utils.ShowMessageToast(results.message);

                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<AnimeHomeModel>(ex);
                Utils.ShowMessageToast(handel.message);
            }
            finally
            {
                LoadingFollow = false;
            }
        }


        public async Task GetFallMore(AnimeFallModel AnimeFallModel)
        {
            try
            {
                AnimeFallModel.ShowMore = false;
                var results = await bangumiApi.AnimeFallMore(AnimeFallModel.wid, AnimeFallModel.items.LastOrDefault().cursor).Request();
                if (results.status)
                {
                    var data = await results.GetJson<List<AnimeFallItemModel>>();
                    foreach (var item in data)
                    {
                        AnimeFallModel.items.Add(item);
                    }
                   
                }
                else
                {
                    Utils.ShowMessageToast(results.message);
                }
            }
            catch (Exception ex)
            {
                var handel= HandelError<List<AnimeFallItemModel>>(ex);
                Utils.ShowMessageToast(handel.message);
            }
            finally
            {
                AnimeFallModel.ShowMore = true;
            }
        }

    }

    public class AnimeHomeModel
    {
        public List<AnimeRankModel> hots { get; set; }
        public List<AnimeBannerModel> banners { get; set; }
        public List<AnimeRankModel> ranks { get; set; }
        public List<AnimeTimelineItemModel> today { get; set; }
        public List<AnimeFallModel> falls { get; set; }
    }
    public class AnimeFallModel : IModules
    {
        public int wid { get; set; }
        public string title { get; set; }

        private bool _showMore = true;
        public bool ShowMore
        {
            get { return _showMore; }
            set { _showMore = value; DoPropertyChanged("ShowMore"); DoPropertyChanged("ShowLoading"); }
        }
     
        public ObservableCollection<AnimeFallItemModel> items { get; set; }
    }
    public class AnimeFallItemModel
    {
        public string cover { get; set; }
        public string desc { get; set; }
        public string title { get; set; }
        public string link { get; set; }
        public long cursor { get; set; }
        public int wid { get; set; }
    }
    public class AnimeBannerModel
    {
        public string title { get; set; }
        public string img { get; set; }
        public string url { get; set; }
    }
    public class AnimeRankModel
    {
        public string display { get; set; }
        public string title { get; set; }
        public string cover { get; set; }
        public int season_id { get; set; }
        public string index_show { get; set; }
        public int follow { get; set; }
        public int danmaku { get; set; }
        public int view { get; set; }
        public bool show_badge
        {
            get
            {
                return !string.IsNullOrEmpty(badge);
            }
        }
        public string badge { get; set; }
    }
  


}
