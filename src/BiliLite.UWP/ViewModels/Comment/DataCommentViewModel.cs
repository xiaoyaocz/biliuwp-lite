using System.Collections.ObjectModel;
using BiliLite.Models.Common.Comment;

namespace BiliLite.ViewModels.Comment
{
    public class DataCommentViewModel
    {
        public int Code { get; set; }
        public string Message { get; set; }

        public DataCommentViewModel Data { get; set; }

        public DataCommentViewModel Page { get; set; }
        public int Acount { get; set; }
        public int Count { get; set; }
        public int Num { get; set; }
        public int Size { get; set; }

        public CommentCursor Cursor { get; set; }

        public ObservableCollection<CommentViewModel> Hots { get; set; }
        public ObservableCollection<CommentViewModel> Replies { get; set; }

        public DataCommentViewModel Upper { get; set; }
        public CommentViewModel Top { get; set; }
    }
}
