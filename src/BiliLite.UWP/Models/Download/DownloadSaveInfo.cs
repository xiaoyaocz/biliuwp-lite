using BiliLite.Models.Common;

namespace BiliLite.Models.Download
{
    public class DownloadSaveInfo
    {
        public DownloadType Type { get; set; }
        public string ID { get; set; }
        public int SeasonType { get; set; }
        public string Title { get; set; }
        public string Cover { get; set; }
     
    }
}