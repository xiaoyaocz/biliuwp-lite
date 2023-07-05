using Newtonsoft.Json;

namespace BiliLite.Models.Common.Comment
{
    public class CommentReplyControlModel
    {
        [JsonProperty("time_desc")]
        public string TimeDesc { get; set; }

        public string Location { get; set; }
    }
}