using BiliLite.Services;
using BiliLite.Models.Common;
using BiliLite.Models.Requests.Api;
using BiliLite.Models.Requests.Api.User;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using BiliLite.Extensions;
using BiliLite.ViewModels.UserDynamic;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages.Home
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class UserDynamicPage : Page
    {
        readonly UserDynamicViewModel m_userDynamicViewModel;
        private bool IsStaggered { get; set; } = false;
        public UserDynamicPage()
        {
            this.InitializeComponent();
            m_userDynamicViewModel = new UserDynamicViewModel();
            m_userDynamicViewModel.OpenCommentEvent += UserDynamicViewModelOpenCommentEvent;
            splitView.PaneClosed += SplitView_PaneClosed;
            this.DataContext = m_userDynamicViewModel;
            if (SettingService.GetValue<bool>(SettingConstants.UI.CACHE_HOME, true))
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
            repost.UserDynamicRepostViewModel.Clear();
        }
        string dynamic_id;
        private void UserDynamicViewModelOpenCommentEvent(object sender, Controls.Dynamic.UserDynamicItemDisplayViewModel e)
        {
            // splitView.IsPaneOpen = true;
            dynamic_id = e.DynamicID;
            pivotRight.SelectedIndex = 1;
            repostCount.Text = e.ShareCount.ToString();
            commentCount.Text = e.CommentCount.ToString();
            CommentApi.CommentType commentType = CommentApi.CommentType.Dynamic;
            var id = e.ReplyID;
            switch (e.Type)
            {

                case UserDynamicDisplayType.Photo:
                    commentType = CommentApi.CommentType.Photo;
                    break;
                case UserDynamicDisplayType.Video:

                    commentType = CommentApi.CommentType.Video;
                    break;
                case UserDynamicDisplayType.Season:
                    id = e.OneRowInfo.AID;
                    commentType = CommentApi.CommentType.Video;
                    break;
                case UserDynamicDisplayType.ShortVideo:
                    commentType = CommentApi.CommentType.MiniVideo;
                    break;
                case UserDynamicDisplayType.Music:
                    commentType = CommentApi.CommentType.Song;
                    break;
                case UserDynamicDisplayType.Article:
                    commentType = CommentApi.CommentType.Article;
                    break;
                case UserDynamicDisplayType.MediaList:
                    if (e.OneRowInfo.Tag != "收藏夹")
                        commentType = CommentApi.CommentType.Video;
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
            Notify.ShowComment(id, (int)commentType, CommentApi.CommentSort.Hot);
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            SetStaggered();
            if (e.NavigationMode == NavigationMode.New && m_userDynamicViewModel.Items == null)
            {
                await m_userDynamicViewModel.GetDynamicItems();
                if (SettingService.GetValue<bool>("动态切换提示", true) && SettingService.GetValue<int>(SettingConstants.UI.DYNAMIC_DISPLAY_MODE, 0) != 1)
                {
                    SettingService.SetValue("动态切换提示", false);
                    Notify.ShowMessageToast("右下角可以切换成瀑布流显示哦~", 5);
                }
            }
        }

        void SetStaggered()
        {
            var staggered = SettingService.GetValue<int>(SettingConstants.UI.DYNAMIC_DISPLAY_MODE, 0) == 1;
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
            if ((int)m_userDynamicViewModel.UserDynamicType != pivot.SelectedIndex)
            {
                m_userDynamicViewModel.UserDynamicType = (DynamicAPI.UserDynamicType)pivot.SelectedIndex;
                m_userDynamicViewModel.Refresh();
            }

        }

        private void btnGrid_Click(object sender, RoutedEventArgs e)
        {
            SettingService.SetValue<int>(SettingConstants.UI.DYNAMIC_DISPLAY_MODE, 1);
            IsStaggered = true;
            btnGrid.Visibility = Visibility.Collapsed;
            btnList.Visibility = Visibility.Visible;
            //顶部
            gridTopBar.MaxWidth = double.MaxValue;
            gridTopBar.Margin = new Thickness(0, 0, 0, 4);
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
            SettingService.SetValue<int>(SettingConstants.UI.DYNAMIC_DISPLAY_MODE, 0);
            //顶部
            gridTopBar.MaxWidth = 800;
            gridTopBar.Margin = new Thickness(8, 0, 8, 0);
            borderTopBar.CornerRadius = new CornerRadius(4);
            borderTopBar.Margin = new Thickness(12, 4, 12, 4);
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
            if (pivotRight.SelectedIndex == 0 && splitView.IsPaneOpen && (repost.UserDynamicRepostViewModel.Items == null || repost.UserDynamicRepostViewModel.Items.Count == 0))
            {
                repost.LoadData(dynamic_id);
            }
        }
    }
}
