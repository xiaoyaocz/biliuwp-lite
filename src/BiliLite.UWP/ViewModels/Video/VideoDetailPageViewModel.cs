using BiliLite.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using AutoMapper;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Video.Detail;
using BiliLite.Models.Responses;
using BiliLite.Models.Requests.Api;
using BiliLite.Models.Requests.Api.User;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using BiliLite.ViewModels.Video;
using Microsoft.Extensions.DependencyInjection;
using PropertyChanged;

namespace BiliLite.Modules
{
    public class VideoDetailPageViewModel : BaseViewModel
    {
        #region Fields

        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        readonly FavoriteApi favoriteAPI;
        readonly VideoAPI videoAPI;
        readonly PlayerAPI PlayerAPI;
        readonly FollowAPI followAPI;
        private readonly IMapper m_mapper;

        #endregion

        #region Constructors

        public VideoDetailPageViewModel()
        {
            m_mapper = App.ServiceProvider.GetService<IMapper>();
            videoAPI = new VideoAPI();
            favoriteAPI = new FavoriteApi();
            PlayerAPI = new PlayerAPI();
            followAPI = new FollowAPI();
            RefreshCommand = new RelayCommand(Refresh);
            LikeCommand = new RelayCommand(DoLike);
            DislikeCommand = new RelayCommand(DoDislike);
            LaunchUrlCommand = new RelayCommand<object>(LaunchUrl);
            CoinCommand = new RelayCommand<string>(DoCoin);
            AttentionCommand = new RelayCommand(DoAttentionUP);
            SetStaffHeightCommand = new RelayCommand<string>(SetStaffHeight);
            OpenRightInfoCommand = new RelayCommand(OpenRightInfo);
        }

        #endregion

        #region Properties

        public ICommand RefreshCommand { get; private set; }

        public ICommand LikeCommand { get; private set; }

        public ICommand DislikeCommand { get; private set; }

        public ICommand CoinCommand { get; private set; }

        public ICommand AttentionCommand { get; private set; }

        public ICommand LaunchUrlCommand { get; private set; }

        public ICommand SetStaffHeightCommand { get; private set; }

        public ICommand OpenRightInfoCommand { get; private set; }

        public bool Loading { get; set; } = true;

        public bool Loaded { get; set; }

        public bool ShowError { get; set; }

        public string ErrorMsg { get; set; } = "";

        public VideoDetailViewModel VideoInfo { get; set; }

        public double StaffHeight { get; set; } = 88.0;

        public bool ShowMoreStaff { get; set; }

        public ObservableCollection<FavoriteItemModel> MyFavorite { get; set; }

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

        [DependsOn(nameof(PageWidth),nameof(IsOpenRightInfo))]
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

        private async void LaunchUrl(object paramenter)
        {
            await MessageCenter.HandelUrl(paramenter.ToString());
        }

        private void SetStaffHeight(string height)
        {
            var h = Convert.ToDouble(height);
            ShowMoreStaff = h > StaffHeight;
            StaffHeight = h;
        }

        private void OpenRightInfo()
        {
            IsOpenRightInfo = !IsOpenRightInfo;
        }

        #endregion

        #region Public Methods

        public async Task LoadFavorite(string avid)
        {
            try
            {
                if (!SettingService.Account.Logined)
                {
                    return;
                }
                var results = await favoriteAPI.MyCreatedFavorite(avid).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        MyFavorite = await data.data["list"].ToString().DeserializeJson<ObservableCollection<FavoriteItemModel>>();
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
                var handel = HandelError<AnimeHomeModel>(ex);
                Notify.ShowMessageToast(handel.message);
            }
        }

