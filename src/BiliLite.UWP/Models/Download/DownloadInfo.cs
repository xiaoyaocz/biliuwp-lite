using System.Collections.Generic;
using BiliLite.Models.Common;

namespace BiliLite.Models.Download
{
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
}