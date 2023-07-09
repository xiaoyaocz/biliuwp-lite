using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using BiliLite.Modules;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using BiliLite.Controls;
using Windows.System;
using BiliLite.Dialogs;
using System.Threading.Tasks;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Models.Requests.Api;
using BiliLite.Services;
using Windows.UI.Xaml.Controls.Primitives;
using BiliLite.Models.Common.Comment;
using BiliLite.Models.Common.Video;
using BiliLite.Models.Common.Video.Detail;
using BiliLite.Models.Download;
using BiliLite.ViewModels.Video;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages
{
    public class VideoPlaylist
    {
        public int Index { get; set; }
        public List<VideoPlaylistItem> Playlist { get; set; }
    }
    public class VideoPlaylistItem
    {
        public string ID { get; set; }
        public string Author { get; set; }
        public string Cover { get; set; }
        public string Title { get; set; }
    }
    public sealed partial class VideoDetailPage : PlayPage
    {
        private static readonly ILogger logger = GlobalLogger.FromCurrentType();

        private readonly VideoDetailPageViewModel m_viewModel;
        string avid = "";
        string bvid = "";
        bool is_bvid = false;
        private bool isFirstUgcSeasonVideo = false;

        public VideoDetailPage()
        {
            this.InitializeComponent();
            Title = "视频详情";
            this.Loaded += VideoDetailPage_Loaded;
            this.Player = this.player;
            NavigationCacheMode = NavigationCacheMode.Enabled;
            m_viewModel = new VideoDetailPageViewModel();
            this.DataContext = m_viewModel;
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            m_viewModel.DefaultRightInfoWidth = new GridLength(SettingService.GetValue<double>(SettingConstants.UI.RIGHT_DETAIL_WIDTH, 320), GridUnitType.Pixel);
            this.RightInfoGridSplitter.IsEnabled = SettingService.GetValue<bool>(SettingConstants.UI.RIGHT_WIDTH_CHANGEABLE, false);
        }

        private void VideoDetailPage_Loaded(object sender, RoutedEventArgs e)
        {

            if (this.Parent is MyFrame)
            {
                (this.Parent as MyFrame).ClosedPage -= VideoDetailPage_ClosedPage;
                (this.Parent as MyFrame).ClosedPage += VideoDetailPage_ClosedPage;
            }

        }

        private void VideoDetailPage_ClosedPage(object sender, EventArgs e)
        {
            ClosePage();
        }
        private void ClosePage()
        {
            if (m_viewModel != null)
            {
                m_viewModel.Loaded = false;
                m_viewModel.Loading = true;
                m_viewModel.VideoInfo = null;
            }
            changedFlag = true;
            player?.FullScreen(false);
            player?.MiniWidnows(false);
            player?.Dispose();
        }
        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            request.Data.Properties.Title = m_viewModel.VideoInfo.Title;
            request.Data.SetWebLink(new Uri(m_viewModel.VideoInfo.ShortLink));
        }
        VideoPlaylist playlist;
        bool flag = false;
        string _id = "";
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New)
            {
                if (SettingService.GetValue<bool>(SettingConstants.Player.AUTO_FULL_SCREEN, false))
                {
                    player.IsFullScreen = true;
                }
                else
                {
                    player.IsFullWindow = SettingService.GetValue<bool>(SettingConstants.Player.AUTO_FULL_WINDOW, false);
                }
                pivot.SelectedIndex = SettingService.GetValue<int>(SettingConstants.UI.DETAIL_DISPLAY, 0);
                if (e.Parameter is VideoPlaylist)
                {
                    playlist = e.Parameter as VideoPlaylist;


                    var element = PlayListTpl.GetElement(new Windows.UI.Xaml.ElementFactoryGetArgs()) as PivotItem;
                    element.DataContext = playlist;

                    pivot.Items.Insert(0, element);
                    pivot.SelectedIndex = 0;
                    await InitializeVideo(playlist.Playlist[playlist.Index].ID);
                }
                else
                {
                    var id = e.Parameter.ToString();
                    await InitializeVideo(id);
                }

            }
            else
            {
                Title = m_viewModel?.VideoInfo?.Title ?? "视频详情";
                MessageCenter.ChangeTitle(this, Title);
            }

        }
        private async Task InitializeVideo(string id)
        {
            _id = id;
            if (flag) return;
            flag = true;
            if (int.TryParse(id, out var aid))
            {
                avid = id;
                is_bvid = false;
            }
            else
            {
                bvid = id;
                is_bvid = true;
            }
            await m_viewModel.LoadVideoDetail(id, is_bvid);
            if (this.VideoCover != null)
            {
                this.VideoCover.Visibility = SettingService.GetValue<bool>(SettingConstants.UI.SHOW_DETAIL_COVER, true) ? Visibility.Visible : Visibility.Collapsed;
            }
            if (SettingService.GetValue<bool>("一键三连提示", true))
            {
                SettingService.SetValue("一键三连提示", false);
                Notify.ShowMessageToast("右键或长按点赞按钮可以一键三连哦~", 5);
            }
            if (m_viewModel.VideoInfo == null)
            {
                flag = false;
                return;
            }

            avid = m_viewModel.VideoInfo.Aid;
            var desc = m_viewModel.VideoInfo.Desc.ToRichTextBlock(null);

            contentDesc.Content = desc;
            ChangeTitle(m_viewModel.VideoInfo.Title);
            CreateQR();
            if (!string.IsNullOrEmpty(m_viewModel.VideoInfo.RedirectUrl))
            {
                var result = await MessageCenter.HandelSeasonID(m_viewModel.VideoInfo.RedirectUrl);
                if (!string.IsNullOrEmpty(result))
                {
                    this.Frame.Navigate(typeof(SeasonDetailPage), result);
                    //从栈中移除当前页面的历史
                    this.Frame.BackStack.Remove(this.Frame.BackStack.FirstOrDefault(x => x.SourcePageType == this.GetType()));
                    return;
                }
            }
            InitPlayInfo();
            
            comment.LoadComment(new LoadCommentInfo()
            {
                CommentMode = (int)CommentApi.CommentType.Video,
                CommentSort = CommentApi.CommentSort.Hot,
                Oid = m_viewModel.VideoInfo.Aid
            });

            if (playlist != null || !m_viewModel.VideoInfo.ShowUgcSeason)
            {
                flag = false;
                return;
            }
            InitUgcSeason(id);

            flag = false;
        }

        private void InitPlayInfo()
        {
            List<PlayInfo> playInfos = new List<PlayInfo>();
            int i = 0;
            foreach (var item in m_viewModel.VideoInfo.Pages)
            {
                playInfos.Add(new PlayInfo()
                {
                    avid = m_viewModel.VideoInfo.Aid,
                    cid = item.Cid,
                    duration = item.Duration,
                    is_interaction = m_viewModel.VideoInfo.Interaction != null,
                    order = i,
                    play_mode = VideoPlayType.Video,
                    title = "P" + item.Page + " " + item.Part,
                    area = m_viewModel.VideoInfo.Title.ParseArea(m_viewModel.VideoInfo.Owner.Mid)
                });
                i++;
            }
            var index = 0;
            if (m_viewModel.VideoInfo.History != null)
            {
                var history = m_viewModel.VideoInfo.Pages.FirstOrDefault(x => x.Cid.Equals(m_viewModel.VideoInfo.History.Cid));
                if (history != null)
                {
                    SettingService.SetValue<double>(history.Cid, Convert.ToDouble(m_viewModel.VideoInfo.History.Progress));
                    index = m_viewModel.VideoInfo.Pages.IndexOf(history);
                    //player.InitializePlayInfo(playInfos, );
                }
            }
            player.InitializePlayInfo(playInfos, index);
        }

        private void InitUgcSeason(string id)
        {
            isFirstUgcSeasonVideo = true;
            playlist = new VideoPlaylist()
            {
                Playlist = new List<VideoPlaylistItem>()
            };
            foreach (var section in m_viewModel.VideoInfo.UgcSeason.Sections)
            {
                foreach (var item in section.Episodes)
                {
                    playlist.Playlist.Add(new VideoPlaylistItem()
                    {
                        ID = item.Aid,
                        Title = item.Title,
                        Author = item?.Author?.Name,
                        Cover = item.Cover
                    });
                }
            }
            var episodeIndex = playlist.Playlist.FindIndex(x => x.ID == id);
            var element = PlayListTpl.GetElement(new Windows.UI.Xaml.ElementFactoryGetArgs()) as PivotItem;

            element.DataContext = playlist;
            var listView = element.Content as ListView;
            listView.SelectedIndex = episodeIndex;
            pivot.Items.Insert(0, element);
        }

        private void CreateQR()
        {
            try
            {
                ZXing.BarcodeWriter barcodeWriter = new ZXing.BarcodeWriter();
                barcodeWriter.Format = ZXing.BarcodeFormat.QR_CODE;
                barcodeWriter.Options = new ZXing.Common.EncodingOptions()
                {
                    Margin = 1,
                    Height = 200,
                    Width = 200
                };
                var data = barcodeWriter.Write(m_viewModel.VideoInfo.ShortLink);
                imgQR.Source = data;
            }
            catch (Exception ex)
            {
                logger.Log("创建二维码失败avid" + avid, LogType.ERROR, ex);
                Notify.ShowMessageToast("创建二维码失败");
            }

        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back || e.SourcePageType.Name == "BlankPage")
            {
                ClosePage();
            }

            base.OnNavigatingFrom(e);
        }
        public void ChangeTitle(string title)
        {
            if ((this.Parent as Frame)?.Parent is TabViewItem)
            {
                if (this.Parent != null)
                {
                    ((this.Parent as Frame).Parent as TabViewItem).Header = title;
                }
            }
            else
            {
                MessageCenter.ChangeTitle(this, title);
            }
        }

        private void txtDesc_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (stDesc.MaxHeight > 80)
            {
                stDesc.MaxHeight = 80;
            }
            else
            {
                stDesc.MaxHeight = 1000.0;
            }
            Notify.ShowMessageToast("右键或长按可以复制内容");
        }

        private void txtDesc_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if ((sender as TextBlock).Text.SetClipboard())
            {
                Notify.ShowMessageToast("已将内容复制到剪切板");
            }
            else
            {
                Notify.ShowMessageToast("复制失败");
            }
        }

        private void txtDesc_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if ((sender as TextBlock).Text.SetClipboard())
            {
                Notify.ShowMessageToast("已将内容复制到剪切板");
            }
            else
            {
                Notify.ShowMessageToast("复制失败");
            }
        }

        private async void btnAttention_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as Button).DataContext as VideoDetailStaffViewModel;
            var result = await m_viewModel.AttentionUP(data.Mid, data.Attention == 1 ? 2 : 1);
            if (result)
            {
                if (data.Attention == 1)
                {
                    data.Attention = 0;
                }
                else
                {
                    data.Attention = 1;
                }
            }

        }


        private void listRelates_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as VideoDetailRelatesViewModel;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Play,
                page = typeof(VideoDetailPage),
                parameters = data.Aid,
                title = data.Title
            });

            //this.Frame.Navigate(typeof(VideoDetailPage), data.aid);
        }

        private void AppBarButton_Holding(object sender, HoldingRoutedEventArgs e)
        {
            Notify.ShowMessageToast("长按");
        }

        private void btnLike_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (m_viewModel.VideoInfo.ReqUser.Like == 0)
            {
                m_viewModel.DoTriple();
            }
        }

        private void btnTagItem_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as HyperlinkButton).DataContext as VideoDetailTagModel;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Find,
                page = typeof(SearchPage),
                parameters = item.TagName,
                title = "搜索:" + item.TagName
            });
        }

        private void btnShare_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }
        private void btnShareCopy_Click(object sender, RoutedEventArgs e)
        {
            $"{m_viewModel.VideoInfo.Title}\r\n{m_viewModel.VideoInfo.ShortLink}".SetClipboard();
            Notify.ShowMessageToast("已复制内容到剪切板");
        }

        private void btnShareCopyUrl_Click(object sender, RoutedEventArgs e)
        {
            m_viewModel.VideoInfo.ShortLink.SetClipboard();
            Notify.ShowMessageToast("已复制链接到剪切板");
        }


        private void PlayerControl_FullScreenEvent(object sender, bool e)
        {
            if (e)
            {
                this.Margin = new Thickness(0, SettingService.GetValue<int>(SettingConstants.UI.DISPLAY_MODE, 0) == 0 ? -48 : -48, 0, 0);
                m_viewModel.DefaultRightInfoWidth = new GridLength(0, GridUnitType.Pixel);
                BottomInfo.Height = new GridLength(0, GridUnitType.Pixel);
            }
            else
            {
                this.Margin = new Thickness(0);
                m_viewModel.DefaultRightInfoWidth = new GridLength(SettingService.GetValue<double>(SettingConstants.UI.RIGHT_DETAIL_WIDTH, 320), GridUnitType.Pixel);
                BottomInfo.Height = GridLength.Auto;
            }
        }

        private void PlayerControl_FullWindowEvent(object sender, bool e)
        {
            if (e)
            {
                m_viewModel.DefaultRightInfoWidth = new GridLength(0, GridUnitType.Pixel);
                BottomInfo.Height = new GridLength(0, GridUnitType.Pixel);
            }
            else
            {
                m_viewModel.DefaultRightInfoWidth = new GridLength(SettingService.GetValue<double>(SettingConstants.UI.RIGHT_DETAIL_WIDTH, 320), GridUnitType.Pixel);
                BottomInfo.Height = GridLength.Auto;
            }
        }
        bool changedFlag = false;
        private void PlayerControl_ChangeEpisodeEvent(object sender, int e)
        {
            changedFlag = true;
            listEpisode.SelectedIndex = e;
            changedFlag = false;
        }

        private void listEpisode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (changedFlag || listEpisode.SelectedIndex == -1)
            {
                return;
            }
            player.ChangePlayIndex(listEpisode.SelectedIndex);
        }

        private void btnOpenUser_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as HyperlinkButton).DataContext;
            if (data is VideoDetailStaffViewModel)
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Contact,
                    title = (data as VideoDetailStaffViewModel).Name,
                    page = typeof(UserInfoPage),
                    parameters = (data as VideoDetailStaffViewModel).Mid
                });
            }
            else
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Contact,
                    title = m_viewModel.VideoInfo.Owner.Name,
                    page = typeof(UserInfoPage),
                    parameters = m_viewModel.VideoInfo.Owner.Mid
                });
            }
        }

        private void btnLike_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (m_viewModel.VideoInfo.ReqUser.Like == 0)
            {
                m_viewModel.DoTriple();
            }
        }

        private void listAddFavorite_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as FavoriteItemModel;
            m_viewModel.DoFavorite(new List<string>() { item.id }, avid);
        }

        private void BtnAddFavorite_Click(object sender, RoutedEventArgs e)
        {
            m_viewModel.DoFavorite(m_viewModel.MyFavorite.Where(x => x.is_fav).Select(x => x.id).ToList(), avid);
        }

        private async void btnOpenWeb_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(m_viewModel.VideoInfo.ShortLink));
        }

        private void ImageEx_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (m_viewModel.VideoInfo?.Pic == null) return;
            MessageCenter.OpenImageViewer(new List<string>() {
                m_viewModel.VideoInfo.Pic
            }, 0);
        }

        private async void btnCreateFavBox_Click(object sender, RoutedEventArgs e)
        {
            if (!SettingService.Account.Logined && await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录");
                return;
            }
            CreateFavFolderDialog createFavFolderDialog = new CreateFavFolderDialog();
            await createFavFolderDialog.ShowAsync();
            await m_viewModel.LoadFavorite(m_viewModel.VideoInfo.Aid);
        }

        private void AddToWatchLater_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as MenuFlyoutItem).DataContext as VideoDetailRelatesViewModel;
            Modules.User.WatchLaterVM.Instance.AddToWatchlater(data.Aid);
        }

        private void BtnWatchLater_Click(object sender, RoutedEventArgs e)
        {
            if (m_viewModel == null || m_viewModel.VideoInfo == null) return;
            Modules.User.WatchLaterVM.Instance.AddToWatchlater(avid);
        }

        private async void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isFirstUgcSeasonVideo)
            {
                isFirstUgcSeasonVideo = false;
                return;
            }
            var liveView = sender as ListView;
            if (liveView.SelectedItem == null) return;
            var item = liveView.SelectedItem as VideoPlaylistItem;

            playlist.Index = playlist.Playlist.IndexOf(item);
            await InitializeVideo(item.ID);

        }

        private void player_AllMediaEndEvent(object sender, EventArgs e)
        {

            if (playlist == null || playlist.Index == playlist.Playlist.Count - 1)
            {
                Notify.ShowMessageToast("播放完毕");
                return;
            }
            var listView = (pivot.Items[0] as PivotItem).Content as ListView;

            listView.SelectedIndex = playlist.Index + 1;
        }

        private async void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            if (m_viewModel.VideoInfo == null || m_viewModel.VideoInfo.Pages == null || m_viewModel.VideoInfo.Pages.Count == 0) return;
            var downloadItem = new DownloadItem()
            {
                Cover = m_viewModel.VideoInfo.Pic,
                ID = m_viewModel.VideoInfo.Aid,
                Episodes = new List<DownloadEpisodeItem>(),
                Subtitle = m_viewModel.VideoInfo.Bvid,
                Title = m_viewModel.VideoInfo.Title,
                Type = DownloadType.Video,
                UpMid = m_viewModel.VideoInfo.Owner.Mid.ToInt32(),
            };
            int i = 0;
            foreach (var item in m_viewModel.VideoInfo.Pages)
            {
                //检查正在下载及下载完成是否存在此视频
                int state = 0;
                if (DownloadVM.Instance.Downloadings.FirstOrDefault(x => x.EpisodeID == item.Cid) != null)
                {
                    state = 2;
                }
                if (DownloadVM.Instance.Downloadeds.FirstOrDefault(x => x.Epsidoes.FirstOrDefault(y => y.CID == item.Cid) != null) != null)
                {
                    state = 3;
                }
                //如果正在下载state=2,下载完成state=3
                downloadItem.Episodes.Add(new DownloadEpisodeItem()
                {
                    AVID = m_viewModel.VideoInfo.Aid,
                    BVID = m_viewModel.VideoInfo.Bvid,
                    CID = item.Cid,
                    EpisodeID = "",
                    Index = i,
                    Title = "P" + item.Page + " " + item.Part,
                    State = state
                });
                i++;
            }

            DownloadDialog downloadDialog = new DownloadDialog(downloadItem);
            await downloadDialog.ShowAsync();
        }

        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (m_viewModel.Loading) return;
            await InitializeVideo(_id);
        }

        private void btnOpenQR_Click(object sender, RoutedEventArgs e)
        {
            qrFlyout.ShowAt(btnMore);
        }

        private void TitleText_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var option = new FlyoutShowOptions();
            var element = sender as UIElement;
            option.Position = e.GetPosition(element);
            TitleRightTappedMenu.ShowAt(element, option);
        }

        private void CopyTitleBtn_Click(object sender, RoutedEventArgs e)
        {
            m_viewModel.VideoInfo.Title.SetClipboard();
        }

        private void CopyAuthorBtn_Click(object sender, RoutedEventArgs e)
        {
            m_viewModel.VideoInfo.Owner.Name.SetClipboard();
        }

        private void listEpisode_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void listEpisode_PreviewKeyUp(object sender, KeyRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void BottomActionBar_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width != e.PreviousSize.Width)
            {
                m_viewModel.BottomActionBarWidth = e.NewSize.Width;
            }
            if (e.NewSize.Height != e.PreviousSize.Height)
            {
                m_viewModel.BottomActionBarHeight = e.NewSize.Height;
            }
        }

        private void VideoDetailPage_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width != e.PreviousSize.Width)
            {
                m_viewModel.PageWidth = e.NewSize.Width;
            }
            if (e.NewSize.Height != e.PreviousSize.Height)
            {
                m_viewModel.PageHeight = e.NewSize.Height;
            }
        }
    }
}
