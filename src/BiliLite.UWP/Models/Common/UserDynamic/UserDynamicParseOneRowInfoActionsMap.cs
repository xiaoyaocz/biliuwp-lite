using System;
using System.Collections.Generic;
using BiliLite.Extensions;
using Newtonsoft.Json.Linq;

namespace BiliLite.Models.Common.UserDynamic
{
    public static class UserDynamicParseOneRowInfoActionsMap
    {
        public static Dictionary<UserDynamicDisplayType, Func<JObject, UserDynamicItemDisplayOneRowInfo>> Map
            = new Dictionary<UserDynamicDisplayType, Func<JObject, UserDynamicItemDisplayOneRowInfo>>()
            {
                { UserDynamicDisplayType.Video, ParseVideo },
                { UserDynamicDisplayType.Season, ParseSeason },
                { UserDynamicDisplayType.Music, ParseMusic },
                { UserDynamicDisplayType.Web, ParseWeb },
                { UserDynamicDisplayType.Article, ParseArticle },
                { UserDynamicDisplayType.Live, ParseLive },
                { UserDynamicDisplayType.LiveShare, ParseLiveShare },
                { UserDynamicDisplayType.MediaList, ParseMediaList },
                { UserDynamicDisplayType.Cheese, ParseChess },
            };

        private static UserDynamicItemDisplayOneRowInfo ParseVideo(JObject obj)
        {
            var duration = TimeSpan.FromSeconds(obj["duration"].ToInt32());
            var coverText = duration.ToString(@"mm\:ss");
            if (duration.TotalHours >= 1)
            {
                coverText = duration.ToString(@"hh\:mm\:ss");
            }
            var info = new UserDynamicItemDisplayOneRowInfo()
            {
                Cover = obj["pic"].ToString() + "@412w_232h_1c.jpg",
                CoverText = coverText,
                Subtitle = "播放:" + obj["stat"]["view"].ToCountString() + " 弹幕:" + obj["stat"]["danmaku"].ToCountString(),
                Tag = "视频",
                ID = obj["aid"].ToString(),
                Desc = obj["desc"].ToString(),
                Title = obj["title"].ToString(),
            };
            info.Url = "http://b23.tv/av" + info.ID;
            return info;
        }

        private static UserDynamicItemDisplayOneRowInfo ParseSeason(JObject obj)
        {
            var info = new UserDynamicItemDisplayOneRowInfo()
            {
                Cover = obj["cover"].ToString() + "@200w.jpg",
                CoverText = "",
                Subtitle = "播放:" + obj["play_count"].ToCountString() + " 弹幕:" + obj["bullet_count"].ToCountString(),
                ID = obj["apiSeasonInfo"]["season_id"].ToString(),
                Title = obj["new_desc"].ToString(),
                CoverWidth = 160,
                AID = obj["aid"].ToString(),
            };
            if (string.IsNullOrEmpty(info.Title))
            {
                info.Title = obj["apiSeasonInfo"]["title"].ToString();
            }
            info.Url = "http://b23.tv/ss" + info.ID;
            return info;
        }

        private static UserDynamicItemDisplayOneRowInfo ParseMusic(JObject obj)
        {
            var info = new UserDynamicItemDisplayOneRowInfo()
            {
                Cover = obj["cover"].ToString() + "@200w.jpg",
                CoverParameter = "200w",
                Subtitle = "播放:" + obj["playCnt"].ToCountString() + " 评论:" + obj["replyCnt"].ToCountString(),
                ID = obj["id"].ToString(),
                Title = obj["title"].ToString(),
                CoverWidth = 80,
                Tag = "音频",

            };
            info.Url = "http://b23.tv/au" + info.ID;
            return info;
        }

