using BiliLite.Controls;
using BiliLite.Helpers;
using BiliLite.Modules;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
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

namespace BiliLite.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SeasonDetailPage : Page
    {
        SeasonDetailVM seasonDetailVM;
        string season_id = "";
        string ep_id="";
        public SeasonDetailPage()
        {
            this.InitializeComponent();
            this.Loaded += SeasonDetailPage_Loaded;
            NavigationCacheMode = NavigationCacheMode.Disabled;
            seasonDetailVM = new SeasonDetailVM();
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            
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
            player?.Dispose();
        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            request.Data.Properties.Title = seasonDetailVM.Detail.title;
            request.Data.SetWebLink(new Uri("http://www.bilibili.com/ss" + season_id));
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
           
            if (e.NavigationMode == NavigationMode.New)
            {
              
                if(e.Parameter is object[])
                {
                   var pars= e.Parameter as object[];
                    season_id = pars[0].ToString();
                    ep_id = pars[1].ToString();
                }
                else
                {
                    season_id = e.Parameter.ToString();
                }
               
                await seasonDetailVM.LoadSeasonDetail(season_id);

                if (seasonDetailVM.Detail != null)
                {
                    ChangeTitle(seasonDetailVM.Detail.title);
                    
                    List<PlayInfo> playInfos = new List<PlayInfo>();
                    int i = 0;
                    foreach (var item in seasonDetailVM.Detail.episodes)
                    {
                        playInfos.Add(new PlayInfo()
                        {
                            avid = item.av_id,
                            cid = item.danmaku,
                            duration = 0,
                            season_id= seasonDetailVM.Detail.season_id,
                            season_type= seasonDetailVM.Detail.type,
                            ep_id= item.id.ToString(),
                            is_vip= item.status!=2,
                            is_interaction = false,
                            order = i,
                            play_mode = VideoPlayType.Season,
                            title =item.title+" "+item.long_title
                        });
                        i++;
                    }
                    var index = 0;
                    if (string.IsNullOrEmpty( ep_id)&& seasonDetailVM.Detail.user_status.progress != null)
                    {
                        ep_id = seasonDetailVM.Detail.user_status.progress.last_ep_id.ToString();
                        SettingHelper.SetValue<double>("ep"+ ep_id, Convert.ToDouble(seasonDetailVM.Detail.user_status.progress.last_time));
                    }
                    var selectItem = playInfos.FirstOrDefault(x => x.ep_id == ep_id);
                    if (selectItem != null)
                    {
                        index = playInfos.IndexOf(selectItem);
                    }
                    player.InitializePlayInfo(playInfos, index);
                    CreateQR();
                    if (playInfos.Count!=0)
                    {
                        comment.LoadComment(new LoadCommentInfo()
                        {
                            commentMode = Api.CommentApi.CommentType.Video,
                            conmmentSort = Api.CommentApi.ConmmentSort.Hot,
                            oid = playInfos[index].avid
                        });
                    }
                }
            }
           
        }
       

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            player?.Dispose();
            seasonDetailVM.Loaded = false;
            seasonDetailVM.Loading = true;
            seasonDetailVM.Detail = null;
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
                RightInfo.Width = new GridLength(0.3, GridUnitType.Star);
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
                RightInfo.Width = new GridLength(0.3, GridUnitType.Star);
                BottomInfo.Height = GridLength.Auto;
            }
        }
        bool changedFlag = false;
        private void PlayerControl_ChangeEpisodeEvent(object sender, int e)
        {
            changedFlag = true;
            listEpisode.SelectedIndex = e;
            ep_id = seasonDetailVM.Detail.episodes[listEpisode.SelectedIndex].id.ToString();
            CreateQR();
            comment.LoadComment(new LoadCommentInfo()
            {
                commentMode = Api.CommentApi.CommentType.Video,
                conmmentSort = Api.CommentApi.ConmmentSort.Hot,
                oid = seasonDetailVM.Detail.episodes[listEpisode.SelectedIndex].av_id
            });
            changedFlag = false;
        }

        private void listEpisode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (changedFlag || listEpisode.SelectedIndex == -1)
            {
                return;
            }
            player.ChangePlayIndex(listEpisode.SelectedIndex);

            ep_id = seasonDetailVM.Detail.episodes[listEpisode.SelectedIndex].id.ToString();
            comment.LoadComment(new LoadCommentInfo()
            {
                commentMode = Api.CommentApi.CommentType.Video,
                conmmentSort = Api.CommentApi.ConmmentSort.Hot,
                oid = seasonDetailVM.Detail.episodes[listEpisode.SelectedIndex].av_id
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
            if (item.season_id.ToString()!=season_id)
            {
                this.Frame.Navigate(typeof(SeasonDetailPage), item.season_id);
            }
        }

        private void btnOpenIndexWithArea_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as HyperlinkButton).DataContext as SeasonDetailAreaItemModel;
            MessageCenter.OpenNewWindow(this, new NavigationInfo()
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
            MessageCenter.OpenNewWindow(this, new NavigationInfo()
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
    }
}
