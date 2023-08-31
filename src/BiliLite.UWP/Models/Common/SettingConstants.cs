using System.Collections.Generic;

namespace BiliLite.Models.Common
{
    public static class SettingConstants
    {
        public class UI
        {
            /// <summary>
            /// 加载原图
            /// </summary>
            public const string ORTGINAL_IMAGE = "originalImage";

            /// <summary>
            /// 主题颜色
            /// </summary>
            public const string THEME_COLOR = "themeColor";

            /// <summary>
            /// 主题,0为默认，1为浅色，2为深色
            /// </summary>
            public const string THEME = "theme";

            /// <summary>
            /// 显示模式,0为多标签，1为单窗口，2为多窗口
            /// </summary>
            public const string DISPLAY_MODE = "displayMode";

            /// <summary>
            /// 缓存首页
            /// </summary>
            public const string CACHE_HOME = "cacheHome";

            /// <summary>
            /// 首页排序
            /// </summary>
            public const string HOEM_ORDER = "homePageOrder";

            /// <summary>
            /// 右侧详情宽度
            /// </summary>
            public const string RIGHT_DETAIL_WIDTH = "PlayerRightDetailWidth";

            /// <summary>
            /// 右侧详情宽度可调整
            /// </summary>
            public const string RIGHT_WIDTH_CHANGEABLE = "PlayerRightDetailWidthChangeable";

            /// <summary>
            /// 图片圆角半径
            /// </summary>
            public const string IMAGE_CORNER_RADIUS = "ImageCornerRadius";

            /// <summary>
            /// 视频详情显示封面
            /// </summary>
            public const string SHOW_DETAIL_COVER = "showDetailCover";
            /// <summary>
            /// 新窗口打开图片预览
            /// </summary>
            public const string NEW_WINDOW_PREVIEW_IMAGE = "newWindowPreviewImage";
            /// <summary>
            /// 动态显示样式
            /// </summary>
            public const string DYNAMIC_DISPLAY_MODE = "dynamicDiaplayMode";
            /// <summary>
            /// 首页推荐样式
            /// </summary>
            public const string RECMEND_DISPLAY_MODE = "recomendDiaplayMode";
            /// <summary>
            /// 右侧选项卡
            /// </summary>
            public const string DETAIL_DISPLAY = "detailDisplay";
            /// <summary>
            /// 动态显示样式
            /// </summary>
            public const string BACKGROUND_IMAGE = "BackgroundImage";
            /// <summary>
            /// 鼠标功能键行为
            /// </summary>
            public const string MOUSE_MIDDLE_ACTION = "MouseMiddleAction";
            /// <summary>
            /// 隐藏赞助按钮
            /// </summary>
            public const string HIDE_SPONSOR = "HideSponsor";
            /// <summary>
            /// 隐藏广告按钮
            /// </summary>
            public const string HIDE_AD = "HideAD";
            /// <summary>
            /// 浏览器打开无法处理的链接
            /// </summary>
            public const string OPEN_URL_BROWSER = "OpenUrlWithBrowser";

            /// <summary>
            /// 启用长评论折叠
            /// </summary>
            public const string ENABLE_COMMENT_SHRINK = "EnableCommentShrink";

            /// <summary>
            /// 折叠评论长度
            /// </summary>
            public const string COMMENT_SHRINK_LENGTH = "CommentShrinkLength";

            /// <summary>
            /// 默认折叠评论长度
            /// </summary>
            public const int COMMENT_SHRINK_DEFAULT_LENGTH = 75;
        }

        public class Account
        {
            /// <summary>
            /// 登录后ACCESS_KEY
            /// </summary>
            public const string ACCESS_KEY = "accesskey";
            /// <summary>
            /// 登录后REFRESH_KEY
            /// </summary>
            public const string REFRESH_KEY = "refreshkey";
            /// <summary>
            /// 到期时间
            /// </summary>
            public const string ACCESS_KEY_EXPIRE_DATE = "expireDate";
            /// <summary>
            /// 用户ID
            /// </summary>
            public const string USER_ID = "uid";
            /// <summary>
            /// 到期时间
            /// </summary>
            public const string USER_PROFILE = "userProfile";

            /// <summary>
            /// 是否web登录
            /// </summary>
            public const string IS_WEB_LOGIN = "isWebLogin";

            /// <summary>
            /// Cookies
            /// </summary>
            public const string BILIBILI_COOKIES = "BiliBiliCookies";
        }

