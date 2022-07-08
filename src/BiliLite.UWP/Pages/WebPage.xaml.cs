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
    public sealed partial class WebPage : BasePage
    {
        public WebPage()
        {
            this.InitializeComponent();
            Title = "网页浏览";
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
            (this.Content as Grid).Children.Remove(webView);
            webView = null;
            GC.Collect();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                var uri = e.Parameter.ToString();
                if (uri.Contains("h5/vlog"))
                {
                    webView.MaxWidth = 500;
                }

                if (uri.Contains("read/cv"))
                {
                    //如果是专栏，内容加载完成再显示
                    webView.Visibility = Visibility.Collapsed;
                }
                webView.Navigate(new Uri(uri));

            }

        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back || e.SourcePageType == typeof(BlankPage))
            {
                NavigationCacheMode = NavigationCacheMode.Disabled;
                webView.NavigateToString("");
                (this.Content as Grid).Children.Remove(webView);
                webView = null;
                GC.Collect();
            }
            base.OnNavigatingFrom(e);
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
            if (this.Parent != null)
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

                //专栏阅读设置
                if (args.Uri != null && args.Uri.AbsoluteUri.Contains("read/cv"))
                {
                    await webView?.InvokeScriptAsync("eval", new List<string>() {
                    @"$('#internationalHeader').hide();
$('.unlogin-popover').hide();
$('.up-info-holder').hide();
$('.nav-tab-bar').hide();
$('.international-footer').hide();
$('.page-container').css('padding-right','0');
$('.no-login').hide();
$('.author-container').show();
$('.author-container').css('margin','12px 0px -12px 0px');"
                });
                    //将专栏图片替换成jpg
                    await webView?.InvokeScriptAsync("eval", new List<string>() {
                        @"document.getElementsByClassName('img-box').forEach(element => {
                element.getElementsByTagName('img').forEach(image => {
                    image.src=image.getAttribute('data-src')+'@progressive.jpg';
               });
            });"
                    });
                }



                await webView?.InvokeScriptAsync("eval", new List<string>() {
                    "$('.h5-download-bar').hide()"
                });



            }
            catch (Exception)
            {

            }
            finally
            {
                if (webView != null) webView.Visibility = Visibility.Visible;
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
            if (args.Uri != null && args.Uri.AbsoluteUri.Contains("read/cv"))
            {
                // args.Cancel = true;
                // return;
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
            Utils.ShowMessageToast("虽然看起来像个浏览器，但这完全这不是个浏览器啊！ ╰（‵□′）╯");
        }
    }
}
