using System.Collections.Generic;
using System.Linq;
using BiliLite.Models.Common.Video;
using BiliLite.Models.Common.Video.Detail;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json;

namespace BiliLite.ViewModels.Video
{
    public class VideoDetailViewModel : BaseViewModel
    {
        public string Bvid { get; set; }

        public string Aid { get; set; }

        /// <summary>
        /// 视频数量
        /// </summary>
        public int Videos { get; set; }

        /// <summary>
        /// 分区ID
        /// </summary>
        public int Tid { get; set; }

        /// <summary>
        /// 分区名
        /// </summary>
        public string Tname { get; set; }

        /// <summary>
        /// 封面
        /// </summary>
        public string Pic { get; set; }

        public string Title { get; set; }

        /// <summary>
        /// 上传时间
        /// </summary>
        [JsonProperty("pubdate")]
        public long PubDate { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public long Ctime { get; set; }

        /// <summary>
        /// 简介
        /// </summary>
        public string Desc { get; set; }

        public long Attribute { get; set; }

        public int State { get; set; }

        /// <summary>
        /// 时长
        /// </summary>
        public int Duration { get; set; }

        public VideoDetailRightsModel Rights { get; set; }

        public string Dynamic { get; set; }

        /// <summary>
        /// UP主
        /// </summary>
        public VideoDetailOwnerModel Owner { get; set; }

        /// <summary>
        /// UP主信息扩展
        /// </summary>
        [JsonProperty("owner_ext")]
        public VideoDetailOwnerExtModel OwnerExt { get; set; } = new VideoDetailOwnerExtModel();

        /// <summary>
        /// 数据
        /// </summary>
        public VideoDetailStatViewModel Stat { get; set; }

        /// <summary>
        /// 用户数据
        /// </summary>
        [JsonProperty("req_user")]
        public VideoDetailReqUserViewModel ReqUser { get; set; } = new VideoDetailReqUserViewModel();

        /// <summary>
        /// Tag
        /// </summary>
        public List<VideoDetailTagModel> Tag { get; set; }

        private List<VideoDetailRelatesViewModel> m_relates;

        /// <summary>
        /// 推荐
        /// </summary>
        public List<VideoDetailRelatesViewModel> Relates
        {
            get { return m_relates; }
            set { m_relates = value?.Where(x => !string.IsNullOrEmpty(x.Aid)).ToList(); ; }
        }

        [JsonProperty("share_subtitle")]
        public string ShareSubtitle { get; set; }

        [JsonProperty("short_link")]
        public string ShortLink { get; set; }

        [JsonProperty("ugc_season")]
        public VideoUgcSeason UgcSeason { get; set; }

        public bool ShowUgcSeason => UgcSeason != null && UgcSeason.Sections != null && UgcSeason.Sections.Count > 0;

        [JsonProperty("redirect_url")]
        public string RedirectUrl { get; set; }

        public List<VideoDetailPagesModel> Pages { get; set; }

        public bool ShowPages => Pages != null && Pages.Count > 1;

        /// <summary>
        /// 互动视频
        /// </summary>
        public VideoDetailInteractionModel Interaction { get; set; }

        public List<VideoDetailStaffViewModel> Staff { get; set; }

        public bool ShowStaff => Staff != null && Staff.Count > 0;

        [JsonProperty("argue_msg")]
        public string ArgueMsg { get; set; }

        public bool ShowArgueMsg => !string.IsNullOrEmpty(ArgueMsg);

        public VideoDetailHistoryModel History { get; set; }
    }
}