        public class VideoDanmaku
        {
            /// <summary>
            /// 默认弹幕引擎
            /// </summary>
            public const DanmakuEngineType DEFAULT_DANMAKU_ENGINE = DanmakuEngineType.NSDanmaku;
            /// <summary>
            /// 弹幕引擎
            /// </summary>
            public const string DANMAKU_ENGINE = "DanmakuEngine";
            /// <summary>
            /// 显示弹幕 Visibility
            /// </summary>
            public const string SHOW = "VideoDanmuShow";
            /// <summary>
            /// 弹幕缩放 double
            /// </summary>
            public const string FONT_ZOOM = "VideoDanmuFontZoom";
            /// <summary>
            /// 弹幕显示区域
            /// </summary>
            public const string AREA = "VideoDanmuArea";
            /// <summary>
            /// 弹幕速度 int
            /// </summary>
            public const string SPEED = "VideoDanmuSpeed";
            /// <summary>
            /// 弹幕加粗 bool
            /// </summary>
            public const string BOLD = "VideoDanmuBold";
            /// <summary>
            /// 弹幕边框样式 int
            /// </summary>
            public const string BORDER_STYLE = "VideoDanmuStyle";
            /// <summary>
            /// 弹幕合并 bool
            /// </summary>
            public const string MERGE = "VideoDanmuMerge";
            /// <summary>
            /// 弹幕半屏显示 bool
            /// </summary>
            public const string DOTNET_HIDE_SUBTITLE = "VideoDanmuDotHide";
            /// <summary>
            /// 弹幕透明度 double，0-1
            /// </summary>
            public const string OPACITY = "VideoDanmuOpacity";
            /// <summary>
            /// 隐藏顶部 bool
            /// </summary>
            public const string HIDE_TOP = "VideoDanmuHideTop";
            /// <summary>
            /// 隐藏底部 bool
            /// </summary>
            public const string HIDE_BOTTOM = "VideoDanmuHideBottom";
            /// <summary>
            /// 隐藏滚动 bool
            /// </summary>
            public const string HIDE_ROLL = "VideoDanmuHideRoll";
            /// <summary>
            /// 隐藏高级弹幕 bool
            /// </summary>
            public const string HIDE_ADVANCED = "VideoDanmuHideAdvanced";

            /// <summary>
            /// 关键词屏蔽 ObservableCollection<string>
            /// </summary>
            public const string SHIELD_WORD = "VideoDanmuShieldWord";

            /// <summary>
            /// 用户屏蔽 ObservableCollection<string>
            /// </summary>
            public const string SHIELD_USER = "VideoDanmuShieldUser";

            /// <summary>
            /// 正则屏蔽 ObservableCollection<string>
            /// </summary>
            public const string SHIELD_REGULAR = "VideoDanmuShieldRegular";

            /// <summary>
            /// 顶部距离
            /// </summary>
            public const string TOP_MARGIN = "VideoDanmuTopMargin";
            /// <summary>
            /// 最大数量
            /// </summary>
            public const string MAX_NUM = "VideoDanmuMaxNum";
            /// <summary>
            /// 弹幕云屏蔽等级
            /// </summary>
            public const string SHIELD_LEVEL = "VideoDanmuShieldLevel";
        }

        public class Live
        {
            /// <summary>
            /// 直播默认清晰度
            /// </summary>
            public const string DEFAULT_QUALITY = "LiveDefaultQuality";
            /// <summary>
            /// 显示弹幕 Visibility
            /// </summary>
            public const string SHOW = "LiveDanmuShow";
            public const string AREA = "LiveDanmuArea";
            /// <summary>
            /// 弹幕缩放 double
            /// </summary>
            public const string FONT_ZOOM = "LiveDanmuFontZoom";
            /// <summary>
            /// 弹幕速度 int
            /// </summary>
            public const string SPEED = "LiveDanmuSpeed";
            /// <summary>
            /// 弹幕加粗 bool
            /// </summary>
            public const string BOLD = "LiveDanmuBold";
            /// <summary>
            /// 弹幕边框样式 int
            /// </summary>
            public const string BORDER_STYLE = "LiveDanmuStyle";
            /// <summary>
            /// 弹幕半屏显示 bool
            /// </summary>
            public const string DOTNET_HIDE_SUBTITLE = "LiveDanmuDotHide";
            /// <summary>
            /// 弹幕透明度 double，0-1
            /// </summary>
            public const string OPACITY = "LiveDanmuOpacity";
            /// <summary>
            /// 关键词屏蔽 ObservableCollection<string>
            /// </summary>
            public const string SHIELD_WORD = "LiveDanmuShieldWord";

            /// <summary>
            /// 硬解 bool
            /// </summary>
            public const string HARDWARE_DECODING = "LiveHardwareDecoding";

            /// <summary>
            /// 自动开启宝箱 bool
            /// </summary>
            public const string AUTO_OPEN_BOX = "LiveAutoOpenBox";

            /// <summary>
            /// 直播弹幕延迟
            /// </summary>
            public const string DELAY = "LiveDelay";

