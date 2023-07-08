using Newtonsoft.Json;

namespace BiliLite.Models.Common.Video.Detail
{
    public class VideoDetailInteractionHistoryNodeModel
    {
        [JsonProperty("node_id")]
        public int NodeId { get; set; }

        public string Title { get; set; }

        public long Cid { get; set; }
    }
}