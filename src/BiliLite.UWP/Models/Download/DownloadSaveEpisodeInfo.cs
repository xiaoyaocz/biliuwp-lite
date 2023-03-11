using System.Collections.Generic;

namespace BiliLite.Models.Download
{
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
}