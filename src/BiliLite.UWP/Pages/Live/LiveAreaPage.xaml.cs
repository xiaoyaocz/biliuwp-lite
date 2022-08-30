using BiliLite.Helpers;
using BiliLite.Modules.Live;
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

namespace BiliLite.Pages.Live
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LiveAreaPage : BasePage
    {
        readonly LiveAreaVM liveAreaVM;
        public LiveAreaPage()
        {
            this.InitializeComponent();
            Title = "直播分区";
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            liveAreaVM = new LiveAreaVM();
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if(e.NavigationMode==  NavigationMode.New&& liveAreaVM.Items == null)
            {
                await liveAreaVM.GetItems();
            }
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var area = e.ClickedItem as LiveAreaItemModel;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Document,
                page = typeof(LiveAreaDetailPage),
                title = area.name,
                parameters = new LiveAreaPar()
                {
                    parent_id = area.parent_id,
                    area_id = area.id
                }
            });
        }
    }
}
