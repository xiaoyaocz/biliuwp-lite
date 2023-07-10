using Newtonsoft.Json;

namespace BiliLite.Models.Common.Season
{
    public class SeasonDetailUserStatusModel
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