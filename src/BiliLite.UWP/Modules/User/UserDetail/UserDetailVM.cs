using BiliLite.Models;
using BiliLite.Models.Requests.Api.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Media;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Services;

namespace BiliLite.Modules.User
{
    public class UserDetailVM : IModules
    {
        private static readonly ILogger logger = GlobalLogger.FromCurrentType();

        public string mid { get; set; }
        private readonly UserDetailAPI userDetailAPI;
        private readonly FollowAPI followAPI;
        public UserDetailVM()
        {
            userDetailAPI = new UserDetailAPI();
            followAPI = new FollowAPI();
            AttentionCommand = new RelayCommand(DoAttentionUP);
        }
        private UserCenterInfoModel _userInfo;
        public UserCenterInfoModel UserInfo
        {
            get { return _userInfo; }
            set { _userInfo = value; DoPropertyChanged("UserInfo"); }
        }

        public ICommand AttentionCommand { get; private set; }

        public async void GetUserInfo()
        {
            try
            {
                var api = await userDetailAPI.UserInfo(mid);
                var result = await api.Request();

                if (result.status)
                {
                    var data = await result.GetData<UserCenterInfoModel>();
                    if (data.success)
                    {
                        data.data.stat = await GetSpeceStat();
                        UserInfo = data.data;
                    }
                    else
                    {
                        Notify.ShowMessageToast(data.message);
                    }

                }
                else
                {
                    Notify.ShowMessageToast(result.message);
                }
            }
            catch (Exception ex)
            {
                logger.Log("读取个人资料失败", LogType.Error, ex);
                Notify.ShowMessageToast("读取个人资料失败");
            }
        }
        public async Task<UserCenterInfoStatModel> GetStat()
        {
            try
            {
                var result = await userDetailAPI.UserStat(mid).Request();

                if (result.status)
                {
                    var data = await result.GetData<UserCenterInfoStatModel>();
                    if (data.success)
                    {
                        return data.data;
                    }
                    else
                    {
                        return null;
                    }

                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                logger.Log("读取个人资料失败", LogType.Error, ex);
                return null;
            }
        }

        public async Task<UserCenterSpaceStatModel> GetSpeceStat()
        {
            try
            {
                var result = await userDetailAPI.Space(mid).Request();

                if (result.status)
                {
                    var data = await result.GetData<JObject>();
                    if (data.success)
                    {
                        UserCenterSpaceStatModel stat = new UserCenterSpaceStatModel();
                        stat.article_count = (data.data["article"]?["count"] ?? 0).ToInt32();
                        stat.video_count = (data.data["archive"]?["count"] ?? 0).ToInt32();
                        stat.favourite_count = (data.data["favourite2"]?["count"] ?? 0).ToInt32();
                        stat.follower = data.data["card"]["fans"].ToInt32();
                        stat.following = data.data["card"]["attention"].ToInt32();
                        return stat;
                    }
                    else
                    {
                        return null;
                    }

                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                logger.Log("读取个人资料失败", LogType.Error, ex);
                return null;
            }
        }
        public async void DoAttentionUP()
        {
            var result = await AttentionUP(UserInfo.mid.ToString(), UserInfo.is_followed ? 2 : 1);
            if (result)
            {
                UserInfo.is_followed = !UserInfo.is_followed;
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
                var results = await followAPI.Attention(mid, mode.ToString()).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<object>>();
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


    }



    public class UserCenterInfoOfficialModel
    {
        public int role { get; set; }
        public string title { get; set; }
        public string desc { get; set; }
        public int type { get; set; }
        public bool showOfficial { get { return type != -1; } }
    }

    public class UserCenterInfoVipLabelModel
    {
        public string path { get; set; }
        public string text { get; set; }
        public string label_theme { get; set; }
    }

    public class UserCenterInfoVipModel
    {
        public int type { get; set; }
        public int status { get; set; }
        public int theme_type { get; set; }
        public UserCenterInfoVipLabelModel label { get; set; }
        public int avatar_subscript { get; set; }
        public string nickname_color { get; set; }
    }

    public class UserCenterInfoPendantModel
    {
        public int pid { get; set; }
        public string name { get; set; }
        public string image { get; set; }
        public int expire { get; set; }
        public string image_enhance { get; set; }
    }

    public class UserCenterInfoNameplateModel
    {
        public int nid { get; set; }
        public string name { get; set; }
        public string image { get; set; }
        public string image_small { get; set; }
        public string level { get; set; }
        public string condition { get; set; }
    }



    public class UserCenterInfoLiveRoomModel
    {
        public int roomStatus { get; set; }
        public int liveStatus { get; set; }
        public string url { get; set; }
        public string title { get; set; }
        public string cover { get; set; }
        public int online { get; set; }
        public int roomid { get; set; }
        public int roundStatus { get; set; }
        public int broadcast_type { get; set; }
    }
    public class UserCenterInfoStatModel
    {
        public long mid { get; set; }
        public int following { get; set; }
        public int whisper { get; set; }
        public int black { get; set; }
        public int follower { get; set; }
    }
    public class UserCenterSpaceStatModel
    {
        public int following { get; set; }
        public string attention
        {
            get
            {
                return following > 0 ? " " + following.ToCountString() : "";
            }
        }
        public int follower { get; set; }
        public string fans
        {
            get
            {
                return follower > 0 ? " " + follower.ToCountString() : "";
            }
        }
        public int video_count { get; set; }
        public string video
        {
            get
            {
                return video_count > 0 ? " " + video_count.ToCountString() : "";
            }
        }
        public int article_count { get; set; }
        public string article
        {
            get
            {
                return article_count > 0 ? " " + article_count.ToCountString() : "";
            }
        }
        public int favourite_count { get; set; }
        public string favourite
        {
            get
            {
                return favourite_count > 0 ? " " + favourite_count.ToCountString() : "";
            }
        }

    }
    public class UserCenterInfoModel : IModules
    {
        public long mid { get; set; }
        public string name { get; set; }
        public string sex { get; set; }
        public string face { get; set; }
        public string sign { get; set; }
        public int rank { get; set; }
        public int level { get; set; }
        public SolidColorBrush level_color
        {
            get
            {
                switch (level)
                {

                    case 2:
                        return new SolidColorBrush(Colors.LightGreen);
                    case 3:
                        return new SolidColorBrush(Colors.LightBlue);
                    case 4:
                        return new SolidColorBrush(Colors.Yellow);
                    case 5:
                        return new SolidColorBrush(Colors.Orange);
                    case 6:
                        return new SolidColorBrush(Colors.Red);
                    case 7:
                        return new SolidColorBrush(Colors.HotPink);
                    case 8:
                        return new SolidColorBrush(Colors.Purple);
                    default:
                        return new SolidColorBrush(Colors.Gray);
                }
            }
        }
        public int jointime { get; set; }
        public int moral { get; set; }
        public int silence { get; set; }
        public string birthday { get; set; }
        public double coins { get; set; }
        public bool fans_badge { get; set; }
        public UserCenterInfoOfficialModel official { get; set; }
        public UserCenterInfoVipModel vip { get; set; }
        public UserCenterInfoPendantModel pendant { get; set; }
        public UserCenterInfoNameplateModel nameplate { get; set; }
        private bool _is_followed;

        public bool is_followed
        {
            get { return _is_followed; }
            set { _is_followed = value; DoPropertyChanged("is_followed"); }
        }

        public string top_photo { get; set; }

        public UserCenterInfoLiveRoomModel live_room { get; set; }
        public UserCenterSpaceStatModel stat { get; set; }
        public string pendant_str
        {
            get
            {
                if (pendant != null)
                {
                    if (pendant.image == "")
                    {
                        return Constants.App.TRANSPARENT_IMAGE;
                    }
                    return pendant.image;
                }
                else
                {
                    return Constants.App.TRANSPARENT_IMAGE;
                }
            }
        }
        public string Verify
        {
            get
            {
                if (official == null)
                {
                    return "";
                }
                switch (official.type)
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
    }
}
