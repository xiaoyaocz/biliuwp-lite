using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.Api
{
    public class SeasonApi
    {
       
        public ApiModel Detail(string season_id)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = $"https://api.bilibili.com/pgc/view/app/season",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&season_id={season_id}"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }
        public ApiModel DetailWeb(string season_id)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = $"https://bangumi.bilibili.com/view/web_api/season",
                parameter = $"season_id={season_id}"
            };
           
            return api;
        }
       
    }
}
