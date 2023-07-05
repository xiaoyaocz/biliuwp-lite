using Newtonsoft.Json;

namespace BiliLite.Models.Common.Comment
{
    public class CommentPagination
    {
        [JsonProperty("next_offset")]
        public string NextOffset { get; set; }
    }
}