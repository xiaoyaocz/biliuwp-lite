using BiliLite.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace BiliLite.Modules
{
    public class DownloadVM : IModules
    {
        private static DownloadVM _downloadVM;
        public static DownloadVM Instance
        {
            get
            {
                if (_downloadVM == null)
                {
                    _downloadVM = new DownloadVM();
                }
                return _downloadVM;
            }
        }
        private ObservableCollection<DownloadedItem> _downloaded;
        public ObservableCollection<DownloadedItem> Downloadeds
        {
            get { return _downloaded; }
            set { _downloaded = value; DoPropertyChanged("Downloadeds"); }
        }

        private ObservableCollection<DownloadingItem> _downloading;
        public ObservableCollection<DownloadingItem> Downloadings
        {
            get { return _downloading; }
            set { _downloading = value; DoPropertyChanged("Downloadings"); }
        }

        public DownloadVM()
        {
            Downloadeds = new ObservableCollection<DownloadedItem>();
            Downloadings = new ObservableCollection<DownloadingItem>();

            RefreshDownloadedCommand = new RelayCommand(RefreshDownloaded);
            PauseItemCommand = new RelayCommand<DownloadingSubItem>(PauseItem);
            ResumeItemCommand = new RelayCommand<DownloadingSubItem>(ResumeItem);
            DeleteItemCommand = new RelayCommand<DownloadingItem>(DeleteItem);
            PauseCommand = new RelayCommand(PauseAll);
            StartCommand = new RelayCommand(StartAll);
            DeleteCommand = new RelayCommand(DeleteAll);
        }
        public ICommand PauseCommand { get; private set; }
        public ICommand StartCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand DeleteItemCommand { get; private set; }
        public ICommand PauseItemCommand { get; private set; }
        public ICommand ResumeItemCommand { get; private set; }

        public ICommand RefreshDownloadedCommand { get; private set; }
        private bool _loadingDownloaded = true;
        public bool LoadingDownloaded
        {
            get { return _loadingDownloaded; }
            set { _loadingDownloaded = value; DoPropertyChanged("LoadingDownloaded"); }
        }

        private double _diskTotal;
        public double DiskTotal
        {
            get { return _diskTotal; }
            set { _diskTotal = value; DoPropertyChanged("DiskTotal"); }
        }
        private double _diskUse;
        public double DiskUse
        {
            get { return _diskUse; }
            set { _diskUse = value; DoPropertyChanged("DiskUse"); }
        }
        private double _diskFree;
        public double DiskFree
        {
            get { return _diskFree; }
            set { _diskFree = value; DoPropertyChanged("DiskFree"); }
        }

        public void RefreshDownloaded()
        {
            if (LoadingDownloaded)
            {
                return;
            }

            LoadDownloaded();
        }
        /// <summary>
        /// 读取下载的视频
        /// </summary>
        /// <returns></returns>
        public async void LoadDownloaded()
        {

            LoadingDownloaded = true;
            Downloadeds.Clear();
            var folder = await GetDownloadFolder();
            await LoadDiskSize(folder);
            // var list = new List<DownloadedItem>();
            foreach (var item in await folder.GetFoldersAsync())
            {
                try
                {
                    //检查是否存在info.json
                    var infoFile = await item.TryGetItemAsync("info.json") as StorageFile;
                    if (infoFile == null)
                    {
                        continue;
                    }
                    var info = JsonConvert.DeserializeObject<DownloadSaveInfo>(await FileIO.ReadTextAsync(infoFile));
                    //旧版无Cover字段，跳过
                    if (string.IsNullOrEmpty(info.Cover))
                    {
                        continue;
                    }
                    List<DownloadedSubItem> lsEpisodes = new List<DownloadedSubItem>();
                    DownloadedItem downloadedItem = new DownloadedItem()
                    {
                        CoverPath = Path.Combine(item.Path, "cover.jpg"),
                        Epsidoes = new ObservableCollection<DownloadedSubItem>(),
                        ID = info.ID,
                        Title = info.Title,
                        UpdateTime = infoFile.DateCreated.LocalDateTime,
                        IsSeason = info.Type == DownloadType.Season,
                        Path = item.Path
                    };
                    var coverFile = await item.TryGetItemAsync("cover.jpg") as StorageFile;
                    if (coverFile != null)
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        var buffer = await FileIO.ReadBufferAsync(coverFile);
                        using (IRandomAccessStream stream = new InMemoryRandomAccessStream())
                        {
                            await stream.WriteAsync(buffer);
                            stream.Seek(0);
                            bitmapImage.SetSource(stream);
                            downloadedItem.Cover = bitmapImage;

                        }

                    }

                    foreach (var episodeItem in await item.GetFoldersAsync())
                    {
                        //检查是否存在info.json
                        var episodeInfoFile = await episodeItem.TryGetItemAsync("info.json") as StorageFile;
                        if (episodeInfoFile == null)
                        {
                            continue;
                        }
                        var files = (await episodeItem.GetFilesAsync()).Where(x => x.FileType == ".blv" || x.FileType == ".mp4" || x.FileType == ".m4s");
                        if (files.Count() == 0)
                        {
                            continue;
                        }
                        bool flag = false;
                        foreach (var subfile in files)
                        {
                            var size = (await subfile.GetBasicPropertiesAsync()).Size;
                            if (size == 0)
                            {
                                flag = true;
                                break;
                            }
                        }
                        files = null;
                        if (flag)
                        {
                            continue;
                        }

                        var episodeInfo = JsonConvert.DeserializeObject<DownloadSaveEpisodeInfo>(await FileIO.ReadTextAsync(episodeInfoFile));
                        var downloadedSubItem = new DownloadedSubItem()
                        {
                            AVID = episodeInfo.AVID,
                            CID = episodeInfo.CID,
                            Index = episodeInfo.Index,
                            EpisodeID = episodeInfo.EpisodeID,
                            IsDash = episodeInfo.IsDash,
                            Paths = new List<string>(),
                            Title = episodeInfo.EpisodeTitle,
                            SubtitlePath = new List<DownloadSubtitleInfo>(),
                            Path = episodeItem.Path
                        };
                        //设置视频
                        foreach (var path in episodeInfo.VideoPath)
                        {
                            downloadedSubItem.Paths.Add(Path.Combine(episodeItem.Path, path));
                        }
                        if (!string.IsNullOrEmpty(episodeInfo.DanmakuPath))
                        {
                            downloadedSubItem.DanmakuPath = Path.Combine(episodeItem.Path, episodeInfo.DanmakuPath);
                        }
                        if (episodeInfo.SubtitlePath != null)
                        {
                            foreach (var subtitle in episodeInfo.SubtitlePath)
                            {
                                downloadedSubItem.SubtitlePath.Add(new DownloadSubtitleInfo()
                                {
                                    Name = subtitle.Name,
                                    Url = Path.Combine(episodeItem.Path, subtitle.Url)
                                });
                            }
                        }

                        lsEpisodes.Add(downloadedSubItem);
                    }
                    //排序
                    foreach (var episode in lsEpisodes.OrderBy(x => x.Index))
                    {
                        downloadedItem.Epsidoes.Add(episode);
                    }

                    Downloadeds.Add(downloadedItem);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    continue;
                }


            }

            // list = list.OrderByDescending(x => x.UpdateTime).ToList();
            if (SettingHelper.GetValue(SettingHelper.Download.LOAD_OLD_DOWNLOAD, false))
            {
                await LoadDownloadedOld();
            }


            //foreach (var item in list)
            //{
            //    Downloadeds.Add(item);
            //}
            LoadingDownloaded = false;
        }
        /// <summary>
        /// 读取旧版下载的视频
        /// </summary>
        /// <returns></returns>
        public async Task LoadDownloadedOld()
        {

            var folder = await GetDownloadOldFolder();

            //var list = new List<DownloadedItem>();
            foreach (var item in await folder.GetFoldersAsync())
            {
                try
                {
                    //检查是否存在info.json
                    var infoFile = await item.TryGetItemAsync("info.json") as StorageFile;
                    if (infoFile == null)
                    {
                        continue;
                    }
                  
                    var info = JObject.Parse(await FileIO.ReadTextAsync(infoFile));
                    //新版下载无thumb字段
                    if (!info.ContainsKey("thumb"))
                    {
                        continue;
                    }
                    List<DownloadedSubItem> lsEpisodes = new List<DownloadedSubItem>();
                    DownloadedItem downloadedItem = new DownloadedItem()
                    {
                        CoverPath = Path.Combine(item.Path, "thumb.jpg"),
                        Epsidoes = new ObservableCollection<DownloadedSubItem>(),
                        ID = info["id"].ToString(),
                        Title = info["title"].ToString(),
                        UpdateTime = infoFile.DateCreated.LocalDateTime,
                        IsSeason = info["type"].ToString() != "video",
                        Path = item.Path
                    };
                    var coverFile = await item.TryGetItemAsync("thumb.jpg") as StorageFile;
                    if (coverFile != null)
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.SetSource(await coverFile.GetScaledImageAsThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.VideosView));
                        downloadedItem.Cover = bitmapImage;
                    }

                    foreach (var episodeItem in await item.GetFoldersAsync())
                    {
                        //检查是否存在info.json
                        var episodeInfoFile = await episodeItem.TryGetItemAsync("info.json") as StorageFile;
                        if (episodeInfoFile == null)
                        {
                            continue;
                        }
                        var files = (await episodeItem.GetFilesAsync()).Where(x => x.FileType == ".blv" || x.FileType == ".flv" || x.FileType == ".mp4" || x.FileType == ".m4s");
                        if (files.Count() == 0)
                        {
                            continue;
                        }
                        bool flag = false;
                        foreach (var subfile in files)
                        {
                            var size = (await subfile.GetBasicPropertiesAsync()).Size;
                            if (size == 0)
                            {
                                flag = true;
                                break;
                            }
                        }

                        if (flag)
                        {
                            continue;
                        }

                        var episodeInfo = JObject.Parse(await FileIO.ReadTextAsync(episodeInfoFile));
                        var downloadedSubItem = new DownloadedSubItem()
                        {
                            AVID = "",
                            CID = episodeInfo["cid"].ToString(),
                            Index = episodeInfo["index"].ToInt32(),
                            EpisodeID = episodeInfo["epid"]?.ToString() ?? "",
                            IsDash = await episodeItem.TryGetItemAsync("video.m4s") != null,
                            Paths = new List<string>(),
                            Title = episodeInfo["title"].ToString(),
                            SubtitlePath = new List<DownloadSubtitleInfo>(),
                            Path = episodeItem.Path
                        };
                        //设置视频
                        foreach (var file in files)
                        {
                            downloadedSubItem.Paths.Add(file.Path);
                        }

                        files = null;
                        downloadedSubItem.DanmakuPath = Path.Combine(episodeItem.Path, downloadedSubItem.CID + ".xml");


                        lsEpisodes.Add(downloadedSubItem);
                    }
                    //排序
                    foreach (var episode in lsEpisodes.OrderBy(x => x.Index))
                    {
                        downloadedItem.Epsidoes.Add(episode);
                    }

                    Downloadeds.Add(downloadedItem);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    continue;
                }


            }

            // list = list.OrderByDescending(x => x.UpdateTime).ToList();

            //return list;

        }
        /// <summary>
        /// 读取磁盘可用空间
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public async Task LoadDiskSize(StorageFolder folder)
        {
            var properties = await folder.Properties.RetrievePropertiesAsync(new string[] { "System.FreeSpace", "System.Capacity" });
            DiskFree = (ulong)properties["System.FreeSpace"] / 1024d / 1024d / 1024d;
            DiskTotal = (ulong)properties["System.Capacity"] / 1024d / 1024d / 1024d;
            DiskUse = DiskTotal - DiskFree;
        }
        /// <summary>
        /// 下载目录
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// 旧版下载目录
        /// </summary>
        /// <returns></returns>
        private async static Task<StorageFolder> GetDownloadOldFolder()
        {
            var path = SettingHelper.GetValue(SettingHelper.Download.OLD_DOWNLOAD_PATH, SettingHelper.Download.DEFAULT_OLD_PATH);
            if (path == SettingHelper.Download.DEFAULT_OLD_PATH)
            {
                var folder = KnownFolders.VideosLibrary;
                return await folder.CreateFolderAsync("BiliBiliDownload", CreationCollisionOption.OpenIfExists);
            }
            else
            {
                return await StorageFolder.GetFolderFromPathAsync(path);
            }
        }

        private IDictionary<string, CancellationTokenSource> cts;
        List<DownloadOperation> downloadOperations;
        List<Task> handelList;
        /// <summary>
        /// 读取下载中
        /// </summary>
        public async void LoadDownloading()
        {
            cts = new Dictionary<string, CancellationTokenSource>();
            if (handelList == null)
            {
                handelList = new List<Task>();
            }
            if (downloadOperations == null)
            {
                downloadOperations = new List<DownloadOperation>();
            }
            ObservableCollection<DownloadingSubItem> subItems = new ObservableCollection<DownloadingSubItem>();
            Downloadings.Clear();
            var ls = await BackgroundDownloader.GetCurrentDownloadsForTransferGroupAsync(DownloadHelper.group);
            foreach (var item in ls)
            {
                CancellationTokenSource cancellationTokenSource = null;

                var data = SettingHelper.GetValue<DownloadGUIDInfo>(item.Guid.ToString(), null);
                if (data == null || data.CID == null) continue;
                if (cts.ContainsKey(data.CID))
                {
                    cancellationTokenSource = cts[data.CID];
                }
                else
                {
                    cancellationTokenSource = new CancellationTokenSource();
                    cts.Add(data.CID, cancellationTokenSource);
                }

                if (!downloadOperations.Contains(item))
                {
                    downloadOperations.Add(item);
                    handelList.Add(Handel(item, cancellationTokenSource));
                }

                subItems.Add(new DownloadingSubItem()
                {
                    ProgressBytes = item.Progress.BytesReceived,
                    TotalBytes = item.Progress.TotalBytesToReceive,
                    Progress = GetProgress(item.Progress.BytesReceived, item.Progress.TotalBytesToReceive),
                    Status = item.Progress.Status,
                    Title = data.Title,
                    FileName = data.FileName,
                    EpisodeTitle = data.EpisodeTitle,
                    Path = data.Path,
                    CID = data.CID,
                    GUID = data.GUID,
                    PauseItemCommand = PauseItemCommand,
                    ResumeItemCommand = ResumeItemCommand
                });
            }
            foreach (var item in subItems.GroupBy(x => x.CID))
            {
                ObservableCollection<DownloadingSubItem> items = new ObservableCollection<DownloadingSubItem>();
                foreach (var item2 in item)
                {
                    items.Add(item2);
                }
                Downloadings.Add(new DownloadingItem()
                {
                    EpisodeID = item.Key,
                    Items = items,
                    Title = item.FirstOrDefault().Title,
                    EpisodeTitle = item.FirstOrDefault().EpisodeTitle,
                    Path = item.FirstOrDefault().Path,
                    DeleteItemCommand = DeleteItemCommand,
                });
            }
            await Task.WhenAll(handelList);

        }
        private async Task Handel(DownloadOperation downloadOperation, CancellationTokenSource cancellationTokenSource)
        {
            bool success = true;
            try
            {

                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(DownloadProgress);
                if (cancellationTokenSource != null)
                {
                    await downloadOperation.AttachAsync().AsTask(cancellationTokenSource.Token, progressCallback);
                }
                else
                {
                    await downloadOperation.AttachAsync().AsTask(progressCallback);
                }

                //var ls = list_Downing.ItemsSource as ObservableCollection<DisplayModel>;
                RefreshDownloaded();

            }
            catch (TaskCanceledException)
            {
                success = false;
            }
            catch (Exception ex)
            {
                success = false;
                if (ex.Message.Contains("0x80072EF1") || ex.Message.Contains("0x80070002") || ex.Message.Contains("0x80004004"))
                {
                    return;
                }
                var guid = downloadOperation.Guid.ToString();
                var item = Downloadings.FirstOrDefault(x => x.Items.FirstOrDefault(y => y.GUID == guid) != null);
                await Utils.ShowDialog("下载出现问题", $"失败视频:{item.Title ?? ""} {item.EpisodeTitle ?? ""}\r\n" + ex.Message);
            }
            finally
            {
                RemoveItem(downloadOperation.Guid.ToString(), success);

            }
        }
        private void DownloadProgress(DownloadOperation op)
        {
            try
            {
                if (Downloadings == null || Downloadings.Count == 0)
                {
                    return;
                }
                var guid = op.Guid.ToString();

                var item = Downloadings.FirstOrDefault(x => x.Items.Count(y => y.GUID == guid) != 0);
                if (item != null)
                {
                    var subItem = item.Items.FirstOrDefault(x => x.GUID == guid);
                    if (subItem != null)
                    {
                        subItem.ProgressBytes = op.Progress.BytesReceived;
                        subItem.Status = op.Progress.Status;
                        subItem.TotalBytes = op.Progress.TotalBytesToReceive;
                        subItem.Progress = GetProgress(subItem.ProgressBytes, subItem.TotalBytes);
                    }
                }

            }
            catch (Exception)
            {
                return;
            }

        }

        private double GetProgress(ulong progress, ulong total)
        {
            if (total >= progress)
            {
                return ((double)progress / total) * 100;
            }
            else
            {
                return 0;
            }
        }
        private void RemoveItem(string guid, bool success = true)
        {
            try
            {
                if (Downloadings == null || Downloadings.Count == 0)
                {
                    return;
                }
                var item = Downloadings.FirstOrDefault(x => x.Items.Count(y => y.GUID == guid) != 0);
                if (item != null)
                {
                    if (item.Items.Count > 1)
                    {
                        var subItem = item.Items.FirstOrDefault(x => x.GUID == guid);
                        if (subItem != null)
                        {
                            item.Items.Remove(subItem);
                        }
                    }
                    else
                    {
                        if (success && SettingHelper.GetValue<bool>(SettingHelper.Download.SEND_TOAST, false))
                        {
                            Utils.ShowMessageToast("《" + item.Title + " " + item.EpisodeTitle + "》下载完成");
                        }
                        Downloadings.Remove(item);

                    }
                }

            }
            catch (Exception ex)
            {
            }

        }
        /// <summary>
        /// 暂停下载
        /// </summary>
        /// <param name="item"></param>
        private void PauseItem(DownloadingSubItem item)
        {
            try
            {
                var op = downloadOperations.FirstOrDefault(x => x.Guid.ToString().Equals(item.GUID));
                op.Pause();
            }
            catch (Exception)
            {
            }
        }
        /// <summary>
        /// 开始下载
        /// </summary>
        /// <param name="item"></param>
        private void ResumeItem(DownloadingSubItem item)
        {
            try
            {
                var op = downloadOperations.FirstOrDefault(x => x.Guid.ToString().Equals(item.GUID));
                op.Resume();
            }
            catch (Exception)
            {
            }

        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="item"></param>
        private async void DeleteItem(DownloadingItem data)
        {
            try
            {
                if (!await Utils.ShowDialog("取消任务", "确定要取消任务吗?"))
                {
                    return;
                }
                cts[data.EpisodeID].Cancel();
                var folder = await StorageFolder.GetFolderFromPathAsync(data.Path);
                await folder.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch (Exception)
            {
            }

        }

        private void StartAll()
        {
            if (Downloadings.Count == 0) return;
            foreach (var item in Downloadings)
            {
                foreach (var item1 in item.Items)
                {
                    ResumeItem(item1);
                }
            }
        }
        private void PauseAll()
        {
            if (Downloadings.Count == 0) return;
            foreach (var item in Downloadings)
            {
                foreach (var item1 in item.Items)
                {
                    PauseItem(item1);
                }
            }
        }
        private async void DeleteAll()
        {
            if (Downloadings.Count == 0) return;
            if (!await Utils.ShowDialog("取消任务", "确定要取消全部任务吗?"))
            {
                return;
            }
            foreach (var item in Downloadings.ToList())
            {
                try
                {
                    cts[item.EpisodeID].Cancel();
                    var folder = await StorageFolder.GetFolderFromPathAsync(item.Path);
                    await folder.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
                catch (Exception)
                {
                }

            }
        }
        public async void UpdateSetting()
        {
            var downList = await BackgroundDownloader.GetCurrentDownloadsForTransferGroupAsync(DownloadHelper.group);
            var parallelDownload = SettingHelper.GetValue<bool>(SettingHelper.Download.PARALLEL_DOWNLOAD, true);
            var allowCostNetwork = SettingHelper.GetValue<bool>(SettingHelper.Download.ALLOW_COST_NETWORK, false);
            //设置下载模式
            foreach (var item in downList)
            {
                if (parallelDownload)
                {
                    item.TransferGroup.TransferBehavior = BackgroundTransferBehavior.Parallel;
                }
                else
                {
                    item.TransferGroup.TransferBehavior = BackgroundTransferBehavior.Serialized;

                }
                item.CostPolicy = allowCostNetwork ? BackgroundTransferCostPolicy.Always : BackgroundTransferCostPolicy.UnrestrictedOnly;
            }

        }
    }
    /// <summary>
    /// 正在下载列表
    /// </summary>
    public class DownloadingItem
    {
        public ICommand DeleteItemCommand { get; set; }
        public string EpisodeID { get; set; }
        public string Title { get; set; }
        public string EpisodeTitle { get; set; }
        public string Path { get; set; }

        public BitmapImage Cover { get; set; }

        /// <summary>
        /// 正在下载的分段内容
        /// </summary>
        public ObservableCollection<DownloadingSubItem> Items { get; set; }
    }

    public class DownloadingSubItem : IModules
    {
        public string GUID { get; set; }
        /// <summary>
        /// 标题
        /// </summary>

        private BackgroundTransferStatus _status;
        /// <summary>
        /// 下载状态
        /// </summary>
        public BackgroundTransferStatus Status
        {
            get { return _status; }
            set { _status = value; DoPropertyChanged("Status"); DoPropertyChanged("StatusString"); }
        }

        private ulong _TotalBytes;
        /// <summary>
        /// 文件总大小
        /// </summary>
        public ulong TotalBytes
        {
            get { return _TotalBytes; }
            set
            {
                _TotalBytes = value;
                DoPropertyChanged("TotalBytes");
                DoPropertyChanged("ShowPause");
                DoPropertyChanged("ShowStart");
            }
        }
        private ulong _ProgressBytes;
        /// <summary>
        /// 已下载大小
        /// </summary>
        public ulong ProgressBytes
        {
            get { return _ProgressBytes; }
            set { _ProgressBytes = value; DoPropertyChanged("ProgressBytes"); }
        }

        private double _Progress = 0;

        public double Progress
        {
            get { return _Progress; }
            set { _Progress = value; DoPropertyChanged("Progress"); }
        }

        public string CID { get; set; }
        public string Title { get; set; }
        public string FileName { get; set; }
        public string EpisodeTitle { get; set; }
        public string Path { get; set; }
        public bool ShowPause { get { return Status == BackgroundTransferStatus.Running; } }
        public bool ShowStart { get { return Status != BackgroundTransferStatus.Running; } }
        public ICommand PauseItemCommand { get; set; }
        public ICommand ResumeItemCommand { get; set; }
    }
    /// <summary>
    /// 已下载的列表
    /// </summary>
    public class DownloadedItem
    {
        public bool IsSeason { get; set; }
        public string ID { get; set; }
        public string CoverPath { get; set; }
        public BitmapImage Cover { get; set; }

        public string Title { get; set; }
        public DateTime UpdateTime { get; set; }
        public ObservableCollection<DownloadedSubItem> Epsidoes { get; set; }
        public string Path { get; set; }
    }
    public class DownloadedSubItem
    {
        public string AVID { get; set; }
        public string CID { get; set; }
        public string EpisodeID { get; set; }
        public string Title { get; set; }
        public bool IsDash { get; set; }
        public int QualityID { get; set; }
        public string QualityName { get; set; }
        public List<string> Paths { get; set; }
        public string DanmakuPath { get; set; }
        public int Index { get; set; }
        public string Path { get; set; }
        public List<DownloadSubtitleInfo> SubtitlePath { get; set; }
    }
}
