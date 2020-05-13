using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.Api
{
    public class CommentApi
    {
        public enum CommentType
        {
            Video=1,
            Dynamic=17,
            Photo=11,
            MiniVideo=5,
            SongMenu=19,
            Song=14
        }
        public enum ConmmentSort
        {
            New = 0,
            Hot=2,
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
        public ApiModel Comment(string oid, ConmmentSort sort,int pn, CommentType type, int ps = 30)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = $"https://api.bilibili.com/x/v2/reply",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&oid={oid}&plat=2&pn={pn}&ps={ps}&sort={(int)sort}&type={(int)type}"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }
        public ApiModel Reply(string oid,string root, int pn, CommentType type, int ps = 30)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = $"https://api.bilibili.com/x/v2/reply/reply",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&oid={oid}&plat=2&pn={pn}&ps={ps}&root={root}&type={(int)type}"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        public ApiModel Like(string oid, string root, int action, CommentType type)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.POST,
                baseUrl = $"https://api.bilibili.com/x/v2/reply/action",
                body = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&oid={oid}&rpid={root}&action={action}&type={(int)type}"
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }

        public ApiModel ReplyComment(string oid, string root, string parent, string message, CommentType type)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.POST,
                baseUrl = $"https://api.bilibili.com/x/v2/reply/add",
                body = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&oid={oid}&root={root}&parent={parent}&type={(int)type}&message={message}"
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }
        public ApiModel DeleteComment(string oid, string rpid, CommentType type)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.POST,
                baseUrl = $"https://api.bilibili.com/x/v2/reply/del",
                body = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&oid={oid}&rpid={rpid}&type={(int)type}"
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }

    }
}
