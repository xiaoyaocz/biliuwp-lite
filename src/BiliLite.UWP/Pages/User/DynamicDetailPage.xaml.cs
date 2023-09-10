using BiliLite.Extensions;
using BiliLite.Models.Requests.Api;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using BiliLite.Models.Common;
using BiliLite.ViewModels.UserDynamic;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages.User
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DynamicDetailPage : BasePage
    {
        readonly UserDynamicViewModel m_userDynamicViewModel;
        public DynamicDetailPage()
        {
            this.InitializeComponent();
            Title = "动态详情";
            m_userDynamicViewModel = new UserDynamicViewModel();
            m_userDynamicViewModel.OpenCommentEvent += UserDynamicViewModelOpenCommentEvent;
            splitView.PaneClosed += SplitView_PaneClosed;
        }
        private void SplitView_PaneClosed(SplitView sender, object args)
        {
            comment.ClearComment();
            repost.UserDynamicRepostViewModel.Clear();
        }
        string dynamic_id;
        private void UserDynamicViewModelOpenCommentEvent(object sender, Controls.Dynamic.UserDynamicItemDisplayViewModel e)
        {
            //splitView.IsPaneOpen = true;
            dynamic_id = e.DynamicID;
            pivotRight.SelectedIndex = 1;
            repostCount.Text = e.ShareCount.ToString();
            commentCount.Text = e.CommentCount.ToString();
            CommentApi.CommentType commentType = CommentApi.CommentType.Dynamic;
            var id = e.ReplyID;
            switch (e.Type)
            {

                case UserDynamicDisplayType.Photo:
                    commentType = CommentApi.CommentType.Photo;
                    break;
                case UserDynamicDisplayType.Video:

                    commentType = CommentApi.CommentType.Video;
                    break;
                case UserDynamicDisplayType.Season:
                    id = e.OneRowInfo.AID;
                    commentType = CommentApi.CommentType.Video;
                    break;
                case UserDynamicDisplayType.ShortVideo:
                    commentType = CommentApi.CommentType.MiniVideo;
                    break;
                case UserDynamicDisplayType.Music:
                    commentType = CommentApi.CommentType.Song;
                    break;
                case UserDynamicDisplayType.Article:
                    commentType = CommentApi.CommentType.Article;
                    break;
                case UserDynamicDisplayType.MediaList:
                    if (e.OneRowInfo.Tag != "收藏夹")
                        commentType = CommentApi.CommentType.Video;
                    break;
                default:
                    id = e.DynamicID;
                    break;
            }
            Notify.ShowComment(id, (int)commentType, CommentApi.CommentSort.Hot);
            //comment.LoadComment(new Controls.LoadCommentInfo()
            //{
            //    CommentMode = (int)commentType,
            //    CommentSort = Api.CommentApi.commentSort.Hot,
            //    Oid = id
            //});
        }


        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New && m_userDynamicViewModel.Items == null)
            {

                await m_userDynamicViewModel.GetDynamicDetail(e.Parameter.ToString());
            }
        }

        private void pivotRight_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivotRight.SelectedIndex == 0 && splitView.IsPaneOpen && (repost.UserDynamicRepostViewModel.Items == null || repost.UserDynamicRepostViewModel.Items.Count == 0))
            {
                repost.LoadData(dynamic_id);
            }
        }
    }
}