        public async Task LoadVideoDetail(string id, bool isbvid = false)
        {
            try
            {
                Loaded = false;
                Loading = true;
                ShowError = false;
                var needGetUserReq = false;
                // 正常app获取视频详情
                var results = await videoAPI.Detail(id, isbvid).Request();
                if (!results.status)
                {
                    throw new CustomizedErrorException(results.message);
                }

                var data = await results.GetJson<ApiDataModel<VideoDetailModel>>();
                if (!data.success)
                {
                    // 通过代理获取视频详情
                    var result_proxy = await videoAPI.DetailProxy(id, isbvid).Request();
                    if (result_proxy.status)
                    {
                        data = await result_proxy.GetJson<ApiDataModel<VideoDetailModel>>();
                    }
                }

                if (!data.success)
                {
                    // 通过web获取视频详情
                    var webResult = await videoAPI.DetailWebInterface(id, isbvid).Request();
                    if (webResult.status)
                    {
                        data = await webResult.GetJson<ApiDataModel<VideoDetailModel>>();
                        needGetUserReq = true;
                    }
                }

                if (!data.success)
                {
                    throw new CustomizedErrorException(data.message);
                }

                var videoInfoViewModel = m_mapper.Map<VideoDetailViewModel>(data.data);
                VideoInfo = videoInfoViewModel;
                if (needGetUserReq)
                {
                    await GetAttentionUp();
                }
                Loaded = true;

                await LoadFavorite(data.data.Aid);
            }
            catch (Exception ex)
            {
                if (ex is CustomizedErrorException customizedErrorException)
                {
                    ShowError = true;
                    ErrorMsg = ex.Message;
                    _logger.Error("视频详情获取失败", ex);
                    return;
                }

                var handel = HandelError<AnimeHomeModel>(ex);
                Notify.ShowMessageToast(handel.message);
                ShowError = true;
                ErrorMsg = handel.message;
            }
            finally
            {
                Loading = false;
            }
        }

        public void Refresh()
        {

        }

