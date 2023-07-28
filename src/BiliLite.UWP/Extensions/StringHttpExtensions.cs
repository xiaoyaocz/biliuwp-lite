using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;
using BiliLite.Models.Common;
using BiliLite.Services;
using Flurl.Http;
using BiliLite.Models.Responses;
using Microsoft.Extensions.DependencyInjection;

namespace BiliLite.Extensions
{
    /// <summary>
    /// 网络请求方法封装
    /// </summary>
    public static class StringHttpExtensions
    {
        private static readonly ILogger logger = GlobalLogger.FromCurrentType();

        /// <summary>
        /// 发送一个获取重定向值的get请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static async Task<HttpResults> GetRedirectHttpResultsAsync(this string url, IDictionary<string, string> headers = null,
            IDictionary<string, string> cookies = null)
        {
            Debug.WriteLine("GET:" + url);
            var biliRequestBuilder = new BiliRequestBuilder(url)
                .SetHeaders(headers)
                .SetCookies(cookies)
                .SetNeedRedirect();
            var biliRequest = biliRequestBuilder.Build();
            var httpResult = await biliRequest.Send();
            return httpResult;
        }

        /// <summary>
        /// 发送get请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="cookies"></param>
        /// <returns></returns>
        public static async Task<HttpResults> GetHttpResultsAsync(this string url, IDictionary<string, string> headers = null,
            IDictionary<string, string> cookies = null)
        {
            Debug.WriteLine("GET:" + url);
            var biliRequestBuilder = new BiliRequestBuilder(url)
                .SetHeaders(headers)
                .SetCookies(cookies);
            var biliRequest = biliRequestBuilder.Build();
            var httpResult = await biliRequest.Send();
            return httpResult;
        }


        /// <summary>
        /// 发送一个获取重定向值的get请求,且带上Cookie
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static async Task<HttpResults> GetRedirectHttpResultsWithWebCookie(this string url, IDictionary<string, string> headers = null)
        {
            try
            {
                var cookies = await GetCookies();
                return await url.GetRedirectHttpResultsAsync(headers, cookies);
            }
            catch (Exception ex)
            {
                logger.Log("GET请求失败" + url, LogType.Error, ex);
                return new HttpResults()
                {
                    code = ex.HResult,
                    status = false,
                    message = "网络请求出现错误(GET)"
                };
            }
        }

        /// <summary>
        /// 发送get请求,且带上Cookie
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="cookies"></param>
        /// <returns></returns>
        public static async Task<HttpResults> GetHttpResultsWithWebCookie(this string url, IDictionary<string, string> headers = null, IDictionary<string,string> extraCookies = null)
        {
            try
            {
                var cookies = await GetCookies();

                if (extraCookies != null)
                {
                    foreach(var kvp in extraCookies.ToList())
                    {
                        cookies.Add(kvp.Key, kvp.Value);
                    }
                }

                return await url.GetHttpResultsAsync(headers, cookies);
            }
            catch (Exception ex)
            {
                logger.Log("GET请求失败" + url, LogType.Error, ex);
                return new HttpResults()
                {
                    code = ex.HResult,
                    status = false,
                    message = "网络请求出现错误(GET)"
                };
            }
        }

        /// <summary>
        /// Get请求，返回Stream
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static async Task<Stream> GetStream(this string url, IDictionary<string, string> headers = null)
        {
            Debug.WriteLine("GET:" + url);
            try
            {
                var stream = await url.WithHeaders(headers).GetAsync().ReceiveStream();
                return stream;
            }
            catch (Exception ex)
            {
                logger.Log("GET请求Stream失败" + url, LogType.Error, ex);
                return null;
            }
        }

        /// <summary>
        /// Get请求，返回buffer
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static async Task<IBuffer> GetBuffer(this string url, IDictionary<string, string> headers = null)
        {
            Debug.WriteLine("GET:" + url);
            try
            {
                var response = await url.WithHeaders(headers).GetAsync();
                var bytes = await response.GetBytesAsync();
                var stream = new MemoryStream(bytes, 0, bytes.Length, true, true);
                var buffer = stream.GetWindowsRuntimeBuffer();
                return buffer;
            }
            catch (Exception ex)
            {
                logger.Log("GET请求Buffer失败" + url, LogType.Error, ex);
                return null;
            }
        }

        /// <summary>
        /// Get请求，返回字符串
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public static async Task<string> GetString(this string url, IDictionary<string, string> headers = null, IDictionary<string, string> cookie = null)
        {
            Debug.WriteLine("GET:" + url);
            try
            {
                var result = await url.WithHeaders(headers).GetAsync().ReceiveString();
                return result;
            }
            catch (Exception ex)
            {
                logger.Log("GET请求String失败" + url, LogType.Error, ex);
                return null;
            }
        }

        /// <summary>
        /// 发送一个POST请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="body"></param>
        /// <param name="headers"></param>
        /// <param name="cookie"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static async Task<HttpResults> PostHttpResultsAsync(this string url, string body, IDictionary<string, string> headers = null, IDictionary<string, string> cookies = null)
        {
            Debug.WriteLine("POST:" + url + "\r\nBODY:" + body);
            var biliRequestBuilder = new BiliRequestBuilder(url)
                .SetHeaders(headers)
                .SetCookies(cookies)
                .SetPostBody(body);
            var biliRequest = biliRequestBuilder.Build();
            var httpResult = await biliRequest.Send();
            return httpResult;
        }

        public static async Task<HttpResults> PostHttpResultsWithCookie(this string url, string body, IDictionary<string, string> headers = null)
        {
            try
            {
                var cookieService = App.ServiceProvider.GetRequiredService<CookieService>();
                var cookies = cookieService.Cookies;
                //没有Cookie
                if (cookies == null || cookies.Count == 0)
                {
                    //访问一遍bilibili.com
                    var getCookieResult = await Constants.BILIBILI_DOMAIN.GetHttpResultsAsync(); 
                    cookieService.Cookies = getCookieResult.cookies;
                }
                cookies = cookieService.Cookies;
                var cookiesCollection = cookies.ToDictionary(x => x.Name, x => x.Value);
                return await url.PostHttpResultsAsync(body, headers, cookiesCollection);
            }
            catch (Exception ex)
            {
                logger.Log("GET请求失败" + url, LogType.Error, ex);
                return new HttpResults()
                {
                    code = ex.HResult,
                    status = false,
                    message = "网络请求出现错误(POST)"
                };
            }
        }

        private static async Task<Dictionary<string, string>> GetCookies()
        {
            var cookieService = App.ServiceProvider.GetRequiredService<CookieService>();
            var cookies = cookieService.Cookies;
            //没有Cookie
            if (cookies == null || cookies.Count == 0)
            {
                //访问一遍bilibili.com拿Cookie
                var getCookieResult = await Constants.BILIBILI_DOMAIN.GetHttpResultsAsync();
                cookieService.Cookies = getCookieResult.cookies;
            }
            cookies = cookieService.Cookies;
            var cookiesCollection = cookies.ToDictionary(x => x.Name, x => x.Value);
            return cookiesCollection;
        }
    }
}
