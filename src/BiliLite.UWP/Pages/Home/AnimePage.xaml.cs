using BiliLite.Helpers;
using BiliLite.Models;
using BiliLite.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

namespace BiliLite.Pages.Home
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AnimePage : Page
    {
        private AnimeType animeType ;
        public Modules.AnimeVM homeBangumi { get; set; }
        public AnimePage()
        {
            this.InitializeComponent();
          
            MessageCenter.LoginedEvent += MessageCenter_LoginedEvent;
            MessageCenter.LogoutedEvent += MessageCenter_LogoutedEvent;
            
        }

        private void MessageCenter_LogoutedEvent(object sender, EventArgs e)
        {
            homeBangumi.ShowFollows = false;
        }

        private async void MessageCenter_LoginedEvent(object sender, object e)
        {
            homeBangumi.ShowFollows = true;
            await homeBangumi.GetFollows();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                animeType = (AnimeType)Convert.ToInt32( e.Parameter);
                homeBangumi = new Modules.AnimeVM(animeType);
                this.DataContext = homeBangumi;
                await LoadData();
            }

        }
        private async Task LoadData()
        {
            await homeBangumi.GetBangumiHome();
            if (SettingHelper.Account.Logined)
            {
                homeBangumi.ShowFollows = true;
                await homeBangumi.GetFollows();
            }
        }

        private async void btnLoadMoreFall_Click(object sender, RoutedEventArgs e)
        {
            var element = (sender as HyperlinkButton);
            var data = element.DataContext as AnimeFallModel;
            await homeBangumi.GetFallMore(element.DataContext as AnimeFallModel);
        }
       

        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
           await  LoadData();
        }

        private async void gvFall_ItemClick(object sender, ItemClickEventArgs e)
        {
            var result = await MessageCenter.HandelUrl((e.ClickedItem as AnimeFallItemModel).link);
            if (!result)
            {
                Utils.ShowMessageToast("不支持打开的链接");
            }

        }

        private void btnTimeline_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Clock,
                page = typeof(Bangumi.TimelinePage),
                title = "番剧时间表",
                parameters = animeType
            });
        }

        private async void RefreshContainer_RefreshRequested(Microsoft.UI.Xaml.Controls.RefreshContainer sender, Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs args)
        {
           await LoadData();
        }

        private async void BannerItem_Click(object sender, RoutedEventArgs e)
        {
            var result = await MessageCenter.HandelUrl(((sender as HyperlinkButton).DataContext as AnimeBannerModel).url);
            if (!result)
            {
                Utils.ShowMessageToast("不支持打开的链接");
            }
        }

        private void btnOpenIndex_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Filter,
                page = typeof(Bangumi.AnimeIndexPage),
                title = animeType == AnimeType.bangumi?"番剧索引":"国创索引",
                parameters=new SeasonIndexParameter()
                {
                    type = animeType == AnimeType.bangumi ? IndexSeasonType.Anime: IndexSeasonType.Guochuang
                }
            });
        }

        private async void btnOpenMyFollow_Click(object sender, RoutedEventArgs e)
        {
            if (!SettingHelper.Account.Logined && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录");
                return;
            }
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.OutlineStar,
                page = typeof(User.FavoritePage),
                title = "我的收藏",
                parameters = User.OpenFavoriteType.Bangumi
            });
        }
        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as PageEntranceModel;
            MessageCenter.NavigateToPage(this, item.NavigationInfo);
        }
    }
}
