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
using BiliLite.Modules;
using Microsoft.UI.Xaml.Controls;
using BiliLite.Helpers;
using FFmpegInterop;
using Windows.Media.Playback;
using Windows.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.DataTransfer;
using BiliLite.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class VideoDetailPage : Page
    {
        VideoDetailVM videoDetailVM;
        string avid = "";
        string bvid = "";
        bool is_bvid = false;
        public VideoDetailPage()
        {
            this.InitializeComponent();
            this.Loaded += VideoDetailPage_Loaded;
            videoDetailVM = new VideoDetailVM();
            this.DataContext = videoDetailVM;
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
         
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
            player?.Dispose();
        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            request.Data.Properties.Title = videoDetailVM.VideoInfo.title;
            request.Data.SetWebLink(new Uri(videoDetailVM.VideoInfo.short_link));
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                var id = e.Parameter.ToString();
                if (int.TryParse(id,out var aid))
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
                
                if (videoDetailVM.VideoInfo != null)
                {
                    avid = videoDetailVM.VideoInfo.aid;
                    ChangeTitle(videoDetailVM.VideoInfo.title);
                    CreateQR();
                    if (!string.IsNullOrEmpty(videoDetailVM.VideoInfo.redirect_url))
                    {
                        
                        var result=await MessageCenter.HandelSeasonID(videoDetailVM.VideoInfo.redirect_url);
                        if (!string.IsNullOrEmpty(result))
                        {
                            this.Frame.Navigate(typeof(SeasonDetailPage), result);
                            return;
                        }
                    }
                    List<PlayInfo> playInfos = new List<PlayInfo>();
                    int i = 0;
                    foreach (var item in videoDetailVM.VideoInfo.pages)
                    {
                        playInfos.Add(new PlayInfo() { 
                            avid= videoDetailVM.VideoInfo.aid,
                            cid=item.cid,
                            duration=item.duration,
                            is_interaction= videoDetailVM.VideoInfo.interaction!=null,
                            order=i,
                            play_mode= VideoPlayType.Video,
                            title="P"+item.page+" "+item.part
                        });
                        i++;
                    }
                    var index = 0;
                    if (videoDetailVM.VideoInfo.history!=null)
                    {
                        var history = videoDetailVM.VideoInfo.pages.FirstOrDefault(x => x.cid.Equals(videoDetailVM.VideoInfo.history.cid));
                        if (history != null)
                        {
                            SettingHelper.SetValue<double>(history.cid,Convert.ToDouble(videoDetailVM.VideoInfo.history.progress));
                            player.InitializePlayInfo(playInfos, videoDetailVM.VideoInfo.pages.IndexOf(history));
                        }
                    }
                    player.InitializePlayInfo(playInfos, index);
                    comment.LoadComment(new LoadCommentInfo()
                    {
                        commentMode = Api.CommentApi.CommentType.Video,
                        conmmentSort = Api.CommentApi.ConmmentSort.Hot,
                        oid = videoDetailVM.VideoInfo.aid
                    });
                }
            }
            
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
                LogHelper.Log("创建二维码失败avid"+avid, LogType.ERROR, ex);
                Utils.ShowMessageToast("创建二维码失败");
            }
            
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            player?.Dispose();
            videoDetailVM.Loaded = false;
            videoDetailVM.Loading = true;
            videoDetailVM.VideoInfo = null;
            changedFlag = true;
            base.OnNavigatingFrom(e);
        }
        public void ChangeTitle(string title)
        {
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
            MessageCenter.OpenNewWindow(this, new NavigationInfo()
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
            MessageCenter.OpenNewWindow(this, new NavigationInfo()
            {
                icon = Symbol.Tag,
                page = typeof(WebPage),
                parameters = "https://www.bilibili.com/tag/" + item.tag_id,
                title = item.tag_name
            });
        }

        private void btnShare_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }

  

        private void PlayerControl_FullScreenEvent(object sender, bool e)
        {
            if (e)
            {
                this.Margin = new Thickness(0, -40, 0, 0);
                RightInfo.Width = new GridLength(0, GridUnitType.Pixel);
                BottomInfo.Height = new GridLength(0, GridUnitType.Pixel);
            }
            else
            {
                this.Margin = new Thickness(0);
                RightInfo.Width = new GridLength(320, GridUnitType.Pixel);
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
                RightInfo.Width = new GridLength(320, GridUnitType.Pixel);
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
            if (changedFlag|| listEpisode.SelectedIndex == -1)
            {
                return;
            }
            player.ChangePlayIndex(listEpisode.SelectedIndex);
        }

        private void btnOpenUser_Click(object sender, RoutedEventArgs e)
        {
           var data= (sender as HyperlinkButton).DataContext;
            if(data is VideoDetailStaffModel)
            {
                MessageCenter.OpenNewWindow(this, new NavigationInfo()
                {
                    icon = Symbol.Contact,
                    title = (data as VideoDetailStaffModel).name,
                    page = typeof(UserInfoPage),
                    parameters = (data as VideoDetailStaffModel).mid
                });
            }
            else
            {
                MessageCenter.OpenNewWindow(this, new NavigationInfo()
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
            videoDetailVM.DoFavorite(new List<string>() {item.id }, avid);
        }

        private void BtnAddFavorite_Click(object sender, RoutedEventArgs e)
        {
            videoDetailVM.DoFavorite(videoDetailVM.MyFavorite.Where(x=>x.is_fav).Select(x=>x.id).ToList(),avid);
        }
    }
}
