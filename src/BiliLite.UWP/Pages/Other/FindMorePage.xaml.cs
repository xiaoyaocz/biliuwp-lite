using BiliLite.Helpers;
using BiliLite.Modules.Other;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
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
        readonly FindMoreVM findMoreVM;
        public FindMorePage()
        {
            this.InitializeComponent();
            findMoreVM=new FindMoreVM();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New && findMoreVM.Items == null)
            {
                findMoreVM.LoadEntrance();
            }
        }

        private async void AdaptiveGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as FindMoreEntranceModel;
            if (item.type == 0)
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo() { 
                    icon =Symbol.Link,
                    title =item.name,
                    page=typeof(WebPage),
                    parameters=item.link
                });
            }
            else if(item.type == 1)
            {
                await Launcher.LaunchUriAsync(new Uri(item.link));
            }
           
        }
    }
}
