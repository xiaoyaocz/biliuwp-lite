using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using AutoMapper;
using BiliLite.Extensions;
using BiliLite.Models;
using BiliLite.Models.Common.Season;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api;
using BiliLite.Models.Requests.Api.User;
using BiliLite.Modules;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PropertyChanged;

namespace BiliLite.ViewModels.Season
{
    public class SeasonDetailPageViewModel : BaseViewModel
    {
        #region Fields

        private readonly SeasonApi m_seasonApi;
        private readonly PlayerAPI m_playerApi;
        private readonly FollowAPI m_followApi;
        private readonly IMapper m_mapper;

        #endregion

        #region Constructors
        public SeasonDetailPageViewModel()
        {
            m_mapper = App.ServiceProvider.GetService<IMapper>();
            m_seasonApi = new SeasonApi();
            m_playerApi = new PlayerAPI();
            m_followApi = new FollowAPI();
            FollowCommand = new RelayCommand(DoFollow);
            OpenRightInfoCommand = new RelayCommand(OpenRightInfo);
        }

        #endregion

        #region Properties

        public ICommand FollowCommand { get; private set; }

        public ICommand OpenRightInfoCommand { get; private set; }

        public SeasonDetailViewModel Detail { get; set; }

        public bool Loading { get; set; } = true;

        public bool Loaded { get; set; }

        public bool ShowError { get; set; }

        public string ErrorMsg { get; set; } = "";

        public List<SeasonDetailEpisodeModel> Episodes { get; set; }

        public bool ShowEpisodes { get; set; }

        public List<SeasonDetailEpisodeModel> Previews { get; set; }

        public bool ShowPreview { get; set; }

        public bool NothingPlay { get; set; }

        public double BottomActionBarHeight { get; set; }

        public double BottomActionBarWidth { get; set; }

        [DependsOn(nameof(BottomActionBarWidth))]
        public bool ShowNormalDownloadBtn => !(BottomActionBarWidth < 460);

        [DependsOn(nameof(BottomActionBarWidth))]
        public bool ShowFlyoutDownloadBtn => (BottomActionBarWidth < 460);

        [DependsOn(nameof(BottomActionBarWidth))]
        public bool ShowNormalShareBtn => !(BottomActionBarWidth < 390);

        [DependsOn(nameof(BottomActionBarWidth))]
        public bool ShowFlyoutShareBtn => (BottomActionBarWidth < 390);

        public double PageHeight { get; set; }

        public double PageWidth { get; set; }

        [DependsOn(nameof(PageWidth))]
        public int PlayerGridColumnSpan => PageWidth < 1000 ? 2 : 1;

        public GridLength DefaultRightInfoWidth { get; set; } = new GridLength(320);

        public bool IsOpenRightInfo { get; set; }

        [DependsOn(nameof(PageWidth))]
        public bool ShowOpenRightInfoBtn => (PageWidth < 1000);

        [DependsOn(nameof(PageWidth), nameof(IsOpenRightInfo))]
        public GridLength RightInfoWidth
        {
            get
            {
                if (PageWidth < 1000 && !IsOpenRightInfo)
                {
                    return new GridLength(0);
                }

                return DefaultRightInfoWidth;
            }
        }

        [DependsOn(nameof(PageHeight), nameof(PageWidth))]
        public double RightInfoHeight
        {
            get
            {
                if (PageWidth < 1000)
                {
                    return PageHeight - BottomActionBarHeight;
                }

                return PageHeight;
            }
        }

        #endregion

        #region Private Methods

        private void OpenRightInfo()
        {
            IsOpenRightInfo = !IsOpenRightInfo;
        }

        #endregion

        #region Public Methods

