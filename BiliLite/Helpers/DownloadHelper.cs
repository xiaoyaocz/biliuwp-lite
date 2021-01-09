using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

namespace BiliLite.Helpers
{
    public enum DownloadType
    {
        /// <summary>
        /// 视频
        /// </summary>
        Video = 0,
        /// <summary>
        /// 番剧、电影、电视剧等
        /// </summary>
        Season = 1,
        /// <summary>
        /// 音乐，暂不支持
        /// </summary>
        Music = 2,
        /// <summary>
        /// 课程，暂不支持
        /// </summary>
        Cheese = 3
    }
    
    public static class DownloadHelper
    {
        public static BackgroundTransferGroup group = BackgroundTransferGroup.CreateGroup("BiliDownlad");//下载组，方便管理
        public async static Task AddDownload(DownloadInfo downloadInfo)
        {
            //读取存储文件夹
            StorageFolder folder = await GetDownloadFolder();
            folder = await folder.CreateFolderAsync((downloadInfo.Type == DownloadType.Season ? ("ss"+ downloadInfo.SeasonID) : downloadInfo.AVID), CreationCollisionOption.OpenIfExists);
            StorageFolder episodeFolder = await folder.CreateFolderAsync(downloadInfo.Type == DownloadType.Season?downloadInfo.EpisodeID: downloadInfo.CID, CreationCollisionOption.OpenIfExists);
            //下载封面
            await DownloadCover(downloadInfo.CoverUrl, folder);
            //下载弹幕
            await DownloadDanmaku(downloadInfo.DanmakuUrl, episodeFolder);
            //下载字幕
            if (downloadInfo.Subtitles != null)
            {
                foreach (var item in downloadInfo.Subtitles)
                {
                    await DownloadSubtitle(item, episodeFolder);
                }
            }
           
            //下载视频
            foreach (var item in downloadInfo.Urls)
            {
                 DownloadVideo(downloadInfo,item, episodeFolder);
            }
            //保存文件
            await SaveInfo(downloadInfo,folder, episodeFolder);
        }
        private async static Task<StorageFolder> GetDownloadFolder()
        {
            var path = SettingHelper.GetValue(SettingHelper.Download.DOWNLOAD_PATH, SettingHelper.Download.DEFAULT_PATH);
            if (path == SettingHelper.Download.DEFAULT_PATH)
            {
                var folder = KnownFolders.VideosLibrary;
                return await folder.CreateFolderAsync("哔哩哔哩下载", CreationCollisionOption.OpenIfExists);
            }
            else
            {
                return await StorageFolder.GetFolderFromPathAsync(path);
            }
        }
        private async static Task DownloadCover(string url, StorageFolder folder)
        {
            try
            {
                var buffer = await HttpHelper.GetBuffer(url + "@200w.jpg");
                StorageFile file = await folder.CreateFileAsync("cover.jpg", CreationCollisionOption.OpenIfExists);
                await FileIO.WriteBufferAsync(file, buffer);
                //Path.Combine(path,"cover.jpg")

            }
            catch (Exception ex)
            {
                LogHelper.Log("封面下载失败:" + url, LogType.ERROR, ex);
            }
        }
        private async static Task DownloadDanmaku(string url, StorageFolder episodeFolder)
        {
            try
            {
                var buffer = await HttpHelper.GetBuffer(url);
                StorageFile file = await episodeFolder.CreateFileAsync("danmaku.xml", CreationCollisionOption.OpenIfExists);
                await FileIO.WriteBufferAsync(file, buffer);
                //Path.Combine(path,"cover.jpg")

            }
            catch (Exception ex)
            {
                LogHelper.Log("弹幕下载失败:" + url, LogType.ERROR, ex);
            }
        }
        private async static Task SaveInfo(DownloadInfo info, StorageFolder folder, StorageFolder episodeFolder)
        {
            try
            {
                DownloadSaveInfo downloadSaveInfo = new DownloadSaveInfo() { 
                    Cover=info.CoverUrl,
                    SeasonType=info.SeasonType,
                    Title=info.Title,
                    Type=info.Type,
                    ID= info.Type== DownloadType.Season? info.SeasonID.ToString():info.AVID
                };
                DownloadSaveEpisodeInfo downloadSaveEpisodeInfo = new DownloadSaveEpisodeInfo() {
                    CID = info.CID,
                    DanmakuPath = "danmaku.xml",
                    EpisodeID = info.EpisodeID,
                    EpisodeTitle = info.EpisodeTitle,
                    AVID = info.AVID,
                    SubtitlePath = new List<DownloadSubtitleInfo>(),
                    Index = info.Index,
                    VideoPath = new List<string>(),
                    QualityID=info.QualityID,
                    QualityName=info.QualityName
                };
                if (info.Subtitles != null)
                {
                    foreach (var item in info.Subtitles)
                    {
                        downloadSaveEpisodeInfo.SubtitlePath.Add(new DownloadSubtitleInfo() { 
                            Name=item.Name,
                            Url=item.Name+".json"
                        });
                    }
                }
                foreach (var item in info.Urls)
                {
                    downloadSaveEpisodeInfo.VideoPath.Add(item.FileName);
                }
                downloadSaveEpisodeInfo.IsDash= downloadSaveEpisodeInfo.VideoPath.FirstOrDefault(x => x.Contains(".m4s"))!=null;
                StorageFile file = await folder.CreateFileAsync("info.json", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(downloadSaveInfo));
                StorageFile episodeFile = await episodeFolder.CreateFileAsync("info.json", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(episodeFile, JsonConvert.SerializeObject(downloadSaveEpisodeInfo));

            }
            catch (Exception ex)
            {
                LogHelper.Log("文件保存失败:" + episodeFolder.Path, LogType.ERROR,ex);
            }
        }
        private async static Task DownloadSubtitle(DownloadSubtitleInfo subtitleInfo, StorageFolder episodeFolder)
        {
            try
            {
                var url = subtitleInfo.Url;
                if (!url.Contains("http:") || !url.Contains("https:"))
                {
                    url = "https:" + url;
                }
                var buffer = await HttpHelper.GetBuffer(url);
                StorageFile file = await episodeFolder.CreateFileAsync(subtitleInfo.Name + ".json", CreationCollisionOption.OpenIfExists);
                await FileIO.WriteBufferAsync(file, buffer);
                //Path.Combine(path,"cover.jpg")

            }
            catch (Exception ex)
            {
                LogHelper.Log($"字幕下载失败:{subtitleInfo.Name}={subtitleInfo.Url}", LogType.ERROR, ex);
            }
        }
        private async static void DownloadVideo(DownloadInfo downloadInfo, DownloadUrlInfo url, StorageFolder episodeFolder)
        {
            BackgroundDownloader downloader = new BackgroundDownloader();
            if (url.HttpHeader!=null)
            {
                foreach (var item in url.HttpHeader)
                {
                    downloader.SetRequestHeader(item.Key, item.Value);
                }
            }
            var parallelDownload = SettingHelper.GetValue<bool>(SettingHelper.Download.PARALLEL_DOWNLOAD, true);
            var allowCostNetwork = SettingHelper.GetValue<bool>(SettingHelper.Download.ALLOW_COST_NETWORK, false);
            //设置下载模式
            if (parallelDownload)
            {
                group.TransferBehavior = BackgroundTransferBehavior.Parallel;
            }
            else
            {
                group.TransferBehavior = BackgroundTransferBehavior.Serialized;
            }
            downloader.TransferGroup = group;


            //创建视频文件
           
            StorageFile file = await episodeFolder.CreateFileAsync(url.FileName, CreationCollisionOption.OpenIfExists);
            DownloadOperation downloadOp = downloader.CreateDownload(new Uri(url.Url), file);
            //设置下载策略
            if (allowCostNetwork)
            {
                downloadOp.CostPolicy = BackgroundTransferCostPolicy.Always;
            }
            else
            {
                downloadOp.CostPolicy = BackgroundTransferCostPolicy.UnrestrictedOnly;
            }
            var guid = downloadOp.Guid.ToString();
            SettingHelper.SetValue(guid, new DownloadGUIDInfo() { 
                CID= downloadInfo.CID,
                Path= episodeFolder.Path,
                EpisodeTitle= downloadInfo.EpisodeTitle,
                FileName=url.FileName,
                Title= downloadInfo.Title,
                GUID=guid,
                Type= downloadInfo.Type,
                ID = downloadInfo.Type== DownloadType.Video? downloadInfo.AVID: downloadInfo.SeasonID.ToString()
            });
           
            try
            {
                await downloadOp.StartAsync();
            }
            catch (Exception)
            {
            }

        }
    }
    public class DownloadInfo
    {
        public DownloadType Type { get; set; }
        public string AVID { get; set; }
        public int SeasonID { get; set; }
        public int SeasonType { get; set; }
        public string EpisodeID { get; set; }
        public string CID { get; set; }
        public string Title { get; set; }
        public string EpisodeTitle { get; set; }
        public int Index { get; set; }
        /// <summary>
        /// 下载链接
        /// </summary>
        public List<DownloadUrlInfo> Urls { get; set; }
        /// <summary>
        /// 封面
        /// </summary>
        public string CoverUrl { get; set; }
        /// <summary>
        /// 弹幕链接
        /// </summary>
        public string DanmakuUrl { get; set; }
        /// <summary>
        /// 字幕
        /// </summary>
        public List<DownloadSubtitleInfo> Subtitles { get; set; }
        public int QualityID { get; set; }
        public string QualityName { get; set; }

    }
    public class DownloadSubtitleInfo
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
    public class DownloadUrlInfo
    {
        /// <summary>
        /// 链接
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 保存文件名称
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// Http请求头
        /// </summary>
        public IDictionary<string, string> HttpHeader { get; set; }
    }


    public class DownloadSaveInfo
    {
        
        public DownloadType Type { get; set; }
        public string ID { get; set; }
        public int SeasonType { get; set; }
        public string Title { get; set; }
        public string Cover { get; set; }
     
    }
    public class DownloadSaveEpisodeInfo
    {
        public int Index { get; set; }
        public string AVID { get; set; }
        public string EpisodeID { get; set; }
        public string CID { get; set; }
        public string EpisodeTitle { get; set; }
        public List<string> VideoPath { get; set; }
        public string DanmakuPath { get; set; }
        public int QualityID { get; set; }
        public string QualityName { get; set; }
        public List<DownloadSubtitleInfo> SubtitlePath { get; set; }
        public bool IsDash { get; set; }
    }


    public class DownloadGUIDInfo
    {
        public string GUID { get; set; }
        public string CID { get; set; }
        public string ID { get; set; }
        public string FileName { get; set; }
        public string Title { get; set; }
        public string EpisodeTitle { get; set; }
        public string Path { get; set; }
        public DownloadType Type { get; set; }
    }
}
