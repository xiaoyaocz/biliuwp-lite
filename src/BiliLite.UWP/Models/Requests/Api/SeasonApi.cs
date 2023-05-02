using BiliLite.Models.Common;
using BiliLite.Services;
using System;

namespace BiliLite.Models.Requests.Api
{
    public class SeasonApi
    {

        public ApiModel Detail(string season_id, bool proxy = false)
        {
            var baseUrl = ApiHelper.API_BASE_URL;

            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{baseUrl}/pgc/view/app/season",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&season_id={season_id}"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        public ApiModel DetailWeb(string season_id)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://bangumi.bilibili.com/view/web_api/season",
                parameter = $"season_id={season_id}"
            };

            return api;
        }
        /// <summary>
        /// 短评
        /// </summary>
        /// <param name="media_id"></param>
        /// <returns></returns>
        public ApiModel ShortReview(int media_id, string next = "", int sort = 0)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/pgc/review/short/list",
                parameter = $"media_id={media_id}&ps=20&sort={sort}&cursor={next}"
            };
            if (SettingService.Account.Logined)
            {
                api.parameter += $"&access_key={SettingService.Account.AccessKey}";
            }
            return api;
        }
        /// <summary>
        /// 点赞短评
        /// </summary>
        /// <param name="media_id"></param>
        /// <returns></returns>
        public ApiModel LikeReview(int media_id, int review_id, ReviewType review_type = ReviewType.Short)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"https://bangumi.bilibili.com/review/api/like",
                body = $"{ApiHelper.MustParameter(ApiHelper.AndroidKey, true)}&media_id={media_id}&review_id={review_id}&review_type={(int)review_type}"
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }
        /// <summary>
        /// 反对短评
        /// </summary>
        /// <param name="media_id"></param>
        /// <returns></returns>
        public ApiModel DislikeReview(int media_id, int review_id, ReviewType review_type = ReviewType.Short)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"https://bangumi.bilibili.com/review/api/dislike",
                body = $"{ApiHelper.MustParameter(ApiHelper.AndroidKey, true)}&media_id={media_id}&review_id={review_id}&review_type={(int)review_type}"
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }
        /// <summary>
        /// 发表点评
        /// </summary>
        /// <param name="media_id">ID</param>
        /// <param name="content">内容</param>
        /// <param name="share_feed">是否分享动态</param>
        /// <param name="score">分数（10分制）</param>
        /// <returns></returns>
        public ApiModel SendShortReview(int media_id, string content, bool share_feed, int score)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"https://bangumi.bilibili.com/review/api/short/post",
                body = $"{ApiHelper.MustParameter(ApiHelper.AndroidKey, true)}&media_id={media_id}&content={Uri.EscapeDataString(content)}&share_feed={(share_feed ? 1 : 0)}&score={score}"
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }
    }

    public enum ReviewType
    {
        Long = 1,
        Short = 2,

    }
}
