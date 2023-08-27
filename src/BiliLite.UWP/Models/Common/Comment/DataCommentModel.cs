using System.Collections.Generic;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Comment
{
    public class DataCommentModel
    {
        public int Code { get; set; }
        public string Message { get; set; }

        public DataCommentModel Data { get; set; }

        public DataCommentModel Page { get; set; }
        public int Acount { get; set; }
        public int Count { get; set; }
        public int Num { get; set; }
        public int Size { get; set; }

        public CommentCursor Cursor { get; set; }

        public List<CommentItem> Hots { get; set; }
        public List<CommentItem> Replies { get; set; }

        public DataCommentModel Upper { get; set; }

        public CommentItem Top { get; set; }

        [JsonProperty("top_replies")]
        public List<CommentItem> TopReplies { get; set; }
    }
}