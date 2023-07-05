using Newtonsoft.Json;

namespace BiliLite.Models.Common.Comment
{
    public class CommentMemberUserSailingCardbgFanModel
    {
        [JsonProperty("is_fan")]
        public int IsFan { get; set; }

        public int Number { get; set; }

        public string Color { get; set; }

        public string Name { get; set; }

        [JsonProperty("num_desc")]
        public string NumDesc { get; set; }
    }
}