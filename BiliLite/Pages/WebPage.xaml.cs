using BiliLite.Controls;
using BiliLite.Helpers;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class WebPage : Page
    {
        public WebPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Disabled;
            this.Loaded += WebPage_Loaded;
        }
        private void WebPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Parent is MyFrame)
            {
                (this.Parent as MyFrame).ClosedPage -= WebPage_ClosedPage;
                (this.Parent as MyFrame).ClosedPage += WebPage_ClosedPage;
            }
        }

        private void WebPage_ClosedPage(object sender, EventArgs e)
        {
            webView.NavigateToString("");
            webView = null;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if(e.NavigationMode== NavigationMode.New)
            {
                var uri = e.Parameter.ToString();
                if (uri.Contains("h5/vlog"))
                {
                    webView.MaxWidth = 500;
                }
                webView.Navigate(new Uri(uri));
                
            }
           
        }
       

     
        private void btnForword_Click(object sender, RoutedEventArgs e)
        {
           if (webView.CanGoForward)
            {
                webView.GoForward();
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            webView.Refresh();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (webView.CanGoBack)
            {
                webView.GoBack();
            }
        }

        private async void webView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if(this.Parent != null)
            {
                if ((this.Parent as Frame).Parent is TabViewItem)
                {
                    if (!string.IsNullOrEmpty(webView.DocumentTitle))
                    {
                        ((this.Parent as Frame).Parent as TabViewItem).Header = webView.DocumentTitle;
                    }
                }
                else
                {
                    MessageCenter.ChangeTitle(webView.DocumentTitle);
                }
            }
            try
            {
               await webView?.InvokeScriptAsync("eval", new List<string>() {
                    "$('.h5-download-bar').hide()"
                });
            }
            catch (Exception)
            {
                
            }
        }

        private void btnShare_Click(object sender, RoutedEventArgs e)
        {
            Utils.SetClipboard(webView.Source.ToString());
        }

        private async void webView_NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            args.Handled = true;
            var re = await MessageCenter.HandelUrl(args.Uri.AbsoluteUri);
            if (!re)
            {
                var md = new MessageDialog("是否使用外部浏览器打开此链接？");
                md.Commands.Add(new UICommand("确定", new UICommandInvokedHandler(async (e) => { await Windows.System.Launcher.LaunchUriAsync(args.Uri); })));
                md.Commands.Add(new UICommand("取消", new UICommandInvokedHandler((e) => { })));
                await md.ShowAsync();
            }
        }

        private async void btnOpenBrowser_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(webView.Source);
        }

        private void webView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if (args.Uri!=null&&args.Uri.AbsoluteUri.Contains("read/cv"))
            {
                args.Cancel = true;
                return;
            }
        }

        private async void webView_UnsupportedUriSchemeIdentified(WebView sender, WebViewUnsupportedUriSchemeIdentifiedEventArgs args)
        {
            if (args.Uri.AbsoluteUri.Contains("article"))
            {
                args.Handled = true;
                return;
            }
            if (args.Uri.AbsoluteUri.Contains("bilibili://"))
            {
                args.Handled = true;
                var re = await MessageCenter.HandelUrl(args.Uri.AbsoluteUri);
                if (!re)
                {
                    Utils.ShowMessageToast("不支持打开的链接" + args.Uri.AbsoluteUri);
                }
            }

        }

        private void btnInfo_Click(object sender, RoutedEventArgs e)
        {
            Utils.ShowMessageToast("虽然看起来像个浏览器，但这完全这不是个浏览器啊！ ╰（‵□′）╯" );
        }
    }
}
