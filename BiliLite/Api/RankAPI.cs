using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.Api
{
    public class RankAPI
    {
        //public ApiModel RankRegion()
        //{
        //    ApiModel api = new ApiModel()
        //    {
        //        method = RestSharp.Method.Get,
        //        baseUrl = $"{ApiHelper.baseUrl}/api/rank/RankRegion"
        //    };
        //    return api;
        //}
        /// <summary>
        /// 排行榜
        /// </summary>
        /// <param name="rid">分区ID</param>
        /// <param name="type">all=全站，origin=原创，rookie=新人</param>
        /// <returns></returns>
        public ApiModel Rank(int rid,string type)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/web-interface/ranking/v2",
                parameter = $"rid={rid}&type={type}"
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
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/pgc/season/rank/list",
                parameter = $"season_type={type}"
            };
            return api;
        }


    }
}
