using BiliLite.Extensions;
using BiliLite.Models;
using BiliLite.Models.Common;
using BiliLite.Modules;
using BiliLite.Services;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using BiliLite.Models.Common.Anime;
using BiliLite.ViewModels.Home;
using Microsoft.Extensions.DependencyInjection;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages.Home
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AnimePage : Page
    {
        private AnimeType animeType;
        public AnimePageViewModel m_viewModel { get; set; }
        public AnimePage()
        {
            m_viewModel = App.ServiceProvider.GetService<AnimePageViewModel>();
            this.InitializeComponent();

            MessageCenter.LoginedEvent += MessageCenter_LoginedEvent;
            MessageCenter.LogoutedEvent += MessageCenter_LogoutedEvent;

        }

        private void MessageCenter_LogoutedEvent(object sender, EventArgs e)
        {
            m_viewModel.ShowFollows = false;
        }

        private async void MessageCenter_LoginedEvent(object sender, object e)
        {
            m_viewModel.ShowFollows = true;
            await m_viewModel.GetFollows();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                animeType = (AnimeType)Convert.ToInt32(e.Parameter);
                m_viewModel.SetAnimeType(animeType);
                this.DataContext = m_viewModel;
                await LoadData();
            }

        }
        private async Task LoadData()
        {
            await m_viewModel.GetBangumiHome();
            if (SettingService.Account.Logined)
            {
                m_viewModel.ShowFollows = true;
                await m_viewModel.GetFollows();
            }
        }

        private async void btnLoadMoreFall_Click(object sender, RoutedEventArgs e)
        {
            var element = (sender as HyperlinkButton);
            var data = element.DataContext as AnimeFallViewModel;
            await m_viewModel.GetFallMore(element.DataContext as AnimeFallViewModel);
        }


        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadData();
        }

        private async void gvFall_ItemClick(object sender, ItemClickEventArgs e)
        {
            var result = await MessageCenter.HandelUrl((e.ClickedItem as AnimeFallItemModel).Link);
            if (!result)
            {
                Notify.ShowMessageToast("不支持打开的链接");
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
            var result = await MessageCenter.HandelUrl(((sender as HyperlinkButton).DataContext as AnimeBannerModel).Url);
            if (!result)
            {
                Notify.ShowMessageToast("不支持打开的链接");
            }
        }

        private void btnOpenIndex_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Filter,
                page = typeof(Bangumi.AnimeIndexPage),
                title = animeType == AnimeType.Bangumi ? "番剧索引" : "国创索引",
                parameters = new SeasonIndexParameter()
                {
                    type = animeType == AnimeType.Bangumi ? IndexSeasonType.Anime : IndexSeasonType.Guochuang
                }
            });
        }

        private async void btnOpenMyFollow_Click(object sender, RoutedEventArgs e)
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录");
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
