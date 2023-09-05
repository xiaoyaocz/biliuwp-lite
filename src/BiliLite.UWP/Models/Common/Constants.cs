namespace BiliLite.Models.Common
{
    public static class Constants
    {
        /// <summary>
        /// 获取Cookie使用的域名
        /// </summary>
        public const string GET_COOKIE_DOMAIN = "https://bilibili.com";

        /// <summary>
        /// b站官网域名
        /// </summary>
        public const string BILIBILI_DOMAIN = "https://www.bilibili.com";

        /// <summary>
        /// 评论中匹配特殊文本正则表达式
        /// </summary>
        public const string COMMENT_SPECIAL_TEXT_REGULAR = @"\[(.*?)\]|https?:\/\/\S+|http?:\/\/\S+|\p{Cs}";

        public static class App
        {
            /// <summary>
            /// 透明图片
            /// </summary>
            public const string TRANSPARENT_IMAGE = "ms-appx:///Assets/MiniIcon/transparent.png";

            /// <summary>
            /// 个人认证图片
            /// </summary>
            public const string VERIFY_PERSONAL_IMAGE = "ms-appx:///Assets/Icon/verify0.png";

            /// <summary>
            /// 企业认证图片
            /// </summary>
            public const string VERIFY_OGANIZATION_IMAGE = "ms-appx:///Assets/Icon/verify1.png";

            /// <summary>
            /// 背景图片
            /// </summary>
            public const string BACKGROUND_IAMGE_URL = "ms-appx:///Assets/Image/background.jpg";
        }

        public static class Images
        {
            /// <summary>
            /// 榜单图标
            /// </summary>
            public const string RANK_ICON_IMAGE = "ms-appx:///Assets/Icon/榜单.png";

            /// <summary>
            /// 索引图标
            /// </summary>
            public const string INDEX_ICON_IMAGE = "ms-appx:///Assets/Icon/索引.png";

            /// <summary>
            /// 时间表图标
            /// </summary>
            public const string TIMELINE_ICON_IMAGE = "ms-appx:///Assets/Icon/时间表.png";

            /// <summary>
            /// 我的图标
            /// </summary>
            public const string MY_ICON_IMAGE = "ms-appx:///Assets/Icon/我的.png";
        }
    }
}
