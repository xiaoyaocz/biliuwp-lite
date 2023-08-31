using System;
using BiliLite.Extensions;
using BiliLite.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace BiliLite.Models.Requests.Api
{
    public class CommentApi
    {
        private readonly CookieService m_cookieService;

        public CommentApi()
        {
            m_cookieService = App.ServiceProvider.GetRequiredService<CookieService>();
        }

        public enum CommentType
        {
            Video = 1,
            Dynamic = 17,
            Article = 12,
            Photo = 11,
            MiniVideo = 5,
            SongMenu = 19,
            Song = 14
        }
        public enum CommentSort
        {
            New = 0,
            Hot = 1,
        }
        /// <summary>
        /// 读取评论
        /// </summary>
        /// <param name="oid">ID</param>
        /// <param name="sort">1=最新，2=最热</param>
        /// <param name="pn">页数</param>
        /// <param name="type">类型，1=视频，17=动态，11=图片，5=小视频，19=歌单，14=歌曲</param>
        /// <param name="ps">每页数量</param>
        /// <returns></returns>
        public ApiModel Comment(string oid, CommentSort sort, int pn, int type, int ps = 30)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v2/reply",
                parameter = $"oid={oid}&plat=2&pn={pn}&ps={ps}&sort={(int)sort}&type={type}",
                need_cookie = true,
            };
            //api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 读取评论V2接口
        /// </summary>
        /// <param name="oid"></param>
        /// <param name="sort"></param>
        /// <param name="pn"></param>
        /// <param name="type"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        public ApiModel CommentV2(string oid, CommentSort sort, int pn, int type, int ps = 30, string offsetStr = null)
        {
            var csrf = m_cookieService.GetCSRFToken();
            var mode = sort == CommentSort.Hot ? 3 : 2;
            var pagination = new
            {
                offset = offsetStr
            };
            var paginationStr = JsonConvert.SerializeObject(pagination);

            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v2/reply/main",
                parameter = $"oid={oid}&ps={ps}&mode={mode}&type={type}&csrf={csrf}&pagination_str={paginationStr}",
                need_cookie = true,
            };
            //api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        public ApiModel Reply(string oid, string root, int pn, int type, int ps = 30)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v2/reply/reply",
                parameter = $"oid={oid}&plat=2&pn={pn}&ps={ps}&root={root}&type={type}",
                need_cookie = true,
            };
            //api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        public ApiModel Like(string oid, string root, int action, int type)
        {
            var csrf = m_cookieService.GetCSRFToken();
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v2/reply/action",
                body = $"&oid={oid}&rpid={root}&action={action}&type={type}&csrf={csrf}",
                need_cookie = true,
            };
            //api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }

        public ApiModel ReplyComment(string oid, string root, string parent, string message, int type)
        {
            var csrf = m_cookieService.GetCSRFToken();
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v2/reply/add",
                body = $"&oid={oid}&root={root}&parent={parent}&type={type}&message={message}&csrf={csrf}",
                need_cookie = true,
            };
            // api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }

        public ApiModel DeleteComment(string oid, string rpid, int type)
        {
            var csrf = m_cookieService.GetCSRFToken();
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v2/reply/del",
                body = $"&oid={oid}&rpid={rpid}&type={type}&csrf={csrf}",
                need_cookie = true,
            };
            // api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }
        public ApiModel AddComment(string oid, CommentType type, string message)
        {
            var csrf = m_cookieService.GetCSRFToken();
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v2/reply/add",
                body = $"&oid={oid}&type={(int)type}&message={Uri.EscapeDataString(message)}&csrf={csrf}",
                need_cookie = true,
            };
            //api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }
    }
}
