using System.Collections.Generic;
using System.Net.Http;
using BiliLite.Models.Requests;

namespace BiliLite.Services
{
    /// <summary>
    /// 网络请求构造器
    /// </summary>
    public class BiliRequestBuilder
    {
        private IDictionary<string, string> m_headers;
        private IDictionary<string, string> m_cookies;
        private HttpMethod m_method = HttpMethod.Get;
        private HttpContent m_body;
        private readonly string m_url;
        private bool m_needRedirect = false;

        public BiliRequestBuilder(string url)
        {
            m_url = url;
        }

        public BiliRequestBuilder SetPostBody(string body)
        {
            m_body = string.IsNullOrEmpty(body) ? null : new StringContent(
                body, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            m_method = HttpMethod.Post;
            return this;
        }

        public BiliRequestBuilder SetHeaders(IDictionary<string, string>  headers = null)
        {
            m_headers = headers;
            return this;
        }

        public BiliRequestBuilder SetCookies(IDictionary<string, string> cookies = null)
        {
            m_cookies = cookies;
            return this;
        }

        public BiliRequestBuilder SetNeedRedirect()
        {
            m_needRedirect = true;
            return this;
        }

        public BiliRequest Build()
        {
            return new BiliRequest(m_url, m_headers, m_cookies, m_method, m_body, m_needRedirect);
        }
    }
}
