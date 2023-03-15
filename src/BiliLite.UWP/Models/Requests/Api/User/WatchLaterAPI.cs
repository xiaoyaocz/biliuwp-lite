using BiliLite.Services;

namespace BiliLite.Models.Requests.Api.User
{
    public class WatchLaterAPI
    {
        public ApiModel Add(string aid)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v2/history/toview/add",
                body = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&aid={aid}"
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }
        public ApiModel Watchlater()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v2/history/toview",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&ps=100"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }
        public ApiModel Clear()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v2/history/toview/clear",
                body = ApiHelper.MustParameter(ApiHelper.AndroidKey, true)
            };
            api.parameter += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }
        public ApiModel Del()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v2/history/toview/del",
                body = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + "&viewed=true"
            };
            api.parameter += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }
        public ApiModel Del(string id)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v2/history/toview/del",
                body = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + "&aid=" + id
            };
            api.parameter += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }
    }
}
