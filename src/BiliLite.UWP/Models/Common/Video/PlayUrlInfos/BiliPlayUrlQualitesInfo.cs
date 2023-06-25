using System.Collections.Generic;

namespace BiliLite.Models.Common.Video.PlayUrlInfos
{
    public class BiliPlayUrlQualitesInfo
    {
        public static BiliPlayUrlQualitesInfo Failure(string message) => new BiliPlayUrlQualitesInfo()
        {
            Success = false,
            Message = message,
        };
        /// <summary>
        /// 请求是否成功
        /// </summary>
        public bool Success { get; set; } = true;
        /// <summary>
        /// 请求失败附带信息
        /// </summary>
        public string Message { get; set; } = "";
        /// <summary>
        /// 清晰度列表
        /// </summary>
        public List<BiliPlayUrlInfo> Qualites { get; set; }

        /// <summary>
        /// 音频清晰度列表
        /// </summary>
        public List<BiliDashAudioPlayUrlInfo> AudioQualites { get; set; }

        /// <summary>
        /// 当前清晰度
        /// </summary>
        public BiliPlayUrlInfo CurrentQuality { get; set; }

        /// <summary>
        /// 当前音质
        /// </summary>
        public BiliDashAudioPlayUrlInfo CurrentAudioQuality { get; set; }
    }
}
