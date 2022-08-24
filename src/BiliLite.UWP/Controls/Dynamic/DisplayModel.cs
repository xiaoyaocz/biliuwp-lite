using BiliLite.Helpers;
using BiliLite.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

namespace BiliLite.Controls.Dynamic
{
    public class DynamicItemDisplayModel : IModules
    {
        /// <summary>
        /// 类型
        /// </summary>
        public int IntType { get; set; }
        public DynamicDisplayType Type { get; set; }
        /// <summary>
        /// 是否显示稍后再看
        /// </summary>
        public bool ShowWatchLater { get { return Type == DynamicDisplayType.Video; } }
        /// <summary>
        /// 动态ID
        /// </summary>
        public string DynamicID { get; set; }
        /// <summary>
        /// 评论ID 
        /// </summary>
        public string ReplyID { get; set; }
        /// <summary>
        /// 评论类型
        /// </summary>
        public int ReplyType { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        public long Mid { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        public string Photo { get; set; }
        public string Verify { get; set; } = AppHelper.TRANSPARENT_IMAGE;
        public string Pendant { get; set; } = AppHelper.TRANSPARENT_IMAGE;

        /// <summary>
        /// 装扮
        /// </summary>
        public string DecorateName { get; set; }
        public string DecorateText { get; set; }
        public string DecorateColor { get; set; }
        public bool ShowDecorateText { get { return !string.IsNullOrEmpty(DecorateText); } }
        public string DecorateImage { get; set; }
        public bool ShowDecorate { get { return !string.IsNullOrEmpty(DecorateImage); } }
        /// <summary>
        /// 被转发用户名旁的文本
        /// </summary>
        public string Tooltip
        {
            get
            {
                switch (Type)
                {

                    case DynamicDisplayType.Text:
                        return "的动态";
                    case DynamicDisplayType.Photo:
                        return "的图片";
                    case DynamicDisplayType.Video:
                        return "的视频";
                    case DynamicDisplayType.ShortVideo:
                        return "的短视频";
                    case DynamicDisplayType.Music:
                        return "的音频";
                    case DynamicDisplayType.Web:
                        return "的分享";
                    case DynamicDisplayType.Article:
                        return "的专栏";
                    case DynamicDisplayType.Live:
                        return "的直播";
                    case DynamicDisplayType.LiveShare:
                        return "的直播";
                    default:
                        return "";
                }
            }
        }
        /// <summary>
        /// 发表时间
        /// </summary>
        public string Time { get; set; }
        /// <summary>
        /// 发表时间(绝对)
        /// </summary>
        public string Datetime { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public RichTextBlock Content { get; set; }
        public string ContentStr { get; set; }
        /// <summary>
        /// 显示内容
        /// </summary>
        public bool ShowContent { get; set; } = true;
        private int _shareCount;
        /// <summary>
        /// 分享数量
        /// </summary>
        public int ShareCount
        {
            get { return _shareCount; }
            set { _shareCount = value; DoPropertyChanged("ShareCount"); }
        }
        private int _commentCount;
        /// <summary>
        /// 评价数量
        /// </summary>
        public int CommentCount
        {
            get { return _commentCount; }
            set { _commentCount = value; DoPropertyChanged("CommentCount"); }
        }
        private int _likeCount;
        /// <summary>
        /// 点赞数量
        /// </summary>
        public int LikeCount
        {
            get { return _likeCount; }
            set { _likeCount = value; DoPropertyChanged("LikeCount"); }
        }
        private bool _Liked = false;

        public bool Liked
        {
            get { return _Liked; }
            set { _Liked = value; DoPropertyChanged("Liked"); }
        }

        /// <summary>
        /// 显示在昵称左侧的TAG
        /// </summary>
        public string TagName { get; set; } = "";
        public bool ShowTag { get; set; } = false;
        /// <summary>
        /// 打开用户个人中心
        /// </summary>
        public ICommand UserCommand { get; set; }
        /// <summary>
        /// 打开抽奖信息
        /// </summary>
        public ICommand LotteryCommand { get; set; }
        /// <summary>
        /// 打开链接
        /// </summary>
        public ICommand LaunchUrlCommand { get; set; }
        /// <summary>
        /// 打开网页
        /// </summary>
        public ICommand WebCommand { get; set; }
        /// <summary>
        /// 打开话题
        /// </summary>
        public ICommand TagCommand { get; set; }
        /// <summary>
        /// 打开投票信息
        /// </summary>
        public ICommand VoteCommand { get; set; }
        /// <summary>
        /// 查看图片信息
        /// </summary>
        public ICommand ImageCommand { get; set; }
        /// <summary>
        /// 点赞
        /// </summary>
        public ICommand LikeCommand { get; set; }
        /// <summary>
        /// 评论
        /// </summary>
        public ICommand CommentCommand { get; set; }
        /// <summary>
        /// 转发
        /// </summary>
        public ICommand RepostCommand { get; set; }
        /// <summary>
        /// 删除动态
        /// </summary>
        public ICommand DeleteCommand { get; set; }
        /// <summary>
        /// 打开详情页面
        /// </summary>
        public ICommand DetailCommand { get; set; }
        /// <summary>
        /// 添加到稍后再看
        /// </summary>
        public ICommand WatchLaterCommand { get; set; }
        /// <summary>
        /// 是否年费大会员，是的话显示粉色昵称
        /// </summary>
        public bool IsYearVip { get; set; }

        public DynamicItemDisplayOneRowInfo OneRowInfo { get; set; }
        public DyanmicItemDisplayShortVideoInfo ShortVideoInfo { get; set; }
        public List<DyanmicItemDisplayImageInfo> ImagesInfo { get; set; }
        /// <summary>
        /// 转发原动态信息
        /// </summary>
        public List<DynamicItemDisplayModel> OriginInfo { get; set; }
        /// <summary>
        /// 是否转发
        /// </summary>
        public bool IsRepost { get; set; } = false;
        /// <summary>
        /// 是否自己动态
        /// </summary>
        public bool IsSelf { get; set; }
        public string DynamicUrl { get { return $"https://t.bilibili.com/{DynamicID}"; } }
    }

    public class DynamicItemDisplayOneRowInfo
    {
        public string Cover { get; set; }
        public string Url { get; set; }
        public string CoverText { get; set; }
        public string CoverParameter { get; set; } = "412w_232h_1c";
        public double CoverWidth { get; set; } = 160;
        public bool ShowCoverText
        {
            get
            {
                return !string.IsNullOrEmpty(CoverText);
            }
        }
        public string Tag { get; set; }
        public bool ShowTag
        {
            get
            {
                return !string.IsNullOrEmpty(Tag);
            }
        }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Desc { get; set; }
        public object ID { get; set; }
        public string AID { get; set; }
    }

    public class DyanmicItemDisplayShortVideoInfo
    {
        public string UploadTime { get; set; }
        public string VideoPlayurl { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
    public class DyanmicItemDisplayImageInfo
    {
        public int Index { get; set; }
        public string ImageUrl { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public List<string> AllImages { get; set; }
        public ICommand ImageCommand { get; set; }
        public bool LongImage { get { return Height > (Width * 2); } }
        public string ImageUrlWithPar
        {
            get
            {
                if (Height > (Width * 2))
                {
                    return ImageUrl + "@240w_320h_!header.webp";
                }
                else if (Height > (Width * 1.3))
                {
                    return ImageUrl + "@240w_320h_1e_1c.jpg";
                }
                else
                {
                    return ImageUrl + "@400w.jpg";
                }
            }
        }
    }
}
