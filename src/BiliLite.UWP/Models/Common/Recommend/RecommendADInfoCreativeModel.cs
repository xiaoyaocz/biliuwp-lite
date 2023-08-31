using Newtonsoft.Json;

namespace BiliLite.Models.Common.Recommend
{
    public class RecommendADInfoCreativeModel
    {
        public string Description { get; set; }

        public string Title { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        public string Url { get; set; }

        [JsonProperty("click_url")]
        public string ClickUrl { get; set; }
    }
}