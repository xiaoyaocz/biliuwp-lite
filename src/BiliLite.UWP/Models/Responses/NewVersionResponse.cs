using Newtonsoft.Json;

namespace BiliLite.Models.Responses
{
    /// <summary>
    /// 新版本响应
    /// </summary>
    public class NewVersionResponse
    {
        /// <summary>
        /// 版本
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; set; }

        /// <summary>
        /// 版本描述
        /// </summary>
        [JsonProperty("version_desc")]
        public string VersionDesc { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        [JsonProperty("version_num")]
        public int VersionNum { get; set; }

        /// <summary>
        /// URL
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}