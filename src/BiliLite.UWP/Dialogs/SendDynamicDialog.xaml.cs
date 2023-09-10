using BiliLite.Controls.Dynamic;
using BiliLite.Extensions;
using BiliLite.Models.Requests.Api;
using BiliLite.Modules;
using BiliLite.Modules.User;
using BiliLite.Modules.User.SendDynamic;
using System;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BiliLite.Dialogs
{
    public sealed partial class SendDynamicDialog : ContentDialog
    {
        readonly EmoteVM emoteVM;
        readonly AtVM atVM;
        readonly SendDynamicVM sendDynamicVM;
        readonly TopicVM topicVM;
        public SendDynamicDialog()
        {
            this.InitializeComponent();
            emoteVM = new EmoteVM();
            atVM = new AtVM();
            sendDynamicVM = new SendDynamicVM();
            topicVM = new TopicVM();
        }
        public SendDynamicDialog(UserDynamicItemDisplayViewModel userDynamicItem)
        {
            this.InitializeComponent();
            emoteVM = new EmoteVM();
            atVM = new AtVM();
            topicVM = new TopicVM();
            sendDynamicVM = new SendDynamicVM(userDynamicItem);
        }

        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private async void btnEmoji_Click(object sender, RoutedEventArgs e)
        {
            FaceFlyout.ShowAt(sender as Button);
            if (emoteVM.Packages == null || emoteVM.Packages.Count == 0)
            {
                await emoteVM.GetEmote(EmoteBusiness.dynamic);
            }
        }

        private void gvEmoji_ItemClick(object sender, ItemClickEventArgs e)
        {
            txtContent.Text += (e.ClickedItem as EmotePackageItemModel).text.ToString();
        }

        private void txtContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtLength.Text = (233 - txtContent.Text.Length).ToString();
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            atVM.Search(sender.Text);
        }

        private void listAt_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as AtUserModel;
            var location = txtContent.Text.Length;
            var at = "[@" + data.UserName + "]";
            txtContent.Text += at;

            sendDynamicVM.AtDisplaylist.Add(new AtDisplayModel()
            {
                data = data.ID,
                text = at,
                location = location,
                length = at.Length
            });
        }

        private async void btnAt_Click(object sender, RoutedEventArgs e)
        {
            AtFlyout.ShowAt(sender as Button);
            if (atVM.Users.Count == 0 && string.IsNullOrEmpty(atVM.Keyword))
            {
                await atVM.GetUser();
            }
        }

        private async void btnTopic_Click(object sender, RoutedEventArgs e)
        {
            TopicFlyout.ShowAt(sender as Button);
            if (topicVM.Items == null || topicVM.Items.Count == 0)
            {
                await topicVM.GetTopic();
            }
        }

        private void listTopic_ItemClick(object sender, ItemClickEventArgs e)
        {
            txtContent.Text += (e.ClickedItem as RcmdTopicModel).display;
            TopicFlyout.Hide();
        }

        private void TextTopic_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            txtContent.Text += args.QueryText;
            TopicFlyout.Hide();
        }

        private void btn_RemovePic_Click(object sender, RoutedEventArgs e)
        {
            sendDynamicVM.Images.Remove((sender as Button).DataContext as UploadImagesModel);
            sendDynamicVM.ShowImage = gv_Pics.Items.Count > 0;
        }

        private async void btnImage_Click(object sender, RoutedEventArgs e)
        {
            if (sendDynamicVM.Images.Count == 9)
            {
                Notify.ShowMessageToast("只能上传9张图片哦");
                return;
            }
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".bmp");
            picker.FileTypeFilter.Add(".webp");
            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                sendDynamicVM.UploadImage(file);
            }
        }

        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            sendDynamicVM.Content = txtContent.Text;
            bool result = false;
            btnSend.IsEnabled = false;
            if (sendDynamicVM.IsRepost)
            {
                result = await sendDynamicVM.SendRepost();
            }
            else
            {
                result = await sendDynamicVM.SendDynamic();
            }
            if (result)
            {
                this.Hide();
            }
            else
            {
                btnSend.IsEnabled = true;
            }

        }
    }
}
