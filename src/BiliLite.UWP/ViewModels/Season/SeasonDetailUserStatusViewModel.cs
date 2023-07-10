using BiliLite.Models.Common.Season;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json;

namespace BiliLite.ViewModels.Season
{
    public class SeasonDetailUserStatusViewModel : BaseViewModel
    {
        public int Follow { get; set; }

        [JsonProperty("follow_bubble")]
        public int FollowBubble { get; set; }

        [JsonProperty("follow_status")]
        public int FollowStatus { get; set; }

        public int Pay { get; set; }

        public int Vip { get; set; }

        public int Sponsor { get; set; }

        [JsonProperty("vip_frozen")]
        public int VipFrozen { get; set; }

        public SeasonDetailUserStatusProgressModel Progress { get; set; }
    }
}