            /// <summary>
            /// 直播弹幕清理
            /// </summary>
            public const string DANMU_CLEAN_COUNT = "LiveCleanCount";

            /// <summary>
            /// 隐藏进场
            /// </summary>
            public const string HIDE_WELCOME = "LiveHideWelcome";

            /// <summary>
            /// 隐藏礼物
            /// </summary>
            public const string HIDE_GIFT = "LiveHideGift";

            /// <summary>
            /// 隐藏公告
            /// </summary>
            public const string HIDE_SYSTEM = "LiveSystemMessage";
            /// <summary>
            /// 隐藏抽奖
            /// </summary>
            public const string HIDE_LOTTERY = "LiveHideLottery";
        }

        public class Player
        {
            /// <summary>
            /// 使用外站视频替换无法播放的视频 bool
            /// </summary>
            public const string USE_OTHER_SITEVIDEO = "PlayerUseOther";

            /// <summary>
            /// 硬解 bool
            /// </summary>
            public const string HARDWARE_DECODING = "PlayerHardwareDecoding";

            /// <summary>
            /// 自动播放 bool
            /// </summary>
            public const string AUTO_PLAY = "PlayerAutoPlay";
            /// <summary>
            /// 自动切换下一个视频
            /// </summary>
            public const string AUTO_NEXT = "PlayerAutoNext";
            /// <summary>
            /// 默认清晰度 int
            /// </summary>
            public const string DEFAULT_QUALITY = "PlayerDefaultQuality";

            /// <summary>
            /// 默认音质 int
            /// </summary>
            public const string DEFAULT_SOUND_QUALITY = "PlayerDefaultSoundQuality";

            /// <summary>
            /// 比例 int
            /// </summary>
            public const string RATIO = "PlayerDefaultRatio";

            /// <summary>
            /// 默认视频类型 int flv=0, dash=1,dash_hevc=2
            /// </summary>
            public const string DEFAULT_VIDEO_TYPE = "PlayerDefaultVideoType";
            public static List<double> VideoSpeed = new List<double>() { 2.0d, 1.5d, 1.25d, 1.0d, 0.75d, 0.5d };

            /// <summary>
            /// 默认视频类型 int 1.0
            /// </summary>
            public const string DEFAULT_VIDEO_SPEED = "PlayerDefaultSpeed";

            /// <summary>
            /// 播放模式 int 0=顺序播放，1=单集循环，2=列表循环
            /// </summary>
            public const string DEFAULT_PLAY_MODE = "PlayerDefaultPlayMode";
            /// <summary>
            /// 音量
            /// </summary>
            public const string PLAYER_VOLUME = "PlayerVolume";
            /// <summary>
            /// 亮度
            /// </summary>
            public const string PLAYER_BRIGHTNESS = "PlayeBrightness";
            /// <summary>
            /// A-B 循环播放模式的播放记录
            /// </summary>
            public const string PLAYER_ABPLAY_HISTORIES = "PlayerABPlayHistories";

            /// <summary>
            /// 字幕颜色
            /// </summary>
            public const string SUBTITLE_COLOR = "subtitleColor";
            /// <summary>
            /// 字幕背景颜色
            /// </summary>
            public const string SUBTITLE_BORDER_COLOR = "subtitleBorderColor";
            /// <summary>
            /// 字幕大小
            /// </summary>
            public const string SUBTITLE_SIZE = "subtitleSize";
            /// <summary>
            /// 字幕显示
            /// </summary>
            public const string SUBTITLE_SHOW = "subtitleShow";
            /// <summary>
            /// 字幕透明度
            /// </summary>
            public const string SUBTITLE_OPACITY = "subtitleOpacity";
            /// <summary>
            /// 字幕底部距离
            /// </summary>
            public const string SUBTITLE_BOTTOM = "subtitleBottom";
            /// <summary>
            /// 字幕加粗
            /// </summary>
            public const string SUBTITLE_BOLD = "subtitleBold";
            /// <summary>
            /// 字幕对齐
            /// 0=居中对齐，1=左对齐，2=右对齐
            /// </summary>
            public const string SUBTITLE_ALIGN = "subtitleAlign";
            /// <summary>
            /// 自动跳转进度
            /// </summary>
            public const string AUTO_TO_POSITION = "PlayerAutoToPosition";
            /// <summary>
            /// 自动铺满窗口
            /// </summary>
            public const string AUTO_FULL_WINDOW = "PlayerAutoToFullWindow";
            /// <summary>
            /// 自动铺满全屏
            /// </summary>
            public const string AUTO_FULL_SCREEN = "PlayerAutoToFullScreen";
            /// <summary>
            /// 双击全屏
            /// </summary>
            public const string DOUBLE_CLICK_FULL_SCREEN = "PlayerDoubleClickFullScreen";