        private static UserDynamicItemDisplayOneRowInfo ParseWeb(JObject obj)
        {
            var cover = obj["sketch"]["cover_url"]?.ToString() ?? "";
            var info = new UserDynamicItemDisplayOneRowInfo()
            {
                Cover = cover == "" ? "" : cover + "@200w.jpg",
                Subtitle = obj["sketch"]["desc_text"]?.ToString() ?? "",
                ID = obj["sketch"]["target_url"]?.ToString() ?? "",
                Title = obj["sketch"]["title"]?.ToString() ?? "",
                CoverWidth = 80,
            };
            info.Url = info.ID.ToString();
            return info;
        }

        private static UserDynamicItemDisplayOneRowInfo ParseArticle(JObject obj)
        {
            var cover = obj["origin_image_urls"]?[0]?.ToString() ?? "";
            var info = new UserDynamicItemDisplayOneRowInfo()
            {
                Cover = cover + "@412w_232h_1c.jpg",
                //CoverText = obj["words"].ToCountString()+"字",
                Subtitle = "浏览:" + obj["stats"]["view"].ToCountString() + " 点赞:" + obj["stats"]["like"].ToCountString(),
                ID = obj["id"].ToString(),
                Title = obj["title"].ToString(),
                Desc = obj["summary"].ToString(),
                Tag = "专栏",

            };
            info.Url = "https://www.bilibili.com/read/cv" + info.ID.ToString();
            return info;
        }

        private static UserDynamicItemDisplayOneRowInfo ParseLive(JObject obj)
        {
            var info = new UserDynamicItemDisplayOneRowInfo()
            {
                Cover = obj["live_play_info"]["cover"].ToString() + "@412w_232h_1c.jpg",
                CoverText = "",
                Subtitle = obj["live_play_info"]["parent_area_name"].ToString() + " · 人气:" + obj["live_play_info"]["online"].ToCountString(),
                Tag = "直播",
                ID = obj["live_play_info"]["room_id"].ToString(),
                Title = obj["live_play_info"]["title"].ToString(),
            };
            info.Url = "https://b23.tv/live" + info.ID;
            return info;
        }

        private static UserDynamicItemDisplayOneRowInfo ParseLiveShare(JObject obj)
        {
            var info = new UserDynamicItemDisplayOneRowInfo()
            {
                Cover = obj["cover"].ToString() + "@412w_232h_1c.jpg",
                CoverText = obj["live_status"].ToInt32() == 0 ? "直播已结束" : "",
                Subtitle = obj["area_v2_name"].ToString(),
                Tag = "直播",
                ID = obj["roomid"].ToString(),
                Title = obj["title"].ToString(),
            };
            info.Url = "https://b23.tv/live" + info.ID;
            return info;
        }

        private static UserDynamicItemDisplayOneRowInfo ParseMediaList(JObject obj)
        {                        //TODO 合集这部分需要重写
            //https://t.bilibili.com/625835271145782341
            if (obj["videos"].ToInt32() == 1)
            {
                return DynamicParseExtensions.ParseOneRowInfo(UserDynamicDisplayType.Video, obj);
            }
            var info = new UserDynamicItemDisplayOneRowInfo()
            {
                Cover = obj["cover"].ToString() + "@412w_232h_1c.jpg",
                Subtitle = obj["media_count"].ToString() + "个内容",
                Tag = "收藏夹",
                ID = obj["id"].ToString(),
                Title = obj["title"].ToString(),
            };
            info.Url = "https://www.bilibili.com/medialist/detail/ml" + info.ID;
            return info;
        }

        private static UserDynamicItemDisplayOneRowInfo ParseChess(JObject obj)
        {
            var info = new UserDynamicItemDisplayOneRowInfo()
            {
                Cover = obj["cover"].ToString() + "@412w_232h_1c.jpg",
                Subtitle = obj["subtitle"].ToString(),
                Tag = "付费课程",
                ID = obj["id"].ToString(),
                Title = obj["title"].ToString(),
            };
            info.Url = obj["url"].ToString();
            return info;
        }
    }
}
