using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.WebApi
{
    public static class Utils
    {
        public static string appkey = "1d8b6e7d45233436";
        public static string appsecret = "560c52ccd288fed045859ed18bffd973";
        public static string GetSign(string url)
        {
            string result;
            string str = url.Substring(url.IndexOf("?", 4) + 1);
            List<string> list = str.Split('&').ToList();
            list.Sort();
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string str1 in list)
            {
                stringBuilder.Append((stringBuilder.Length > 0 ? "&" : string.Empty));
                stringBuilder.Append(str1);
            }
            stringBuilder.Append(appsecret);
            result = ToMD5(stringBuilder.ToString()).ToLower();
            return url+"&sign=" + result;
        }
        public static string ToMD5(string input)
        {
            MD5 mD5 = MD5.Create();
            var com= mD5.ComputeHash(Encoding.UTF8.GetBytes(input));
            var str = "";
            foreach (var item in com)
            {
                str += item.ToString("x2");
            }
            return str;
        }
        public static string ToStatusCodeMessage(this int statuscode)
        {
            switch (statuscode)
            {
                case 401:
                    return "未授权";
                case 404:
                    return "没有找到请求的资源";
                case 500:
                    return "服务器出错";
                case 503:
                    return "请求过快";
                default:
                    return statuscode.ToString();
            }
        }
        /// <summary>
        /// 生成时间戳/秒
        /// </summary>
        /// <returns></returns>
        public static long GetTimestampS()
        {
            return Convert.ToInt64((DateTime.Now - new DateTime(1970, 1, 1, 8, 0, 0, 0)).TotalSeconds);
        }
        /// <summary>
        /// 生成时间戳/豪秒
        /// </summary>
        /// <returns></returns>
        public static long GetTimestampMS()
        {
            return Convert.ToInt64((DateTime.Now - new DateTime(1970, 1, 1, 8, 0, 0, 0)).TotalMilliseconds);
        }
        public static int ToInt32(this object obj)
        {

            if (int.TryParse(obj.ToString(), out var value))
            {
                return value;
            }
            else
            {
                return 0;
            }
        }
        public static string NumberToString(this object value)
        {
            var number = System.Convert.ToDouble(value);
            if (number >= 10000)
            {
                return ((double)number / 10000).ToString("0.0") + "万";
            }
            return number.ToString();
        }
    }
}
