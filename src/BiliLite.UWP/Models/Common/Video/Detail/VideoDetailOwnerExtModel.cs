using Newtonsoft.Json;

namespace BiliLite.Models.Common.Video.Detail
{
    public class VideoDetailOwnerExtModel
    {
        /// <summary>
        /// 粉丝数
        /// </summary>
        public int Fans { get; set; }

        /// <summary>
        /// 认证信息
        /// </summary>
        [JsonProperty("official_verify")]
        public VideoDetailOwnerExtOfficialVerifyModel OfficialVerify { get; set; }
    }
}