using BiliLite.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.Api.Home
{
    public class DynamicAPI
    {
        public ApiModel NewDynamic(string type="8,512")
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = $"https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/dynamic_new",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&type_list={Uri.EscapeDataString(type)}&uid={SettingHelper.Account.Profile.mid}"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }
        public ApiModel HistoryDynamic(string dynamic_id, string type = "8,512")
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = $"https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/dynamic_history",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&offset_dynamic_id={dynamic_id}&type_list={Uri.EscapeDataString(type)}&uid={SettingHelper.Account.Profile.mid}"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }
    }
}
