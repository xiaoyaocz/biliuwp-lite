using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiliLite.Api;

namespace BiliLite.Api.Home
{
   
    public class AnimeAPI
    {

        public ApiModel BangumiHome()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.IL_BASE_URL}/api/anime/bangumi"
            };
            return api;
        }
        public ApiModel GuochuangHome()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.IL_BASE_URL}/api/anime/guochuang"
            };
            return api;
        }
        public ApiModel Timeline(int type)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.IL_BASE_URL}/api/anime/timeline",
                parameter="type="+ type
            };
            return api;
        }
        public ApiModel AnimeFallMore(int wid, long cursor = 0)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.IL_BASE_URL}/api/anime/bangumiFalls",
                parameter= $"wid={wid}&cursor={cursor}"
            };
            return api;
        }

       
    }
}
