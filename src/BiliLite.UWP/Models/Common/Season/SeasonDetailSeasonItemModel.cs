using Newtonsoft.Json;

namespace BiliLite.Models.Common.Season
{
    public class SeasonDetailSeasonItemModel
    {
        [JsonProperty("season_id")]
        public int SeasonId { get; set; }

        public string Cover { get; set; }

        [JsonProperty("season_title")]
        public string SeasonTitle { get; set; }

        public string Title { get; set; }
    }
}