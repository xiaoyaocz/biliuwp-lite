using BiliLite.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class NoTabMainPage : Page
    {
        public NoTabMainPage()
        {
            this.InitializeComponent();
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            mode = SettingHelper.GetValue<int>(SettingHelper.UI.DISPLAY_MODE, 0);
            Window.Current.SetTitleBar(TitleBar);
            frame.Navigated += Frame_Navigated;
            MessageCenter.OpenNewWindowEvent += NavigationHelper_OpenNewWindowEvent;
            MessageCenter.ChangeTitleEvent += MessageCenter_ChangeTitleEvent;
            Window.Current.Content.PointerPressed += Content_PointerPressed;
        }
        private void Content_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var par = e.GetCurrentPoint(sender as Frame).Properties.PointerUpdateKind;
            if (par == Windows.UI.Input.PointerUpdateKind.XButton1Pressed || par == Windows.UI.Input.PointerUpdateKind.MiddleButtonPressed)
            {
                //处理多标签
                if (this.frame.CanGoBack)
                {
                    this.frame.GoBack();
                    e.Handled = true;
                }

            }
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            if(e.Content is Pages.HomePage)
            {
                txtTitle.Text = "哔哩哔哩 UWP";
            }
            if (frame.CanGoBack)
            {
                btnBack.Visibility = Visibility.Visible;
            }
            else
            {
                btnBack.Visibility = Visibility.Collapsed;
            }
        }
        private int mode = 1;
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
           
            frame.Navigate(typeof(Pages.HomePage));
            await Utils.CheckVersion();
        }

        private void MessageCenter_ChangeTitleEvent(object sender, string e)
        {
            if (mode == 1)
            {
                txtTitle.Text = e;
            }
        }

        private void NavigationHelper_OpenNewWindowEvent(object sender, NavigationInfo e)
        {
            if (mode==1)
            {
                txtTitle.Text = e.title;
                frame.Navigate(e.page, e.parameters);
            }
            else
            {
               OpenNewWindow(e);
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (frame.CanGoBack)
            {
                frame.GoBack();
            }
        }

        private async void OpenNewWindow(NavigationInfo e)
        {
            CoreApplicationView newView = CoreApplication.CreateNewView();
            int newViewId = 0;
            await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var res=App.Current.Resources;
                Frame frame = new Frame();
                frame.Navigate(e.page, e.parameters);
                Window.Current.Content = frame;
                Window.Current.Activate();
                newViewId = ApplicationView.GetForCurrentView().Id;
            });
            bool viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);
        }

    }
}
