using Newtonsoft.Json;

namespace BiliLite.Models.Common.Recommend
{
    public class RecommendADInfoModel
    {
        [JsonProperty("creative_id")]
        public string CreativeId { get; set; }

        [JsonProperty("creative_content")]
        public RecommendADInfoCreativeModel CreativeContent { get; set; }
    }
}