using Newtonsoft.Json;

namespace BiliLite.Models.Common.Video.Detail
{
    public class VideoDetailInteractionModel
    {
        [JsonProperty("graph_version")]
        public int GraphVersion { get; set; }

        [JsonProperty("history_node")]
        public VideoDetailInteractionHistoryNodeModel HistoryNode { get; set; }
    }
}