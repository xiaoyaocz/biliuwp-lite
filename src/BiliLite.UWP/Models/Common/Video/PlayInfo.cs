//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Models.Common.Video
{
    public class PlayInfo
    {
        /// <summary>
        /// 播放模式
        /// </summary>
        public VideoPlayType play_mode { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int order { get; set; }
        /// <summary>
        /// 专题ID
        /// </summary>
        public int season_id { get; set; }
        /// <summary>
        /// 专题类型
        /// </summary>
        public int season_type { get; set; }
        /// <summary>
        /// 专题分集ID
        /// </summary>
        public string ep_id { get; set; }
        /// <summary>
        /// 视频ID
        /// </summary>
        public string avid { get; set; }
        /// <summary>
        /// 必须，视频分集ID
        /// </summary>
        public string cid { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// 是否VIP
        /// </summary>
        public bool is_vip { get; set; }
        /// <summary>
        /// 是否互动视频
        /// </summary>
        public bool is_interaction { get; set; } = false;
        /// <summary>
        /// 互动视频分支ID
        /// </summary>
        public int node_id { get; set; } = 0;
        /// <summary>
        /// 时长（毫秒）
        /// </summary>
        public long duration { get; set; }
        public LocalPlayInfo LocalPlayInfo { get; set; }
        public object parameter { get; set; }

        public string area { get; set; } = "";
    }
}
