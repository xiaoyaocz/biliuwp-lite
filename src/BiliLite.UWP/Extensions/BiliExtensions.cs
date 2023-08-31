using BiliLite.Models.Common;
using BiliLite.Models.Requests.Api;
using BiliLite.Models.Responses;
using BiliLite.Services;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace BiliLite.Extensions
{
    public static class BiliExtensions
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        /// <summary>
        /// 根据Epid取番剧ID
        /// </summary>
        /// <returns></returns>
        public static async Task<string> BangumiEpidToSid(string epid)
        {
            try
            {
                var re = await $"https://bangumi.bilibili.com/view/web_api/season?ep_id={epid}".GetString();
                var obj = JObject.Parse(re);
                return obj["result"]["season_id"].ToString();
            }
            catch (Exception)
            {
                return "";
            }
        }

        /// <summary>
        /// 短链接还原
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetShortLinkLocation(string shortlink)
        {
            try
            {
                HttpMessageHandler httpMessageHandler = new HttpClientHandler()
                {
                    AllowAutoRedirect = false,
                };
                using (HttpClient client = new HttpClient(httpMessageHandler))
                {
                    var response = await client.GetAsync(shortlink);
                    return response.Headers.Location.ToString();
                }
            }
            catch (Exception)
            {
                return shortlink;
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

        public static async Task CheckVersion()
        {
            try
            {
                var num = $"{SystemInformation.ApplicationVersion.Major}{SystemInformation.ApplicationVersion.Minor.ToString("00")}{SystemInformation.ApplicationVersion.Build.ToString("00")}";
                _logger.Log($"BiliLite.UWP version: {num}", LogType.Necessary);
                var result = await new GitApi().CheckUpdate().Request();
                var ver = JsonConvert.DeserializeObject<NewVersionResponse>(result.results);
                var ignoreVersion = SettingService.GetValue(SettingConstants.Other.IGNORE_VERSION, "");
                if (ignoreVersion.Equals(ver.Version)) return;
                // 获取临时版本号
                var revision = SystemInformation.ApplicationVersion.Revision;
                var v = int.Parse(num);
                // 存在临时版本号时，正式版本号减一
                if (revision > 0)
                {
                    v--;
                }
                if (ver.VersionNum > v)
                {
                    var dialog = new ContentDialog();

                    dialog.Title = $"发现新版本 Ver {ver.Version}";
                    MarkdownTextBlock markdownText = new MarkdownTextBlock()
                    {
                        Text = ver.VersionDesc,
                        TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap,
                        IsTextSelectionEnabled = true,
                        Background = new SolidColorBrush(Colors.Transparent)
                    };
                    markdownText.LinkClicked += new EventHandler<LinkClickedEventArgs>(async (sender, args) =>
                    {
                        await Launcher.LaunchUriAsync(new Uri(args.Link));
                    });
                    dialog.Content = markdownText;
                    dialog.PrimaryButtonText = "查看详情";
                    dialog.CloseButtonText = "取消";
                    dialog.SecondaryButtonText = "忽略该版本";

                    dialog.PrimaryButtonClick += new Windows.Foundation.TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs>(async (sender, e) =>
                    {
                        await Launcher.LaunchUriAsync(new Uri(ver.Url));
                    });
                    dialog.SecondaryButtonClick += (sender, e) =>
                    {
                        SettingService.SetValue(SettingConstants.Other.IGNORE_VERSION, ver.Version);
                    };
                    await dialog.ShowAsync();
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
