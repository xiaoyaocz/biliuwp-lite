using System.Collections.Generic;

namespace BiliLite.Models.Common.UserDynamic
{
    public static class UserDynamicDisplayTypesMap
    {
        //纯文字
        //视频
        //音乐
        //网页
        //专题
        //投票
        //活动
        //图片
        //直播 4308
        public static Dictionary<int, UserDynamicDisplayType> Map = new Dictionary<int, UserDynamicDisplayType>()
        {
            {1, UserDynamicDisplayType.Repost},
            {2, UserDynamicDisplayType.Photo},
            {4, UserDynamicDisplayType.Text},
            {8, UserDynamicDisplayType.Video},
            {16, UserDynamicDisplayType.ShortVideo},
            {64, UserDynamicDisplayType.Article},
            {256, UserDynamicDisplayType.Music},
            {1024, UserDynamicDisplayType.Miss},
            {512, UserDynamicDisplayType.Season},
            {4097, UserDynamicDisplayType.Season},
            {4098, UserDynamicDisplayType.Season},
            {4099, UserDynamicDisplayType.Season},
            {4100, UserDynamicDisplayType.Season},
            {4101, UserDynamicDisplayType.Season},
            {2048, UserDynamicDisplayType.Web},
            {2049, UserDynamicDisplayType.Web},
            {4308, UserDynamicDisplayType.Live},
            {4200, UserDynamicDisplayType.LiveShare},
            {4300, UserDynamicDisplayType.MediaList},
            {4310, UserDynamicDisplayType.MediaList},
            {4303, UserDynamicDisplayType.Cheese},
            {4302, UserDynamicDisplayType.Cheese}
        };
    }
}
