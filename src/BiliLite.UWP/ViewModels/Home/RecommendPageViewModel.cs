using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BiliLite.Extensions;
using BiliLite.Models.Common.Recommend;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests;
using BiliLite.Models.Requests.Api.Home;
using BiliLite.Modules;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json;

namespace BiliLite.ViewModels.Home
{
    public class RecommendPageViewModel : BaseViewModel
    {
        #region Fields

        private readonly RecommendAPI m_recommendApi;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        #endregion

        #region Constructors

        public RecommendPageViewModel()
        {
            m_recommendApi = new RecommendAPI();
            Banner = new ObservableCollection<RecommendBannerItemModel>();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
        }

        #endregion

        #region Properties

        public ICommand RefreshCommand { get; private set; }

        public ICommand LoadMoreCommand { get; private set; }

        public bool Loading { get; set; } = true;

        public ObservableCollection<RecommendBannerItemModel> Banner { get; set; }

        public ObservableCollection<RecommendItemModel> Items { get; set; }

        #endregion

        #region Private Methods

        private async void LoadMore()
        {
            if (Items == null || Items.Count == 0)
            {
                return;
            }
            if (Loading)
            {
                return;
            }
            await GetRecommend(Items.LastOrDefault().Idx);
        }

        private void LoadBanner(RecommendItemModel banner)
        {
            try
            {
                if (Banner == null && Banner.Count != 0) return;
                foreach (var item in banner.BannerItem)
                {
                    if (item["type"].ToString() == "static")
                    {
                        Banner.Add(JsonConvert.DeserializeObject<RecommendBannerItemModel>(item["static_banner"].ToString()));
                    }
                    if (item["type"].ToString() == "ad")
                    {
                        Banner.Add(JsonConvert.DeserializeObject<RecommendBannerItemModel>(item["ad_banner"].ToString()));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Debug($"catch error: {ex.Message}");
            }
        }

        #endregion

        #region Public Methods

        public async Task GetRecommend(string idx = "0")
        {
            try
            {
                Loading = true;
                var result = await m_recommendApi.Recommend(idx).Request();
                if (!result.status)
                {
                    throw new CustomizedErrorException(result.message);
                }
                var obj = result.GetJObject();
                if (obj["code"].ToInt32() != 0)
                {
                    throw new CustomizedErrorException(obj["message"].ToString());
                }

                var items = JsonConvert.DeserializeObject<ObservableCollection<RecommendItemModel>>(obj["data"]["items"].ToString().Replace("left_bottom_rcmd_reason_style", "rcmd_reason_style"));
                var banner = items.FirstOrDefault(x => x.CardGoto == "banner");
                if (banner != null)
                {
                    //处理banner
                    LoadBanner(banner);
                    items.Remove(banner);
                }
                for (var i = items.Count - 1; i >= 0; i--)
                {
                    if (items[i].ShowAd)
                    {
                        items.Remove(items[i]);
                        continue;
                    }
                    var item = items[i];
                    if (item.ThreePointV2 != null && item.ThreePointV2.Count > 0 && item.CardGoto == "av")
                    {
                        item.ThreePointV2.Insert(1, new RecommendThreePointV2ItemModel()
                        {
                            Idx = item.Idx,
                            Url = $"https://b23.tv/av{item.Param}",
                            Title = "使用浏览器打开",
                            Type = "browser"
                        });
                    }
                }

                if (Items == null)
                {
                    Items = items;
                    //await GetRecommend(items.LastOrDefault().idx);
                }
                else
                {
                    foreach (var item in items)
                    {
                        Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is CustomizedErrorException)
                {
                    _logger.Error(ex.Message, ex);
                    Notify.ShowMessageToast(ex.Message);
                    return;
                }
                var handel = HandelError<RecommendPageViewModel>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }

        public async void Refresh()
        {
            if (Loading)
            {
                Notify.ShowMessageToast("正在加载中....");
                return;
            }
            Banner = null;
            Items = null;
            await GetRecommend();
        }

        public async Task Dislike(string idx, RecommendThreePointV2ItemModel threePointV2Item, RecommendThreePointV2ItemReasonsModel itemReasons)
        {
            try
            {
                if (!SettingService.Account.Logined && await Notify.ShowLoginDialog())
                {
                    Notify.ShowMessageToast("请先登录");
                    return;
                }
                var recommendItem = Items.FirstOrDefault(x => x.Idx == idx);
                var feedbackParams = new RecommondFeedbackParams()
                {
                    GoTo = recommendItem.CardGoto,
                    Id = recommendItem.Param,
                    Mid = recommendItem.Args.UpId,
                    ReasonId = itemReasons?.Id ?? 0,
                    Rid = recommendItem.Args.Rid,
                    TagId = recommendItem.Args.Tid,
                };
                var api = m_recommendApi.Dislike(feedbackParams);
                if (threePointV2Item.Type == "feedback")
                {
                    api = m_recommendApi.Feedback(feedbackParams);
                }
                var result = await api.Request();

                if (!result.status)
                {
                    throw new CustomizedErrorException(result.message);
                }
                var obj = result.GetJObject();
                if (obj["code"].ToInt32() != 0)
                {
                    throw new CustomizedErrorException(obj["message"].ToString());
                }
                Items.Remove(recommendItem);
            }
            catch (Exception ex)
            {
                if (ex is CustomizedErrorException)
                {
                    _logger.Error(ex.Message, ex);
                    Notify.ShowMessageToast(ex.Message);
                    return;
                }
                var handel = HandelError<RecommendPageViewModel>(ex);
                Notify.ShowMessageToast(handel.message);
            }
        }

        #endregion
    }
}
