using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Account;
using BiliLite.Models.Requests.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

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
        private static int[] mixinKeyEncTab = new int[] {
            46, 47, 18, 2, 53, 8, 23, 32, 15, 50, 10, 31, 58, 3, 45, 35, 27, 43, 5, 49,
            33, 9, 42, 19, 29, 28, 14, 39, 12, 38, 41, 13, 37, 48, 7, 16, 24, 55, 40,
            61, 26, 17, 0, 1, 60, 51, 30, 4, 22, 25, 54, 21, 56, 59, 6, 63, 57, 62, 11,
            36, 20, 34, 44, 52
        };

        private static async Task<(string, string)> GetWbiKeys()
        {
            // 获取最新的 img_key 和 sub_key
            var response = await new AccountApi().Nav().Request();
            var result = await response.GetData<WebInterfaceNav>();
            var imgUrl = result.data.WbiImg.ImgUrl;
            var subUrl = result.data.WbiImg.SubUrl;
            var imgKey = imgUrl.Substring(imgUrl.LastIndexOf('/') + 1).Split('.')[0];
            var subKey = subUrl.Substring(subUrl.LastIndexOf('/') + 1).Split('.')[0];
            return (imgKey, subKey);
        }

        private static string GetMixinKey(string origin)
        {
            // 对 imgKey 和 subKey 进行字符顺序打乱编码
            return mixinKeyEncTab.Aggregate("", (s, i) => s + origin[i]).Substring(0, 32);
        }

        public static async Task<string> GetWbiSign(string url)
        {
            var (imgKey, subKey) = await GetWbiKeys();

            // 为请求参数进行 wbi 签名
            var mixinKey = GetMixinKey(imgKey + subKey);
            var currentTime = (long)Math.Round(DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);

            var queryString = HttpUtility.ParseQueryString(url);

            var queryParams = queryString.Cast<string>().ToDictionary(k => k, v => queryString[v]);
            queryParams["wts"] = currentTime + ""; // 添加 wts 字段
            queryParams = queryParams.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value); // 按照 key 重排参数
                                                                                                  // 过滤 value 中的 "!'()*" 字符
            queryParams = queryParams.ToDictionary(x => x.Key, x => string.Join("", x.Value.ToString().Where(c => "!'()*".Contains(c) == false)));

            var query = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));

            var wbi_sign = $"{query}{mixinKey}".ToMD5();

            return $"{query}&w_rid={wbi_sign}";
        }

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
        /// 获取访问令牌参数
        /// </summary>
        /// <param name="apikey"></param>
        /// <returns></returns>
        public static string GetAccessParameter(ApiKeyInfo apikey)
        {
            var url = $"access_key={SettingService.Account.AccessKey}&appkey={apikey.Appkey}";
            return url;
        }

        public static string GetBuvid()
        {
            var mac = new List<string>();
            for (var i = 0; i < 6; i++)
            {
                var min = Math.Min(0, 0xff);
                var max = Math.Max(0, 0xff);
                var num = int.Parse((new Random().Next(min, max + 1)).ToString()).ToString("x2");
                mac.Add(num);
            }
            var md5 = string.Join(":", mac).ToMD5();
            var md5Arr = md5.ToCharArray();
            return $"XY{md5Arr[2]}{md5Arr[12]}{md5Arr[22]}{md5}";
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