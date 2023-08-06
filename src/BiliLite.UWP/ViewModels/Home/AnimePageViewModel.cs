using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using AutoMapper;
using BiliLite.Extensions;
using BiliLite.Models;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Anime;
using BiliLite.Models.Requests.Api.Home;
using BiliLite.Models.Requests.Api.User;
using BiliLite.Modules;
using BiliLite.Pages.Bangumi;
using BiliLite.Pages.User;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using PropertyChanged;

namespace BiliLite.ViewModels.Home
{
    public class AnimePageViewModel : BaseViewModel
    {
        #region Fields

        private readonly FollowAPI m_followApi;
        private readonly AnimeAPI m_bangumiApi;
        private readonly IMapper m_mapper;
        private AnimeType m_animeType;

        #endregion

        #region Constructors

        public AnimePageViewModel()
        {
            m_mapper = App.ServiceProvider.GetService<IMapper>();
            m_bangumiApi = new AnimeAPI();
            m_followApi = new FollowAPI();
        }

        #endregion

        #region Properties

        public List<AnimeFallViewModel> AnimeFalls { get; set; }

        [DoNotNotify]
        public List<PageEntranceModel> Entrances { get; set; }

        public bool ShowFollows { get; set; }

        public bool Loading { get; set; } = true;

        public bool LoadingFollow { get; set; } = true;

        public ObservableCollection<FollowSeasonModel> Follows { get; set; }

        public AnimeHomeModel HomeData { get; set; }

        #endregion

        #region Private Methods

        private void SeasonItemOpen(object sender, object seasonId, string title, bool dontGoTo = false)
        {
            if (seasonId == null) return;
            MessageCenter.NavigateToPage(sender, new NavigationInfo()
            {
                icon = Symbol.Play,
                page = typeof(Pages.SeasonDetailPage),
                parameters = seasonId,
                title = title.ToString(),
                dontGoTo = dontGoTo,
            });
        }

        #endregion

        #region Public Methods

        public void SetAnimeType(AnimeType animeType)
        {
            m_animeType = animeType;

            Entrances = new List<PageEntranceModel>() {
                new PageEntranceModel(){
                    Logo=Constants.Images.RANK_ICON_IMAGE,
                    Name="热门榜单",
                    NavigationInfo=new NavigationInfo(){
                            icon= Symbol.FourBars,
                            page=typeof(SeasonRankPage),
                            title="热门榜单",
                            parameters=(int)animeType
                    }
                },
                new PageEntranceModel(){
                    Logo=Constants.Images.INDEX_ICON_IMAGE,
                    Name="索引",
                    NavigationInfo=new NavigationInfo(){
                            icon= Symbol.Filter,
                            page=typeof(AnimeIndexPage),
                            title="索引",
                            parameters=new SeasonIndexParameter()
                            {
                                type=  IndexSeasonType.Anime,
                                area=m_animeType== AnimeType.Bangumi?"-1":"1,6,7"
                            }
                    }
                },
                  new PageEntranceModel(){
                    Logo=Constants.Images.TIMELINE_ICON_IMAGE,
                    Name="时间表",
                    NavigationInfo=new NavigationInfo(){
                            icon= Symbol.Filter,
                            page=typeof(TimelinePage),
                            title="时间表",
                            parameters=m_animeType
                    }
                },
                  new PageEntranceModel(){
                    Logo=Constants.Images.MY_ICON_IMAGE,
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

        public void SeasonItemClick(object sender, ItemClickEventArgs e)
        {
            var seasonId = e.ClickedItem.GetType().GetProperty("season_id").GetValue(e.ClickedItem, null);
            var title = e.ClickedItem.GetType().GetProperty("title").GetValue(e.ClickedItem, null) ?? "";
            SeasonItemOpen(sender, seasonId, title.ToString());
        }

        public void SeasonItemPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!e.IsMiddleButtonNewTap(sender)) return;
            var element = e.OriginalSource as FrameworkElement;
            var seasonId = element.DataContext.GetType()?.GetProperty("season_id")?.GetValue(element.DataContext, null);
            var title = element.DataContext.GetType()?.GetProperty("title")?.GetValue(element.DataContext, null) ?? "";
            SeasonItemOpen(sender, seasonId, title?.ToString(), true);
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
                var api = m_bangumiApi.BangumiHome();
                if (m_animeType == AnimeType.GuoChuang)
                {
                    api = m_bangumiApi.GuochuangHome();
                }
                var results = await api.Request();
                if (!results.status)
                {
                    throw new Exception(results.message);
                }

                var data = await results.GetJson<ApiDataModel<AnimeHomeModel>>();
                if (!data.success)
                {
                    throw new Exception(data.message);
                }

                HomeData = data.data;
                AnimeFalls = m_mapper.Map<List<AnimeFallViewModel>>(HomeData.Falls);
            }
            catch (Exception ex)
            {
                var handel = HandelError<AnimeHomeModel>(ex);
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
                var results = await m_followApi.MyFollowBangumi().Request();
                if (!results.status)
                {
                    throw new Exception(results.message);
                }

                var data = await results.GetJson<ApiResultModel<JObject>>();
                if (!data.success)
                {
                    throw new Exception(data.message);
                }

                Follows = await data.result["follow_list"].ToString()
                    .DeserializeJson<ObservableCollection<FollowSeasonModel>>();
            }
            catch (Exception ex)
            {
                var handel = HandelError<AnimeHomeModel>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                LoadingFollow = false;
            }
        }

        public async Task GetFallMore(AnimeFallViewModel animeFallViewModel)
        {
            try
            {
                animeFallViewModel.ShowMore = false;
                var results = await m_bangumiApi.AnimeFallMore(animeFallViewModel.Wid, animeFallViewModel.Items.LastOrDefault().Cursor).Request();
                if (!results.status)
                {
                    throw new Exception(results.message);
                }

                var data = await results.GetJson<List<AnimeFallItemModel>>();
                foreach (var item in data)
                {
                    animeFallViewModel.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<List<AnimeFallItemModel>>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                animeFallViewModel.ShowMore = true;
            }
        }

        #endregion
    }
}