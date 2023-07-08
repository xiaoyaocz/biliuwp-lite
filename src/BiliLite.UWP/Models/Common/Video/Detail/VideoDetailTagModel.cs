using Newtonsoft.Json;

namespace BiliLite.Models.Common.Video.Detail
{
    public class VideoDetailTagModel
    {
        [JsonProperty("tag_id")]
        public int TagId { get; set; }

        [JsonProperty("tag_name")]
        public string TagName { get; set; }
    }
}