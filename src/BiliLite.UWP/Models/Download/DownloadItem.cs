using BiliLite.Models.Common;
using System.Collections.Generic;

namespace BiliLite.Models.Download
{
    public class DownloadItem
    {
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
        public DownloadType Type { get; set; }

        public List<DownloadEpisodeItem> Episodes { get; set; }
    }
}
