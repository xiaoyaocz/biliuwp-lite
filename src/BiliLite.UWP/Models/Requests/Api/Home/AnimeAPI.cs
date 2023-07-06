using BiliLite.Models.Common;
using BiliLite.Services;

namespace BiliLite.Models.Requests.Api.Home
{

    public class AnimeAPI
    {
        public ApiModel BangumiHome()
        {
            var baseUrl = SettingService.GetValue(SettingConstants.Other.BILI_LITE_WEB_API_BASE_URL, ApiConstants.BILI_LITE_WEB_API_DEFAULT_BASE_URL);
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{baseUrl}api/anime/bangumi"
            };
            return api;
        }
        public ApiModel GuochuangHome()
        {
            var baseUrl = SettingService.GetValue(SettingConstants.Other.BILI_LITE_WEB_API_BASE_URL, ApiConstants.BILI_LITE_WEB_API_DEFAULT_BASE_URL);
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{baseUrl}api/anime/guochuang"
            };
            return api;
        }
        public ApiModel Timeline(int type)
        {
            var baseUrl = SettingService.GetValue(SettingConstants.Other.BILI_LITE_WEB_API_BASE_URL, ApiConstants.BILI_LITE_WEB_API_DEFAULT_BASE_URL);
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{baseUrl}api/anime/timeline",
                parameter = "type=" + type
            };
            return api;
        }
        public ApiModel AnimeFallMore(int wid, long cursor = 0)
        {
            var baseUrl = SettingService.GetValue(SettingConstants.Other.BILI_LITE_WEB_API_BASE_URL, ApiConstants.BILI_LITE_WEB_API_DEFAULT_BASE_URL);
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{baseUrl}api/anime/bangumiFalls",
                parameter = $"wid={wid}&cursor={cursor}"
            };
            return api;
        }


    }
}
