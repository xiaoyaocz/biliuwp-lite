using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Windows.Web.Http;
using Windows.Storage.Streams;
using Windows.Web.Http.Filters;
using BiliLite.Models;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;
using BiliLite.Models.Common;

namespace BiliLite.Helpers
{
    /// <summary>
    /// 网络请求方法封装
    /// </summary>
    public static class HttpHelper
    {
        /// <summary>
        /// 发送一个GET请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public static async Task<HttpResults> GetRedirectAsync(string url, IDictionary<string, string> headers = null)
        {
            Debug.WriteLine("GET:" + url);
            var biliRequestBuilder = new BiliRequestBuilder(url)
                .SetHeaders(headers)
                .SetNeedRedirect();
            var biliRequest = biliRequestBuilder.Build();
            var httpResult = await biliRequest.Send();
            return httpResult;
        }

        public static async Task<HttpResults> GetAsync(string url, IDictionary<string, string> headers = null,
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

        public static async Task<HttpResults> GetRedirectWithWebCookie(string url, IDictionary<string, string> headers = null)
        {
            try
            {
                if (headers == null)
                {
                    headers = new Dictionary<string, string>();
                }
                HttpBaseProtocolFilter fiter = new HttpBaseProtocolFilter();
                var cookies = fiter.CookieManager.GetCookies(new Uri(Constants.COOKIE_DOMAIN));
                //没有Cookie
                if (cookies == null || cookies.Count == 0)
                {
                    //访问一遍bilibili.com
                    await GetAsync("https://www.bilibili.com");
                }
                cookies = fiter.CookieManager.GetCookies(new Uri(Constants.COOKIE_DOMAIN));
                string cookie = "";
                foreach (var item in cookies)
                {
                    cookie += $"{item.Name}={item.Value};";
                }
                headers.Add("Cookie", cookie);
                return await GetRedirectAsync(url, headers);
            }
            catch (Exception ex)
            {
                LogHelper.Log("GET请求失败" + url, LogType.ERROR, ex);
                return new HttpResults()
                {
                    code = ex.HResult,
                    status = false,
                    message = "网络请求出现错误(GET)"
                };
            }
        }

        public static async Task<HttpResults> GetWithWebCookie(string url, IDictionary<string, string> headers = null)
        {
            try
            {
                HttpBaseProtocolFilter fiter = new HttpBaseProtocolFilter();
                var cookies = fiter.CookieManager.GetCookies(new Uri(Constants.COOKIE_DOMAIN));
                //没有Cookie
                if(cookies==null|| cookies.Count == 0)
                {
                    //访问一遍bilibili.com拿Cookie
                    var getCookieResult = await GetAsync("https://www.bilibili.com");
                    Utils.SaveCookie(getCookieResult.cookies);
                }
                cookies = fiter.CookieManager.GetCookies(new Uri(Constants.COOKIE_DOMAIN));
                var cookiesCollection = cookies.ToDictionary(x => x.Name, x => x.Value);
                return await GetAsync(url, headers, cookiesCollection);
            }
            catch (Exception ex)
            {
                LogHelper.Log("GET请求失败" + url, LogType.ERROR, ex);
                return new HttpResults()
                {
                    code = ex.HResult,
                    status = false,
                    message = "网络请求出现错误(GET)"
                };
            }
        }

        /// <summary>
        /// 发送一个GET请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public static async Task<Stream> GetStream(string url, IDictionary<string, string> headers = null)
        {
            Debug.WriteLine("GET:" + url);
            try
            {
                using (var client = new HttpClient())
                {
                    if (headers != null)
                    {
                        foreach (var item in headers)
                        {
                            client.DefaultRequestHeaders.Add(item.Key, item.Value);
                        }
                    }

                    var response = await client.GetAsync(new Uri(url));
                    response.EnsureSuccessStatusCode();
                    return (await response.Content.ReadAsInputStreamAsync()).AsStreamForRead();
                }


            }
            catch (Exception ex)
            {
                LogHelper.Log("GET请求Stream失败" + url, LogType.ERROR, ex);
                return null;

            }



        }
        /// <summary>
        /// 发送一个GET请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public static async Task<IBuffer> GetBuffer(string url, IDictionary<string, string> headers = null)
        {
            Debug.WriteLine("GET:" + url);
            try
            {
                using (var client = new HttpClient())
                {
                    if (headers != null)
                    {
                        foreach (var item in headers)
                        {
                            client.DefaultRequestHeaders.Add(item.Key, item.Value);
                        }
                    }

                    var response = await client.GetAsync(new Uri(url));
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsBufferAsync();
                }


            }
            catch (Exception ex)
            {
                LogHelper.Log("GET请求Stream失败" + url, LogType.ERROR, ex);
                return null;

            }



        }
        /// <summary>
        /// 发送一个GET请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public static async Task<String> GetString(string url, IDictionary<string, string> headers = null, IDictionary<string, string> cookie = null)
        {
            Debug.WriteLine("GET:" + url);
            try
            {
                using (var client = new HttpClient())
                {
                    if (headers != null)
                    {
                        foreach (var item in headers)
                        {
                            client.DefaultRequestHeaders.Add(item.Key, item.Value);
                        }
                    }

                    var response = await client.GetAsync(new Uri(url));
                    response.EnsureSuccessStatusCode();
                    var buffer = await response.Content.ReadAsBufferAsync();
                    return Encoding.UTF8.GetString(buffer.ToArray());
                }


            }
            catch (Exception ex)
            {
                LogHelper.Log("GET请求String失败" + url, LogType.ERROR, ex);

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
        public static async Task<HttpResults> PostAsync(string url, string body, IDictionary<string, string> headers = null, IDictionary<string, string> cookies = null)
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

        public static async Task<HttpResults> PostWithCookie(string url, string body, IDictionary<string, string> headers = null)
        {
            try
            {
                HttpBaseProtocolFilter fiter = new HttpBaseProtocolFilter();
                var cookies = fiter.CookieManager.GetCookies(new Uri(Constants.COOKIE_DOMAIN));
                //没有Cookie
                if (cookies == null || cookies.Count == 0)
                {
                    //访问一遍bilibili.com
                    await GetAsync("https://www.bilibili.com");
                }
                cookies = fiter.CookieManager.GetCookies(new Uri(Constants.COOKIE_DOMAIN));
                var cookiesCollection = cookies.ToDictionary(x => x.Name, x => x.Value);
                return await PostAsync(url, body, headers, cookiesCollection);
            }
            catch (Exception ex)
            {
                LogHelper.Log("GET请求失败" + url, LogType.ERROR, ex);
                return new HttpResults()
                {
                    code = ex.HResult,
                    status = false,
                    message = "网络请求出现错误(POST)"
                };
            }
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
    }


    public class HttpResults
    {
        public int code { get; set; }
        public string message { get; set; }
        public string results { get; set; }
        public bool status { get; set; }
        public List<HttpCookieItem> cookies { get; set; }
        public async Task<T> GetJson<T>()
        {
            return await Task.Run<T>(() =>
             {
                 return JsonConvert.DeserializeObject<T>(results);
             });

        }
        public JObject GetJObject()
        {
            try
            {
                var obj = JObject.Parse(results);
                return obj;
            }
            catch (Exception)
            {
                return null;
            }

        }
        public async Task<ApiDataModel<T>> GetData<T>()
        {
            try
            {
                return await GetJson<ApiDataModel<T>>();
            }
            catch (Exception)
            {
                return null;
            }

        }
        public async Task<ApiResultModel<T>> GetResult<T>()
        {
            try
            {
                return await GetJson<ApiResultModel<T>>();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
