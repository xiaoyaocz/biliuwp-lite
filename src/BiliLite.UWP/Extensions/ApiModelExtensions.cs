using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BiliLite.Models.Requests;
using BiliLite.Models.Responses;
using BiliLite.Services;

namespace BiliLite.Extensions
{
    public static class ApiModelExtensions
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        /// <summary>
        /// 发送请求，扩展方法
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        public static async Task<HttpResults> Request(this ApiModel api, [CallerMemberName] string methodName = null)
        {
            _logger.Trace($"请求记录 {methodName} {api.baseUrl}");
            if (api.method == RestSharp.Method.Get)
            {
                if (api.need_redirect)
                {
                    return await api.url.GetRedirectHttpResultsWithWebCookie(api.headers);
                }

                if (api.need_cookie)
                {
                    return await api.url.GetHttpResultsWithWebCookie(api.headers, api.ExtraCookies);
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