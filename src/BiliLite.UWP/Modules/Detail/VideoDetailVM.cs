using BiliLite.Api;
using BiliLite.Helpers;
using BiliLite.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BiliLite.Modules
{
    public class VideoDetailVM:IModules
    {
        readonly Api.User.FavoriteApi favoriteAPI;
        readonly VideoAPI videoAPI;
        readonly PlayerAPI  PlayerAPI;
        readonly Api.User.FollowAPI followAPI;
        public VideoDetailVM()
        {
            videoAPI = new VideoAPI();
            favoriteAPI = new Api.User.FavoriteApi();
            PlayerAPI = new PlayerAPI();
            followAPI = new Api.User.FollowAPI();
            RefreshCommand = new RelayCommand(Refresh);
            LikeCommand = new RelayCommand(DoLike);
            DislikeCommand = new RelayCommand(DoDislike);
            LaunchUrlCommand=new RelayCommand<object>(LaunchUrl);
            CoinCommand = new RelayCommand<string>(DoCoin);
            //FavoriteCommand = new RelayCommand<string>(DoFavorite);
            AttentionCommand = new RelayCommand(DoAttentionUP);
            SetStaffHeightCommand = new RelayCommand<string>(SetStaffHeight);
        }
        public ICommand RefreshCommand { get; private set; }
        public ICommand LikeCommand{ get; private set; }
        public ICommand DislikeCommand { get; private set; }
        public ICommand CoinCommand { get; private set; }
        //public ICommand FavoriteCommand { get; private set; }
        public ICommand AttentionCommand { get; private set; }
        public ICommand LaunchUrlCommand { get; private set; }

        private async void LaunchUrl(object paramenter)
        {

            await MessageCenter.HandelUrl(paramenter.ToString());
            return;


        }


        private bool _loading = true;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }
        private bool _loaded = false;
        public bool Loaded
        {
            get { return _loaded; }
            set { _loaded = value; DoPropertyChanged("Loaded"); }
        }
        private bool _ShowError=false;
        public bool ShowError
        {
            get { return _ShowError; }
            set { _ShowError = value; DoPropertyChanged("ShowError"); }
        }
        private string _errorMsg="";

        public string ErrorMsg
        {
            get { return _errorMsg; }
            set { _errorMsg = value; DoPropertyChanged("ErrorMsg"); }
        }


        private VideoDetailModel _videoInfo;
        public VideoDetailModel VideoInfo
        {
            get { return _videoInfo; }
            set { _videoInfo = value; DoPropertyChanged("VideoInfo"); }
        }

        private double _staffHeight = 88.0;
        public double StaffHeight
        {
            get { return _staffHeight; }
            set { _staffHeight = value; DoPropertyChanged("StaffHeight"); }
        }

        private bool _showMoreStaff;

        public bool showMoreStaff
        {
            get { return _showMoreStaff; }
            set { _showMoreStaff = value; DoPropertyChanged("showMoreStaff"); }
        }
        public ICommand SetStaffHeightCommand { get; private set; }
        public void SetStaffHeight(string height)
        {
            var h = Convert.ToDouble(height);
            showMoreStaff = h > StaffHeight;
            StaffHeight = h;

        }


        private ObservableCollection<FavoriteItemModel> _myFavorite;
        public ObservableCollection<FavoriteItemModel> MyFavorite
        {
            get { return _myFavorite; }
            set { _myFavorite = value; DoPropertyChanged("MyFavorite"); }
        }
        public async Task LoadFavorite(string avid)
        {
            try
            {
                if (!SettingHelper.Account.Logined)
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
        }

        public async Task LoadVideoDetail(string id,bool isbvid=false)
        {
            try
            {
                Loaded = false;
                Loading = true;
                ShowError = false;
                var results = await videoAPI.Detail(id, isbvid).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<VideoDetailModel>>();
                    if (!data.success)
                    {
                        var result_proxy = await videoAPI.DetailProxy(id, isbvid).Request();
                        if (result_proxy.status)
                        {
                            data = await result_proxy.GetJson<ApiDataModel<VideoDetailModel>>();
                        }
                    }
                    if (data.success)
                    {
                        VideoInfo = data.data;
                        Loaded = true;

                        await LoadFavorite(data.data.aid);
                    }
                    else
                    {
                        ShowError = true;
                        ErrorMsg = data.message;
                        //Utils.ShowMessageToast(data.message);
                    }
                }
                else
                {
                    ShowError = true;
                    ErrorMsg = results.message;
                    //Utils.ShowMessageToast(results.message);

                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<AnimeHomeModel>(ex);
                Utils.ShowMessageToast(handel.message);
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
            if (!SettingHelper.Account.Logined&&!await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录后再操作");
                return;
            }
            try
            {
                var results = await videoAPI.Like(VideoInfo.aid, VideoInfo.req_user.dislike, VideoInfo.req_user.like).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        if (VideoInfo.req_user.like == 1)
                        {
                            VideoInfo.req_user.like = 0;
                            VideoInfo.stat.like -= 1;
                        }
                        else
                        {
                            VideoInfo.req_user.like = 1;
                            VideoInfo.req_user.dislike = 0;
                            VideoInfo.stat.like += 1;
                        }
                        if (!string.IsNullOrEmpty( data.data["toast"]?.ToString()))
                        {
                            Utils.ShowMessageToast(data.data["toast"].ToString());
                        }
                        else
                        {
                            Utils.ShowMessageToast("操作成功");
                        }
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
               var handel = HandelError<object>(ex);
                Utils.ShowMessageToast(handel.message);
            }


        }
        public async void DoDislike()
        {
            if (!SettingHelper.Account.Logined && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录后再操作");
                return;
            }
            try
            {
                var results = await videoAPI.Dislike(VideoInfo.aid, VideoInfo.req_user.dislike, VideoInfo.req_user.like).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        if (VideoInfo.req_user.dislike == 1)
                        {
                            VideoInfo.req_user.dislike = 0;
                        }
                        else
                        {
                            VideoInfo.req_user.dislike = 1;
                            if (VideoInfo.req_user.like==1)
                            {
                                VideoInfo.req_user.like = 0;
                                VideoInfo.stat.like -= 1;
                            }
                          
                            
                        }
                        if (!string.IsNullOrEmpty(data.data["toast"]?.ToString()))
                        {
                            Utils.ShowMessageToast(data.data["toast"].ToString());
                        }
                        else
                        {
                            Utils.ShowMessageToast("操作成功");
                        }
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
                var handel = HandelError<object>(ex);
                Utils.ShowMessageToast(handel.message);
            }
            


        }
        public async void DoTriple()
        {
            if (!SettingHelper.Account.Logined && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录后再操作");
                return;
            }
            try
            {
                var results = await videoAPI.Triple(VideoInfo.aid).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        VideoInfo.req_user.like = 1;
                        VideoInfo.stat.like += 1;
                        VideoInfo.req_user.coin = 1;
                        VideoInfo.stat.coin += 1;
                        VideoInfo.req_user.favorite = 1;
                        VideoInfo.stat.favorite += 1;
                        if (VideoInfo.req_user.dislike == 1)
                        {
                            VideoInfo.req_user.dislike = 0;
                        }

                        Utils.ShowMessageToast("三连完成");
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
                var handel = HandelError<object>(ex);
                Utils.ShowMessageToast(handel.message);
            }



        }

        public async void DoCoin(string num)
        {
            if (!SettingHelper.Account.Logined && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录后再操作");
                return;
            }
            var coinNum = Convert.ToInt32(num);
            try
            {
                var results = await videoAPI.Coin(VideoInfo.aid, coinNum).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        if (VideoInfo.req_user.coin == 1)
                        {
                            VideoInfo.req_user.coin = 0;
                            VideoInfo.stat.coin -= coinNum;
                        }
                        else
                        {
                            VideoInfo.req_user.coin = 1;
                            VideoInfo.stat.coin += coinNum;
                        }
                        if (!string.IsNullOrEmpty(data.data["toast"]?.ToString()))
                        {
                            Utils.ShowMessageToast(data.data["toast"].ToString());
                        }
                        else
                        {
                            Utils.ShowMessageToast("操作成功");
                        }
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
                var handel = HandelError<object>(ex);
                Utils.ShowMessageToast(handel.message);
            }
          


        }
        public async void DoFavorite(List<string> fav_ids,string avid)
        {
            if (!SettingHelper.Account.Logined && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录后再操作");
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
                            VideoInfo.req_user.favorite = 1;
                            VideoInfo.stat.favorite += 1;
                        }
                        else
                        {
                            VideoInfo.req_user.favorite = 0;
                            VideoInfo.stat.favorite -= 1;
                        }
                        if (!string.IsNullOrEmpty(data.data["toast"]?.ToString()))
                        {
                            Utils.ShowMessageToast(data.data["toast"].ToString());
                        }
                        else
                        {
                            Utils.ShowMessageToast("操作成功");
                            await LoadFavorite(avid);
                        }
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
                var handel = HandelError<object>(ex);
                Utils.ShowMessageToast(handel.message);
            }
           
           

        }
      

        public async void DoAttentionUP()
        {
            var result = await AttentionUP(VideoInfo.owner.mid, VideoInfo.req_user.attention==1?2:1);
            if (result)
            {
                if (VideoInfo.req_user.attention == 1)
                {
                    VideoInfo.req_user.attention = -999;
                    VideoInfo.owner_ext.fans -= 1;
                }
                else
                {
                    VideoInfo.req_user.attention = 1;
                    VideoInfo.owner_ext.fans += 1;
                }
            }
        }
        public async Task<bool> AttentionUP(string mid,int mode)
        {
            if (!SettingHelper.Account.Logined && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录后再操作");
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
                       
                        Utils.ShowMessageToast("操作成功");
                        return true;
                    }
                    else
                    {
                        Utils.ShowMessageToast(data.message);
                        return false;
                    }
                }
                else
                {
                    Utils.ShowMessageToast(results.message);
                    return false;
                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<object>(ex);
                Utils.ShowMessageToast(handel.message);
                return false;
            }



        }
    
        public async Task<string> GetPlayUrl()
        {
            try
            {
                var results = await PlayerAPI.VideoPlayUrl(VideoInfo.aid,VideoInfo.pages[0].cid,80,false).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        return data.data["durl"][0]["url"].ToString();
                    }
                    else
                    {
                        Utils.ShowMessageToast(data.message);
                        return "";
                    }
                }
                else
                {
                    Utils.ShowMessageToast(results.message);
                    return "";
                }
            }
            catch (Exception ex)
            {

                var handel = HandelError<string>(ex);
                Utils.ShowMessageToast(handel.message);
                return "";
            }
        }
    
    }

    public class VideoDetailModel:IModules
    {
        public string bvid { get; set; }
        public string aid { get; set; }
        /// <summary>
        /// 视频数量
        /// </summary>
        public int videos { get; set; }
        /// <summary>
        /// 分区ID
        /// </summary>
        public int tid { get; set; }
        /// <summary>
        /// 分区名
        /// </summary>
        public string tname { get; set; }
        /// <summary>
        /// 封面
        /// </summary>
        public string pic { get; set; }
        public string title { get; set; }
        /// <summary>
        /// 上传时间
        /// </summary>
        public long pubdate { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public long ctime { get; set; }
        /// <summary>
        /// 简介
        /// </summary>
        public string desc { get; set; }

        public long attribute { get; set; }
        public int state { get; set; }
        /// <summary>
        /// 时长
        /// </summary>
        public int duration { get; set; }
        public VideoDetailRightsModel rights { get; set; }
        public string dynamic { get; set; }
        /// <summary>
        /// UP主
        /// </summary>
        public VideoDetailOwnerModel owner { get; set; }
        /// <summary>
        /// UP主信息扩展
        /// </summary>
        public VideoDetailOwnerExtModel owner_ext { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        public VideoDetailStatModel stat { get; set; }
        /// <summary>
        /// 用户数据
        /// </summary>
        public VideoDetailReqUserModel req_user { get; set; }
        /// <summary>
        /// Tag
        /// </summary>
        public List<VideoDetailTagModel> tag { get; set; }
       

        private List<VideoDetailRelatesModel> _relates;
        /// <summary>
        /// 推荐
        /// </summary>
        public List<VideoDetailRelatesModel> relates
        {
            get { return _relates; }
            set { _relates = value.Where(x => !string.IsNullOrEmpty(x.aid)).ToList(); }
        }



        public string share_subtitle { get; set; }
        public string short_link { get; set; }
        public string redirect_url { get; set; }

        public List<VideoDetailPagesModel> pages { get; set; }

        public bool showPages
        {
            get
            {
                return pages != null && pages.Count > 1;
            }
        }
        /// <summary>
        /// 互动视频
        /// </summary>
        public VideoDetailInteractionModel interaction { get; set; }
        public List<VideoDetailStaffModel> staff { get; set; }

        public bool showStaff
        {
            get
            {
                return staff != null;
            }
        }

        public string argue_msg { get; set; }
        public bool showArgueMsg
        {
            get
            {
                return !string.IsNullOrEmpty(argue_msg);
            }
        }
        public VideoDetailHistoryModel history { get; set; }
    } 
    public class VideoDetailRightsModel
    {
        public int bp { get; set; }
        /// <summary>
        /// 能不能充电
        /// </summary>
        public int elec { get; set; }
        /// <summary>
        /// 能不能下载
        /// </summary>
        public int download { get; set; }
        /// <summary>
        /// 是不是电影
        /// </summary>
        public int movie { get; set; }
        /// <summary>
        /// 是不是付费
        /// </summary>
        public int pay { get; set; }
    }
    public class VideoDetailOwnerModel
    {
        public string mid { get; set; }
        public string name { get; set; }
        public string face { get; set; }
    }
    public class VideoDetailOwnerExtModel
    {
        /// <summary>
        /// 粉丝数
        /// </summary>
        public int fans { get; set; }
        /// <summary>
        /// 大会员信息
        /// </summary>
       public VideoDetailOwnerExtVipModel vip { get; set; }
        /// <summary>
        /// 认证信息
        /// </summary>
        public VideoDetailOwnerExtOfficialVerifyModel official_verify { get; set; }
    }
    public class VideoDetailOwnerExtVipModel
    {
        public int vipType { get; set; }
        public int vipStatus { get; set; }

    }
    public class VideoDetailOwnerExtOfficialVerifyModel
    {
        /// <summary>
        /// 0个人认证,1企业认证
        /// </summary>
        public int type { get; set; }
        public string desc { get; set; }

    }
    public class VideoDetailStatModel : IModules
    {
        public string aid { get; set; }
        /// <summary>
        /// 播放
        /// </summary>
        public int view { get; set; }
        /// <summary>
        /// 弹幕
        /// </summary>
        public int danmaku { get; set; }
        /// <summary>
        /// 评论
        /// </summary>
        public int reply { get; set; }

        private int _favorite;
        /// <summary>
        /// 收藏
        /// </summary>
        public int favorite
        {
            get { return _favorite; }
            set { _favorite = value;DoPropertyChanged("favorite"); }
        }

        private int _coin;
        /// <summary>
        /// 投币
        /// </summary>
        public int coin
        {
            get { return _coin; }
            set { _coin = value; DoPropertyChanged("coin"); }
        }
        private int _share;
        /// <summary>
        /// 分享
        /// </summary>
        public int share
        {
            get { return _share; }
            set { _share = value; DoPropertyChanged("share"); }
        }

        private int _like;
        /// <summary>
        /// 点赞
        /// </summary>
        public int like
        {
            get { return _like; }
            set { _like = value; DoPropertyChanged("like"); }
        }
        /// <summary>
        /// 不喜欢，固定0
        /// </summary>
        public int dislike { get; set; }
    }
    public class VideoDetailReqUserModel:IModules
    {
        private int _attention;
        /// <summary>
        /// 是否关注
        /// </summary>
        public int attention
        {
            get { return _attention; }
            set { _attention = value;DoPropertyChanged("attention"); }
        }

        private int _guest_attention;
        /// <summary>
        /// 是否特别关注
        /// </summary>
        public int guest_attention
        {
            get { return _guest_attention; }
            set { _guest_attention = value; DoPropertyChanged("guest_attention"); }
        }

        private int _favorite;
        /// <summary>
        /// 是否收藏
        /// </summary>
        public int favorite
        {
            get { return _favorite; }
            set { _favorite = value; DoPropertyChanged("favorite"); }
        }

        private int _like;
        /// <summary>
        /// 是否点赞
        /// </summary>
        public int like
        {
            get { return _like; }
            set { _like = value; DoPropertyChanged("like"); }
        }

        private int _coin;
        /// <summary>
        /// 是否投币
        /// </summary>
        public int coin
        {
            get { return _coin; }
            set { _coin = value; DoPropertyChanged("coin"); }
        }

        private int _dislike;
        /// <summary>
        /// 是否不喜欢
        /// </summary>
        public int dislike
        {
            get { return _dislike; }
            set { _dislike = value; DoPropertyChanged("dislike"); }
        }
    }

    public class VideoDetailTagModel
    {
        public int tag_id { get; set; }
        public string tag_name { get; set; }
    }

    public class VideoDetailRelatesModel
    {
        public string aid { get; set; }
        public string pic { get; set; }
        public string title { get; set; }
        public VideoDetailOwnerModel owner { get; set; }
        public VideoDetailStatModel stat { get; set; }
    }

    public class VideoDetailPagesModel
    {
        public string cid { get; set; }
        public string page { get; set; }
        public string part { get; set; }
        public int duration { get; set; }
        public string dmlink { get; set; }
        public string download_title { get; set; }
        public string download_subtitle { get; set; }
    }
    public class VideoDetailInteractionModel
    {
        public int graph_version { get; set; }
        public VideoDetailInteractionHistoryNodeModel history_node { get; set; }

    }
    public class VideoDetailInteractionHistoryNodeModel
    {
        public int node_id { get; set; }
        public string title { get; set; }
        public long cid { get; set; }
    }
    public class VideoDetailStaffModel : IModules
    {
        public string mid { get; set; }
        public string title { get; set; }
        public string face { get; set; }
        public string name { get; set; }
   
        public VideoDetailOwnerExtVipModel vip { get; set; }
        public VideoDetailOwnerExtOfficialVerifyModel official_verify { get; set; }
        private int _attention;

        public int attention
        {
            get { return _attention; }
            set { _attention = value; DoPropertyChanged("attention"); }
        }

    }

    public class VideoDetailHistoryModel
    {
        public string cid { get; set; }
        public int progress { get; set; }

    }
}
