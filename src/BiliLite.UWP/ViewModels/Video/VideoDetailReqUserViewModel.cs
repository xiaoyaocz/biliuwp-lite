using BiliLite.ViewModels.Common;
using Newtonsoft.Json;

namespace BiliLite.ViewModels.Video
{
    public class VideoDetailReqUserViewModel : BaseViewModel
    {
        /// <summary>
        /// 是否关注
        /// </summary>
        public int Attention { get; set; }

        /// <summary>
        /// 是否特别关注
        /// </summary>
        [JsonProperty("guest_attention")]
        public int GuestAttention { get; set; }
        
        /// <summary>
        /// 是否收藏
        /// </summary>
        public int Favorite { get; set; }
        
        /// <summary>
        /// 是否点赞
        /// </summary>
        public int Like { get; set; }
        
        /// <summary>
        /// 是否投币
        /// </summary>
        public int Coin { get; set; }

        /// <summary>
        /// 是否不喜欢
        /// </summary>
        public int Dislike { get; set; }
    }
}