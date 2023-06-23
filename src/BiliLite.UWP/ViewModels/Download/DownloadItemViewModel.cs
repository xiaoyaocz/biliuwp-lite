using System.Collections.Generic;
using BiliLite.Models.Common;
using BiliLite.ViewModels.Common;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BiliLite.ViewModels.Download
{
    public class DownloadItemViewModel : BaseViewModel
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

        public List<DownloadEpisodeItemViewModel> Episodes { get; set; }

    }
}
