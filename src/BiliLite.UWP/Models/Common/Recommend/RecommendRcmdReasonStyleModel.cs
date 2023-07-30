using Newtonsoft.Json;

namespace BiliLite.Models.Common.Recommend
{
    public class RecommendRcmdReasonStyleModel
    {
        public string Text { get; set; }

        [JsonProperty("text_color")]
        public string TextColor { get; set; }

        [JsonProperty("bg_color")]
        public string BgColor { get; set; }

        [JsonProperty("border_color")]
        public string BorderColor { get; set; }

        [JsonProperty("text_color_night")]
        public string TextColorNight { get; set; }

        [JsonProperty("bg_color_night")]
        public string BgColorNight { get; set; }

        [JsonProperty("border_color_night")]
        public string BorderColorNight { get; set; }

        [JsonProperty("bg_style")]
        public int BgStyle { get; set; }
    }
}