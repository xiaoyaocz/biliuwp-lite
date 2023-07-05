using Newtonsoft.Json;

namespace BiliLite.Models.Common.Comment
{
    public class CommentMemberFansDetailModel
    {
        public long Uid { get; set; }

        [JsonProperty("medal_id")]
        public int MedalId { get; set; }

        [JsonProperty("medal_name")]
        public string MedalName { get; set; }

        public int Level { get; set; }

        [JsonProperty("medal_color")]
        public long MedalColor { get; set; }

        [JsonProperty("medal_color_end")]
        public long MedalColorEnd { get; set; }

        [JsonProperty("medal_color_border")]
        public long MedalColorBorder { get; set; }

        [JsonProperty("medal_color_name")]
        public long MedalColorName { get; set; }

        [JsonProperty("medal_color_level")]
        public long MedalColorLevel { get; set; }
    }
}