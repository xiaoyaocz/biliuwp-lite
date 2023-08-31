using Newtonsoft.Json;

namespace BiliLite.Models.Common.Anime
{
    public class AnimeRankModel
    {
        public string Display { get; set; }

        public string Title { get; set; }

        public string Cover { get; set; }

        [JsonProperty("season_id")]
        public int SeasonId { get; set; }

        [JsonProperty("index_show")]
        public string IndexShow { get; set; }

        public int Follow { get; set; }

        public int Danmaku { get; set; }

        public int View { get; set; }

        [JsonProperty("show_badge")]
        public bool ShowBadge => !string.IsNullOrEmpty(Badge);

        public string Badge { get; set; }
    }
}