using BiliLite.Helpers;
using BiliLite.Modules.User;
using Microsoft.Toolkit.Uwp.UI.Controls;
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
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages.Home
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class UserDynamicPage : Page
    {
        readonly DynamicVM dynamicVM;
        private bool IsStaggered { get; set; } =false;
        public UserDynamicPage()
        {
            this.InitializeComponent();
            dynamicVM = new DynamicVM();
            dynamicVM.OpenCommentEvent += DynamicVM_OpenCommentEvent;
            splitView.PaneClosed += SplitView_PaneClosed;
            this.DataContext = dynamicVM;
            if (SettingHelper.GetValue<bool>(SettingHelper.UI.CACHE_HOME, true))
            {
                this.NavigationCacheMode = NavigationCacheMode.Enabled;
            }
            else
            {
                this.NavigationCacheMode = NavigationCacheMode.Disabled;
            }
        }

        private void SplitView_PaneClosed(SplitView sender, object args)
        {
            comment.ClearComment();
            repost.dynamicRepostVM.Clear();
        }
        string dynamic_id;
        private void DynamicVM_OpenCommentEvent(object sender, Controls.Dynamic.DynamicItemDisplayModel e)
        {
           // splitView.IsPaneOpen = true;
            dynamic_id = e.DynamicID;
            pivotRight.SelectedIndex = 1;
            repostCount.Text = e.ShareCount.ToString();
            commentCount.Text = e.CommentCount.ToString();
            Api.CommentApi.CommentType commentType= Api.CommentApi.CommentType.Dynamic;
            var id = e.ReplyID;
            switch (e.Type)
            {
              
                case Controls.Dynamic.DynamicDisplayType.Photo:
                    commentType = Api.CommentApi.CommentType.Photo;
                    break;
                case Controls.Dynamic.DynamicDisplayType.Video:
              
                    commentType = Api.CommentApi.CommentType.Video;
                    break;
                case Controls.Dynamic.DynamicDisplayType.Season:
                    id = e.OneRowInfo.AID;
                    commentType = Api.CommentApi.CommentType.Video;
                    break;
                case Controls.Dynamic.DynamicDisplayType.ShortVideo:
                    commentType = Api.CommentApi.CommentType.MiniVideo;
                    break;
                case Controls.Dynamic.DynamicDisplayType.Music:
                    commentType = Api.CommentApi.CommentType.Song;
                    break;
                case Controls.Dynamic.DynamicDisplayType.Article:
                    commentType = Api.CommentApi.CommentType.Article;
                    break;
                case Controls.Dynamic.DynamicDisplayType.MediaList:
                    if (e.OneRowInfo.Tag != "收藏夹")
                        commentType = Api.CommentApi.CommentType.Video;
                    break;
                default:
                    id = e.DynamicID;
                    break;
            }

            //comment.LoadComment(new Controls.LoadCommentInfo()
            //{
            //    commentMode = (int)commentType,
            //    commentSort = Api.CommentApi.commentSort.Hot,
            //    oid = id
            //});
            Utils.ShowComment(id, (int)commentType, Api.CommentApi.CommentSort.Hot);
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            SetStaggered();
            if (e.NavigationMode== NavigationMode.New && dynamicVM.Items == null)
            {
                await dynamicVM.GetDynamicItems();
                if (SettingHelper.GetValue<bool>("动态切换提示", true) && SettingHelper.GetValue<int>(SettingHelper.UI.DYNAMIC_DISPLAY_MODE, 0) != 1)
                {
                    SettingHelper.SetValue("动态切换提示", false);
                    Utils.ShowMessageToast("右下角可以切换成瀑布流显示哦~", 5);
                }
            }
        }

        void SetStaggered()
        {
            var staggered = SettingHelper.GetValue<int>(SettingHelper.UI.DYNAMIC_DISPLAY_MODE, 0) == 1;
            if (staggered != IsStaggered)
            {
                IsStaggered = staggered;
                if (staggered)
                {
                    btnGrid_Click(this, null);
                }
                else
                {
                    btnList_Click(this, null);
                }
            }
        }

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((int)dynamicVM.UserDynamicType != pivot.SelectedIndex)
            {
                dynamicVM.UserDynamicType = (Api.User.DynamicAPI.UserDynamicType)pivot.SelectedIndex;
                dynamicVM.Refresh();
            }
          
        }

        private void btnGrid_Click(object sender, RoutedEventArgs e)
        {
            SettingHelper.SetValue<int>(SettingHelper.UI.DYNAMIC_DISPLAY_MODE, 1);
            IsStaggered = true;
            btnGrid.Visibility = Visibility.Collapsed;
            btnList.Visibility = Visibility.Visible;
            //顶部
            gridTopBar.MaxWidth = double.MaxValue;
            gridTopBar.Margin = new Thickness(0,0,0,4);
            borderTopBar.CornerRadius = new CornerRadius(0);
            borderTopBar.Margin = new Thickness(0);
           
            //XAML
//            var tmp = @" <controls:StaggeredPanel DesiredColumnWidth='450' HorizontalAlignment='Stretch' ColumnSpacing='-12' RowSpacing='8' />";
//            var xaml = $@"<ItemsPanelTemplate 
//xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' 
//xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' 
//xmlns:controls='using:Microsoft.Toolkit.Uwp.UI.Controls'>
//                   {tmp}
//               </ItemsPanelTemplate>";
            //list.ItemsPanel = (ItemsPanelTemplate)XamlReader.Load(xaml);
            list.ItemsPanel = (ItemsPanelTemplate)this.Resources["GridPanel"];
        }

        private void btnList_Click(object sender, RoutedEventArgs e)
        {
            IsStaggered = false;
            //右下角按钮
            btnGrid.Visibility = Visibility.Visible;
            btnList.Visibility = Visibility.Collapsed;
            //设置
            SettingHelper.SetValue<int>(SettingHelper.UI.DYNAMIC_DISPLAY_MODE, 0);
            //顶部
            gridTopBar.MaxWidth = 800;
            gridTopBar.Margin = new Thickness(8, 0,8,0);
            borderTopBar.CornerRadius = new CornerRadius(4);
            borderTopBar.Margin = new Thickness(12,4,12,4);
            //XAML
//            var tmp = @" <ItemsStackPanel/>";
//            var xaml = $@"<ItemsPanelTemplate 
//xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' 
//xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
//                   {tmp}
//               </ItemsPanelTemplate>";
            //list.ItemsPanel = (ItemsPanelTemplate)XamlReader.Load(xaml);
            list.ItemsPanel = (ItemsPanelTemplate)this.Resources["ListPanel"];
        }

        private void btnTop_Click(object sender, RoutedEventArgs e)
        {
            list.ScrollIntoView(list.Items[0]);
        }

        private void pivotRight_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivotRight.SelectedIndex == 0&&splitView.IsPaneOpen&& (repost.dynamicRepostVM.Items==null|| repost.dynamicRepostVM.Items.Count==0))
            {
                repost.LoadData(dynamic_id);
            }
        }
    }
}
