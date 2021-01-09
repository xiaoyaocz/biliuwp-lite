using BiliLite.Helpers;
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

namespace BiliLite.Pages.Other
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FindMorePage : Page
    {
        readonly List<NavigatorItem> navigatorItems;
        public FindMorePage()
        {
            this.InitializeComponent();
            navigatorItems = new List<NavigatorItem>() {
                new NavigatorItem(){
                    Name="二维码扫描",
                    Symbol= Symbol.Camera,
                    NavigationInfo=new NavigationInfo()
                    {
                        icon= Symbol.Camera,
                        page=typeof(BlankPage),
                        title="二维码扫描"
                    }
                },
                new NavigatorItem(){
                    Name="随机视频",
                    Symbol= Symbol.Refresh,
                    NavigationInfo=new NavigationInfo()
                    {
                        icon= Symbol.Camera,
                        page=typeof(BlankPage),
                        title="二维码扫描"
                    }
                },
                new NavigatorItem(){
                        Name="NS Plugin",
                        Symbol= Symbol.World,
                        NavigationInfo=new NavigationInfo()
                        {
                            icon= Symbol.Camera,
                            page=typeof(BlankPage),
                            title="测试"
                        }
                    },
                new NavigatorItem(){
                        Name="活动中心",
                        Symbol= Symbol.Tag,
                        NavigationInfo=new NavigationInfo()
                        {
                            icon= Symbol.Camera,
                            page=typeof(BlankPage),
                            title="二维码扫描"
                        }
                 },
            };
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            list.ItemsSource = navigatorItems;
        }

        private void AdaptiveGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageCenter.OpenNewWindow(this, (e.ClickedItem as NavigatorItem).NavigationInfo);
        }
    }

    public class NavigatorItem
    {
        public string Name { get; set; }
        public Symbol Symbol { get; set; }
        public NavigationInfo NavigationInfo { get; set; }
    }
}
