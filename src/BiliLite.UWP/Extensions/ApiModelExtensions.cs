using System.Threading.Tasks;
using BiliLite.Helpers;
using BiliLite.Models.Requests;

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
                    return await HttpHelper.GetRedirectWithWebCookie(api.url, api.headers);
                }

                if (api.need_cookie)
                {
                    return await HttpHelper.GetWithWebCookie(api.url, api.headers);
                }

                return await HttpHelper.GetAsync(api.url, api.headers);
            }
            else
            {
                if (api.need_cookie)
                {
                    return await HttpHelper.PostWithCookie(api.url, api.body, api.headers);
                }

                return await HttpHelper.PostAsync(api.url, api.body, api.headers);
            }
        }
    }
}