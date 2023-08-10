using Newtonsoft.Json;

namespace BiliLite.Models.Common.Dynamic
{
    public class DynamicSeasonCardModel
    {
        public string Aid { get; set; }

        public string Cover { get; set; }

        [JsonProperty("index_title")]
        public string IndexTitle { get; set; }

        public string Index { get; set; }

        [JsonProperty("new_desc")]
        public string NewDesc { get; set; }

        public string Url { get; set; }

        [JsonProperty("play_count")]
        public int PlayCount { get; set; }

        [JsonProperty("reply_count")]
        public int ReplyCount { get; set; }

        [JsonProperty("bullet_count")]
        public int BulletCount { get; set; }

        [JsonProperty("episode_id")]
        public int EpisodeId { get; set; }

        public DynamicSeasonCardApiSeasonInfoModel Season { get; set; }
    }
}