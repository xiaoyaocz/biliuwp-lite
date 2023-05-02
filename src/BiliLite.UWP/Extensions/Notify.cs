using BiliLite.Controls;
using BiliLite.Dialogs;
using BiliLite.Models.Requests.Api;
using BiliLite.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace BiliLite.Extensions
{
    public static class Notify
    {
        private static bool dialogShowing = false;

        public static void ShowMessageToast(string message, int seconds = 2)
        {
            MessageToast ms = new MessageToast(message, TimeSpan.FromSeconds(seconds));
            ms.Show();
        }

        public static void ShowMessageToast(string message, List<MyUICommand> commands, int seconds = 15)
        {
            MessageToast ms = new MessageToast(message, TimeSpan.FromSeconds(seconds), commands);
            ms.Show();
        }

        public static void ShowComment(string oid, int commentMode, CommentApi.CommentSort commentSort)
        {
            CommentDialog ms = new CommentDialog();
            ms.Show(oid, commentMode, commentSort);
        }

        public static async Task<bool> ShowLoginDialog()
        {
            if (!dialogShowing)
            {
                LoginDialog login = new LoginDialog();
                dialogShowing = true;
                await login.ShowAsync();
                dialogShowing = false;
            }
            if (SettingService.Account.Logined)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static async Task<bool> ShowDialog(string title, string content)
        {
            MessageDialog messageDialog = new MessageDialog(content, title);
            messageDialog.Commands.Add(new UICommand() { Label = "确定", Id = true });
            messageDialog.Commands.Add(new UICommand() { Label = "取消", Id = false });
            var result = await messageDialog.ShowAsync();
            return (bool)result.Id;
        }
    }
}
