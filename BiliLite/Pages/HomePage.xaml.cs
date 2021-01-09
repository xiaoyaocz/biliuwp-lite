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
        protected  override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if(e.NavigationMode== NavigationMode.New&& homeVM.IsLogin&&homeVM.Profile == null)
            {
                 CheckLoginStatus();
                //await homeVM.LoginUserCard();
            }
            if(SettingHelper.GetValue<bool>(SettingHelper.UI.HIDE_SPONSOR, false))
            {
                btnSponsor.Visibility = Visibility.Collapsed;
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
            this.UpdateLayout();
        }


        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            var data = await Utils.ShowLoginDialog();
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.SendLogout();
            UserFlyout.Hide();
        }

        private void btnDownlaod_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.OpenNewWindow(this, new NavigationInfo()
            {
                icon = Symbol.Download,
                page = typeof(DownloadPage),
                title = "下载",
               
            });
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

        private async void MenuMyLive_Click(object sender, RoutedEventArgs e)
        {
            if (!SettingHelper.Account.Logined && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录");
                return;
            }
            MessageCenter.OpenNewWindow(this, new NavigationInfo()
            {
                icon = Symbol.Contact,
                page = typeof(Live.LiveCenterPage),
                title = "直播中心",
                
            });
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

        private void MenuHistory_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.OpenNewWindow(this, new NavigationInfo()
            {
                icon = Symbol.Clock,
                page = typeof(User.HistoryPage),
                title = "历史记录"
            });
        }

        private void MenuUserCenter_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.OpenNewWindow(this, new NavigationInfo()
            {
                icon = Symbol.Contact,
                title = SettingHelper.Account.Profile.name,
                page = typeof(UserInfoPage),
                parameters = SettingHelper.Account.UserID
            });
        }

        private void MenuMessage_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.OpenNewWindow(this, new NavigationInfo()
            {
                icon = Symbol.Message,
                title = "消息中心",
                page = typeof(WebPage),
                parameters = $"https://message.bilibili.com/#whisper"
            });
        }

        private async void MenuWatchlater_Click(object sender, RoutedEventArgs e)
        {
            if (!SettingHelper.Account.Logined && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录");
                return;
            }
            MessageCenter.OpenNewWindow(this, new NavigationInfo()
            {
                icon = Symbol.Play,
                page = typeof(User.WatchlaterPage),
                title = "稍后再看",

            });
        }

        private async void btnSponsor_Click(object sender, RoutedEventArgs e)
        {
            var x = new ContentDialog() { 
                Title="赞助作者"
            };
            ScrollViewer scrollViewer = new ScrollViewer();
            StackPanel st = new StackPanel();
           
            st.Children.Add(new TextBlock()
            {
                TextWrapping = TextWrapping.Wrap,
                IsTextSelectionEnabled = true,
                Text = "\r\n如果觉得应用还不错，就请我喝杯咖啡吧!\r\n支付宝：2500655055@qq.com\r\n\r\n如果您不想显示此按钮，请到设置-个性化中设置"
            });
            st.Children.Add(new Image()
            {
                Width=280,
                Source = new BitmapImage(new Uri("ms-appx:///Assets/zfb.jpg"))
            });
            scrollViewer.Content = st;
            x.Content = scrollViewer;
            x.PrimaryButtonText = "知道了";
            x.IsPrimaryButtonEnabled = true;
            await x.ShowAsync();
        }
    }


}
