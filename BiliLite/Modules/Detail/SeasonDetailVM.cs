using BiliLite.Api;
using BiliLite.Helpers;
using BiliLite.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BiliLite.Modules
{
    public class SeasonDetailVM : IModules
    {
        readonly Api.SeasonApi seasonApi;
        readonly PlayerAPI PlayerAPI;
        readonly Api.User.FollowAPI followAPI;
        public SeasonDetailVM()
        {
            seasonApi = new Api.SeasonApi();
            PlayerAPI = new PlayerAPI();
            followAPI = new Api.User.FollowAPI();
            FollowCommand = new RelayCommand(DoFollow);
        }
        private SeasonDetailModel _Detail;

        public SeasonDetailModel Detail
        {
            get { return _Detail; }
            set { _Detail = value; DoPropertyChanged("Detail"); }
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
        public ICommand FollowCommand { get; private set; }

        private List<SeasonDetailEpisodeModel> _episodes;
        public List<SeasonDetailEpisodeModel> Episodes
        {
            get { return _episodes; }
            set { _episodes = value; DoPropertyChanged("Episodes"); }
        }
        private bool _showEpisodes = false;
        public bool ShowEpisodes
        {
            get { return _showEpisodes; }
            set { _showEpisodes = value; DoPropertyChanged("ShowEpisodes"); }
        }

        private List<SeasonDetailEpisodeModel> _previews;
        public List<SeasonDetailEpisodeModel> Previews
        {
            get { return _previews; }
            set { _previews = value; DoPropertyChanged("Previews"); }
        }
        private bool _showPreview = false;
        public bool ShowPreview
        {
            get { return _showPreview; }
            set { _showPreview = value; DoPropertyChanged("ShowPreview"); }
        }
        private bool _nothingPlay = false;
        public bool NothingPlay
        {
            get { return _nothingPlay; }
            set { _nothingPlay = value; DoPropertyChanged("NothingPlay"); }
        }

        public async Task LoadSeasonDetail(string season_id)
        {
            try
            {
                Loaded = false;
                Loading = true;
                var results = await seasonApi.Detail(season_id).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiResultModel<SeasonDetailModel>>();
                    if (data.success)
                    {
                        if (data.result.limit != null)
                        {
                            var reulsts_web = await seasonApi.DetailWeb(season_id).Request();
                            if (reulsts_web.status)
                            {
                                var data_2 = reulsts_web.GetJObject();
                                if (data_2["code"].ToInt32() == 0)
                                {
                                    data.result.episodes = await Utils.DeserializeJson<List<SeasonDetailEpisodeModel>>(data_2["result"]["episodes"].ToString());


                                }
                            }
                        }
                        if (data.result.section != null)
                        {
                            foreach (var item in data.result.section)
                            {
                                foreach (var item2 in item.episodes)
                                {
                                    item2.section_type = 1;
                                }
                                data.result.episodes.InsertRange(0, item.episodes);
                                //data.result.episodes= data.result.episodes.Concat(item.episodes).ToList();
                            }
                        }
                        Detail = data.result;

                        Episodes = data.result.episodes.Where(x => !x.IsPreview).ToList();
                        ShowEpisodes = Episodes.Count > 0;
                        Previews = data.result.episodes.Where(x => x.IsPreview).ToList();
                        ShowPreview = Previews.Count > 0;
                        NothingPlay = !ShowEpisodes && !ShowPreview;
                        Loaded = true;
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

        public async void DoFollow()
        {
            if (!SettingHelper.Account.Logined && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录后再操作");
                return;
            }
            try
            {
                var api = followAPI.FollowSeason(Detail.season_id.ToString());
                if (Detail.user_status.follow == 1)
                {
                    api = followAPI.CancelFollowSeason(Detail.season_id.ToString());
                }
                var results = await api.Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiResultModel<JObject>>();
                    if (data.success)
                    {
                        if (Detail.user_status.follow == 1)
                        {
                            Detail.user_status.follow = 0;
                        }
                        else
                        {
                            Detail.user_status.follow = 1;
                        }
                        if (!string.IsNullOrEmpty(data.result["toast"]?.ToString()))
                        {
                            Utils.ShowMessageToast(data.result["toast"].ToString());
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

    }

    public class SeasonDetailModel
    {
        public int season_id { get; set; }
        public string season_title { get; set; }
        public string title { get; set; }
        public string cover { get; set; }
        public string evaluate { get; set; }
        public string alias { get; set; }
        public string badge { get; set; }
        public int badge_type { get; set; }
        public int status { get; set; }
        public string subtitle { get; set; }
        public bool show_badge { get { return !string.IsNullOrEmpty(badge); } }
        public string link { get; set; }
        public string short_link { get; set; }
        public string square_cover { get; set; }
        public int media_id { get; set; }
        public int mode { get; set; }
        public SeasonDetailActorModel actor { get; set; }
        public SeasonDetailActorModel staff { get; set; }
        public List<SeasonDetailAreaItemModel> areas { get; set; }
        public string area
        {
            get
            {
                var r = "";
                if (areas != null)
                {
                    foreach (var item in areas)
                    {
                        r += item.name + " ";
                    }
                    return r;
                }
                else
                {
                    return "";
                }
            }
        }
        public SeasonDetailNewEpModel new_ep { get; set; }
        public List<SeasonDetailEpisodeModel> episodes { get; set; }
        public string origin_name { get; set; }
        public SeasonDetailRatingModel rating { get; set; }
        public bool show_rating
        {
            get
            {
                return rating != null;
            }
        }
        public SeasonDetailPublishModel publish { get; set; }
        public List<SeasonDetailSeasonItemModel> seasons { get; set; }
        public bool show_seasons
        {
            get
            {
                return seasons != null && seasons.Count > 1;
            }
        }
        public SeasonDetailSeasonItemModel current_season
        {
            get
            {
                if (seasons != null)
                {
                    return seasons.FirstOrDefault(x => x.season_id == season_id);
                }
                else
                {
                    return null;
                }
            }
        }

        public List<SeasonDetailStyleItemModel> styles { get; set; }
        public SeasonDetailStatModel stat { get; set; }
        public int total { get; set; }
        public int type { get; set; }
        public string type_name { get; set; }
        public SeasonDetailUserStatusModel user_status { get; set; }
        public SeasonDetailLimitModel limit { get; set; }
        public List<SeasonDetailSectionItemModel> section { get; set; }
        public SeasonDetailPaymentModel payment { get; set; }
        public bool show_payment
        {
            get
            {
                return payment != null && payment.dialog != null;
            }
        }
    }
    public class SeasonDetailPaymentModel
    {
        public string price { get; set; }
        public string tv_price { get; set; }
        public SeasonDetailPaymentDialogModel dialog { get; set; }
    }
    public class SeasonDetailPaymentDialogModel
    {
        public string title { get; set; }
        public string desc { get; set; }

    }
    public class SeasonDetailSectionItemModel
    {
        public string title { get; set; }
        public int id { get; set; }
        public int type { get; set; }
        public List<SeasonDetailEpisodeModel> episodes { get; set; }
    }
    public class SeasonDetailEpisodeModel
    {
        public string aid { get; set; }
        public string cid { get; set; }
        public int badge_type { get; set; }
        public string badge { get; set; }
        public bool show_badge
        {
            get
            {
                return !string.IsNullOrEmpty(badge);
            }
        }
        public string cover { get; set; }
        public string bvid { get; set; }

        private int? _id;
        public int id
        {
            get
            {
                if (_id == null && ep_id != null)
                {
                    return ep_id.Value;
                }
                return _id.Value;
            }
            set { _id = value; }
        }
        private int? _status;
        public int status
        {
            get
            {
                if (_status == null && episode_status != null)
                {
                    return episode_status.Value;
                }
                return _status.Value;
            }
            set { _status = value; }
        }

        private string _title;

        public string title
        {
            get
            {
                if (_title == null && index != null)
                {
                    return index;
                }
                return _title;
            }
            set { _title = value; }
        }


        private string _long_title;

        public string long_title
        {
            get
            {
                if (_long_title == null && index_title != null)
                {
                    return index_title;
                }
                return _long_title;
            }
            set { _long_title = value; }
        }


        public int? ep_id { get; set; }
        public int? episode_status { get; set; }
        public string index { get; set; }
        public string index_title { get; set; }
        public int section_type { get; set; }
        public bool IsPreview { get { return section_type != 0; } }
    }
    public class SeasonDetailUserStatusModel : IModules
    {

        private int _follow;

        public int follow
        {
            get { return _follow; }
            set { _follow = value; DoPropertyChanged("follow"); }
        }

        public int follow_bubble { get; set; }
        public int follow_status { get; set; }
        public int pay { get; set; }
        public int vip { get; set; }
        public int sponsor { get; set; }
        public int vip_frozen { get; set; }
        public SeasonDetailUserStatusProgressModel progress { get; set; }
    }
    public class SeasonDetailUserStatusProgressModel
    {
        public string last_ep_index { get; set; }
        public int last_ep_id { get; set; }
        public int last_time { get; set; }
    }
    public class SeasonDetailLimitModel
    {
        public string content { get; set; }
        public string image { get; set; }
    }
    public class SeasonDetailStatModel
    {
        public int coins { get; set; }
        public int danmakus { get; set; }
        public int favorites { get; set; }
        public string followers { get; set; }
        public string play { get; set; }
        public int reply { get; set; }
        public int share { get; set; }
        public int views { get; set; }
    }
    public class SeasonDetailNewEpModel
    {
        public string desc { get; set; }
        public string id { get; set; }
        public string title { get; set; }
    }
    public class SeasonDetailStyleItemModel
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }
    public class SeasonDetailPublishModel
    {
        public int is_finish { get; set; }
        public int is_started { get; set; }
        public int weekday { get; set; }
        public string pub_time { get; set; }
        public string pub_time_show { get; set; }
        public string release_date_show { get; set; }
        public string time_length_show { get; set; }
    }
    public class SeasonDetailSeasonItemModel
    {
        public int season_id { get; set; }
        public string cover { get; set; }
        public string season_title { get; set; }
        public string title { get; set; }
    }
    public class SeasonDetailRatingModel
    {
        public int count { get; set; }
        public double score { get; set; }
        public double score_5
        {
            get
            {
                return score / 2;
            }
        }
    }
    public class SeasonDetailActorModel
    {
        public string title { get; set; }
        public string info { get; set; }
    }
    public class SeasonDetailAreaItemModel
    {
        public string id { get; set; }
        public string name { get; set; }
    }

}
