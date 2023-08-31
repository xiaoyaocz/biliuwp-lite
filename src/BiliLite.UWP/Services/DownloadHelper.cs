using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using BiliLite.Models.Common;
using BiliLite.Models.Download;
using BiliLite.Extensions;

namespace BiliLite.Services
{
    public static class DownloadHelper
    {
        private static readonly ILogger logger = GlobalLogger.FromCurrentType();

        public static BackgroundTransferGroup group = BackgroundTransferGroup.CreateGroup("BiliDownlad");//下载组，方便管理
        public static async Task AddDownload(DownloadInfo downloadInfo)
        {
            //读取存储文件夹
            StorageFolder folder = await GetDownloadFolder();
            folder = await folder.CreateFolderAsync((downloadInfo.Type == DownloadType.Season ? ("ss" + downloadInfo.SeasonID) : downloadInfo.AVID), CreationCollisionOption.OpenIfExists);
            StorageFolder episodeFolder = await folder.CreateFolderAsync(downloadInfo.Type == DownloadType.Season ? downloadInfo.EpisodeID : downloadInfo.CID, CreationCollisionOption.OpenIfExists);
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
                DownloadVideo(downloadInfo, item, episodeFolder);
            }
            //保存文件
            await SaveInfo(downloadInfo, folder, episodeFolder);
        }
        private static async Task<StorageFolder> GetDownloadFolder()
        {
            var path = SettingService.GetValue(SettingConstants.Download.DOWNLOAD_PATH, SettingConstants.Download.DEFAULT_PATH);
            if (path == SettingConstants.Download.DEFAULT_PATH)
            {
                var folder = KnownFolders.VideosLibrary;
                return await folder.CreateFolderAsync("哔哩哔哩下载", CreationCollisionOption.OpenIfExists);
            }
            else
            {
                return await StorageFolder.GetFolderFromPathAsync(path);
            }
        }
        private static async Task DownloadCover(string url, StorageFolder folder)
        {
            try
            {
                var buffer = await (url + "@200w.jpg").GetBuffer();
                StorageFile file = await folder.CreateFileAsync("cover.jpg", CreationCollisionOption.OpenIfExists);
                await FileIO.WriteBufferAsync(file, buffer);
                //Path.Combine(path,"cover.jpg")

            }
            catch (Exception ex)
            {
                logger.Log("封面下载失败:" + url, LogType.Error, ex);
            }
        }
        private static async Task DownloadDanmaku(string url, StorageFolder episodeFolder)
        {
            try
            {
                var buffer = await url.GetBuffer();
                StorageFile file = await episodeFolder.CreateFileAsync("danmaku.xml", CreationCollisionOption.OpenIfExists);
                await FileIO.WriteBufferAsync(file, buffer);
                //Path.Combine(path,"cover.jpg")

            }
            catch (Exception ex)
            {
                logger.Log("弹幕下载失败:" + url, LogType.Error, ex);
            }
        }
        private static async Task SaveInfo(DownloadInfo info, StorageFolder folder, StorageFolder episodeFolder)
        {
            try
            {
                DownloadSaveInfo downloadSaveInfo = new DownloadSaveInfo()
                {
                    Cover = info.CoverUrl,
                    SeasonType = info.SeasonType,
                    Title = info.Title,
                    Type = info.Type,
                    ID = info.Type == DownloadType.Season ? info.SeasonID.ToString() : info.AVID
                };
                DownloadSaveEpisodeInfo downloadSaveEpisodeInfo = new DownloadSaveEpisodeInfo()
                {
                    CID = info.CID,
                    DanmakuPath = "danmaku.xml",
                    EpisodeID = info.EpisodeID,
                    EpisodeTitle = info.EpisodeTitle,
                    AVID = info.AVID,
                    SubtitlePath = new List<DownloadSubtitleInfo>(),
                    Index = info.Index,
                    VideoPath = new List<string>(),
                    QualityID = info.QualityID,
                    QualityName = info.QualityName
                };
                if (info.Subtitles != null)
                {
                    foreach (var item in info.Subtitles)
                    {
                        downloadSaveEpisodeInfo.SubtitlePath.Add(new DownloadSubtitleInfo()
                        {
                            Name = item.Name,
                            Url = item.Name + ".json"
                        });
                    }
                }
                foreach (var item in info.Urls)
                {
                    downloadSaveEpisodeInfo.VideoPath.Add(item.FileName);
                }
                downloadSaveEpisodeInfo.IsDash = downloadSaveEpisodeInfo.VideoPath.FirstOrDefault(x => x.Contains(".m4s")) != null;
                StorageFile file = await folder.CreateFileAsync("info.json", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(downloadSaveInfo));
                StorageFile episodeFile = await episodeFolder.CreateFileAsync("info.json", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(episodeFile, JsonConvert.SerializeObject(downloadSaveEpisodeInfo));

            }
            catch (Exception ex)
            {
                logger.Log("文件保存失败:" + episodeFolder.Path, LogType.Error, ex);
            }
        }
        private static async Task DownloadSubtitle(DownloadSubtitleInfo subtitleInfo, StorageFolder episodeFolder)
        {
            try
            {
                var url = subtitleInfo.Url;
                if (!url.Contains("http:") || !url.Contains("https:"))
                {
                    url = "https:" + url;
                }
                var buffer = await url.GetBuffer();
                StorageFile file = await episodeFolder.CreateFileAsync(subtitleInfo.Name + ".json", CreationCollisionOption.OpenIfExists);
                await FileIO.WriteBufferAsync(file, buffer);
                //Path.Combine(path,"cover.jpg")

            }
            catch (Exception ex)
            {
                logger.Log($"字幕下载失败:{subtitleInfo.Name}={subtitleInfo.Url}", LogType.Error, ex);
            }
        }
        private static async void DownloadVideo(DownloadInfo downloadInfo, DownloadUrlInfo url, StorageFolder episodeFolder)
        {
            BackgroundDownloader downloader = new BackgroundDownloader();
            if (url.HttpHeader != null)
            {
                foreach (var item in url.HttpHeader)
                {
                    downloader.SetRequestHeader(item.Key, item.Value);
                }
            }
            var parallelDownload = SettingService.GetValue<bool>(SettingConstants.Download.PARALLEL_DOWNLOAD, true);
            var allowCostNetwork = SettingService.GetValue<bool>(SettingConstants.Download.ALLOW_COST_NETWORK, false);
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
            SettingService.SetValue(guid, new DownloadGUIDInfo()
            {
                CID = downloadInfo.CID,
                Path = episodeFolder.Path,
                EpisodeTitle = downloadInfo.EpisodeTitle,
                FileName = url.FileName,
                Title = downloadInfo.Title,
                GUID = guid,
                Type = downloadInfo.Type,
                ID = downloadInfo.Type == DownloadType.Video ? downloadInfo.AVID : downloadInfo.SeasonID.ToString()
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
}
