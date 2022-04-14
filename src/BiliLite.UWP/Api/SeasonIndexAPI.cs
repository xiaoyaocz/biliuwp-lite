using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.Api
{
    public class SeasonIndexAPI
    {
        /// <summary>
        /// 筛选条件
        /// </summary>
        /// <param name="season_type">1=番剧,2=电影,3=纪录片,4=国创?,5=电视剧</param>
        /// <returns></returns>
        public ApiModel Condition(int season_type)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/pgc/season/index/condition",//$"https://bangumi.bilibili.com/media/api/search/v2/condition",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, false) + $"&season_type={season_type}&type=0"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 结果
        /// </summary>
        /// <param name="page">页码</param>
        /// <param name="season_type">1=番剧,2=电影,3=纪录片,4=国创?,5=电视剧</param>
        /// <param name="condition">拼接的条件,&par1=1&par2=2</param>
        /// <param name="pagesize">页数</param>
        /// <returns></returns>
        public ApiModel Result(int page,int season_type,string condition,int pagesize=24)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/pgc/season/index/result",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, false) + condition+ $"&page={page}&pagesize={pagesize}&season_type={season_type}&type=0"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

    }
}
