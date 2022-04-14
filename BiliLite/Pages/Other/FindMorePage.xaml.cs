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
    public sealed partial class FindMorePage : BasePage
    {
        readonly List<NavigatorItem> navigatorItems;
        public FindMorePage()
        {
            this.InitializeComponent();
            navigatorItems = new List<NavigatorItem>() {
                new NavigatorItem(){
                    Name="2022年4月港澳台片单",
                    Symbol= Symbol.Link,
                    NavigationInfo=new NavigationInfo()
                    {
                        icon= Symbol.Link,
                        page=typeof(WebPage),
                        title="2022年4月港澳台片单",
                        parameters="https://www.bilibili.com/bangumi/list/sl59578"
                    }
                },
                new NavigatorItem(){
                    Name="2022年1月港澳台片单",
                    Symbol= Symbol.Link,
                    NavigationInfo=new NavigationInfo()
                    {
                        icon= Symbol.Link,
                        page=typeof(WebPage),
                        title="2022年1月港澳台片单",
                        parameters="https://www.bilibili.com/bangumi/list/sl58464"
                    }
                },
                 new NavigatorItem(){
                    Name="2021年10月港澳台片单",
                    Symbol= Symbol.Link,
                    NavigationInfo=new NavigationInfo()
                    {
                        icon= Symbol.Link,
                        page=typeof(WebPage),
                        title="2021年10月港澳台片单",
                        parameters="https://www.bilibili.com/bangumi/list/sl56740"
                    }
                },
                  new NavigatorItem(){
                    Name="2021年7月港澳台片单",
                    Symbol= Symbol.Link,
                    NavigationInfo=new NavigationInfo()
                    {
                        icon= Symbol.Link,
                        page=typeof(WebPage),
                        title="2021年7月港澳台片单",
                        parameters="https://www.bilibili.com/bangumi/list/sl55865"
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
            MessageCenter.NavigateToPage(this, (e.ClickedItem as NavigatorItem).NavigationInfo);
        }
    }

    public class NavigatorItem
    {
        public string Name { get; set; }
        public Symbol Symbol { get; set; }
        public NavigationInfo NavigationInfo { get; set; }
    }
}