        public async void DoLike()
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录后再操作");
                return;
            }
            try
            {
                var results = await videoAPI.Like(VideoInfo.Aid, VideoInfo.ReqUser.Dislike, VideoInfo.ReqUser.Like).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        if (VideoInfo.ReqUser.Like == 1)
                        {
                            VideoInfo.ReqUser.Like = 0;
                            VideoInfo.Stat.Like -= 1;
                        }
                        else
                        {
                            VideoInfo.ReqUser.Like = 1;
                            VideoInfo.ReqUser.Dislike = 0;
                            VideoInfo.Stat.Like += 1;
                        }
                        if (!string.IsNullOrEmpty(data.data["toast"]?.ToString()))
                        {
                            Notify.ShowMessageToast(data.data["toast"].ToString());
                        }
                        else
                        {
                            Notify.ShowMessageToast("操作成功");
                        }
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
        public async void DoDislike()
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录后再操作");
                return;
            }
            try
            {
                var results = await videoAPI.Dislike(VideoInfo.Aid, VideoInfo.ReqUser.Dislike, VideoInfo.ReqUser.Like).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        if (VideoInfo.ReqUser.Dislike == 1)
                        {
                            VideoInfo.ReqUser.Dislike = 0;
                        }
                        else
                        {
                            VideoInfo.ReqUser.Dislike = 1;
                            if (VideoInfo.ReqUser.Like == 1)
                            {
                                VideoInfo.ReqUser.Like = 0;
                                VideoInfo.Stat.Like -= 1;
                            }


                        }
                        if (!string.IsNullOrEmpty(data.data["toast"]?.ToString()))
                        {
                            Notify.ShowMessageToast(data.data["toast"].ToString());
                        }
                        else
                        {
                            Notify.ShowMessageToast("操作成功");
                        }
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
        public async void DoTriple()
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录后再操作");
                return;
            }
            try
            {
                var results = await videoAPI.Triple(VideoInfo.Aid).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        VideoInfo.ReqUser.Like = 1;
                        VideoInfo.Stat.Like += 1;
                        VideoInfo.ReqUser.Coin = 1;
                        VideoInfo.Stat.Coin += 1;
                        VideoInfo.ReqUser.Favorite = 1;
                        VideoInfo.Stat.Favorite += 1;
                        if (VideoInfo.ReqUser.Dislike == 1)
                        {
                            VideoInfo.ReqUser.Dislike = 0;
                        }

                        Notify.ShowMessageToast("三连完成");
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

        public async void DoCoin(string num)
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录后再操作");
                return;
            }
            var coinNum = Convert.ToInt32(num);
            try
            {
                var results = await videoAPI.Coin(VideoInfo.Aid, coinNum).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        if (VideoInfo.ReqUser.Coin == 1)
                        {
                            VideoInfo.ReqUser.Coin = 0;
                            VideoInfo.Stat.Coin -= coinNum;
                        }
                        else
                        {
                            VideoInfo.ReqUser.Coin = 1;
                            VideoInfo.Stat.Coin += coinNum;
                        }
                        if (!string.IsNullOrEmpty(data.data["toast"]?.ToString()))
                        {
                            Notify.ShowMessageToast(data.data["toast"].ToString());
                        }
                        else
                        {
                            Notify.ShowMessageToast("操作成功");
                        }
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
        public async void DoFavorite(List<string> fav_ids, string avid)
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录后再操作");
                return;
            }
            try
            {
                var results = await favoriteAPI.AddFavorite(fav_ids, avid).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        if (fav_ids.Count != 0)
                        {
                            VideoInfo.ReqUser.Favorite = 1;
                            VideoInfo.Stat.Favorite += 1;
                        }
                        else
                        {
                            VideoInfo.ReqUser.Favorite = 0;
                            VideoInfo.Stat.Favorite -= 1;
                        }
                        if (!string.IsNullOrEmpty(data.data["toast"]?.ToString()))
                        {
                            Notify.ShowMessageToast(data.data["toast"].ToString());
                        }
                        else
                        {
                            Notify.ShowMessageToast("操作成功");
                            await LoadFavorite(avid);
                        }
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

        public async Task GetAttentionUp()
        {
            VideoInfo.ReqUser ??= new VideoDetailReqUserViewModel();
            VideoInfo.ReqUser.Attention = -999;
            if (!SettingService.Account.Logined)
            {
                return;
            }

            var result = await followAPI.GetAttention(VideoInfo.Owner.Mid).Request();
            if (!result.status) return;
            var data = await result.GetJson<ApiDataModel<UserAttentionResponse>>();
            if (data.data.Attribute == 2 || data.data.Attribute == 6)
            {
                VideoInfo.ReqUser.Attention = 1;
            }
        }

        public async void DoAttentionUP()
        {
            var result = await AttentionUP(VideoInfo.Owner.Mid, VideoInfo.ReqUser.Attention == 1 ? 2 : 1);
            if (result)
            {
                if (VideoInfo.ReqUser.Attention == 1)
                {
                    VideoInfo.ReqUser.Attention = -999;
                    VideoInfo.OwnerExt.Fans -= 1;
                }
                else
                {
                    VideoInfo.ReqUser.Attention = 1;
                    VideoInfo.OwnerExt.Fans += 1;
                }
            }
        }
        public async Task<bool> AttentionUP(string mid, int mode)
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录后再操作");
                return false;
            }

            try
            {
                var results = await videoAPI.Attention(mid, mode.ToString()).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();

                    if (data.success)
                    {

                        Notify.ShowMessageToast("操作成功");
                        return true;
                    }
                    else
                    {
                        Notify.ShowMessageToast(data.message);
                        return false;
                    }
                }
                else
                {
                    Notify.ShowMessageToast(results.message);
                    return false;
                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<object>(ex);
                Notify.ShowMessageToast(handel.message);
                return false;
            }
        }

        public async Task<string> GetPlayUrl()
        {
            try
            {
                var results = await PlayerAPI.VideoPlayUrl(VideoInfo.Aid, VideoInfo.Pages[0].Cid, 80, false).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        return data.data["durl"][0]["url"].ToString();
                    }
                    else
                    {
                        Notify.ShowMessageToast(data.message);
                        return "";
                    }
                }
                else
                {
                    Notify.ShowMessageToast(results.message);
                    return "";
                }
            }
            catch (Exception ex)
            {

                var handel = HandelError<string>(ex);
                Notify.ShowMessageToast(handel.message);
                return "";
            }
        }

        #endregion
    }
}
