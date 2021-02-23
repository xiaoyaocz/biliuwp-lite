using BiliLite.Api;
using BiliLite.Controls;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls;
using Windows.UI;
using System.IO;
using BiliLite.Dialogs;
using Windows.UI.Popups;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace BiliLite.Helpers
{
    public static class Utils
    {
        /// <summary>
        /// 发送请求，扩展方法
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        public async static Task<HttpResults> Request(this ApiModel api)
        {
            if (api.method == RestSharp.Method.GET)
            {
                return await HttpHelper.Get(api.url, api.headers);
            }
            else
            {
                return await HttpHelper.Post(api.url, api.body, api.headers);
            }
        }

        /// <summary>
        /// 默认一些请求头
        /// </summary>
        /// <returns></returns>
        public static IDictionary<string, string> GetDefaultHeaders()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("user-agent", "Mozilla/5.0 BiliDroid/5.34.1 (bbcallen@gmail.com)");
            headers.Add("Referer", "https://www.bilibili.com/");
            return headers;
        }
        /// <summary>
        /// 将时间戳转为时间
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        public static DateTime TimestampToDatetime(long ts)
        {
            DateTime dtStart = new DateTime(1970, 1, 1, 8, 0, 0);
            long lTime = long.Parse(ts + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
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

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToMD5(string input)
        {
            var provider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            IBuffer buffer = CryptographicBuffer.ConvertStringToBinary(input, BinaryStringEncoding.Utf8);
            var hashed = provider.HashData(buffer);
            var result = CryptographicBuffer.EncodeToHexString(hashed);
            return result;
        }
        public static void ShowMessageToast(string message,int seconds = 2)
        {
            MessageToast ms = new MessageToast(message, TimeSpan.FromSeconds(seconds));
            ms.Show();
        }
        public static void ShowMessageToast(string message, List<MyUICommand> commands, int seconds = 15)
        {
            MessageToast ms = new MessageToast(message, TimeSpan.FromSeconds(seconds), commands);
            ms.Show();
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
        public static string ToCountString(this object obj)
        {
            if (obj == null) return "0";
            if (double.TryParse(obj.ToString(), out var number))
            {
               
                if (number >= 10000)
                {
                    return ((double)number / 10000).ToString("0.0") + "万";
                }
                return obj.ToString();
            }
            else
            {
                return obj.ToString();
            }
        }

        /// <summary>
        /// 根据Epid取番剧ID
        /// </summary>
        /// <returns></returns>
        public async static Task<string> BangumiEpidToSid(string url)
        {
            try
            {
                if (!url.Contains("http"))
                {
                    url = "https://www.bilibili.com/bangumi/play/ep" + url;
                }

                var re = await HttpHelper.GetString(url);
                var data = RegexMatch(re, @"ss(\d+)");
                if (data != "")
                {
                    return data;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception)
            {
                return "";
            }
        }
        private static bool dialogShowing = false;
        public async static Task<bool> ShowLoginDialog()
        {
            if (!dialogShowing)
            {
                LoginDialog login = new LoginDialog();
                dialogShowing = true;
                await login.ShowAsync();
                dialogShowing = false;
            }
            if (SettingHelper.Account.Logined)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async static Task<bool> ShowDialog(string title,string content)
        {
            MessageDialog messageDialog = new MessageDialog(content,title);
            messageDialog.Commands.Add(new UICommand() { Label="确定",Id=true});
            messageDialog.Commands.Add(new UICommand() { Label = "取消", Id = false });
           var result= await messageDialog.ShowAsync();
            return (bool)result.Id;
        }

        public static string RegexMatch(string input, string regular)
        {
            var data = Regex.Match(input, regular);
            if (data.Groups.Count >= 2 && data.Groups[1].Value != "")
            {
                return data.Groups[1].Value;
            }
            else
            {
                return "";
            }
        }
        public static async Task<T> DeserializeJson<T>(this string results)
        {
            return await Task.Run<T>(() =>
            {
                return JsonConvert.DeserializeObject<T>(results);
            });
        }

        public static bool SetClipboard(string content)
        {
            try
            {
                Windows.ApplicationModel.DataTransfer.DataPackage pack = new Windows.ApplicationModel.DataTransfer.DataPackage();
                pack.SetText(content);
                Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(pack);
                Windows.ApplicationModel.DataTransfer.Clipboard.Flush();
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public static string HandelTimestamp(string ts)
        {
            if (ts.Length == 10)
            {
                ts += "0000000";
            }
            DateTime dtStart = new DateTime(1970, 1, 1, 0, 0, 0);
            long lTime = long.Parse(ts);
            TimeSpan toNow = new TimeSpan(lTime);
            DateTime dt = dtStart.Add(toNow).ToLocalTime();
            TimeSpan span = DateTime.Now.Date - dt.Date;
            if (span.TotalDays <= 0)
            {
                return "今天" + dt.ToString("HH:mm");
            }
            else if (span.TotalDays >= 1 && span.TotalDays < 2)
            {
                return "昨天" + dt.ToString("HH:mm");
            }
            else
            {
                return dt.ToString("yyyy-MM-dd HH:mm");
            }
        }

        public async static Task CheckVersion()
        {
            try
            {
                var url = $"https://cdn.jsdelivr.net/gh/xiaoyaocz/biliuwp-lite/BiliLite/Assets/Text/new_version.json?ts{Utils.GetTimestampS()}";
                var result = await HttpHelper.GetString(url);
                var ver = JsonConvert.DeserializeObject<NewVersion>(result);
                var num = $"{ SystemInformation.ApplicationVersion.Major }{ SystemInformation.ApplicationVersion.Minor.ToString("00")}{ SystemInformation.ApplicationVersion.Build.ToString("00")}";
                var v= int.Parse(num);
                if (ver.version_num>v)
                {
                    var dialog = new ContentDialog();

                    dialog.Title = $"发现新版本 Ver {ver.version}";
                    MarkdownTextBlock markdownText = new MarkdownTextBlock() {
                        Text= ver.version_desc,
                        TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap,
                        IsTextSelectionEnabled = true,
                        Background=new SolidColorBrush(Colors.Transparent)
                    };
                    markdownText.LinkClicked += new EventHandler<LinkClickedEventArgs>(async (sender, args) => 
                    {
                        await Launcher.LaunchUriAsync(new Uri(args.Link));
                    });
                    dialog.Content = markdownText;
                    dialog.PrimaryButtonText = "查看详情";
                    dialog.SecondaryButtonText = "忽略";

                    dialog.PrimaryButtonClick += new Windows.Foundation.TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs>(async (sender, e) =>
                    {
                        await Windows.System.Launcher.LaunchUriAsync(new Uri(ver.url));
                    });
                    await dialog.ShowAsync();
                }
            }
            catch (Exception)
            {
            }
        }

        public static Color ToColor(this string obj)
        {
            obj = obj.Replace("#", "");
            if (int.TryParse(obj,out var c))
            {
                obj = c.ToString("X2");
            }
            Color color = new Color();
            if (obj.Length == 4)
            {
                obj = "00" + obj;
            }
            if (obj.Length == 6)
            {
                color.R = byte.Parse(obj.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                color.G = byte.Parse(obj.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                color.B = byte.Parse(obj.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                color.A = 255;
            }
            if (obj.Length == 8)
            {
                color.R = byte.Parse(obj.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                color.G = byte.Parse(obj.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                color.B = byte.Parse(obj.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
                color.A = byte.Parse(obj.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return color;
        }
        public static void ReadB(this Stream stream, byte[] buffer, int offset, int count)
        {
            if (offset + count > buffer.Length)
                throw new ArgumentException();
            var read = 0;
            while (read < count)
            {
                var available = stream.Read(buffer, offset, count - read);
                if (available == 0)
                {
                    // throw new ObjectDisposedException(null);
                }
                read += available;
                offset += available;
            }
        }
      
        public static T ObjectClone<T>(this T obj)
        {
            var type = typeof(T);

            if (!type.IsSerializable)
                return default(T);

            if (Object.ReferenceEquals(obj, null))
                return default(T);

            IFormatter format = new BinaryFormatter();

            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    format.Serialize(ms, obj);
                    ms.Seek(0, SeekOrigin.Begin);
                    return (T)format.Deserialize(ms);
                }
                catch (Exception e)
                {
                    return default(T);
                }
            }
        }
    }
    public class NewVersion
    {
        public string version { get; set; }
        public string version_desc { get; set; }
        public int version_num { get; set; }
        public string url { get; set; }
    }
}
