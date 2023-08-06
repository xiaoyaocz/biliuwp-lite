using BiliLite.Modules;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Dynamic
{
    public class DynamicVideoCardModel
    {
        public string Aid { get; set; }

        public int Attribute { get; set; }

        public string Cid { get; set; }

        public long Ctime { get; set; }

        public string Desc { get; set; }

        public int Duration { get; set; }

        public string Dynamic { get; set; }

        [JsonProperty("jump_url")]
        public string JumpUrl { get; set; }

        public DynamicVideoCardOwnerModel Owner { get; set; }

        public string Pic { get; set; }

        public long Pubdate { get; set; }

        public string Title { get; set; }

        public DynamicVideoCardStatModel Stat { get; set; }
    }
}