        public async Task LoadSeasonDetail(string seasonId)
        {
            try
            {
                Loaded = false;
                Loading = true;
                ShowError = false;
                var results = await m_seasonApi.Detail(seasonId).Request();

                if (!results.status)
                {
                    throw new CustomizedErrorException(results.message);
                }

                //通过代理访问番剧详情
                var data = await results.GetJson<ApiResultModel<SeasonDetailModel>>();
                //代理访问失败，使用Web的Api访问
                if (!data.success)
                {
                    data = await GetWebSeasonDetail(seasonId);
                }

                if (!data.success)
                {
                    throw new CustomizedErrorException(data.message);
                }

                if (data.result.Limit != null)
                {
                    var reulstsWeb = await m_seasonApi.DetailWeb(seasonId).Request();
                    if (reulstsWeb.status)
                    {
                        var data_2 = reulstsWeb.GetJObject();
                        if (data_2["code"].ToInt32() == 0)
                        {
                            data.result.Episodes = await data_2["result"]["episodes"].ToString().DeserializeJson<List<SeasonDetailEpisodeModel>>();
                        }
                    }
                }
                else
                {
                    try
                    {
                        //build 6235200
                        data.result.Episodes = JsonConvert.DeserializeObject<List<SeasonDetailEpisodeModel>>(data.result.Modules.FirstOrDefault(x => x["style"].ToString() == "positive")?["data"]?["episodes"]?.ToString() ?? "[]");
                        data.result.Seasons = JsonConvert.DeserializeObject<List<SeasonDetailSeasonItemModel>>(data.result.Modules.FirstOrDefault(x => x["style"].ToString() == "season")?["data"]?["seasons"]?.ToString() ?? "[]");
                        var pv = JsonConvert.DeserializeObject<List<SeasonDetailEpisodeModel>>(data.result.Modules.FirstOrDefault(x => x["style"].ToString() == "section")?["data"]?["episodes"]?.ToString() ?? "[]");
                        foreach (var item in pv)
                        {
                            item.SectionType = 1;
                            data.result.Episodes.Add(item);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

                if (data.result.Section != null)
                {
                    foreach (var item in data.result.Section)
                    {
                        foreach (var item2 in item.Episodes)
                        {
                            item2.SectionType = 1;
                        }
                        data.result.Episodes.InsertRange(0, item.Episodes);
                        //data.result.episodes= data.result.episodes.Concat(item.episodes).ToList();
                    }
                }

                var seasonDetail = m_mapper.Map<SeasonDetailViewModel>(data.result);
                Detail = seasonDetail;

                Episodes = data.result.Episodes.Where(x => !x.IsPreview).ToList();
                ShowEpisodes = Episodes.Count > 0;
                Previews = data.result.Episodes.Where(x => x.IsPreview).ToList();
                ShowPreview = Previews.Count > 0;
                NothingPlay = !ShowEpisodes && !ShowPreview;
                Loaded = true;
            }
            catch (Exception ex)
            {
                var handel = HandelError<SeasonDetailPageViewModel>(ex);
                //Notify.ShowMessageToast(handel.message);
                ShowError = true;
                ErrorMsg = handel.message;
            }
            finally
            {
                Loading = false;
            }
        }

        public async Task<ApiResultModel<SeasonDetailModel>> GetWebSeasonDetail(string seasonId)
        {
            var reulsts_web = await m_seasonApi.DetailWeb(seasonId).Request();
            if (!reulsts_web.status)
                return new ApiResultModel<SeasonDetailModel>()
                {
                    code = -101,
                    message = "无法读取内容"
                };
            var data = reulsts_web.GetJObject();
            if (data["code"].ToInt32() != 0)
                return new ApiResultModel<SeasonDetailModel>()
                {
                    code = -101,
                    message = "无法读取内容"
                };
            var objText = data["result"].ToString();
            //处理下会出错的字段
            objText = objText.Replace("\"staff\"", "staff1");
            var model = JsonConvert.DeserializeObject<SeasonDetailModel>(objText);
            model.Episodes = await data["result"]["episodes"].ToString().DeserializeJson<List<SeasonDetailEpisodeModel>>();
            model.UserStatus = new SeasonDetailUserStatusModel()
            {
                FollowStatus = 0,
                Follow = 0
            };
            return new ApiResultModel<SeasonDetailModel>() { code = 0, message = "", result = model, };
        }

        public async void DoFollow()
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录后再操作");
                return;
            }
            try
            {
                var api = m_followApi.FollowSeason(Detail.SeasonId.ToString());
                if (Detail.UserStatus.Follow == 1)
                {
                    api = m_followApi.CancelFollowSeason(Detail.SeasonId.ToString());
                }
                var results = await api.Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiResultModel<JObject>>();
                    if (data.success)
                    {
                        Detail.UserStatus.Follow = Detail.UserStatus.Follow == 1 ? 0 : 1;
                        Notify.ShowMessageToast(!string.IsNullOrEmpty(data.result["toast"]?.ToString())
                            ? data.result["toast"].ToString()
                            : "操作成功");
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
                var handel = HandelError<object>(ex);
                Notify.ShowMessageToast(handel.message);
            }
        }

        #endregion
    }
}
