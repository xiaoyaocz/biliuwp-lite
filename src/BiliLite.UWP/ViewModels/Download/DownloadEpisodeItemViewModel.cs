using BiliLite.ViewModels.Common;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BiliLite.ViewModels.Download
{
    public class DownloadEpisodeItemViewModel : BaseViewModel
    {
        public int Index { get; set; }
        public string EpisodeID { get; set; }
        public string AVID { get; set; }
        public string BVID { get; set; }
        public string CID { get; set; }
        public string Title { get; set; }
        public bool ShowBadge { get; set; } = false;
        public string Badge { get; set; }

        /// <summary>
        /// 0=等待下载，1=读取视频链接中，2=正在下载，3=已下载，99=下载失败
        /// </summary>
        public int State { get; set; }

        public string ErrorMessage { get; set; }
        public bool IsPreview { get; set; } = false;
    }
}
