using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using BiliLite.Models.Common;
using BiliLite.Models.Common.UserDynamic;
using BiliLite.ViewModels.Common;
using PropertyChanged;

namespace BiliLite.Controls.Dynamic
{
    public class UserDynamicItemDisplayViewModel : BaseViewModel
    {
        private static readonly Dictionary<UserDynamicDisplayType, string> _dynamicTypeTooltipSuffixes;

        static UserDynamicItemDisplayViewModel()
        {
            _dynamicTypeTooltipSuffixes = new Dictionary<UserDynamicDisplayType, string>()
            {
                {UserDynamicDisplayType.Text,"的动态"},
                {UserDynamicDisplayType.Photo,"的图片"},
                {UserDynamicDisplayType.Video,"的视频"},
                {UserDynamicDisplayType.ShortVideo,"的短视频"},
                {UserDynamicDisplayType.Music,"的音频"},
                {UserDynamicDisplayType.Web,"的分享"},
                {UserDynamicDisplayType.Article,"的专栏"},
                {UserDynamicDisplayType.Live,"的直播"},
                {UserDynamicDisplayType.LiveShare,"的直播"},
            };
        }

        /// <summary>
        /// 类型
        /// </summary>
        [DoNotNotify]
        public int IntType { get; set; }

        [DoNotNotify]
        public UserDynamicDisplayType Type { get; set; }

        /// <summary>
        /// 是否显示稍后再看
        /// </summary>
        [DoNotNotify]
        public bool ShowWatchLater => Type == UserDynamicDisplayType.Video;

        /// <summary>
        /// 动态ID
        /// </summary>
        [DoNotNotify]
        public string DynamicID { get; set; }

        /// <summary>
        /// 评论ID 
        /// </summary>
        [DoNotNotify]
        public string ReplyID { get; set; }

        /// <summary>
        /// 评论类型
        /// </summary>
        [DoNotNotify]
        public int ReplyType { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        [DoNotNotify]
        public long Mid { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [DoNotNotify]
        public string UserName { get; set; }

        [DoNotNotify]
        public string Photo { get; set; }

        [DoNotNotify]
        public string Verify { get; set; } = Constants.App.TRANSPARENT_IMAGE;

        [DoNotNotify]
        public string Pendant { get; set; } = Constants.App.TRANSPARENT_IMAGE;

        /// <summary>
        /// 装扮
        /// </summary>
        [DoNotNotify]
        public string DecorateName { get; set; }

        [DoNotNotify]
        public string DecorateText { get; set; }

        [DoNotNotify]
        public string DecorateColor { get; set; }

        [DoNotNotify]
        public bool ShowDecorateText { get { return !string.IsNullOrEmpty(DecorateText); } }

        [DoNotNotify]
        public string DecorateImage { get; set; }

        [DoNotNotify]
        public bool ShowDecorate { get { return !string.IsNullOrEmpty(DecorateImage); } }

        /// <summary>
        /// 被转发用户名旁的文本
        /// </summary>
        [DoNotNotify]
        public string Tooltip
        {
            get
            {
                var success = _dynamicTypeTooltipSuffixes.TryGetValue(Type, out var suffix);
                return success ? suffix : "";
            }
        }

        /// <summary>
        /// 发表时间
        /// </summary>
        [DoNotNotify]
        public string Time { get; set; }

        /// <summary>
        /// 发表时间(绝对)
        /// </summary>
        [DoNotNotify]
        public string Datetime { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        [DoNotNotify]
        public RichTextBlock Content { get; set; }

        [DoNotNotify]
        public string ContentStr { get; set; }

        /// <summary>
        /// 显示内容
        /// </summary>
        [DoNotNotify]
        public bool ShowContent { get; set; } = true;
        
        /// <summary>
        /// 分享数量
        /// </summary>
        public int ShareCount { get; set; }
        
        /// <summary>
        /// 评价数量
        /// </summary>
        public int CommentCount { get; set; }

        /// <summary>
        /// 点赞数量
        /// </summary>
        public int LikeCount { get; set; }

        public bool Liked { get; set; }

        /// <summary>
        /// 显示在昵称左侧的TAG
        /// </summary>
        [DoNotNotify]
        public string TagName { get; set; } = "";

        [DoNotNotify]
        public bool ShowTag { get; set; } = false;

        /// <summary>
        /// 是否年费大会员，是的话显示粉色昵称
        /// </summary>
        [DoNotNotify]
        public bool IsYearVip { get; set; }

        [DoNotNotify]
        public UserDynamicItemDisplayOneRowInfo OneRowInfo { get; set; }

        [DoNotNotify]
        public UserDynamicItemDisplayShortVideoInfo ShortVideoInfo { get; set; }

        [DoNotNotify]
        public List<UserDynamicItemDisplayImageInfo> ImagesInfo { get; set; }

        /// <summary>
        /// 转发原动态信息
        /// </summary>
        [DoNotNotify]
        public List<UserDynamicItemDisplayViewModel> OriginInfo { get; set; }

        /// <summary>
        /// 是否转发
        /// </summary>
        [DoNotNotify]
        public bool IsRepost { get; set; } = false;

        /// <summary>
        /// 是否自己动态
        /// </summary>
        [DoNotNotify]
        public bool IsSelf { get; set; }

        [DoNotNotify]
        public string DynamicUrl { get { return $"https://t.bilibili.com/{DynamicID}"; } }

        [DoNotNotify]
        public UserDynamicItemDisplayCommands UserDynamicItemDisplayCommands { get; set; }
    }
}