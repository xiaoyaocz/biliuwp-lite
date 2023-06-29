using System.Collections.Generic;

namespace BiliLite.Models.Requests
{
    public class ApiModel
    {
        /// <summary>
        /// 请求方法
        /// </summary>
        public RestSharp.Method method { get; set; }
        /// <summary>
        /// API地址
        /// </summary>
        public string baseUrl { get; set; }
        /// <summary>
        /// Url参数
        /// </summary>
        public string parameter { get; set; }
        /// <summary>
        /// 发送内容体，用于POST方法
        /// </summary>
        public string body { get; set; }
        /// <summary>
        /// 请求头
        /// </summary>
        public IDictionary<string, string> headers { get; set; }
        /// <summary>
        /// 需要Cookie
        /// </summary>
        public bool need_cookie { get; set; } = false;
        /// <summary>
        /// 需要重定向
        /// </summary>
        public bool need_redirect { get; set; } = false;

        /// <summary>
        /// 额外的Cookies
        /// </summary>
        public IDictionary<string, string> ExtraCookies { get; set; }

        /// <summary>
        /// 请求地址
        /// </summary>
        public string url
        {
            get
            {
                return baseUrl + "?" + parameter;
            }
        }
    }
}