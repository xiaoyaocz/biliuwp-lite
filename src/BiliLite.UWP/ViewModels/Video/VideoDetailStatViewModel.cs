using BiliLite.ViewModels.Common;
using Newtonsoft.Json;

namespace BiliLite.ViewModels.Video
{
    public class VideoDetailStatViewModel : BaseViewModel
    {
        public string Aid { get; set; }

        /// <summary>
        /// 播放
        /// </summary>
        public int View { get; set; }

        /// <summary>
        /// 弹幕
        /// </summary>
        public int Danmaku { get; set; }

        /// <summary>
        /// 评论
        /// </summary>
        public int Reply { get; set; }
        
        /// <summary>
        /// 收藏
        /// </summary>
        public int Favorite { get; set; }
        
        /// <summary>
        /// 投币
        /// </summary>
        public int Coin { get; set; }
        
        /// <summary>
        /// 分享
        /// </summary>
        public int Share { get; set; }
        
        /// <summary>
        /// 点赞
        /// </summary>
        public int Like { get; set; }

        /// <summary>
        /// 不喜欢，固定0
        /// </summary>
        [JsonProperty("dislike")]
        public int DisLike { get; set; }
    }
}