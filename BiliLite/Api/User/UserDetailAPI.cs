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
        /// 用户信息
        /// </summary>
        /// <param name="mid"></param>
        /// <returns></returns>
        public ApiModel UserInfo(string mid)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = "https://api.bilibili.com/x/space/acc/info",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey)+$"&mid={mid}",
            };

            return api;
        }
        /// <summary>
        /// 个人空间
        /// </summary>
        /// <param name="mid"></param>
        /// <returns></returns>
        public ApiModel Space(string mid)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = "https://app.bilibili.com/x/v2/space",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey) + $"&vmid={mid}",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }
        /// <summary>
        /// 数据
        /// </summary>
        /// <param name="mid"></param>
        /// <returns></returns>
        public ApiModel UserStat(string mid)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = "https://api.bilibili.com/x/relation/stat",
                parameter =  $"vmid={mid}",
            };
         
            return api;
        }

        /// <summary>
        /// 用户视频投稿
        /// </summary>
        /// <param name="mid">用户ID</param>
        /// <param name="page">页数</param>
        /// <param name="pagesize">每页数量</param>
        /// <returns></returns>
        public ApiModel SubmitVideos(string mid, int page = 1, int pagesize = 30,string keyword="",int tid=0, SubmitVideoOrder order= SubmitVideoOrder.pubdate)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = "https://api.bilibili.com/x/space/arc/search",
                parameter = $"mid={mid}&ps={pagesize}&tid={tid}&pn={page}&keyword={keyword}&order={order.ToString()}",
            };
            return api;
        }
        /// <summary>
        /// 用户专栏投稿
        /// </summary>
        /// <param name="mid">用户ID</param>
        /// <param name="page">页数</param>
        /// <param name="pagesize">每页数量</param>
        /// <returns></returns>
        public ApiModel SubmitArticles(string mid, int page = 1, int pagesize = 30, SubmitArticleOrder order = SubmitArticleOrder.publish_time)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = "https://api.bilibili.com/x/space/article",
                parameter = $"mid={mid}&ps={pagesize}&pn={page}&sort={order.ToString()}",
            };
            return api;
        }
        /// <summary>
        /// 关注的人
        /// </summary>
        /// <param name="mid">用户ID</param>
        /// <param name="page">页数</param>
        /// <param name="pagesize">每页数量</param>
        /// <returns></returns>
        public ApiModel Followings(string mid, int page = 1, int pagesize = 30)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = "https://api.bilibili.com/x/relation/followings",
                parameter = $"vmid={mid}&ps={pagesize}&pn={page}&order=desc",
            };
            return api;
        }

        /// <summary>
        /// 粉丝
        /// </summary>
        /// <param name="mid">用户ID</param>
        /// <param name="page">页数</param>
        /// <param name="pagesize">每页数量</param>
        /// <returns></returns>
        public ApiModel Followers(string mid, int page = 1, int pagesize = 30)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = "https://api.bilibili.com/x/relation/followers",
                parameter = $"vmid={mid}&ps={pagesize}&pn={page}&order=desc",
            };
            return api;
        }

        /// <summary>
        /// 用户收藏夹
        /// </summary>
        /// <param name="mid">用户ID</param>
        /// <returns></returns>
        public ApiModel Favlist(string mid)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = "https://api.bilibili.com/x/v3/fav/folder/created/list-all",
                parameter = $"up_mid={mid}",
            };
            return api;
        }
    }

    public enum SubmitVideoOrder
    {
        pubdate,
        click,
        stow
    }
    public enum SubmitArticleOrder
    {
        publish_time,
        view,
        fav
    }
}
