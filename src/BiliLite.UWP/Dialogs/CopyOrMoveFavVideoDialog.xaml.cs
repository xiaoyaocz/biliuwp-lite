using BiliLite.Models;
using BiliLite.Models.Requests.Api.User;
using BiliLite.Modules;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BiliLite.Extensions;
using BiliLite.Models.Responses;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BiliLite.Dialogs
{
    public sealed partial class CopyOrMoveFavVideoDialog : ContentDialog
    {
        readonly string fid, mid;
        readonly bool isMove;
        readonly List<FavoriteInfoVideoItemModel> selectItems;
        readonly FavoriteApi favoriteApi;
        public CopyOrMoveFavVideoDialog(string fid, string mid, bool isMove, List<FavoriteInfoVideoItemModel> items)
        {
            this.InitializeComponent();
            favoriteApi = new FavoriteApi();
            this.fid = fid;
            this.mid = mid;
            this.isMove = isMove;
            this.selectItems = items;
            LoadFav();
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

            if (listView.SelectedItem == null) { Notify.ShowMessageToast("请选择收藏夹"); return; }
            try
            {
                IsPrimaryButtonEnabled = false;
                HttpResults results;
                var item = listView.SelectedItem as FavoriteItemModel;
                List<string> ids = new List<string>();
                foreach (var videoItem in selectItems)
                {
                    ids.Add(videoItem.id);
                }
                if (isMove)
                {
                    results = await favoriteApi.Move(fid, item.id, ids).Request();
                }
                else
                {
                    results = await favoriteApi.Copy(fid, item.id, ids, mid).Request();
                }
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<object>>();
                    if (data.success)
                    {
                        Notify.ShowMessageToast("操作完成");
                        this.Hide();
                    }
                    else
                    {
                        Notify.ShowMessageToast(data.message);
                    }
                }
                else
                {
                    Notify.ShowMessageToast(results.message);
                }


            }
            catch (Exception ex)
            {
                Notify.ShowMessageToast(ex.Message);
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

        private async void LoadFav()
        {
            try
            {
                prLoading.Visibility = Visibility.Visible;

                var results = await favoriteApi.MyFavorite().Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JArray>>();
                    if (data.success)
                    {
                        if (data.data[0]["mediaListResponse"] != null)
                        {
                            var list = await data.data[0]["mediaListResponse"]["list"].ToString().DeserializeJson<List<FavoriteItemModel>>();
                            listView.ItemsSource = list.Where(x => x.id != fid).ToList();
                        }
                    }
                    else
                    {
                        Notify.ShowMessageToast(data.message);
                    }
                }
                else
                {
                    Notify.ShowMessageToast(results.message);
                }
            }
            catch (Exception ex)
            {

                Notify.ShowMessageToast(ex.Message);
            }
            finally
            {
                prLoading.Visibility = Visibility.Collapsed;
            }
        }

    }
}
