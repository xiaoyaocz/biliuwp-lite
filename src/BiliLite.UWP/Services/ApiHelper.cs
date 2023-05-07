using BiliLite.Extensions;
using BiliLite.Models.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BiliLite.Services
{
    public static class ApiHelper
    {
        // BiliLite.WebApi 项目部署的服务器
        //public static string baseUrl = "http://localhost:5000";
        public const string IL_BASE_URL = "https://biliapi.iliili.cn";

        // GIT RAW路径
        public const string GIT_RAW_URL = "https://raw.githubusercontent.com/ywmoyue/biliuwp-lite/master";

        // 哔哩哔哩API
        public const string API_BASE_URL = "https://api.bilibili.com";

        //漫游默认的服务器
        public const string ROMAING_PROXY_URL = "https://b.chuchai.vip";

        public static ApiKeyInfo AndroidKey = new ApiKeyInfo("1d8b6e7d45233436", "560c52ccd288fed045859ed18bffd973");
        public static ApiKeyInfo AndroidVideoKey = new ApiKeyInfo("iVGUTjsxvpLeuDCf", "aHRmhWMLkdeMuILqORnYZocwMBpMEOdt");
        public static ApiKeyInfo WebVideoKey = new ApiKeyInfo("84956560bc028eb7", "94aba54af9065f71de72f5508f1cd42e");
        public static ApiKeyInfo AndroidTVKey = new ApiKeyInfo("4409e2ce8ffd12b8", "59b43e04ad6965f34319062b478f83dd");
        public static ApiKeyInfo LoginKey = new ApiKeyInfo("783bbb7264451d82", "2653583c8873dea268ab9386918b1d65");
        private const string build = "6235200";
        private const string _mobi_app = "android";
        private const string _platform = "android";
        public static string deviceId = "";
        public static string GetSign(string url, ApiKeyInfo apiKeyInfo, string par = "&sign=")
        {
            string result;
            string str = url.Substring(url.IndexOf("?", 4) + 1);
            List<string> list = str.Split('&').ToList();
            list.Sort();
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string str1 in list)
            {
                stringBuilder.Append(stringBuilder.Length > 0 ? "&" : string.Empty);
                stringBuilder.Append(str1);
            }
            stringBuilder.Append(apiKeyInfo.Secret);
            result = stringBuilder.ToString().ToMD5().ToLower();
            return par + result;
        }

        public static string GetSign(IDictionary<string, string> pars, ApiKeyInfo apiKeyInfo)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in pars.OrderBy(x => x.Key))
            {
                sb.Append(item.Key);
                sb.Append("=");
                sb.Append(item.Value);
                sb.Append("&");
            }
            var results = sb.ToString().TrimEnd('&');
            results = results + apiKeyInfo.Secret;
            return "&sign=" + results.ToMD5().ToLower();
        }

        /// <summary>
        /// 一些必要的参数
        /// </summary>
        /// <param name="needAccesskey">是否需要accesskey</param>
        /// <returns></returns>
        public static string MustParameter(ApiKeyInfo apikey, bool needAccesskey = false)
        {
            var url = "";
            if (needAccesskey && SettingService.Account.Logined)
            {
                url = $"access_key={SettingService.Account.AccessKey}&";
            }
            return url + $"appkey={apikey.Appkey}&build={build}&mobi_app={_mobi_app}&platform={_platform}&ts={TimeExtensions.GetTimestampS()}";
        }
        /// <summary>
        /// 默认一些请求头
        /// </summary>
        /// <returns></returns>
        public static IDictionary<string, string> GetDefaultHeaders()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("user-agent", "Mozilla/5.0 BiliDroid/5.44.2 (bbcallen@gmail.com)");
            headers.Add("Referer", "https://www.bilibili.com/");
            return headers;
        }

        public static IDictionary<string, string> GetAuroraHeaders()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("x-bili-aurora-zone", "sh001");
            headers.Add("x-bili-aurora-eid", "UlMFQVcABlAH");
            return headers;
        }

    }
}