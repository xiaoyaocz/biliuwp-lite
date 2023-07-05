using System.Collections.Generic;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Comment
{
    public class CommentItem
    {
        public int Action { get; set; }

        [JsonProperty("rpid")]
        public long RpId { get; set; }

        public long Oid { get; set; }

        public int Type { get; set; }

        public long Mid { get; set; }

        public long Root { get; set; }

        public long Parent { get; set; }

        public int Count { get; set; }

        public int Rcount { get; set; }

        public int Like { get; set; }

        public int Floor { get; set; }

        public int State { get; set; }

        public long Ctime { get; set; }

        [JsonProperty("rpid_str")]
        public string RpidStr { get; set; }

        public CommentMemberModel Member { get; set; }

        public CommentContentModel Content { get; set; }

        [JsonProperty("up_action")]
        public CommentUPActionModel UpAction { get; set; }

        [JsonProperty("reply_control")]
        public CommentReplyControlModel ReplyControl { get; set; }

        public List<CommentItem> Replies { get; set; }
    }
}
