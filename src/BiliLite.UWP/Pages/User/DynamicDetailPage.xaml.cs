using BiliLite.Helpers;
using BiliLite.Modules.User;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages.User
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DynamicDetailPage : BasePage
    {
        readonly DynamicVM dynamicVM;
        public DynamicDetailPage()
        {
            this.InitializeComponent();
            Title = "动态详情";
            dynamicVM = new DynamicVM();
            dynamicVM.OpenCommentEvent += DynamicVM_OpenCommentEvent;
            splitView.PaneClosed += SplitView_PaneClosed;
        }
        private void SplitView_PaneClosed(SplitView sender, object args)
        {
            comment.ClearComment();
            repost.dynamicRepostVM.Clear();
        }
        string dynamic_id;
        private void DynamicVM_OpenCommentEvent(object sender, Controls.Dynamic.DynamicItemDisplayModel e)
        {
            //splitView.IsPaneOpen = true;
            dynamic_id = e.DynamicID;
            pivotRight.SelectedIndex = 1;
            repostCount.Text = e.ShareCount.ToString();
            commentCount.Text = e.CommentCount.ToString();
            Api.CommentApi.CommentType commentType = Api.CommentApi.CommentType.Dynamic;
            var id = e.ReplyID;
            switch (e.Type)
            {

                case Controls.Dynamic.DynamicDisplayType.Photo:
                    commentType = Api.CommentApi.CommentType.Photo;
                    break;
                case Controls.Dynamic.DynamicDisplayType.Video:

                    commentType = Api.CommentApi.CommentType.Video;
                    break;
                case Controls.Dynamic.DynamicDisplayType.Season:
                    id = e.OneRowInfo.AID;
                    commentType = Api.CommentApi.CommentType.Video;
                    break;
                case Controls.Dynamic.DynamicDisplayType.ShortVideo:
                    commentType = Api.CommentApi.CommentType.MiniVideo;
                    break;
                case Controls.Dynamic.DynamicDisplayType.Music:
                    commentType = Api.CommentApi.CommentType.Song;
                    break;
                case Controls.Dynamic.DynamicDisplayType.Article:
                    commentType = Api.CommentApi.CommentType.Article;
                    break;
                case Controls.Dynamic.DynamicDisplayType.MediaList:
                    if(e.OneRowInfo.Tag!= "收藏夹")
                    commentType = Api.CommentApi.CommentType.Video;
                    break;
                default:
                    id = e.DynamicID;
                    break;
            }
            Utils.ShowComment(id, (int)commentType, Api.CommentApi.CommentSort.Hot);
            //comment.LoadComment(new Controls.LoadCommentInfo()
            //{
            //    CommentMode = (int)commentType,
            //    CommentSort = Api.CommentApi.commentSort.Hot,
            //    Oid = id
            //});
        }


        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New && dynamicVM.Items == null)
            {

                await dynamicVM.GetDynamicDetail(e.Parameter.ToString());
            }
        }

        private void pivotRight_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivotRight.SelectedIndex == 0 && splitView.IsPaneOpen && (repost.dynamicRepostVM.Items == null || repost.dynamicRepostVM.Items.Count == 0))
            {
                repost.LoadData(dynamic_id);
            }
        }
    }
}
