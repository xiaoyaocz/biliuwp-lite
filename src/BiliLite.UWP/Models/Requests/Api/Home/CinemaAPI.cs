using BiliLite.Services;

namespace BiliLite.Models.Requests.Api.Home
{
    public class CinemaAPI
    {
        public ApiModel CinemaHome()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.IL_BASE_URL}/api/cinema/home"
            };
            return api;
        }
        public ApiModel CinemaFallMore(int wid, long cursor = 0)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.IL_BASE_URL}/api/cinema/falls",
                parameter = $"wid={wid}&cursor={cursor}"
            };
            return api;
        }

    }
}
