using BiliLite.Dialogs;
using BiliLite.Helpers;
using BiliLite.Modules;
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
    public class FavoriteDetailArgs
    {
        public int Type { get; set; } = 11;
        public string Id { get; set; }
    }
    /// <summary>
    /// 收藏夹详情、播放列表详情
    /// </summary>
    public sealed partial class FavoriteDetailPage : BasePage
    {
        FavoriteDetailVM favoriteDetailVM;
        public FavoriteDetailPage()
        {
            this.InitializeComponent();
            Title = "收藏夹详情";
            favoriteDetailVM = new FavoriteDetailVM();
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New && favoriteDetailVM.FavoriteInfo == null)
            {
                FavoriteDetailArgs args = e.Parameter as FavoriteDetailArgs;
                favoriteDetailVM.Id = args.Id;
                favoriteDetailVM.Type = args.Type;
                favoriteDetailVM.Page = 1;
                favoriteDetailVM.Keyword = "";
                await favoriteDetailVM.LoadFavoriteInfo();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private void Video_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as FavoriteInfoVideoItemModel;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Play,
                page = typeof(VideoDetailPage),
                title = data.title,
                parameters = data.id
            });
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            favoriteDetailVM.Search(searchBox.Text);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            listView.SelectAll();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            listView.SelectedItems.Clear();
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (listView.SelectedItems.Count > 0)
            {
                if (!await Utils.ShowDialog("批量取消收藏", $"是否确定要取消收藏选中的{listView.SelectedItems.Count}个视频?"))
                {
                    return;
                }
                List<FavoriteInfoVideoItemModel> ls = new List<FavoriteInfoVideoItemModel>();
                foreach (FavoriteInfoVideoItemModel item in listView.SelectedItems)
                {
                    ls.Add(item);
                }
                await favoriteDetailVM.Delete(ls);
            }
        }

        private async void btnMove_Click(object sender, RoutedEventArgs e)
        {
            if (listView.SelectedItems.Count > 0)
            {
                List<FavoriteInfoVideoItemModel> ls = new List<FavoriteInfoVideoItemModel>();
                foreach (FavoriteInfoVideoItemModel item in listView.SelectedItems)
                {
                    ls.Add(item);
                }
                CopyOrMoveFavVideoDialog copyOrMoveFavVideoDialog = new CopyOrMoveFavVideoDialog(favoriteDetailVM.Id, favoriteDetailVM.FavoriteInfo.mid, true, ls);
                await copyOrMoveFavVideoDialog.ShowAsync();
                favoriteDetailVM.Refresh();
            }
        }

        private async void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (listView.SelectedItems.Count > 0)
            {
                List<FavoriteInfoVideoItemModel> ls = new List<FavoriteInfoVideoItemModel>();
                foreach (FavoriteInfoVideoItemModel item in listView.SelectedItems)
                {
                    ls.Add(item);
                }
                CopyOrMoveFavVideoDialog copyOrMoveFavVideoDialog = new CopyOrMoveFavVideoDialog(favoriteDetailVM.Id, favoriteDetailVM.FavoriteInfo.mid, false, ls);
                await copyOrMoveFavVideoDialog.ShowAsync();
            }
        }

        private async void btnClean_Click(object sender, RoutedEventArgs e)
        {
            if (!await Utils.ShowDialog("清除失效", $"是否确定要清除已失效的视频?\r\n失效视频说不定哪天就恢复了哦~"))
            {
                return;
            }

            await favoriteDetailVM.Clean();
        }

        private void AddToWatchLater_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as MenuFlyoutItem).DataContext as FavoriteInfoVideoItemModel;
            Modules.User.WatchLaterVM.Instance.AddToWatchlater(data.id);
        }

        private async void PlayAll_Click(object sender, RoutedEventArgs e)
        {

            if (favoriteDetailVM.ShowLoadMore)
            {
                Utils.ShowMessageToast("正在读取全部视频，请稍后");
                while (favoriteDetailVM.ShowLoadMore)
                {
                    await favoriteDetailVM.LoadFavoriteInfo();
                }
            }
            List<VideoPlaylistItem> items = new List<VideoPlaylistItem>();
            foreach (var item in favoriteDetailVM.Videos)
            {
                if (item.title != "已失效视频")
                {
                    items.Add(new VideoPlaylistItem()
                    {
                        Cover = item.cover,
                        Author = item.upper.name,
                        ID = item.id,
                        Title = item.title
                    });
                }

            }
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Play,
                page = typeof(VideoDetailPage),
                title = "视频播放",
                parameters = new VideoPlaylist()
                {
                    Index = 0,
                    Playlist = items
                }
            });
        }
    }
}
