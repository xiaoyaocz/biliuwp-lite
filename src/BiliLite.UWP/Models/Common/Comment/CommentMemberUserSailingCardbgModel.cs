using Newtonsoft.Json;

namespace BiliLite.Models.Common.Comment
{
    public class CommentMemberUserSailingCardbgModel
    {
        public string Name { get; set; }

        public string Image { get; set; }

        [JsonProperty("jump_url")]
        public string JumpUrl { get; set; }

        public bool Show => Fan is { NumDesc: { } } && Fan.NumDesc != "";

        public CommentMemberUserSailingCardbgFanModel Fan { get; set; }
    }
}