            /// <summary>
            /// 方向键右键行为
            /// </summary>
            public const string PLAYER_KEY_RIGHT_ACTION = "PlayerKeyRightAction";

            /// <summary>
            /// 按住手势行为
            /// </summary>
            public const string HOLDING_GESTURE_ACTION = "HoldingGestureAction";

            /// <summary>
            /// 按住手势可被其他手势取消
            /// </summary>
            public const string HOLDING_GESTURE_CAN_CANCEL = "HoldingGestureCanCancel";

            /// <summary>
            /// 倍速播放速度
            /// </summary>
            public const string HIGH_RATE_PLAY_SPEED = "HighRatePlaySpeed";
            public static List<double> HIGH_RATE_PLAY_SPEED_LIST = new List<double>() { 3.0d, 2.0d };

            /// <summary>
            /// 自动打开AI字幕
            /// </summary>
            public const string AUTO_OPEN_AI_SUBTITLE = "PlayerAutoOpenAISubtitle";


            /// <summary>
            /// 替换CDN
            /// </summary>
            public const string REPLACE_CDN = "PlayerReplaceCDN";

            /// <summary>
            /// CDN服务器
            /// </summary>
            public const string CDN_SERVER = "PlayerCDNServer";
        }

        public class Roaming
        {
            /// <summary>
            /// 自定义服务器
            /// </summary>
            public const string CUSTOM_SERVER = "RoamingCustomServer";
            /// <summary>
            /// 自定义服务器链接
            /// </summary>
            public const string CUSTOM_SERVER_URL = "RoamingCustomServerUrl";

            /// <summary>
            /// 自定义香港服务器链接
            /// </summary>
            public const string CUSTOM_SERVER_URL_HK = "RoamingCustomServerUrlHK";

            /// <summary>
            /// 自定义台湾服务器链接
            /// </summary>
            public const string CUSTOM_SERVER_URL_TW = "RoamingCustomServerUrlTW";

            /// <summary>
            /// 自定义大陆服务器链接
            /// </summary>
            public const string CUSTOM_SERVER_URL_CN = "RoamingCustomServerUrlCN";

            /// <summary>
            /// 简体中文
            /// </summary>
            public const string TO_SIMPLIFIED = "RoamingSubtitleToSimplified";
            /// <summary>
            /// 只使用AkamaiCDN链接
            /// </summary>
            //public const string AKAMAI_CDN = "RoamingAkamaiCDN";
        }

        public class Download
        {
            /// <summary>
            /// 下载目录
            /// </summary>
            public const string DOWNLOAD_PATH = "downloadPath";
            public const string DEFAULT_PATH = "视频库/哔哩哔哩下载";
            /// <summary>
            /// 旧版下载目录
            /// </summary>
            public const string OLD_DOWNLOAD_PATH = "downloadOldPath";
            public const string DEFAULT_OLD_PATH = "视频库/BiliBiliDownload";
            /// <summary>
            /// 允许付费网络下载
            /// </summary>
            public const string ALLOW_COST_NETWORK = "allowCostNetwork";

            /// <summary>
            /// 并行下载
            /// </summary>
            public const string PARALLEL_DOWNLOAD = "parallelDownload";

            /// <summary>
            /// 并行下载
            /// </summary>
            public const string SEND_TOAST = "sendToast";

            /// <summary>
            /// 加载旧版下载视频
            /// </summary>
            public const string LOAD_OLD_DOWNLOAD = "loadOldDownload";

            /// <summary>
            /// 下载视频类型
            /// </summary>
            public const string DEFAULT_VIDEO_TYPE = "DownloadDefaultVideoType";

        }

        public class Other
        {
            /// <summary>
            /// 自动清理日志文件
            /// </summary>
            public const string AUTO_CLEAR_LOG_FILE = "autoClearLogFile";

            /// <summary>
            /// 自动清理多少天前的日志文件
            /// </summary>
            public const string AUTO_CLEAR_LOG_FILE_DAY = "autoClearLogFileDay";

            /// <summary>
            /// 保护日志敏感信息
            /// </summary>
            public const string PROTECT_LOG_INFO = "protectLogInfo";

            /// <summary>
            /// 日志级别
            /// </summary>
            public const string LOG_LEVEL = "LogLevel";

            /// <summary>
            /// 忽略版本
            /// </summary>
            public const string IGNORE_VERSION = "ignoreVersion";

            /// <summary>
            /// WebApi地址
            /// </summary>
            public const string BILI_LITE_WEB_API_BASE_URL = "BiliLiteWebApiBaseUrl";

            /// <summary>
            /// 优先使用Grpc请求动态
            /// </summary>
            public const string FIRST_GRPC_REQUEST_DYNAMIC = "FirstGrpcRequestDynamic";
        }
    }
}
