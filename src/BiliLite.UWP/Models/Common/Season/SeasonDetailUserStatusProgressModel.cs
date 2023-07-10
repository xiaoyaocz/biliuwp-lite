using Newtonsoft.Json;

namespace BiliLite.Models.Common.Season
{
    public class SeasonDetailUserStatusProgressModel
    {
        [JsonProperty("last_ep_index")]
        public string LastEpIndex { get; set; }

        [JsonProperty("last_ep_id")]
        public int LastEpId { get; set; }

        [JsonProperty("last_time")]
        public int LastTime { get; set; }
    }
}