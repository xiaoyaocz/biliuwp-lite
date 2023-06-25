namespace BiliLite.Models.Common.Video.PlayUrlInfos
{
    public class BiliDashPlayUrlInfo
    {
        /// <summary>
        /// 时长，秒
        /// </summary>
        public int Duration { get; set; }
        /// <summary>
        /// 最小缓冲时间
        /// </summary>
        public double MinBufferTime { get; set; } = 1.5;
        /// <summary>
        /// 视频
        /// </summary>
        public BiliDashItem Video { get; set; }
        /// <summary>
        /// 音频
        /// </summary>
        public BiliDashItem Audio { get; set; }

    }
}
