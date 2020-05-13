using BiliLite.Helpers;
using BiliLite.Modules;
using BiliLite.Pages.Home;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HomePage : Page
    {
        readonly HomeVM homeVM;
        readonly Account account;
        public HomePage()
        {
            this.InitializeComponent();

            MessageCenter.LoginedEvent += MessageCenter_LoginedEvent;
            MessageCenter.LogoutedEvent += MessageCenter_LogoutedEvent;
            homeVM = new HomeVM();
            account = new Account();
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
        protected  override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if(e.NavigationMode== NavigationMode.New&& homeVM.IsLogin&&homeVM.Profile == null)
            {
                 CheckLoginStatus();
                //await homeVM.LoginUserCard();
            }
        }
        private async void CheckLoginStatus()
        {
            if (SettingHelper.Account.Logined)
            {
                if (await account.CheckLoginState())
                {
                    await homeVM.LoginUserCard();
                }
                else
                {
                    var result = await account.RefreshToken();
                    if (!result)
                    {
                        MessageCenter.SendLogout();
                        Utils.ShowMessageToast("登录过期，请重新登录");
                        await Utils.ShowLoginDialog();
                    }
                }
            }
        }


        private async void LaodUserStatus()
        {
            if (SettingHelper.Account.Logined)
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
            MessageCenter.OpenNewWindow(this, new NavigationInfo()
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
        }


        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            var data = await Utils.ShowLoginDialog();
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendLogout();
        }

        private void btnDownlaod_Click(object sender, RoutedEventArgs e)
        {
            Utils.ShowMessageToast("劳资还没写好￣へ￣");
        }

        private async void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (string.IsNullOrEmpty( SearchBox.Text))
            {
                Utils.ShowMessageToast("关键字不能为空");
                return;
            }

            if (await MessageCenter.HandelUrl(SearchBox.Text))
            {
                return;
            }

            MessageCenter.OpenNewWindow(this, new NavigationInfo()
            {
                icon = Symbol.Find,
                page = typeof(SearchPage),
                title = "搜索:"+ SearchBox.Text,
                parameters = new SearchParameter()
                {
                    keyword=SearchBox.Text,
                    searchType= SearchType.Video
                }
            });
        }

        private async void MenuMyFollow_Click(object sender, RoutedEventArgs e)
        {
            if (!SettingHelper.Account.Logined&&!await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录");
                return;
            }
            MessageCenter.OpenNewWindow(this, new NavigationInfo()
            {
                icon = Symbol.OutlineStar,
                page = typeof(User.FavoritePage),
                title = "我的收藏",
                parameters = User.OpenFavoriteType.Video
            });
        }

        private void MenuMyLive_Click(object sender, RoutedEventArgs e)
        {
            Utils.ShowMessageToast("劳资还没写好￣へ￣");
        }

        private void btnOpenFans_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.OpenNewWindow(this, new NavigationInfo()
            {
                icon = Symbol.World,
                page = typeof(WebPage),
                title = "我的好友",
                parameters = "https://space.bilibili.com/h5/follow"
            });
        }

        private void btnFeedback_Click(object sender, RoutedEventArgs e)
        {
            //MessageCenter.OpenNewWindow(this, new NavigationInfo()
            //{
            //    icon = Symbol.Emoji2,
            //    page = typeof(WebPage),
            //    title = "开发版",
            //    parameters = "https://www.showdoc.cc/biliuwpdev"
            //});
            MessageCenter.OpenNewWindow(this, new NavigationInfo()
            {
                icon = Symbol.Help,
                page = typeof(HelpPage),
                title = "帮助",
            });
        }

        private void MenuHistory_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.OpenNewWindow(this, new NavigationInfo()
            {
                icon = Symbol.Clock,
                page = typeof(User.HistoryPage),
                title = "历史记录"
            });
        }
    }


}
