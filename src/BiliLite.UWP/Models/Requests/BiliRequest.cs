using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BiliLite.Models.Common;
using BiliLite.Models.Responses;
using BiliLite.Services;
using Flurl.Http;

namespace BiliLite.Models.Requests
{
    public class BiliRequest
    {
        private static readonly ILogger logger = GlobalLogger.FromCurrentType();

        private readonly HttpMethod m_method;
        private readonly HttpContent m_body;
        private readonly IFlurlRequest m_request;
        private readonly string m_url;
        private bool m_needRedirect;

        public BiliRequest(string url, IDictionary<string, string> headers, IDictionary<string, string> cookies,
            HttpMethod method, HttpContent body = null, bool needRedirect = false)
        {
            m_url = url;
            m_method = method;
            m_body = body;
            m_request = new FlurlRequest(url);
            if (headers != null)
            {
                m_request = m_request.WithHeaders(headers);
            }

            if (cookies != null)
            {
                m_request = m_request.WithCookies(cookies);
            }

            m_needRedirect = needRedirect;
            m_request.WithHeader("user-agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36");
        }

        private static string StatusCodeToMessage(int code)
        {

            switch (code)
            {
                case 0:
                case 200:
                    return "请求成功";
                case 412:
                    return "请求太频繁，请稍后再试（412）";
                case 504:
                    return "请求超时了";
                case 301:
                case 302:
                case 303:
                case 305:
                case 306:
                case 400:
                case 401:
                case 402:
                case 403:
                case 404:
                case 500:
                case 501:
                case 502:
                case 503:
                case 505:
                    return "网络请求失败，响应代码:" + code;
                case -2147012867:
                case -2147012889:
                    return "请检查的网络连接";
                default:
                    return "未知错误,响应代码：" + code;
            }
        }

        private HttpResults ConstructErrorResults(IFlurlResponse response)
        {
            var httpResults = new HttpResults()
            {
                code = response.StatusCode,
                status = false,
                message = StatusCodeToMessage(response.StatusCode)
            };
            return httpResults;
        }

        private async Task<HttpResults> ConstructExResults(Exception ex)
        {
            var flurlEx = ex as FlurlHttpException;
            var message = "其他错误";
            if (flurlEx != null)
            {
                var exMessage = await flurlEx.Call?.Response?.GetStringAsync();
                if (exMessage != null) message = exMessage;
            }

            var httpResults = new HttpResults()
            {
                code = ex.HResult,
                status = false,
                message = $"网络请求出现错误({m_method.Method}) : {message}"
            };
            return httpResults;
        }

        private async Task<HttpResults> ConstructNormalResults(IFlurlResponse response, HttpResponseMessage responseMsg)
        {
            var responseCookies = response.Cookies.Select(x => new HttpCookieItem()
            {
                Name = x.Name,
                Domain = x.Domain,
                Expires = x.Expires,
                HttpOnly = x.HttpOnly,
                Value = x.Value,
                Secure = x.Secure,
            }).ToList();
            var results = await response.GetStringAsync();
            var httpResults = new HttpResults()
            {
                code = (int) response.StatusCode,
                status = responseMsg.StatusCode == HttpStatusCode.OK,
                results = results,
                message = "",
                cookies = responseCookies,
            };
            return httpResults;
        }

        private HttpResults ConstructRedirectResults(IFlurlResponse response)
        {
            var success = response.Headers.TryGetFirst("location", out var results);
            HttpResults httpResults = new HttpResults()
            {
                code = response.StatusCode,
                status = success,
                results = results,
                message = "",
            };
            return httpResults;
        }

        private async Task LogRequest()
        {
            var body = "";
            if (m_body != null)
                body = await m_body?.ReadAsStringAsync();
            logger.Log($"网络请求: [{m_method}]{m_url} {body}", LogType.Info);
        }

        public async Task<HttpResults> Send()
        {
            await LogRequest();
            IFlurlResponse response = null;
            HttpResults httpResults;
            try
            {
                if (m_method == HttpMethod.Get)
                {
                    response = await m_request.GetAsync();
                }
                else if (m_method == HttpMethod.Post)
                {
                    response = await m_request.PostAsync(m_body);
                }

                var responseMsg = response.ResponseMessage;
                if (m_needRedirect)
                {
                    httpResults = ConstructRedirectResults(response);
                }
                else if (!responseMsg.IsSuccessStatusCode)
                {
                    httpResults = ConstructErrorResults(response);
                }
                else
                {
                    httpResults = await ConstructNormalResults(response, responseMsg);
                }
            }
            catch (Exception ex)
            {
                logger.Log($"{m_method.Method}请求失败" + m_url, LogType.Error, ex);
                httpResults = await ConstructExResults(ex);
            }

            return httpResults;
        }
    }
}
