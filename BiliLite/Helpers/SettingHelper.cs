using BiliLite.Models;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.Helpers
{
    public class SettingHelper
    {

        public static LocalObjectStorageHelper storageHelper = new LocalObjectStorageHelper();
        public static T GetValue<T>(string key, T _default)
        {
            if (storageHelper.KeyExists(key))
            {
                return storageHelper.Read<T>(key);
            }
            else
            {
                return _default;
            }
        }
        public static void SetValue<T>(string key, T value)
        {
            storageHelper.Save<T>(key, value);
        }
        public class UI
        {
            /// <summary>
            /// 加载原图
            /// </summary>
            public const string ORTGINAL_IMAGE = "originalImage";
            public static bool? _loadOriginalImage = null;
            public static bool LoadOriginalImage
            {
                get
                {
                    if (_loadOriginalImage == null)
                    {
                        _loadOriginalImage = GetValue(ORTGINAL_IMAGE, false);
                    }
                    return _loadOriginalImage.Value;
                }
            }
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
            public const string HOEM_ORDER = "homeOrder";

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
            public static MyProfileModel Profile
            {
                get
                {
                    return storageHelper.Read<MyProfileModel>(USER_PROFILE);
                }
            }
            public static bool Logined
            {
                get
                {
                    return storageHelper.KeyExists(Account.ACCESS_KEY) && !string.IsNullOrEmpty(storageHelper.Read<string>(Account.ACCESS_KEY, null));
                }
            }
            public static string AccessKey
            {
                get
                {
                    return GetValue(ACCESS_KEY, "");
                }
            }
            public static int UserID
            {
                get
                {
                    return GetValue(USER_ID, 0);
                }
            }
        }
        public class VideoDanmaku
        {
            /// <summary>
            /// 显示弹幕 Visibility
            /// </summary>
            public const string SHOW = "VideoDanmuShow";
            /// <summary>
            /// 弹幕缩放 double
            /// </summary>
            public const string FONT_ZOOM = "VideoDanmuFontZoom";
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
        }
        public class Live
        {
            /// <summary>
            /// 显示弹幕 Visibility
            /// </summary>
            public const string SHOW = "LiveDanmuShow";
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
            /// 默认清晰度 int
            /// </summary>
            public const string DEFAULT_QUALITY = "PlayerDefaultQuality";

            /// <summary>
            /// 比例 int
            /// </summary>
            public const string RATIO = "PlayerDefaultRatio";

            /// <summary>
            /// 默认视频类型 int flv=0, dash=1,dash_hevc=2
            /// </summary>
            public const string DEFAULT_VIDEO_TYPE = "PlayerDefaultVideoType";

            /// <summary>
            /// 播放模式 int 0=顺序播放，1=单集循环，2=列表循环
            /// </summary>
            public const string DEFAULT_PLAY_MODE = "PlayerDefaultPlayMode";

            /// <summary>
            /// 视频音量 double 0-1
            /// </summary>
            public const string VOLUME = "PlayerVolume";

            /// <summary>
            /// 字幕颜色
            /// </summary>
            public const string SUBTITLE_COLOR = "subtitleColor";
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
        }

    }
}
