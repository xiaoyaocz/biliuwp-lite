using BiliLite.Models.Common;
using BiliLite.Services;

namespace BiliLite.Models.Requests.Api.Home
{
    public class CinemaAPI
    {
        public ApiModel CinemaHome()
        {
            var baseUrl = SettingService.GetValue(SettingConstants.Other.BILI_LITE_WEB_API_BASE_URL, ApiConstants.BILI_LITE_WEB_API_DEFAULT_BASE_URL);
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{baseUrl}api/cinema/home"
            };
            return api;
        }
        public ApiModel CinemaFallMore(int wid, long cursor = 0)
        {
            var baseUrl = SettingService.GetValue(SettingConstants.Other.BILI_LITE_WEB_API_BASE_URL, ApiConstants.BILI_LITE_WEB_API_DEFAULT_BASE_URL);
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{baseUrl}api/cinema/falls",
                parameter = $"wid={wid}&cursor={cursor}"
            };
            return api;
        }

    }
}
