using BiliLite.Controls;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Comment
{
    public class CommentMemberUserSailingModel
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public CommentMemberUserSailingPendantModel Pendant { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public CommentMemberUserSailingCardbgModel Cardbg { get; set; }
    }
}