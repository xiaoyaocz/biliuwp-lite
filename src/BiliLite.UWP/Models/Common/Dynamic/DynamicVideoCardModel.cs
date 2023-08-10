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

        public long Duration { get; set; }

        public string Dynamic { get; set; }

        [JsonProperty("jump_url")]
        public string JumpUrl { get; set; }

        public DynamicVideoCardOwnerModel Owner { get; set; }

        public string Pic { get; set; }

        public long Pubdate { get; set; }

        public string Title { get; set; }

        public long SeasonId { get; set; }

        public DynamicVideoCardStatModel Stat { get; set; }

        public string ViewCountText { get; set; }

        public string DanmakuCountText { get; set; }

        public string DisplayViewCountText => !string.IsNullOrEmpty(ViewCountText) ? ViewCountText : $"观看:{Stat.View}";

        public string DisplayDanmakuCountText => !string.IsNullOrEmpty(DanmakuCountText) ? DanmakuCountText : $"弹幕:{Stat.Danmaku}";
    }
}