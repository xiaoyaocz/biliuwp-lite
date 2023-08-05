using Newtonsoft.Json;
using System.Text;
using System;
using BiliLite.Models.Common;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Markup;
using OpenCCNET;
using BiliLite.Services;
using Windows.UI;
using Windows.Security.Cryptography.Core;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using System.Threading.Tasks;

namespace BiliLite.Extensions
{
    /// <summary>
    /// cc转为SRT
    /// </summary>
    /// <param name="json">CC字幕</param>
    /// <param name="toSimplified">转为简体</param>
    /// <returns></returns>
    public static class StringExtensions
    {
        public static string CcConvertToSrt(this string json, bool toSimplified = false)
        {
            var subtitle = JsonConvert.DeserializeObject<Subtitle>(json);
            var stringBuilder = new StringBuilder();
            var i = 1;
            foreach (var item in subtitle.body)
            {
                var start = TimeSpan.FromSeconds(item.from);
                var end = TimeSpan.FromSeconds(item.to);
                stringBuilder.AppendLine(i.ToString());
                stringBuilder.AppendLine($"{start:hh\\:mm\\:ss\\,fff} --> {end:hh\\:mm\\:ss\\,fff}");
                var content = item.content;
                if (toSimplified)
                {
                    content = content.ToHansFromTW(true);
                }

                stringBuilder.AppendLine(content);
                stringBuilder.AppendLine();
                i++;
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// 简体转繁体
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string SimplifiedToTraditional(this string input)
        {
            return input.ToHKFromHans();
        }

        /// <summary>
        /// 繁体转简体
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string TraditionalToSimplified(this string input)
        {
            return input.ToHansFromTW(true);
        }

        /// <summary>
        /// 文本转富文本控件
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="emote"></param>
        /// <returns></returns>
        public static RichTextBlock ToRichTextBlock(this string txt, JObject emote)
        {
            string input = txt;
            try
            {
                if (txt != null)
                {

                    //处理特殊字符
                    input = input.Replace("&", "&amp;");
                    input = input.Replace("<", "&lt;");
                    input = input.Replace(">", "&gt;");
                    input = input.Replace("\r\n", "<LineBreak/>");
                    input = input.Replace("\n", "<LineBreak/>");
                    //处理链接
                    input = HandelUrl(input);

                    //处理表情
                    input = HandelEmoji(input, emote);

                    //处理av号
                    input = HandelVideoID(input);



                    //生成xaml
                    var xaml = string.Format(@"<RichTextBlock HorizontalAlignment=""Stretch"" TextWrapping=""Wrap""  xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                                            xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns:d=""http://schemas.microsoft.com/expression/blend/2008""
    xmlns:mc = ""http://schemas.openxmlformats.org/markup-compatibility/2006"" LineHeight=""20"">
                                          <Paragraph>{0}</Paragraph>
                                      </RichTextBlock>", input);
                    var p = (RichTextBlock)XamlReader.Load(xaml);
                    return p;
                }
                else
                {
                    var tx = new RichTextBlock();
                    Paragraph paragraph = new Paragraph();
                    Run run = new Run() { Text = txt };
                    paragraph.Inlines.Add(run);
                    tx.Blocks.Add(paragraph);
                    return tx;
                }
            }
            catch (Exception)
            {
                var tx = new RichTextBlock();
                Paragraph paragraph = new Paragraph();
                Run run = new Run() { Text = txt };
                paragraph.Inlines.Add(run);
                tx.Blocks.Add(paragraph);
                return tx;

            }
        }

        public static string ProtectValues(this string url, params string[] keys)
        {
            foreach (string key in keys)
            {
                string pattern = $@"({key}=)([^&]*)";
                string replacement = $"$1{{hasValue}}";
                url = Regex.Replace(url, pattern, replacement);
            }
            return url;
        }

        public static string ChooseProxyServer(this string area)
        {
            var proxyUrl = SettingService.GetValue(SettingConstants.Roaming.CUSTOM_SERVER_URL, ApiHelper.ROMAING_PROXY_URL);
            var proxyUrlCN = SettingService.GetValue(SettingConstants.Roaming.CUSTOM_SERVER_URL_CN, "");
            var proxyUrlHK = SettingService.GetValue(SettingConstants.Roaming.CUSTOM_SERVER_URL_HK, "");
            var proxyUrlTW = SettingService.GetValue(SettingConstants.Roaming.CUSTOM_SERVER_URL_TW, "");
            switch (area)
            {
                case "cn":
                    return string.IsNullOrEmpty(proxyUrlCN) ? proxyUrl : proxyUrlCN;
                case "hk":
                    return string.IsNullOrEmpty(proxyUrlHK) ? proxyUrl : proxyUrlHK;
                case "tw":
                    return string.IsNullOrEmpty(proxyUrlTW) ? proxyUrl : proxyUrlTW;
                default:
                    return proxyUrl;
            }
        }

        public static string ParseArea(this string title, long mid)
        {
            if (Regex.IsMatch(title, @"僅.*港.*地區"))
            {
                return "hk";
            }
            else if (Regex.IsMatch(title, @"僅.*台.*地區"))
            {
                return "tw";
            }
            //如果是哔哩哔哩番剧出差这个账号上传的
            //且标题不含僅**地區，返回地区设置为港澳台
            if (mid == 11783021)
            {
                return "hk";
            }
            return "cn";
        }

        public static string ParseArea(this string title, string mid)
        {
            return title.ParseArea(mid.ToInt32());
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToMD5(this string input)
        {
            var provider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            IBuffer buffer = CryptographicBuffer.ConvertStringToBinary(input, BinaryStringEncoding.Utf8);
            var hashed = provider.HashData(buffer);
            var result = CryptographicBuffer.EncodeToHexString(hashed);
            return result;
        }

        public static string ToSimplifiedChinese(this string content)
        {
            content = content.TraditionalToSimplified();
            return content;
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
            return await Task.Run(() =>
            {
                return JsonConvert.DeserializeObject<T>(results);
            });
        }
        public static bool SetClipboard(this string content)
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

        public static Color StrToColor(this string obj)
        {
            obj = obj.Replace("#", "");
            if (int.TryParse(obj, out var c))
            {
                obj = c.ToString("X2");
            }
            Color color = new Color();
            if (obj.Length <= 6)
            {
                obj = obj.PadLeft(6, '0');
                color.R = byte.Parse(obj.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                color.G = byte.Parse(obj.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                color.B = byte.Parse(obj.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                color.A = 255;
            }
            else
            {
                obj = obj.PadLeft(8, '0');
                color.R = byte.Parse(obj.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                color.G = byte.Parse(obj.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                color.B = byte.Parse(obj.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
                color.A = byte.Parse(obj.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return color;
        }

        #region Private methods

        /// <summary>
        /// 处理表情
        /// </summary>
        private static string HandelEmoji(string input, JObject emote)
        {
            if (emote == null) return input;
            //替换表情
            MatchCollection mc = Regex.Matches(input, @"\[.*?\]");
            foreach (Match item in mc)
            {
                if (emote != null && emote.ContainsKey(item.Groups[0].Value))
                {
                    var emoji = emote[item.Groups[0].Value];
                    input = input.Replace(item.Groups[0].Value,
                        string.Format(
                            @"<InlineUIContainer><Border  Margin=""0 -4 4 -4""><Image Source=""{0}"" Width=""{1}"" Height=""{1}"" /></Border></InlineUIContainer>",
                            emoji["url"].ToString(), emoji["meta"]["size"].ToInt32() == 1 ? "20" : "36"));
                }
            }

            return input;
        }

        /// <summary>
        /// 处理视频AVID,BVID,CVID
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string HandelVideoID(string input)
        {
            //处理AV号
            List<string> keyword = new List<string>();
            //如果是链接就不处理了
            if (!Regex.IsMatch(input, @"/[aAbBcC][vV]([a-zA-Z0-9]+)"))
            {
                //处理AV号
                MatchCollection av = Regex.Matches(input, @"[aA][vV](\d+)");
                foreach (Match item in av)
                {
                    if (keyword.Contains(item.Groups[0].Value))
                    {
                        continue;
                    }

                    keyword.Add(item.Groups[0].Value);
                    var data =
                        @"<InlineUIContainer><HyperlinkButton Command=""{Binding LaunchUrlCommand}""  IsEnabled=""True"" Margin=""0 -4 0 -4"" Padding=""0"" " +
                        string.Format(
                            @" CommandParameter=""{1}"" ><TextBlock>{0}</TextBlock></HyperlinkButton></InlineUIContainer>",
                            item.Groups[0].Value, "bilibili://video/" + item.Groups[0].Value);
                    input = input.Replace(item.Groups[0].Value, data);
                }

                //处理AV号
                MatchCollection bv = Regex.Matches(input, @"[bB][vV]([a-zA-Z0-9]{8,})");
                foreach (Match item in bv)
                {
                    if (keyword.Contains(item.Groups[0].Value))
                    {
                        continue;
                    }

                    keyword.Add(item.Groups[0].Value);
                    var data =
                        @"<InlineUIContainer><HyperlinkButton Command=""{Binding LaunchUrlCommand}""  IsEnabled=""True"" Margin=""0 -4 0 -4"" Padding=""0"" " +
                        string.Format(
                            @" CommandParameter=""{1}"" ><TextBlock>{0}</TextBlock></HyperlinkButton></InlineUIContainer>",
                            item.Groups[0].Value, "bilibili://video/" + item.Groups[0].Value);
                    input = input.Replace(item.Groups[0].Value, data);
                }

                //处理CV号

                MatchCollection cv = Regex.Matches(input, @"[cC][vV](\d+)");
                foreach (Match item in cv)
                {
                    if (keyword.Contains(item.Groups[0].Value))
                    {
                        continue;
                    }

                    keyword.Add(item.Groups[0].Value);
                    var data =
                        @"<InlineUIContainer><HyperlinkButton Command=""{Binding LaunchUrlCommand}""  IsEnabled=""True"" Margin=""0 -4 0 -4"" Padding=""0"" " +
                        string.Format(
                            @" CommandParameter=""{1}"" ><TextBlock>{0}</TextBlock></HyperlinkButton></InlineUIContainer>",
                            item.Groups[0].Value, "bilibili://article/" + item.Groups[1].Value);
                    input = input.Replace(item.Groups[0].Value, data);
                }
            }

            keyword.Clear();
            keyword = null;
            return input;
        }

        /// <summary>
        /// 处理URL链接
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string HandelUrl(string input)
        {
            //处理AV号
            List<string> keyword = new List<string>();
            MatchCollection url = Regex.Matches(input,
                @"(https?|ftp|file)://[-A-Za-z0-9+&@#/%?=~_|!:,.;]+[-A-Za-z0-9+&@#/%=~_|]");
            foreach (Match item in url)
            {
                if (keyword.Contains(item.Groups[0].Value))
                {
                    continue;
                }

                keyword.Add(item.Groups[0].Value);
                var data =
                    @"<InlineUIContainer><HyperlinkButton x:Name=""btn"" Command=""{Binding LaunchUrlCommand}""  IsEnabled=""True"" Margin=""0 -4 0 -4"" Padding=""0"" " +
                    string.Format(
                        @" CommandParameter=""{0}"" ><TextBlock>🔗网页链接</TextBlock></HyperlinkButton></InlineUIContainer>",
                        item.Groups[0].Value);
                input = input.Replace(item.Groups[0].Value, data);
            }


            return input;

            //MatchCollection url = Regex.Matches(input, @"(https?|ftp|file)://[-A-Za-z0-9+&@#/%?=~_|!:,.;]+[-A-Za-z0-9+&@#/%=~_|]");
            //foreach (Match item in url)
            //{
            //    var data = @"<InlineUIContainer><HyperlinkButton x:Name=""btn"" Command=""{Binding LaunchUrlCommand}""  IsEnabled=""True"" Margin=""0 -4 0 -4"" Padding=""0"" " + string.Format(@" Tag=""{0}""  CommandParameter=""{0}"" >{0}</HyperlinkButton></InlineUIContainer>", item.Groups[0].Value);
            //    input = input.Replace(item.Groups[0].Value, data);
            //}


            //return input;
        }

        #endregion
    }
}
