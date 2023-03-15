using System.Threading.Tasks;
using BiliLite.Models.Requests;
using BiliLite.Models.Responses;

namespace BiliLite.Extensions
{
    public static class ApiModelExtensions
    {
        /// <summary>
        /// 发送请求，扩展方法
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        public static async Task<HttpResults> Request(this ApiModel api)
        {
            if (api.method == RestSharp.Method.Get)
            {
                if (api.need_redirect)
                {
                    return await api.url.GetRedirectHttpResultsWithWebCookie(api.headers);
                }

                if (api.need_cookie)
                {
                    return await api.url.GetHttpResultsWithWebCookie(api.headers);
                }

                return await api.url.GetHttpResultsAsync(api.headers);
            }
            else
            {
                if (api.need_cookie)
                {
                    return await api.url.PostHttpResultsWithCookie(api.body, api.headers);
                }

                return await api.url.PostHttpResultsAsync(api.body, api.headers);
            }
        }
    }
}