using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.Api.Home
{
    public class LiveAPI
    {
        public ApiModel LiveHome()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = "https://api.live.bilibili.com/xlive/app-interface/v2/index/getAllList",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true)+ "&device=android&rec_page=1&relation_page=1&scale=xxhdpi",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }
        public ApiModel LiveHomeItems()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = "https://api.live.bilibili.com/xlive/web-interface/v1/index/getList",
                parameter= "platform=web"
            };
            return api;
        }
        public ApiModel FollowLive()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = "https://api.live.bilibili.com/xlive/app-interface/v1/relation/liveAnchor",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true)+ "&qn=0&sortRule=0&filterRule=0",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

    }
}
