using Newtonsoft.Json;

namespace BiliLite.Models.Common.Recommend
{
    public class RecommendItemArgsModel
    {
        [JsonProperty("up_id")]
        public string UpId { get; set; }

        [JsonProperty("up_name")]
        public string UpName { get; set; }

        public int Rid { get; set; }

        public int Tid { get; set; }

        public string Tname { get; set; }

        public string Rname { get; set; }

        public int Aid { get; set; }
    }
}