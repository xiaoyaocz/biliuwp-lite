using BiliLite.Api.User;
using BiliLite.Helpers;
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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BiliLite.Dialogs
{
    public sealed partial class CreateFavFolderDialog : ContentDialog
    {
        readonly FavoriteApi favoriteApi;
        public CreateFavFolderDialog()
        {
            this.InitializeComponent();
            favoriteApi = new FavoriteApi();
        }
        public bool Success { get; set; } = false;
        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (string.IsNullOrEmpty(txtTitle.Text))
            {
                Utils.ShowMessageToast("请输入收藏夹名称");
                return;
            }
            try
            {
                IsPrimaryButtonEnabled = false;
                var result = await favoriteApi.CreateFavorite(txtTitle.Text,txtDesc.Text,checkPrivacy.IsChecked.Value).Request();
                if (result.status)
                {
                    var data = await result.GetData<object>();
                    if (data.success)
                    {
                        Utils.ShowMessageToast("创建成功");
                        Success = true;
                        this.Hide();
                    }
                    else
                    {
                        Utils.ShowMessageToast(data.message);
                    }
                }
                else
                {
                    Utils.ShowMessageToast(result.message);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowMessageToast(ex.Message);
            }
            finally
            {
                IsPrimaryButtonEnabled = true;
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Hide();
        }

    }
}
