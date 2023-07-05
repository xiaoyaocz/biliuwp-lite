using BiliLite.Controls;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Comment
{
    public class CommentCursor
    {
        [JsonProperty("pagination_reply")]
        public CommentPagination PaginationReply { get; set; }
    }
}