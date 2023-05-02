using BiliLite.Models.Requests.Api;
using BiliLite.Modules;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BiliLite.Extensions;
using static BiliLite.Models.Requests.Api.CommentApi;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BiliLite.Dialogs
{
    public sealed partial class SendCommentDialog : ContentDialog
    {
        readonly CommentApi commentApi;
        readonly EmoteVM emoteVM;
        readonly string oid;
        readonly CommentType commentType;
        public SendCommentDialog(string oid, CommentType commentType)
        {
            this.InitializeComponent();
            commentApi = new CommentApi();
            emoteVM = new EmoteVM();
            this.oid = oid;
            this.commentType = commentType;
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (string.IsNullOrEmpty(txt_Comment.Text.Trim()))
            {
                Notify.ShowMessageToast("检查下你的输入哦");
                return;
            }
            try
            {
                IsPrimaryButtonEnabled = false;
                var text = txt_Comment.Text;
                var result = await commentApi.AddComment(oid, commentType, text).Request();
                var data = await result.GetData<object>();
                if (data.code == 0)
                {
                    Notify.ShowMessageToast("发表评论成功");
                    this.Hide();

                }
                else
                {
                    Notify.ShowMessageToast(data.message.ToString());
                }

            }
            catch (Exception)
            {
                IsPrimaryButtonEnabled = true;
                Notify.ShowMessageToast("发送评论失败");
                // throw;
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Hide();
        }

        private async void btnOpenFace_Click(object sender, RoutedEventArgs e)
        {
            FaceFlyout.ShowAt(sender as Button);
            if (emoteVM.Packages == null || emoteVM.Packages.Count == 0)
            {
                await emoteVM.GetEmote(EmoteBusiness.reply);
            }

        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            txt_Comment.Text += (e.ClickedItem as EmotePackageItemModel).text.ToString();
        }
    }
}
