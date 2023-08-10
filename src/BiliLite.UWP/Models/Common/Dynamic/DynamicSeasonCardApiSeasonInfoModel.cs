using Newtonsoft.Json;

namespace BiliLite.Models.Common.Dynamic
{
    public class DynamicSeasonCardApiSeasonInfoModel
    {
        [JsonProperty("type_name")]
        public string TypeName { get; set; }

        public string Cover { get; set; }

        public string Title { get; set; }

        [JsonProperty("is_finish")]
        public int IsFinish { get; set; }

        [JsonProperty("season_id")]
        public long SeasonId { get; set; }
    }
}