using Newtonsoft.Json;

namespace BiliLite.Models.Common.Account
{
    public class WebInterfaceNav
    {
        [JsonProperty("wbi_img")]
        public WbiData WbiImg { get; set; }
    }

    public class WbiData
    {
        [JsonProperty("img_url")]
        public string ImgUrl { get; set; }

        [JsonProperty("sub_url")]
        public string SubUrl { get; set; }
    }
}
