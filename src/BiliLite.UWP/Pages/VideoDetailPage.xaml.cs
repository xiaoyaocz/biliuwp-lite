using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using BiliLite.Modules;
using Microsoft.UI.Xaml.Controls;
using BiliLite.Helpers;
using Windows.ApplicationModel.DataTransfer;
using BiliLite.Controls;
using Windows.System;
using BiliLite.Dialogs;
using System.Threading.Tasks;

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
        VideoDetailVM videoDetailVM;
        string avid = "";
        string bvid = "";
        bool is_bvid = false;
        public VideoDetailPage()
        {
            this.InitializeComponent();
            Title = "视频详情";
            this.Loaded += VideoDetailPage_Loaded;
            this.Player = this.player;
            NavigationCacheMode = NavigationCacheMode.Enabled;
            videoDetailVM = new VideoDetailVM();
            this.DataContext = videoDetailVM;
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            this.RightInfo.Width = new GridLength(SettingHelper.GetValue<double>(SettingHelper.UI.RIGHT_DETAIL_WIDTH, 320), GridUnitType.Pixel);
            this.RightInfoGridSplitter.IsEnabled = SettingHelper.GetValue<bool>(SettingHelper.UI.RIGHT_WIDTH_CHANGEABLE, false);
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
            if (videoDetailVM != null)
            {
                videoDetailVM.Loaded = false;
                videoDetailVM.Loading = true;
                videoDetailVM.VideoInfo = null;
            }
            changedFlag = true;
            player?.FullScreen(false);
            player?.MiniWidnows(false);
            player?.Dispose();
        }
        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            request.Data.Properties.Title = videoDetailVM.VideoInfo.title;
            request.Data.SetWebLink(new Uri(videoDetailVM.VideoInfo.short_link));
        }
        VideoPlaylist playlist;
        bool flag = false;
        string _id = "";
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New)
            {
                if(SettingHelper.GetValue<bool>(SettingHelper.Player.AUTO_FULL_SCREEN, false))
                {
                    player.IsFullScreen = true;
                }
                else
                {
                    player.IsFullWindow = SettingHelper.GetValue<bool>(SettingHelper.Player.AUTO_FULL_WINDOW, false);
                }
                pivot.SelectedIndex = SettingHelper.GetValue<int>(SettingHelper.UI.DETAIL_DISPLAY, 0);
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
                Title = videoDetailVM?.VideoInfo?.title ?? "视频详情";
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
            await videoDetailVM.LoadVideoDetail(id, is_bvid);
            if (this.VideoCover != null)
            {
                this.VideoCover.Visibility = SettingHelper.GetValue<bool>(SettingHelper.UI.SHOW_DETAIL_COVER, true) ? Visibility.Visible : Visibility.Collapsed;
            }
            if (SettingHelper.GetValue<bool>("一键三连提示", true))
            {
                SettingHelper.SetValue("一键三连提示", false);
                Utils.ShowMessageToast("右键或长按点赞按钮可以一键三连哦~", 5);
            }
            if (videoDetailVM.VideoInfo != null)
            {
                avid = videoDetailVM.VideoInfo.aid;
                var desc = ControlHelper.StringToRichText(videoDetailVM.VideoInfo.desc, null);

                contentDesc.Content = desc;
                ChangeTitle(videoDetailVM.VideoInfo.title);
                CreateQR();
                if (!string.IsNullOrEmpty(videoDetailVM.VideoInfo.redirect_url))
                {
                    var result = await MessageCenter.HandelSeasonID(videoDetailVM.VideoInfo.redirect_url);
                    if (!string.IsNullOrEmpty(result))
                    {
                        this.Frame.Navigate(typeof(SeasonDetailPage), result);
                        //从栈中移除当前页面的历史
                        this.Frame.BackStack.Remove(this.Frame.BackStack.FirstOrDefault(x => x.SourcePageType == this.GetType()));
                        return;
                    }
                }
                List<PlayInfo> playInfos = new List<PlayInfo>();
                int i = 0;
                foreach (var item in videoDetailVM.VideoInfo.pages)
                {
                    playInfos.Add(new PlayInfo()
                    {
                        avid = videoDetailVM.VideoInfo.aid,
                        cid = item.cid,
                        duration = item.duration,
                        is_interaction = videoDetailVM.VideoInfo.interaction != null,
                        order = i,
                        play_mode = VideoPlayType.Video,
                        title = "P" + item.page + " " + item.part,
                        area = Utils.ParseArea(videoDetailVM.VideoInfo.title,videoDetailVM.VideoInfo.owner.mid)
                    });
                    i++;
                }
                var index = 0;
                if (videoDetailVM.VideoInfo.history != null)
                {
                    var history = videoDetailVM.VideoInfo.pages.FirstOrDefault(x => x.cid.Equals(videoDetailVM.VideoInfo.history.cid));
                    if (history != null)
                    {
                        SettingHelper.SetValue<double>(history.cid, Convert.ToDouble(videoDetailVM.VideoInfo.history.progress));
                        index = videoDetailVM.VideoInfo.pages.IndexOf(history);
                        //player.InitializePlayInfo(playInfos, );
                    }
                }
                player.InitializePlayInfo(playInfos, index);
                comment.LoadComment(new LoadCommentInfo()
                {
                    CommentMode = (int)Api.CommentApi.CommentType.Video,
                    CommentSort = Api.CommentApi.CommentSort.Hot,
                    Oid = videoDetailVM.VideoInfo.aid
                });
            }
            flag = false;
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
                var data = barcodeWriter.Write(videoDetailVM.VideoInfo.short_link);
                imgQR.Source = data;
            }
            catch (Exception ex)
            {
                LogHelper.Log("创建二维码失败avid" + avid, LogType.ERROR, ex);
                Utils.ShowMessageToast("创建二维码失败");
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
            Utils.ShowMessageToast("右键或长按可以复制内容");
        }

        private void txtDesc_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (Utils.SetClipboard((sender as TextBlock).Text))
            {
                Utils.ShowMessageToast("已将内容复制到剪切板");
            }
            else
            {
                Utils.ShowMessageToast("复制失败");
            }
        }

        private void txtDesc_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (Utils.SetClipboard((sender as TextBlock).Text))
            {
                Utils.ShowMessageToast("已将内容复制到剪切板");
            }
            else
            {
                Utils.ShowMessageToast("复制失败");
            }
        }

        private async void btnAttention_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as Button).DataContext as VideoDetailStaffModel;
            var result = await videoDetailVM.AttentionUP(data.mid, data.attention == 1 ? 2 : 1);
            if (result)
            {
                if (data.attention == 1)
                {
                    data.attention = 0;
                }
                else
                {
                    data.attention = 1;
                }
            }

        }


        private void listRelates_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as VideoDetailRelatesModel;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Play,
                page = typeof(VideoDetailPage),
                parameters = data.aid,
                title = data.title
            });

            //this.Frame.Navigate(typeof(VideoDetailPage), data.aid);
        }

        private void AppBarButton_Holding(object sender, HoldingRoutedEventArgs e)
        {
            Utils.ShowMessageToast("长按");
        }

        private void btnLike_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (videoDetailVM.VideoInfo.req_user.like == 0)
            {
                videoDetailVM.DoTriple();
            }
        }

        private void btnTagItem_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as HyperlinkButton).DataContext as VideoDetailTagModel;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Find,
                page = typeof(SearchPage),
                parameters = item.tag_name,
                title = "搜索:" + item.tag_name
            });
        }

        private void btnShare_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }
        private void btnShareCopy_Click(object sender, RoutedEventArgs e)
        {
            Utils.SetClipboard($"{videoDetailVM.VideoInfo.title}\r\n{videoDetailVM.VideoInfo.short_link}");
            Utils.ShowMessageToast("已复制内容到剪切板");
        }

        private void btnShareCopyUrl_Click(object sender, RoutedEventArgs e)
        {
            Utils.SetClipboard(videoDetailVM.VideoInfo.short_link);
            Utils.ShowMessageToast("已复制链接到剪切板");
        }


        private void PlayerControl_FullScreenEvent(object sender, bool e)
        {
            if (e)
            {
                this.Margin = new Thickness(0, SettingHelper.GetValue<int>(SettingHelper.UI.DISPLAY_MODE, 0) == 0 ? -40 : -32, 0, 0);
                RightInfo.Width = new GridLength(0, GridUnitType.Pixel);
                BottomInfo.Height = new GridLength(0, GridUnitType.Pixel);
            }
            else
            {
                this.Margin = new Thickness(0);
                RightInfo.Width = new GridLength(SettingHelper.GetValue<double>(SettingHelper.UI.RIGHT_DETAIL_WIDTH, 320), GridUnitType.Pixel);
                BottomInfo.Height = GridLength.Auto;
            }
        }

        private void PlayerControl_FullWindowEvent(object sender, bool e)
        {
            if (e)
            {
                RightInfo.Width = new GridLength(0, GridUnitType.Pixel);
                BottomInfo.Height = new GridLength(0, GridUnitType.Pixel);
            }
            else
            {
                RightInfo.Width = new GridLength(SettingHelper.GetValue<double>(SettingHelper.UI.RIGHT_DETAIL_WIDTH, 320), GridUnitType.Pixel);
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
            if (data is VideoDetailStaffModel)
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Contact,
                    title = (data as VideoDetailStaffModel).name,
                    page = typeof(UserInfoPage),
                    parameters = (data as VideoDetailStaffModel).mid
                });
            }
            else
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Contact,
                    title = videoDetailVM.VideoInfo.owner.name,
                    page = typeof(UserInfoPage),
                    parameters = videoDetailVM.VideoInfo.owner.mid
                });
            }
        }

        private void btnLike_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (videoDetailVM.VideoInfo.req_user.like == 0)
            {
                videoDetailVM.DoTriple();
            }
        }

        private void listAddFavorite_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as FavoriteItemModel;
            videoDetailVM.DoFavorite(new List<string>() { item.id }, avid);
        }

        private void BtnAddFavorite_Click(object sender, RoutedEventArgs e)
        {
            videoDetailVM.DoFavorite(videoDetailVM.MyFavorite.Where(x => x.is_fav).Select(x => x.id).ToList(), avid);
        }

        private async void btnOpenWeb_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(videoDetailVM.VideoInfo.short_link));
        }

        private void ImageEx_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (videoDetailVM.VideoInfo?.pic == null) return;
            MessageCenter.OpenImageViewer(new List<string>() {
                videoDetailVM.VideoInfo.pic
            }, 0);
        }

        private async void btnCreateFavBox_Click(object sender, RoutedEventArgs e)
        {
            if (!SettingHelper.Account.Logined && await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录");
                return;
            }
            CreateFavFolderDialog createFavFolderDialog = new CreateFavFolderDialog();
            await createFavFolderDialog.ShowAsync();
            await videoDetailVM.LoadFavorite(videoDetailVM.VideoInfo.aid);
        }

        private void AddToWatchLater_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as MenuFlyoutItem).DataContext as VideoDetailRelatesModel;
            Modules.User.WatchLaterVM.Instance.AddToWatchlater(data.aid);
        }

        private void BtnWatchLater_Click(object sender, RoutedEventArgs e)
        {
            if (videoDetailVM == null || videoDetailVM.VideoInfo == null) return;
            Modules.User.WatchLaterVM.Instance.AddToWatchlater(avid);
        }

        private async void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            var liveView = sender as ListView;
            if (liveView.SelectedItem == null ) return;
            var item = liveView.SelectedItem as VideoPlaylistItem;
           
            playlist.Index = playlist.Playlist.IndexOf(item);
            await InitializeVideo(item.ID);

        }

        private void player_AllMediaEndEvent(object sender, EventArgs e)
        {

            if(playlist==null||playlist.Index== playlist.Playlist.Count - 1)
            {
                Utils.ShowMessageToast("播放完毕");
                return;
            }
            var listView= (pivot.Items[0] as PivotItem).Content as ListView;
         
            listView.SelectedIndex = playlist.Index + 1; 
        }

        private async void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            if (videoDetailVM.VideoInfo == null|| videoDetailVM.VideoInfo.pages==null || videoDetailVM.VideoInfo.pages.Count == 0) return;
            var downloadItem = new DownloadItem() { 
                Cover= videoDetailVM.VideoInfo.pic,
                ID=videoDetailVM.VideoInfo.aid,
                Episodes =new List<DownloadEpisodeItem>(),
                Subtitle= videoDetailVM.VideoInfo.bvid,
                Title=videoDetailVM.VideoInfo.title,
                Type= DownloadType.Video,
                UpMid = videoDetailVM.VideoInfo.owner.mid.ToInt32(),
            };
            int i = 0;
            foreach (var item in videoDetailVM.VideoInfo.pages)
            {
                //检查正在下载及下载完成是否存在此视频
                int state = 0;
                if (DownloadVM.Instance.Downloadings.FirstOrDefault(x=>x.EpisodeID==item.cid)!=null)
                {
                    state = 2;
                }
                if (DownloadVM.Instance.Downloadeds.FirstOrDefault(x => x.Epsidoes.FirstOrDefault(y=>y.CID==item.cid)!=null) != null)
                {
                    state = 3;
                }
                //如果正在下载state=2,下载完成state=3
                downloadItem.Episodes.Add(new DownloadEpisodeItem() {
                    AVID= videoDetailVM.VideoInfo.aid,
                    BVID= videoDetailVM.VideoInfo.bvid,
                    CID =item.cid,
                    EpisodeID="",
                    Index=i,
                    Title= "P" + item.page + " " + item.part,
                    State= state
                });
                i++;
            }
            
            DownloadDialog downloadDialog = new DownloadDialog(downloadItem);
            await downloadDialog.ShowAsync();
        }

        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (videoDetailVM.Loading) return;
            await InitializeVideo(_id);
        }

        private void btnOpenQR_Click(object sender, RoutedEventArgs e)
        {
            qrFlyout.ShowAt(btnMore);
        }
    }
}
