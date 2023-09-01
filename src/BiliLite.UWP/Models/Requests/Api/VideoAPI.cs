using BiliLite.Services;
using System;

namespace BiliLite.Models.Requests.Api
{
    public class VideoAPI
    {
        public ApiModel Detail(string id, bool isbvid)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://app.bilibili.com/x/v2/view",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&{(isbvid ? "bvid=" : "aid=")}{id}&plat=0"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        public ApiModel DetailWebInterface(string id, bool isBvId)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://api.bilibili.com/x/web-interface/view",
                parameter = $"&{(isBvId ? "bvid=" : "aid=")}{id}",
                need_cookie = true,
            };
            return api;
        }

        public ApiModel RelatesWebInterface(string id, bool isBvId)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://api.bilibili.com/x/web-interface/archive/related",
                parameter = $"&{(isBvId ? "bvid=" : "aid=")}{id}"
            };
            return api;
        }

        public ApiModel DetailProxy(string id, bool isbvid)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://app.bilibili.com/x/v2/view",
                parameter = ApiHelper.GetAccessParameter(ApiHelper.AndroidKey) + $"&{(isbvid ? "bvid=" : "aid=")}{id}&plat=0"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            var apiUrl = Uri.EscapeDataString(api.url);
            api.baseUrl = "https://biliproxy.iill.moe/app.php";
            api.parameter = "url=" + apiUrl;
            return api;
        }
        /// <summary>
        ///点赞
        /// </summary>
        /// <param name="dislike"> 当前dislike状态</param>
        /// <param name="like">当前like状态</param>
        /// <returns></returns>
        public ApiModel Like(string aid, int dislike, int like)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"https://app.bilibili.com/x/v2/view/like",
                body = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&aid={aid}&dislike={dislike}&from=7&like={like}"
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }
        /// <summary>
        ///点赞
        /// </summary>
        /// <param name="dislike"> 当前dislike状态</param>
        /// <param name="like">当前like状态</param>
        /// <returns></returns>
        public ApiModel Dislike(string aid, int dislike, int like)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"https://app.biliapi.net/x/v2/view/dislike",
                body = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&aid={aid}&dislike={dislike}&from=7&like={like}"
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        ///一键三连
        /// </summary>
        /// <returns></returns>
        public ApiModel Triple(string aid)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"https://app.bilibili.com/x/v2/view/like/triple",
                body = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&aid={aid}"
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }

        public ApiModel Coin(string aid, int num)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"https://app.biliapi.net/x/v2/view/coin/add",
                body = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&aid={aid}&multiply={num}&platform=android&select_like=0"
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }
        /// <summary>
        /// 关注
        /// </summary>
        /// <param name="mid">用户ID</param>
        /// <param name="mode">1为关注，2为取消关注</param>
        /// <returns></returns>
        public ApiModel Attention(string mid, string mode)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/relation/modify",
                body = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&act={mode}&fid={mid}&re_src=32"
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }


    }
}
