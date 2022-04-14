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
    public enum OpenFavoriteType
    {
        Video=0,
        Bangumi=1,
        Cinema = 2,
        Music=3
    }
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FavoritePage : BasePage
    {
        MyFollowSeasonVM animeVM;
        MyFollowSeasonVM cinemaVM;
        MyFollowVideoVM videoVM;
        public FavoritePage()
        {
            this.InitializeComponent();
            Title = "我的收藏";
            animeVM = new MyFollowSeasonVM(true);
            cinemaVM = new MyFollowSeasonVM(false);
            videoVM = new MyFollowVideoVM();
        }
        OpenFavoriteType openFavoriteType= OpenFavoriteType.Video;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if(e.NavigationMode== NavigationMode.New)
            {
                if (e.Parameter != null)
                {
                    openFavoriteType = (OpenFavoriteType)e.Parameter; 
                }
                if (openFavoriteType== OpenFavoriteType.Bangumi)
                {
                    pivot.SelectedIndex = 1;
                }
                if (openFavoriteType == OpenFavoriteType.Cinema)
                {
                    pivot.SelectedIndex = 2;
                }
            }
        }

        private async void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot.SelectedItem==null)
            {
                return;
            }
            switch (pivot.SelectedIndex)
            {
                case 0:
                    if (videoVM.Loading || videoVM.MyFavorite != null)
                    {
                        return;
                    }
                    await videoVM.LoadFavorite();
                    break;
                case 1:
                    if (animeVM.Loading||animeVM.Follows!=null)
                    {
                        return;
                    }
                    await animeVM.LoadFollows();
                    break;
                case 2:
                    if (cinemaVM.Loading || cinemaVM.Follows != null)
                    {
                        return;
                    }
                    await cinemaVM.LoadFollows();
                    break;
                default:
                    break;
            }
        }

        private void BangumiSeason_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data= e.ClickedItem as FollowSeasonModel;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Play,
                page = typeof(SeasonDetailPage),
                title = data.title,
                parameters = data.season_id
            });
        }

        private void VideoFavorite_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as FavoriteItemModel;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.OutlineStar,
                page = typeof(FavoriteDetailPage),
                title = data.title,
                parameters = new FavoriteDetailArgs()
                {
                    Id = data.id,
                    Type=data.type
                } 
            });
        }

        private async void btnCreateFavBox_Click(object sender, RoutedEventArgs e)
        {
            CreateFavFolderDialog createFavFolderDialog = new CreateFavFolderDialog();
            await createFavFolderDialog.ShowAsync();
            videoVM.Refresh();
        }

        private async void btnFavBoxEdit_Click(object sender, RoutedEventArgs e)
        {
           var data= (sender as MenuFlyoutItem).DataContext as FavoriteItemModel;
            EditFavFolderDialog editFavFolderDialog = new EditFavFolderDialog(data.id, data.title,data.intro,data.privacy?false:true);
            await editFavFolderDialog.ShowAsync();
            videoVM.Refresh();
        }

        private async void btnFavBoxDel_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as MenuFlyoutItem).DataContext as FavoriteItemModel;
            await videoVM.DelFavorite(data.id);
            videoVM.Refresh();
        }
    }
}
