using BiliLite.Models.Requests.Api;

namespace BiliLite.Models.Common.Comment
{
    public class LoadCommentInfo
    {
        public int CommentMode { get; set; }
        public CommentApi.CommentSort CommentSort { get; set; }
        public string Oid { get; set; }
        public bool IsDialog { get; set; } = false;
    }
}