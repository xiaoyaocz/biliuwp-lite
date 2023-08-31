using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Modules;
using BiliLite.Services;
using Microsoft.Toolkit.Uwp.Connectivity;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HomePage : Page
    {
        private static readonly ILogger logger = GlobalLogger.FromCurrentType();

        DownloadVM downloadVM;
        readonly HomeVM homeVM;
        readonly Account account;

        public HomePage()
        {
            this.InitializeComponent();
            MessageCenter.LoginedEvent += MessageCenter_LoginedEvent;
            MessageCenter.LogoutedEvent += MessageCenter_LogoutedEvent;
            homeVM = new HomeVM();
            account = new Account();
            downloadVM = DownloadVM.Instance;
            this.DataContext = homeVM;
        }
        private void MessageCenter_LogoutedEvent(object sender, EventArgs e)
        {
            LaodUserStatus();
        }

        private void MessageCenter_LoginedEvent(object sender, object e)
        {
            LaodUserStatus();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New && homeVM.IsLogin && homeVM.Profile == null)
            {
                CheckLoginStatus();
                //await homeVM.LoginUserCard();
            }
        }
        private async void CheckLoginStatus()
        {
            if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
            {
                return;
            }

            if (SettingService.Account.Logined)
            {
                try
                {
                    if (await account.CheckLoginState())
                    {
                        await account.CheckUpdateCookies();
                        await homeVM.LoginUserCard();
                    }
                    else
                    {
                        var result = await account.RefreshToken();
                        if (!result)
                        {
                            homeVM.IsLogin = false;
                            MessageCenter.SendLogout();
                            Notify.ShowMessageToast("登录过期，请重新登录");
                            await Notify.ShowLoginDialog();
                        }
                    }
                }
                catch (Exception ex)
                {
                    homeVM.IsLogin = false;
                    logger.Log("读取access_key信息失败", LogType.Info, ex);
                    Notify.ShowMessageToast("读取登录信息失败，请重新登录");
                    //throw;
                }

            }
        }


        private async void LaodUserStatus()
        {
            if (SettingService.Account.Logined)
            {
                homeVM.IsLogin = true;
                await homeVM.LoginUserCard();
                foreach (var item in homeVM.HomeNavItems)
                {
                    if (!item.Show && item.NeedLogin) item.Show = true;
                }
            }
            else
            {
                homeVM.IsLogin = false;
                foreach (var item in homeVM.HomeNavItems)
                {
                    if (item.Show && item.NeedLogin) item.Show = false;
                }
            }
        }

        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Setting,
                page = typeof(SettingPage),
                title = "设置"
            });
        }

        private void navView_SelectionChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            var item = args.SelectedItem as HomeNavItem;
            frame.Navigate(item.Page, item.Parameters);
            this.UpdateLayout();
        }


        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            var data = await Notify.ShowLoginDialog();
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendLogout();
            UserFlyout.Hide();
        }

        private void btnDownlaod_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Download,
                page = typeof(DownloadPage),
                title = "下载",

            });
        }

        private async void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (string.IsNullOrEmpty(SearchBox.Text))
            {
                Notify.ShowMessageToast("关键字不能为空");
                return;
            }

            if (await MessageCenter.HandelUrl(args.QueryText))
            {
                return;
            }

            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Find,
                page = typeof(SearchPage),
                title = "搜索:" + args.QueryText,
                parameters = new SearchParameter()
                {
                    keyword = args.QueryText,
                    searchType = SearchType.Video
                }
            });
        }

        private async void MenuMyFollow_Click(object sender, RoutedEventArgs e)
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
                parameters = User.OpenFavoriteType.Video
            });
        }

        private async void MenuMyLive_Click(object sender, RoutedEventArgs e)
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录");
                return;
            }
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Contact,
                page = typeof(Live.LiveCenterPage),
                title = "直播中心",

            });
        }


        private void MenuHistory_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Clock,
                page = typeof(User.HistoryPage),
                title = "历史记录"
            });
        }

        private void MenuUserCenter_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Contact,
                title = SettingService.Account.Profile.name,
                page = typeof(UserInfoPage),
                parameters = SettingService.Account.UserID
            });
        }

        private void MenuMessage_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Message,
                title = "消息中心",
                page = typeof(WebPage),
                parameters = $"https://message.bilibili.com/#whisper"
            });
        }

        private async void MenuWatchlater_Click(object sender, RoutedEventArgs e)
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录");
                return;
            }
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Play,
                page = typeof(User.WatchlaterPage),
                title = "稍后再看",

            });
        }

        private void btnOpenFans_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Contact,
                title = SettingService.Account.Profile.name,
                page = typeof(UserInfoPage),
                parameters = new UserInfoParameter()
                {
                    Mid = SettingService.Account.UserID.ToString(),
                    Tab = UserTab.Fans
                }
            });
            //MessageCenter.NavigateToPage(this, new NavigationInfo()
            //{
            //    icon = Symbol.World,
            //    page = typeof(WebPage),
            //    title = "我的好友",
            //    parameters = "https://space.bilibili.com/h5/follow"
            //});
        }
        private void btnOpenAttention_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Contact,
                title = SettingService.Account.Profile.name,
                page = typeof(UserInfoPage),
                parameters = new UserInfoParameter()
                {
                    Mid = SettingService.Account.UserID.ToString(),
                    Tab = UserTab.Attention
                }
            });
        }

        private void btnOpenDynamic_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Contact,
                title = SettingService.Account.Profile.name,
                page = typeof(UserInfoPage),
                parameters = new UserInfoParameter()
                {
                    Mid = SettingService.Account.UserID.ToString(),
                    Tab = UserTab.Dynamic
                }
            });
        }

        private async void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput) return;
            var text = sender.Text;
            var suggestSearchContents = await new SearchService().GetSearchSuggestContents(text);
            if (homeVM.SuggestSearchContents == null)
            {
                homeVM.SuggestSearchContents = new System.Collections.ObjectModel.ObservableCollection<string>(suggestSearchContents);
            }
            else
            {
                homeVM.SuggestSearchContents.ReplaceRange(suggestSearchContents);
            }
        }
    }
}
