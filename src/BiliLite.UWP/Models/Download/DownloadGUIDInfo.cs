using BiliLite.Models.Common;

namespace BiliLite.Models.Download
{
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