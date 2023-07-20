using Newtonsoft.Json;

namespace BiliLite.Models.Common.User
{
    public class SubmitVideoCursorItem
    {
        public string Title { get; set; }

        public string Cover { get; set; }

        // 需要判断是否番剧url，是则map to RedirectUrl
        public string Uri { get; set; }

        public long Duration { get; set; }

        public long Play { get; set; }

        public long Danmaku { get; set; }

        [JsonProperty("ctime")]
        public long CTime { get; set; }

        public string Author { get; set; }

        [JsonProperty("bvid")]
        public string BvId { get; set; }

        [JsonProperty("param")]
        public string Aid { get; set; }

        public string RedirectUrl => Uri.Contains("bangumi") ? Uri : "";
    }
}
