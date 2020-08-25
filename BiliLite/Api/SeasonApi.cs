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
        public ApiModel DetailBiliPlus(string season_id)
        {
            Dictionary<string, string> header =new Dictionary<string, string>();
            header.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.108 Safari/537.36");
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = $"https://www.biliplus.com/api/bangumi",
                parameter = $"season={season_id}",
                headers = header
            };
           
            return api;
        }
       
    }
}
