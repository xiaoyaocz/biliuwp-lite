using BiliLite.Services;

namespace BiliLite.Models.Requests.Api.Live
{
    public class LiveCenterAPI
    {
        public ApiModel FollowLive()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = "https://api.live.bilibili.com/xlive/app-interface/v1/relation/liveAnchor",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + "&qn=0&sortRule=0&filterRule=0",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }
        public ApiModel FollowUnLive(int page)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = "https://api.live.bilibili.com/xlive/app-interface/v1/relation/unliveAnchor",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&page={page}&pagesize=30",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }
        public ApiModel History(int page)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = "https://app.bilibili.com/x/v2/history/liveList",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&pn={page}&ps=20",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }
        public ApiModel SignInfo()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = "https://api.live.bilibili.com/rc/v2/Sign/getSignInfo",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + "&actionKey=appkey",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }
        public ApiModel DoSign()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://api.live.bilibili.com/rc/v1/Sign/doSign",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + "&actionKey=appkey"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

    }

}
