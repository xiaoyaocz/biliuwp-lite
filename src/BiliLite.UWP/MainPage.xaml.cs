using BiliLite.Pages;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using BiliLite.Controls;
using BiliLite.Models.Common;
using BiliLite.Extensions;
using BiliLite.Services;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace BiliLite
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Windows.UI.Xaml.Controls.Page
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        public MainPage()
        {
            this.InitializeComponent();
            // 处理标题栏
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;

            Window.Current.SetTitleBar(CustomDragRegion);

            //处理页面跳转
            MessageCenter.NavigateToPageEvent += NavigationHelper_NavigateToPageEvent;
            MessageCenter.ChangeTitleEvent += MessageCenter_ChangeTitleEvent;
            MessageCenter.ViewImageEvent += MessageCenter_ViewImageEvent;
            MessageCenter.MiniWindowEvent += MessageCenter_MiniWindowEvent;
            MessageCenter.GoBackEvent += MessageCenter_GoBackEvent;

            App.Current.Suspending += Current_Suspending;
            // Window.Current.Content.PointerPressed += Content_PointerPressed;
        }

        private async void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            _logger.Trace("应用挂起");
            var tabs = tabView.TabItems;
            foreach (var tab in tabs)
            {
                if(!(tab is TabViewItem tabItem))continue;
                if(!(tabItem.Content is MyFrame frame)) continue;
                var page = frame.Content;
                if(!(page is PlayPage playPage)) continue;
                await playPage.ReportHistory();
            }
        }

        private void MessageCenter_GoBackEvent(object sender, EventArgs e)
        {
            GoBack();
        }

        private void MessageCenter_MiniWindowEvent(object sender, bool e)
        {
            if (e)
            {
                MiniWindowsTitleBar.Visibility = Visibility.Visible;
                Window.Current.SetTitleBar(MiniWindowsTitleBar);
            }
            else
            {
                MiniWindowsTitleBar.Visibility = Visibility.Collapsed;
                Window.Current.SetTitleBar(CustomDragRegion);
            }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New && e.Parameter != null && !string.IsNullOrEmpty(e.Parameter.ToString()))
            {
                var result = await MessageCenter.HandelUrl(e.Parameter.ToString());
                if (!result)
                {
                    Notify.ShowMessageToast("无法打开链接:" + e.Parameter.ToString());
                }
            }
#if !DEBUG
             await BiliExtensions.CheckVersion();
#endif
        }

        private void MessageCenter_ChangeTitleEvent(object sender, string e)
        {
            if (sender == null)
            {
                (tabView.SelectedItem as TabViewItem).Header = e;
                return;
            }

            foreach (var item in tabView.TabItems)
            {
                var tabViewItem = item as TabViewItem;
                if (tabViewItem == null) continue;
                var frame = tabViewItem.Content as MyFrame;
                if (frame == null) continue;
                if (sender == frame.Content)
                {
                    tabViewItem.Header = e;
                    break;
                }
            }
        }

        private void NavigationHelper_NavigateToPageEvent(object sender, NavigationInfo e)
        {
            var item = new TabViewItem()
            {
                Header = e.title,
                IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource() { Symbol = e.icon }
            };
            var frame = new MyFrame();
            //注册鼠标点击事件
            frame.PointerPressed += Content_PointerPressed;
            frame.Navigate(e.page, e.parameters);
            item.Content = frame;

            tabView.TabItems.Add(item);
            if (!e.dontGoTo)
                tabView.SelectedItem = item;
            item.UpdateLayout();
        }
        private void Content_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (SettingService.GetValue(SettingConstants.UI.MOUSE_MIDDLE_ACTION, (int)MouseMiddleActions.Back) == (int)MouseMiddleActions.Back
                && e.IsUseMiddleButton(sender))
            {
                GoBack();
                e.Handled = true;

            }
        }

        private void GoBack()
        {
            //如果打开了图片浏览，则关闭图片浏览
            if (gridViewer.Visibility == Visibility.Visible)
            {
                imgViewer_CloseEvent(this, null);
                return;
            }

            //处理多标签
            if (tabView.SelectedItem != tabView.TabItems[0])
            {
                var frame = (tabView.SelectedItem as TabViewItem).Content as MyFrame;
                if (frame.CanGoBack)
                {
                    frame.Close();
                    frame.GoBack();
                }
                else
                {
                    ClosePage(tabView.SelectedItem as TabViewItem);
                    //frame.Close();
                    //tabView.TabItems.Remove(tabView.SelectedItem);
                }
            }
        }

        /// <summary>
        /// 处理标题栏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            if (FlowDirection == FlowDirection.LeftToRight)
            {
                CustomDragRegion.MinWidth = sender.SystemOverlayRightInset;
                ShellTitlebarInset.MinWidth = sender.SystemOverlayLeftInset;
            }
            else
            {
                CustomDragRegion.MinWidth = sender.SystemOverlayLeftInset;
                ShellTitlebarInset.MinWidth = sender.SystemOverlayRightInset;
            }
            CustomDragRegion.Height = ShellTitlebarInset.Height = sender.Height;
        }

        private void TabView_AddTabButtonClick(Microsoft.UI.Xaml.Controls.TabView sender, object args)
        {
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                page = typeof(NewPage),
                title = "新建页面"
            });

        }

        private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            ClosePage(args.Tab);
        }
        private void ClosePage(TabViewItem tabItem)
        {
            var frame = tabItem.Content as MyFrame;
            ((frame.Content as Page).Content as Grid).Children.Clear();

            frame.Close();
            //frame.Navigate(typeof(BlankPage));
            // frame.BackStack.Clear();
            tabItem.Content = null;
            tabView.TabItems.Remove(tabItem);
            //GC.Collect();
        }
        private void tabView_Loaded(object sender, RoutedEventArgs e)
        {
            var frame = new MyFrame();

            frame.Navigate(typeof(HomePage));

            (tabView.TabItems[0] as TabViewItem).Content = frame;
        }
        private async void MessageCenter_ViewImageEvent(object sender, ImageViewerParameter e)
        {
            gridViewer.Visibility = Visibility.Visible;
            await gridViewer.FadeInAsync();
            imgViewer.InitImage(e);
        }
        private async void imgViewer_CloseEvent(object sender, EventArgs e)
        {
            if (gridViewer.Visibility == Visibility.Visible)
            {
                imgViewer.ClearImage();
                await gridViewer.FadeOutAsync();
                gridViewer.Visibility = Visibility.Collapsed;
            }
        }

        private void NewTabKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                page = typeof(NewPage),
                title = "新建页面"
            });
            args.Handled = true;
        }

        private void CloseSelectedTabKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            if (((TabViewItem)tabView.SelectedItem).IsClosable)
            {

                ClosePage((TabViewItem)tabView.SelectedItem);
            }
            args.Handled = true;

        }

        private void tabView_TabItemsChanged(TabView sender, IVectorChangedEventArgs args)
        {

        }
    }
}
