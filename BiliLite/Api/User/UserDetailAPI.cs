using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.Api.User
{
    public class UserDetailAPI
    {
        /// <summary>
        /// 用户视频投稿
        /// </summary>
        /// <param name="mid">用户ID</param>
        /// <param name="page">页数</param>
        /// <param name="pagesize">每页数量</param>
        /// <returns></returns>
        public ApiModel SubmitVideos(string mid, int page = 1, int pagesize = 30)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = "https://api.bilibili.com/x/space/arc/search",
                parameter = $"mid={mid}&ps={pagesize}&tid=0&pn={page}&keyword=&order=pubdate",
            };
            return api;
        }

    }
}
