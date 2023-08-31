using Newtonsoft.Json;

namespace BiliLite.Models.Common.Recommend
{
    public class RecommendBannerItemModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Image { get; set; }


        public string Hash { get; set; }

        public string Uri { get; set; }

        [JsonProperty("request_id")]
        public string RequestId { get; set; }

        /// <summary>
        /// Server_type
        /// </summary>
        [JsonProperty("server_type")]
        public int ServerType { get; set; }

        /// <summary>
        /// Resource_id
        /// </summary>
        [JsonProperty("resource_id")]
        public int ResourceId { get; set; }

        /// <summary>
        /// Index
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Cm_mark
        /// </summary>
        [JsonProperty("cm_mark")]
        public int CmMark { get; set; }
    }
}