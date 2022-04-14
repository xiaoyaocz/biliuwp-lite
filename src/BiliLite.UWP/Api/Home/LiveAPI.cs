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
                method = RestSharp.Method.Get,
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
                method = RestSharp.Method.Get,
                baseUrl = "https://api.live.bilibili.com/xlive/web-interface/v1/index/getList",
                parameter= "platform=web"
            };
            return api;
        }
        

    }
}
