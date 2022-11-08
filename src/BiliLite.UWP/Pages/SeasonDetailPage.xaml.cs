using BiliLite.Controls;
using BiliLite.Dialogs;
using BiliLite.Helpers;
using BiliLite.Modules;
using BiliLite.Modules.Season;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
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

namespace BiliLite.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SeasonDetailPage : PlayPage
    {
        SeasonDetailVM seasonDetailVM;
        SeasonReviewVM seasonReviewVM;
        string season_id = "";
        string ep_id = "";
        bool selectProview = false;
        public SeasonDetailPage()
        {
            this.InitializeComponent();
            Title = "剧集详情";
            this.Loaded += SeasonDetailPage_Loaded;
            this.Player = this.player;
            NavigationCacheMode = NavigationCacheMode.Enabled;
            seasonDetailVM = new SeasonDetailVM();
            seasonReviewVM = new SeasonReviewVM();
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            RightInfo.Width = new GridLength(SettingHelper.GetValue<double>(SettingHelper.UI.RIGHT_DETAIL_WIDTH, 320), GridUnitType.Pixel);
        }

        private void SeasonDetailPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Parent is MyFrame)
            {
                (this.Parent as MyFrame).ClosedPage -= SeasonDetailPage_ClosedPage;
                (this.Parent as MyFrame).ClosedPage += SeasonDetailPage_ClosedPage;
            }
        }

        private void SeasonDetailPage_ClosedPage(object sender, EventArgs e)
        {
            ClosePage();
        }
        private void ClosePage()
        {
            if (seasonDetailVM != null)
            {
                seasonDetailVM.Loaded = false;
                seasonDetailVM.Loading = true;
                seasonDetailVM.Detail = null;
            }
            changedFlag = true;
            player?.FullScreen(false);
            player?.MiniWidnows(false);
            player?.Dispose();
        }
        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            request.Data.Properties.Title = seasonDetailVM.Detail.title;
            request.Data.SetWebLink(new Uri("http://b23.tv/ss" + season_id));
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New)
            {
                if (SettingHelper.GetValue<bool>(SettingHelper.Player.AUTO_FULL_SCREEN, false))
                {
                    player.IsFullScreen = true;
                }
                else
                {
                    player.IsFullWindow = SettingHelper.GetValue<bool>(SettingHelper.Player.AUTO_FULL_WINDOW, false);
                }
                pivot.SelectedIndex = SettingHelper.GetValue<int>(SettingHelper.UI.DETAIL_DISPLAY, 0);
                if (e.Parameter is object[])
                {
                    var pars = e.Parameter as object[];
                    season_id = pars[0].ToString();
                    ep_id = pars[1].ToString();
                }
                else
                {
                    season_id = e.Parameter.ToString();
                }

                await InitSeasonDetail();
            }
            else
            {
                Title = seasonDetailVM?.Detail?.title ?? "视频详情";
                MessageCenter.ChangeTitle(Title);
            }

        }

        private async Task InitSeasonDetail()
        {
            await seasonDetailVM.LoadSeasonDetail(season_id);

            if (seasonDetailVM.Detail != null)
            {
                ChangeTitle(seasonDetailVM.Detail.title);


                seasonReviewVM.MediaID = seasonDetailVM.Detail.media_id;

                InitializePlayInfo();
                CreateQR();

            }
        }

        private void InitializePlayInfo()
        {
            if (seasonDetailVM.NothingPlay) return;
            selectProview = !seasonDetailVM.ShowEpisodes;

            var index = 0;
            if (string.IsNullOrEmpty(ep_id) && seasonDetailVM.Detail.user_status?.progress != null)
            {
                ep_id = seasonDetailVM.Detail.user_status.progress.last_ep_id.ToString();
                SettingHelper.SetValue<double>("ep" + ep_id, Convert.ToDouble(seasonDetailVM.Detail.user_status.progress.last_time));
            }
            var selectItem = (selectProview?seasonDetailVM.Previews: seasonDetailVM.Episodes).FirstOrDefault(x => x.id.ToString() == ep_id);
            if (selectItem != null)
            {
                index = (selectProview ? seasonDetailVM.Previews : seasonDetailVM.Episodes).IndexOf(selectItem);
            }
            if (selectProview)
            {
                UpdatePlayInfoToPreview(index);
            }
            else
            {
                UpdatePlayInfoToEpisode(index);
            }
        }
        private void UpdatePlayInfoToEpisode(int index)
        {
            List<PlayInfo> playInfos = new List<PlayInfo>();
            int i = 0;
            foreach (var item in seasonDetailVM.Episodes)
            {
                playInfos.Add(new PlayInfo()
                {
                    avid = item.aid,
                    cid = item.cid,
                    duration = 0,
                    season_id = seasonDetailVM.Detail.season_id,
                    season_type = seasonDetailVM.Detail.type,
                    ep_id = item.id.ToString(),
                    is_vip = item.status != 2,
                    is_interaction = false,
                    order = i,
                    play_mode = VideoPlayType.Season,
                    title = item.title + " " + item.long_title,
                    area = Utils.ParseArea(seasonDetailVM.Detail.title, seasonDetailVM.Detail.up_info?.mid??0)
                });
                i++;
            }
            player.InitializePlayInfo(playInfos, index);
            comment.LoadComment(new LoadCommentInfo()
            {
                CommentMode = (int)Api.CommentApi.CommentType.Video,
                CommentSort = Api.CommentApi.CommentSort.Hot,
                Oid = playInfos[index].avid
            });
        }
        private void UpdatePlayInfoToPreview(int index)
        {
            List<PlayInfo> playInfos = new List<PlayInfo>();
            int i = 0;
            foreach (var item in seasonDetailVM.Previews)
            {
                playInfos.Add(new PlayInfo()
                {
                    avid = item.aid,
                    cid = item.cid,
                    duration = 0,
                    season_id = seasonDetailVM.Detail.season_id,
                    season_type = seasonDetailVM.Detail.type,
                    ep_id = item.id.ToString(),
                    is_vip = item.status != 2,
                    is_interaction = false,
                    order = i,
                    play_mode = VideoPlayType.Season,
                    title = item.title + " " + item.long_title,
                    area=Utils.ParseArea(seasonDetailVM.Detail.title, seasonDetailVM.Detail.up_info?.mid ?? 0)
                });
                i++;
            }
            player.InitializePlayInfo(playInfos, index);
            comment.LoadComment(new LoadCommentInfo()
            {
                CommentMode = (int)Api.CommentApi.CommentType.Video,
                CommentSort = Api.CommentApi.CommentSort.Hot,
                Oid = playInfos[index].avid
            });
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if(e.NavigationMode== NavigationMode.Back||e.SourcePageType.Name == "BlankPage")
            {
                ClosePage();
            }
            
            base.OnNavigatingFrom(e);
        }
        public void ChangeTitle(string title)
        {
            Title = title;
            if ((this.Parent as Frame).Parent is TabViewItem)
            {
                if (this.Parent != null)
                {
                    ((this.Parent as Frame).Parent as TabViewItem).Header = title;
                }
            }
            else
            {
                MessageCenter.ChangeTitle(title);
            }
        }
        private void btnShare_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }
        private void btnShareCopy_Click(object sender, RoutedEventArgs e)
        {
            Utils.SetClipboard($"{seasonDetailVM.Detail.title}\r\nhttp://b23.tv/ss{season_id}");
            Utils.ShowMessageToast("已复制内容到剪切板");
        }

        private void btnShareCopyUrl_Click(object sender, RoutedEventArgs e)
        {
            Utils.SetClipboard("http://b23.tv/ss" + season_id);
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
            var aid = "";
            if (selectProview)
            {
                listPreview.SelectedIndex = e;
                ep_id = seasonDetailVM.Previews[e].id.ToString();
                aid = seasonDetailVM.Previews[e].aid;
            }
            else
            {
                listEpisode.SelectedIndex = e;
                ep_id = seasonDetailVM.Episodes[e].id.ToString();
                aid = seasonDetailVM.Episodes[e].aid;
            }
           
            CreateQR();
            comment.LoadComment(new LoadCommentInfo()
            {
                CommentMode = (int)Api.CommentApi.CommentType.Video,
                CommentSort = Api.CommentApi.CommentSort.Hot,
                Oid = aid
            });
            changedFlag = false;
        }

        private void listEpisode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (changedFlag || listEpisode.SelectedIndex == -1)
            {
                return;
            }
            if (selectProview)
            {
                listPreview.SelectedIndex = -1;
                selectProview = false;
                UpdatePlayInfoToEpisode(listEpisode.SelectedIndex);
            }
            player.ChangePlayIndex(listEpisode.SelectedIndex);
            ep_id =seasonDetailVM.Episodes[listEpisode.SelectedIndex].id.ToString();
            comment.LoadComment(new LoadCommentInfo()
            {
                CommentMode = (int)Api.CommentApi.CommentType.Video,
                CommentSort = Api.CommentApi.CommentSort.Hot,
                Oid = seasonDetailVM.Episodes[listEpisode.SelectedIndex].aid
            });
            CreateQR();
        }
        private void listPreview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (changedFlag || listPreview.SelectedIndex == -1)
            {
                return;
            }
            if (!selectProview)
            {
                listEpisode.SelectedIndex = -1;
                selectProview = true;
                UpdatePlayInfoToPreview(listPreview.SelectedIndex);
            }
            player.ChangePlayIndex(listPreview.SelectedIndex);
            ep_id = seasonDetailVM.Episodes[listPreview.SelectedIndex].id.ToString();
            comment.LoadComment(new LoadCommentInfo()
            {
                CommentMode = (int)Api.CommentApi.CommentType.Video,
                CommentSort = Api.CommentApi.CommentSort.Hot,
                Oid = seasonDetailVM.Episodes[listPreview.SelectedIndex].aid
            });
            CreateQR();
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
                var data = barcodeWriter.Write("https://b23.tv/ep" + ep_id);
                imgQR.Source = data;
            }
            catch (Exception ex)
            {
                LogHelper.Log("创建二维码失败epid" + ep_id, LogType.ERROR, ex);
                Utils.ShowMessageToast("创建二维码失败");
            }

        }

        private void SeasonList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SeasonList.SelectedItem == null)
            {
                return;
            }
            var item = SeasonList.SelectedItem as SeasonDetailSeasonItemModel;
            if (item.season_id.ToString() != season_id)
            {
                
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Play,
                    page = typeof(SeasonDetailPage),
                    parameters = item.season_id,
                    title = item.title
                });
                //this.Frame.Navigate(typeof(SeasonDetailPage), item.season_id);
            }
        }

        private void btnOpenIndexWithArea_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as HyperlinkButton).DataContext as SeasonDetailAreaItemModel;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Filter,
                page = typeof(Bangumi.AnimeIndexPage),
                title = data.name,
                parameters = new SeasonIndexParameter()
                {
                    type = (IndexSeasonType)seasonDetailVM.Detail.type,
                    area = data.id
                }
            });
        }

        private void btnOpenIndexWithStyle_Click(object sender, RoutedEventArgs e)
        {

            var data = (sender as HyperlinkButton).DataContext as SeasonDetailStyleItemModel;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Filter,
                page = typeof(Bangumi.AnimeIndexPage),
                title = data.name,
                parameters = new SeasonIndexParameter()
                {
                    type = (IndexSeasonType)seasonDetailVM.Detail.type,
                    style = data.id
                }
            });
        }

        private async void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot.SelectedIndex == 3 && (seasonReviewVM.Items == null || seasonReviewVM.Items.Count==0))
            {
                await seasonReviewVM.GetItems();
            }
        }

        private void btnReviewLike_Click(object sender, RoutedEventArgs e)
        {
            var item= (sender as HyperlinkButton).DataContext as SeasonShortReviewItemModel;
            seasonReviewVM.Like(item);
        }

       
        private void btnReviewDislike_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as HyperlinkButton).DataContext as SeasonShortReviewItemModel;
            seasonReviewVM.Dislike(item);
        }

        private async void btnSendReview_Click(object sender, RoutedEventArgs e)
        {
            if (seasonReviewVM == null || seasonReviewVM.MediaID == 0) return;
            if (!SettingHelper.Account.Logined && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录后再操作");
                return;
            }
            
            SendReviewDialog sendReviewDialog = new SendReviewDialog(seasonReviewVM.MediaID);
            await sendReviewDialog.ShowAsync();
        }

        private async void btnOpenWeb_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("http://b23.tv/ss" + season_id));
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (seasonDetailVM.Detail?.cover == null) return;
            MessageCenter.OpenImageViewer(new List<string>() {
                seasonDetailVM.Detail.cover
            }, 0);
        }

        private async void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            if (seasonDetailVM.Detail == null || seasonDetailVM.Detail.episodes == null || seasonDetailVM.Detail.episodes.Count == 0) return;
            var downloadItem = new DownloadItem()
            {
                Cover = seasonDetailVM.Detail.cover,
               SeasonID= seasonDetailVM.Detail.season_id,
               SeasonType=seasonDetailVM.Detail.type,
                Episodes = new List<DownloadEpisodeItem>(),
                Subtitle = seasonDetailVM.Detail.subtitle,
                Title = seasonDetailVM.Detail.title,
                UpMid=seasonDetailVM.Detail.up_info?.mid??0,
                Type = DownloadType.Season
            };
            int i = 0;
            foreach (var item in seasonDetailVM.Detail.episodes)
            {
                // 检查正在下载及下载完成是否存在此视频
                int state = 0;
                if (DownloadVM.Instance.Downloadings.FirstOrDefault(x => x.EpisodeID == item.id.ToString()) != null)
                {
                    state = 2;
                }
                if (DownloadVM.Instance.Downloadeds.FirstOrDefault(x => x.Epsidoes.FirstOrDefault(y => y.EpisodeID == item.id.ToString()) != null) != null)
                {
                    state = 3;
                }

                //如果正在下载state=2,下载完成state=3
                downloadItem.Episodes.Add(new DownloadEpisodeItem()
                {
                    CID = item.cid,
                    EpisodeID = item.id.ToString(),
                    Index = i,
                    Title = item.title+" "+ item.long_title,
                    State = state,
                    AVID=item.aid,
                    BVID=item.bvid,
                    ShowBadge= item.show_badge,
                    Badge=item.badge,
                    IsPreview=item.IsPreview
                });
                i++;
            }

            DownloadDialog downloadDialog = new DownloadDialog(downloadItem);
            await downloadDialog.ShowAsync();
        }

        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (seasonDetailVM.Loading) return;
            await InitSeasonDetail();
        }

        private void btnOpenQR_Click(object sender, RoutedEventArgs e)
        {
            qrFlyout.ShowAt(btnMore);
        }
    }
}
