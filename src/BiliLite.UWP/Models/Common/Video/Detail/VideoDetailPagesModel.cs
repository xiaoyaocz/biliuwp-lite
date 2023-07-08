using Newtonsoft.Json;

namespace BiliLite.Models.Common.Video.Detail
{
    public class VideoDetailPagesModel
    {
        public string Cid { get; set; }

        public string Page { get; set; }

        public string Part { get; set; }

        public int Duration { get; set; }

        public string Dmlink { get; set; }

        [JsonProperty("download_title")]
        public string DownloadTitle { get; set; }

        [JsonProperty("download_subtitle")]
        public string DownloadSubtitle { get; set; }
    }
}