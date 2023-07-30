using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BiliLite.Models.Common.Recommend
{
    public class RecommendItemModel
    {
        //public ObservableCollection<RecommendBannerItemModel> banner_item { get; set; }
        [JsonProperty("banner_item")]
        public JArray BannerItem { get; set; }

        private string m_title = "";

        public string Title
        {
            get
            {
                if (!string.IsNullOrEmpty(m_title) || !string.IsNullOrEmpty(Uri)) return m_title;
                return AdInfo == null ? "你追的番剧更新啦~" : AdInfo.CreativeContent.Title;
            }
            set => m_title = value;
        }

        private string m_cover;

        public string Cover
        {
            get
            {
                if (string.IsNullOrEmpty(m_cover) && AdInfo != null)
                {
                    return AdInfo.CreativeContent.ImageUrl;
                }
                return m_cover;
            }
            set => m_cover = value;
        }

        public string Uri { get; set; }

        public string Param { get; set; }

        [JsonProperty("card_goto")]
        public string CardGoto { get; set; }

        public string Idx { get; set; }

        private List<RecommendThreePointV2ItemModel> m_threePointV2;

        [JsonProperty("three_point_v2")]
        public List<RecommendThreePointV2ItemModel> ThreePointV2
        {
            get
            {
                if (m_threePointV2 == null) return m_threePointV2;
                foreach (var item in m_threePointV2)
                {
                    item.Idx = Idx;
                }

                return m_threePointV2;
            }
            set => m_threePointV2 = value;
        }

        public RecommendItemArgsModel Args { get; set; }

        [JsonProperty("rcmd_reason_style")]
        public RecommendRcmdReasonStyleModel RcmdReasonStyle { get; set; }

        [JsonProperty("desc_button")]
        public RecommendDescButtonModel DescButton { get; set; }

        [JsonProperty("ad_info")]
        public RecommendADInfoModel AdInfo { get; set; }

        [JsonProperty("cover_left_text_1")]
        public string CoverLeftText1 { get; set; }

        [JsonProperty("cover_left_text_2")]
        public string CoverLeftText2 { get; set; }

        [JsonProperty("cover_left_icon_1")]
        public int CoverLeftIcon1 { get; set; }

        [JsonProperty("cover_left_icon_2")]
        public int CoverLeftIcon2 { get; set; }

        public string LeftText => $"{IconToText(CoverLeftIcon1)}{CoverLeftText1 ?? ""} {IconToText(CoverLeftIcon2)}{CoverLeftText2 ?? ""}";

        [JsonProperty("cover_right_text")]
        public string CoverRightText { get; set; }

        public string Badge { get; set; }

        public bool ShowBadge => !string.IsNullOrEmpty(Badge);

        public bool ShowCoverText => !string.IsNullOrEmpty(CoverLeftText1) || !string.IsNullOrEmpty(CoverLeftText2) || !string.IsNullOrEmpty(CoverRightText);

        public bool ShowRcmd => RcmdReasonStyle != null;

        public bool ShowAd => AdInfo is { CreativeContent: { } };

        public string BottomText => DescButton != null ? DescButton.Text : Args.UpName;

        public string IconToText(int icon)
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
}