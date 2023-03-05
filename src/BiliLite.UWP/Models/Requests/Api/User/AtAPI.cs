using BiliLite.Services;
using System;

namespace BiliLite.Models.Requests.Api.User
{
    public class AtApi
    {
        public ApiModel RecommendAt(int page = 1, int pagesize = 20)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = "https://api.vc.bilibili.com/dynamic_mix/v1/dynamic_mix/rcmd_at",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&need_attention=1&need_recent_at=1&page={page}&pagesize={pagesize}&teenagers_mode=0",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        public ApiModel SearchUser(string keyword, int page = 1, int pagesize = 20)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = "https://app.bilibili.com/x/v2/search/user",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&keyword={Uri.EscapeDataString(keyword)}&order=totalrank&order_sort=0&pn={page}&ps={pagesize}",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }
    }
}
