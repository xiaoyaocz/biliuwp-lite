using BiliLite.Api;
using BiliLite.Helpers;
using BiliLite.Modules;
using BiliLite.Modules.Player.Playurl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BiliLite.Dialogs
{
    public sealed partial class DownloadDialog : ContentDialog
    {
        PlayerVM playerVM;
        DownloadItem downloadItem;
        List<DownloadEpisodeItem> allEpisodes;
        public DownloadDialog(DownloadItem downloadItem)
        {
            this.InitializeComponent();
            allEpisodes = downloadItem.Episodes;
            if (downloadItem.Type== DownloadType.Season)
            {
                checkHidePreview.Visibility = Visibility.Visible;
            }
           
            this.downloadItem = downloadItem;
            playerVM = new PlayerVM(true);
            cbVideoType.SelectedIndex = SettingHelper.GetValue<int>(SettingHelper.Download.DEFAULT_VIDEO_TYPE, 1);
            cbVideoType.Loaded += new RoutedEventHandler((sender, e) =>
            {
                cbVideoType.SelectionChanged += new SelectionChangedEventHandler((obj, args) =>
                {
                    SettingHelper.SetValue(SettingHelper.Download.DEFAULT_VIDEO_TYPE, cbVideoType.SelectedIndex);
                    LoadQuality();
                });
            });
          
            LoadQuality();

        }
        private async void LoadQuality()
        {
            var episode = downloadItem.Episodes.OrderByDescending(x=>x.Index).FirstOrDefault(x=>!x.IsPreview);
            if (episode == null)
            {
                episode = downloadItem.Episodes.OrderByDescending(x => x.Index).FirstOrDefault();
            }
            var data = await playerVM.GetPlayUrls(new Controls.PlayInfo()
            {
                avid = episode.AVID,
                cid = episode.CID,
                ep_id = episode.EpisodeID,
                play_mode = downloadItem.Type == Helpers.DownloadType.Season ? Controls.VideoPlayType.Season : Controls.VideoPlayType.Video,
                season_id = downloadItem.SeasonID,
                season_type = downloadItem.SeasonType,
                area=Utils.ParseArea(downloadItem.Title,downloadItem.UpMid)
            }, 0);
            if (!data.Success)
            {
                Utils.ShowMessageToast("读取可下载清晰度时失败：" + data.Message);
                return;
            }
            if (data.Qualites == null || data.Qualites.Count < 1)
            {
                return;
            }
            cbQuality.ItemsSource = data.Qualites;
            cbQuality.SelectedIndex = 0;
        }
        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
            bool hide = true;
            if (listView.SelectedItems == null || listView.SelectedItems.Count == 0)
            {
                Utils.ShowMessageToast("请选择要下载的剧集");
                return;
            }
            IsPrimaryButtonEnabled = false;
            foreach (DownloadEpisodeItem item in listView.SelectedItems)
            {
                if (item.State != 0 && item.State != 99)
                {
                    continue;
                }
                try
                {
                    item.State = 1;

                    var downloadInfo = new DownloadInfo()
                    {
                        CoverUrl = downloadItem.Cover,
                        DanmakuUrl = $"{ApiHelper.API_BASE_URL}/x/v1/dm/list.so?oid=" + item.CID,
                        EpisodeID =item.EpisodeID,
                        CID= item.CID,
                        AVID = downloadItem.ID,
                        SeasonID=downloadItem.SeasonID,
                        SeasonType=downloadItem.SeasonType,
                        Title = downloadItem.Title,
                        EpisodeTitle = item.Title,
                        Type = downloadItem.Type,
                        Index= item.Index
                    };
                    //读取视频信息
                    //读取视频字幕
                    var info = await playerVM.GetPlayInfo(item.AVID, item.CID);
                    if (info.subtitle != null && info.subtitle.subtitles != null && info.subtitle.subtitles.Count > 0)
                    {
                        downloadInfo.Subtitles = new List<DownloadSubtitleInfo>();
                        foreach (var subtitleItem in info.subtitle.subtitles)
                        {
                            downloadInfo.Subtitles.Add(new DownloadSubtitleInfo()
                            {
                                Name = subtitleItem.lan_doc,
                                Url = subtitleItem.subtitle_url
                            });
                        }
                    }
                    //读取视频地址
                    var playUrl = await playerVM.GetPlayUrls(new Controls.PlayInfo()
                    {
                        avid = item.AVID,
                        cid = item.CID,
                        ep_id = item.EpisodeID,
                        play_mode = downloadItem.Type == Helpers.DownloadType.Season ? Controls.VideoPlayType.Season : Controls.VideoPlayType.Video,
                        season_id = downloadItem.SeasonID,
                        season_type = downloadItem.SeasonType,
                        area = Utils.ParseArea(downloadItem.Title, downloadItem.UpMid)
                    }, qn: (cbQuality.SelectedItem as BiliPlayUrlInfo).QualityID);
                    if (!playUrl.Success)
                    {
                        item.State = 99;
                        item.ErrorMessage = playUrl.Message;
                        continue;
                    }
                    
                    downloadInfo.QualityID = playUrl.CurrentQuality.QualityID;
                    downloadInfo.QualityName = playUrl.CurrentQuality.QualityName;
                    downloadInfo.Urls = new List<DownloadUrlInfo>();
                    if (playUrl.CurrentQuality.PlayUrlType ==  Modules.Player.Playurl.BiliPlayUrlType.DASH)
                    {
                        var quality = playUrl.CurrentQuality;
                        var audio = playUrl.CurrentQuality.DashInfo.Audio;
                        var video = playUrl.CurrentQuality.DashInfo.Video;
                       
                        if (audio != null)
                        {
                            downloadInfo.Urls.Add(new DownloadUrlInfo()
                            {
                                FileName = "video.m4s",
                                HttpHeader = quality.GetHttpHeader(),
                                Url = video.Url
                            }); ;
                            downloadInfo.Urls.Add(new DownloadUrlInfo()
                            {
                                FileName = "audio.m4s",
                                HttpHeader = quality.GetHttpHeader(),
                                Url = audio.Url
                            });
                        }
                        else
                        {
                         
                            downloadInfo.Urls.Add(new DownloadUrlInfo()
                            {
                                FileName = "0.blv",
                                HttpHeader = quality.GetHttpHeader(),
                                Url = video.Url
                            });
                        }
                      
                    }
                    if (playUrl.CurrentQuality.PlayUrlType == Modules.Player.Playurl.BiliPlayUrlType.MultiFLV)
                    {
                        int i = 0;
                        foreach (var videoItem in playUrl.CurrentQuality.FlvInfo)
                        {
                            downloadInfo.Urls.Add(new DownloadUrlInfo()
                            {
                                FileName = $"{i}.blv",
                                HttpHeader = playUrl.CurrentQuality.GetHttpHeader(),
                                Url = videoItem.Url
                            });
                        }
                    }
                    if (playUrl.CurrentQuality.PlayUrlType == Modules.Player.Playurl.BiliPlayUrlType.SingleFLV)
                    {
                        downloadInfo.Urls.Add(new DownloadUrlInfo()
                        {
                            FileName = "0.blv",
                            HttpHeader = playUrl.CurrentQuality.GetHttpHeader(),
                            Url = playUrl.CurrentQuality.FlvInfo.First().Url
                        });

                    }
                    //添加下载
                    await DownloadHelper.AddDownload(downloadInfo);

                    item.State = 2;
                }
                catch (Exception ex)
                {
                    hide = false;
                    item.State = 99;
                    item.ErrorMessage = ex.Message;
                }


            }
            DownloadVM.Instance.LoadDownloading();
            IsPrimaryButtonEnabled = true;
            if (hide)
            {
                Utils.ShowMessageToast("已添加至下载列表");
                this.Hide();
            }
            else
            {
                Utils.ShowMessageToast("有视频下载失败");
            }
           
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            DownloadVM.Instance.LoadDownloading();
            this.Hide();
        }

        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            listView.SelectAll();
        }

        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            listView.SelectedItems.Clear();
        }

        private void checkHidePreview_Checked(object sender, RoutedEventArgs e)
        {
            downloadItem.Episodes = allEpisodes.Where(x => !x.IsPreview).ToList();
        }

        private void checkHidePreview_Unchecked(object sender, RoutedEventArgs e)
        {
            downloadItem.Episodes = allEpisodes;
        }
    }

    public class DownloadItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void DoPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// 视频AVID
        /// </summary>
        public string ID { get; set; }

        public int SeasonID { get; set; }
        public int SeasonType { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Cover { get; set; }
        public long UpMid { get; set; } = 0;
        public Helpers.DownloadType Type { get; set; }
        private List<DownloadEpisodeItem> _episodes;

        public List<DownloadEpisodeItem> Episodes
        {
            get { return _episodes; }
            set { _episodes = value; DoPropertyChanged("Episodes"); }
        }

    }
    public class DownloadEpisodeItem : INotifyPropertyChanged
    {
        public int Index { get; set; }
        public string EpisodeID { get; set; }
        public string AVID { get; set; }
        public string BVID { get; set; }
        public string CID { get; set; }
        public string Title { get; set; }
        public bool ShowBadge { get; set; } = false;
        public string Badge { get; set; }

        private int _State = 0;
        /// <summary>
        /// 0=等待下载，1=读取视频链接中，2=正在下载，3=已下载，99=下载失败
        /// </summary>
        public int State
        {
            get { return _State; }
            set { _State = value; DoPropertyChanged("State"); }
        }

        private string _message;
        public string ErrorMessage
        {
            get { return _message; }
            set { _message = value; DoPropertyChanged("Message"); }
        }
        public bool IsPreview { get; set; } = false;

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void DoPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
