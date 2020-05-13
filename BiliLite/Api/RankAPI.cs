using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.Api
{
    public class RankAPI
    {
        public ApiModel RankRegion()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = $"{ApiHelper.baseUrl}/api/rank/RankRegion"
            };
            return api;
        }
        /// <summary>
        /// 排行榜
        /// </summary>
        /// <param name="rid">分区ID</param>
        /// <param name="type">1=全站，2原创</param>
        /// <param name="day">3,7,30</param>
        /// <returns></returns>
        public ApiModel Rank(int rid,int type,int day=3)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = $"https://api.bilibili.com/x/web-interface/ranking",
                parameter = $"rid={rid}&day={day}&type={type}&arc_type=0"
            };
            return api;
        }


        /// <summary>
        /// 排行榜
        /// </summary>
        /// <param name="type">1=全站，2原创</param>
        /// <returns></returns>
        public ApiModel SeasonRank(int type)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = $"https://api.bilibili.com/pgc/season/rank/list",
                parameter = $"season_type={type}"
            };
            return api;
        }


    }
}
