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
using Newtonsoft.Json;
using System.Windows.Input;
using BiliLite.Modules.User;

namespace BiliLite.Modules
{
    public class RecommendVM : IModules
    {
        readonly Api.Home.RecommendAPI recommendAPI;
        public RecommendVM()
        {
            recommendAPI = new Api.Home.RecommendAPI();
            Banner = new ObservableCollection<RecommendBannerItemModel>();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
        }
        public ICommand RefreshCommand { get; private set; }
        public ICommand LoadMoreCommand { get; private set; }

        private bool _loading = true;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }

        private ObservableCollection<RecommendBannerItemModel> _banner;

        public ObservableCollection<RecommendBannerItemModel> Banner
        {
            get { return _banner; }
            set { _banner = value; DoPropertyChanged("Banner"); }
        }

        private ObservableCollection<RecommendItemModel> _items;

        public ObservableCollection<RecommendItemModel> Items
        {
            get { return _items; }
            set { _items = value; DoPropertyChanged("Items"); }
        }


        public async Task GetRecommend(string idx = "0")
        {
            try
            {
                Loading = true;
                var result = await recommendAPI.Recommend(idx).Request();
                if (result.status)
                {
                    var obj = result.GetJObject();
                    if (obj["code"].ToInt32() == 0)
                    {
                        var items = JsonConvert.DeserializeObject<ObservableCollection<RecommendItemModel>>(obj["data"]["items"].ToString().Replace("left_bottom_rcmd_reason_style", "rcmd_reason_style"));
                        var banner = items.FirstOrDefault(x => x.card_goto == "banner");
                        if (banner != null)
                        {
                            //处理banner
                            LoadBanner(banner);
                            items.Remove(banner);
                        }
                        for (int i = items.Count - 1; i >= 0; i--)
                        {
                            if (items[i].showAD)
                            {
                                items.Remove(items[i]);
                                continue;
                            }
                            var item = items[i];
                            if (item.three_point_v2 != null && item.three_point_v2.Count > 0 && item.card_goto == "av")
                            {
                                item.three_point_v2.Insert(1, new RecommendThreePointV2ItemModel()
                                {
                                    idx = item.idx,
                                    url = $"https://b23.tv/av{item.param}",
                                    title = "使用浏览器打开",
                                    type = "browser"
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
                    else
                    {
                        Utils.ShowMessageToast(obj["message"].ToString());
                    }
                }
                else
                {
                    Utils.ShowMessageToast(result.message);
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

        private void LoadBanner(RecommendItemModel banner)
        {
            try
            {
                if (Banner != null || Banner.Count == 0)
                {
                    foreach (var item in banner.banner_item)
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
            }
            catch (Exception)
            {
            }
           
        }
        public async void LoadMore()
        {
            if (Items == null || Items.Count == 0)
            {
                return;
            }
            if (Loading)
            {
                return;
            }
            await GetRecommend(Items.LastOrDefault().idx);
        }
        public async void Refresh()
        {
            if (Loading)
            {
                Utils.ShowMessageToast("正在加载中....");
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
                if (!SettingHelper.Account.Logined && await Utils.ShowLoginDialog())
                {
                    Utils.ShowMessageToast("请先登录");
                    return;
                }
                var recommendItem = Items.FirstOrDefault(x => x.idx == idx);
                var api = recommendAPI.Dislike(_goto: recommendItem.card_goto, id: recommendItem.param, mid: recommendItem.args.up_id, reason_id: itemReasons?.id ?? 0, rid: recommendItem.args.rid, tag_id: recommendItem.args.tid);
                if (threePointV2Item.type == "feedback")
                {
                    recommendAPI.Feedback(_goto: recommendItem.card_goto, id: recommendItem.param, mid: recommendItem.args.up_id, feedback_id: itemReasons?.id ?? 0, rid: recommendItem.args.rid, tag_id: recommendItem.args.tid);
                }
                var result = await api.Request();
                if (result.status)
                {
                    var obj = result.GetJObject();
                    if (obj["code"].ToInt32() == 0)
                    {
                        Items.Remove(recommendItem);
                    }
                    else
                    {
                        Utils.ShowMessageToast(obj["message"].ToString());
                    }
                }
                else
                {
                    Utils.ShowMessageToast(result.message);
                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<AnimeHomeModel>(ex);
                Utils.ShowMessageToast(handel.message);
            }
        }


    }
    public class RecommendItemModel
    {
        //public ObservableCollection<RecommendBannerItemModel> banner_item { get; set; }
        public JArray banner_item { get; set; }
        private string _title = "";
        public string title
        {
            get
            {
                if (string.IsNullOrEmpty(_title) && string.IsNullOrEmpty(uri))
                {
                    if (ad_info == null)
                    {
                        return "你追的番剧更新啦~";
                    }
                    else
                    {
                        return ad_info.creative_content.title;
                    }
                }
                return _title;
            }
            set { _title = value; }
        }

        public string _cover { get; set; }

        public string cover
        {
            get
            {
                if (string.IsNullOrEmpty(_cover) && ad_info != null)
                {
                    return ad_info.creative_content.image_url;
                }
                else
                {
                    return _cover;
                }
            }
            set
            {
                _cover = value;
            }
        }

        public string uri { get; set; }
        public string param { get; set; }
        public string card_goto { get; set; }

        public string idx { get; set; }

        private List<RecommendThreePointV2ItemModel> _three_point_v2;

        public List<RecommendThreePointV2ItemModel> three_point_v2
        {
            get
            {
                if (_three_point_v2 != null)
                {
                    foreach (var item in _three_point_v2)
                    {
                        item.idx = idx;
                    }
                }

                return _three_point_v2;
            }
            set { _three_point_v2 = value; }
        }


        public RecommendItemArgsModel args { get; set; }

        public RecommendRcmdReasonStyleModel rcmd_reason_style { get; set; }
        public RecommendDescButtonModel desc_button { get; set; }
        public RecommendADInfoModel ad_info { get; set; }
        public string cover_left_text_1 { get; set; }
        public string cover_left_text_2 { get; set; }
        public int cover_left_icon_1 { get; set; }

        public int cover_left_icon_2 { get; set; }
        public string left_text
        {
            get
            {
                return $"{iconToText(cover_left_icon_1)}{cover_left_text_1 ?? ""} {iconToText(cover_left_icon_2)}{cover_left_text_2 ?? ""}";
            }
        }
        public string cover_right_text { get; set; }

        public string badge { get; set; }
        public bool showBadge
        {
            get
            {
                return !string.IsNullOrEmpty(badge);
            }
        }

        public bool showCoverText
        {
            get
            {
                return !string.IsNullOrEmpty(cover_left_text_1) || !string.IsNullOrEmpty(cover_left_text_2) || !string.IsNullOrEmpty(cover_right_text);
            }
        }
        public bool showRcmd
        {
            get
            {
                return rcmd_reason_style != null;
            }
        }
        public bool showAD
        {
            get
            {
                return ad_info != null && ad_info.creative_content != null;
            }
        }
        public string bottomText
        {
            get
            {
                if (desc_button != null)
                {
                    return desc_button.text;
                }
                if (card_goto=="live")
                {
                    return args.up_name;
                }
                return "";
            }
        }

        public string iconToText(int icon)
        {
            switch (icon)
            {
                case 1:
                case 6:
                    return "观看:";
                case 2:
                    return "人气:";
                case 3:
                    return "弹幕:";
                case 4:
                    return "追番:";
                case 7:
                    return "评论:";
                default:
                    return "";
            }
        }
    }
    public class RecommendBannerItemModel
    {
        public int id { get; set; }
        public string title { get; set; }
        public string image { get; set; }


        public string hash { get; set; }

        public string uri { get; set; }

        public string request_id { get; set; }
        /// <summary>
        /// Server_type
        /// </summary>
        public int server_type { get; set; }
        /// <summary>
        /// Resource_id
        /// </summary>
        public int resource_id { get; set; }
        /// <summary>
        /// Index
        /// </summary>
        public int index { get; set; }
        /// <summary>
        /// Cm_mark
        /// </summary>
        public int cm_mark { get; set; }
    }
    public class RecommendThreePointV2ItemModel
    {

        public string idx { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public string subtitle { get; set; }
        public string url { get; set; }
        public List<RecommendThreePointV2ItemReasonsModel> reasons { get; set; }

    }
    public class RecommendThreePointV2ItemReasonsModel
    {
        public int id { get; set; }
        public string name { get; set; }

    }
    public class RecommendItemArgsModel
    {
        public string up_id { get; set; }
        public string up_name { get; set; }
        public int rid { get; set; }
        public int tid { get; set; }
        public string tname { get; set; }
        public string rname { get; set; }
        public int aid { get; set; }

    }
    public class RecommendRcmdReasonStyleModel
    {
        public string text { get; set; }
        public string text_color { get; set; }

        public string bg_color { get; set; }

        public string border_color { get; set; }
        public string text_color_night { get; set; }
        public string bg_color_night { get; set; }
        public string border_color_night { get; set; }
        public int bg_style { get; set; }

    }
    public class RecommendDescButtonModel
    {
        public string text { get; set; }
        public string uri { get; set; }
    }
    public class RecommendADInfoModel
    {
        public string creative_id { get; set; }
        public RecommendADInfoCreativeModel creative_content { get; set; }
    }
    public class RecommendADInfoCreativeModel
    {
        public string description { get; set; }
        public string title { get; set; }
        public string image_url { get; set; }
        public string url { get; set; }
        public string click_url { get; set; }
    }
}
