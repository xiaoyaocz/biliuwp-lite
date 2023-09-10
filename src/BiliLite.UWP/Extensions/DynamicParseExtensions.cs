using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Markup;
using BiliLite.Controls.Dynamic;
using BiliLite.Models.Common;
using BiliLite.Models.Common.UserDynamic;
using BiliLite.Models.Dynamic;
using BiliLite.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BiliLite.Extensions
{
    public static class DynamicParseExtensions
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        public static UserDynamicDisplayType ParseType(int type)
        {
            return UserDynamicDisplayTypesMap.Map.TryGetValue(type, out var result) ? result : UserDynamicDisplayType.Other;
        }

        public static UserDynamicItemDisplayOneRowInfo ParseOneRowInfo(UserDynamicDisplayType type, JObject obj)
        {
            var success = UserDynamicParseOneRowInfoActionsMap.Map.TryGetValue(type, out var parseFunc);
            return !success ? null : parseFunc(obj);
        }

        public static UserDynamicItemDisplayShortVideoInfo ParseShortVideoInfo(JObject obj)
        {
            try
            {
                return new UserDynamicItemDisplayShortVideoInfo()
                {
                    Height = obj["item"]["height"].ToInt32(),
                    Width = obj["item"]["width"].ToInt32(),
                    UploadTime = obj["item"]["upload_time"].ToString(),
                    VideoPlayUrl = obj["item"]["video_playurl"].ToString(),
                };
            }
            catch (Exception ex)
            {
                _logger.Error("解析短视频信息错误", ex);
                return null;
            }
        }
        /**
         * Command
         * UserCommand=>打开用户页面
         * LotteryCommand=>打开抽奖页面
         * LaunchUrlCommand=>打开网页
         * TagCommand=>打开话题
         **/


        /// <summary>
        /// 文本转为RichText
        /// </summary>
        /// <param name="id">动态id</param>
        /// <param name="txt"></param>
        /// <param name="emote"></param>
        /// <param name="extend_json"></param>
        /// <returns></returns>
        public static RichTextBlock UserDynamicStringToRichText(this string txt, string id, List<DynamicCardDisplayEmojiInfoItemModel> emote, JObject extend_json)
        {
            if (string.IsNullOrEmpty(txt)) return new RichTextBlock();
            var input = txt;
            try
            {
                //处理特殊字符
                input = input.Replace("&", "&amp;");
                input = input.Replace("<", "&lt;");
                input = input.Replace(">", "&gt;");

                //处理换行
                input = input.Replace("\r\n", "<LineBreak/>");
                input = input.Replace("\n", "<LineBreak/>");
                //处理@
                input = HandelAtAndVote(input, txt, extend_json);
                //处理网页🔗
                input = HandelUrl(input);

                //处理表情
                input = HandelEmoji(input, emote);
                //处理话题
                input = HandelTag(input);

                //互动抽奖🎁
                input = HandelLottery(input, id, extend_json);
                input = HandelVideoID(input);
                input = input.Replace("^x$%^", "@");
                //生成xaml
                var xaml = string.Format(@"<RichTextBlock HorizontalAlignment=""Stretch"" TextWrapping=""Wrap""  xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                                            xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns:d=""http://schemas.microsoft.com/expression/blend/2008""
    xmlns:mc = ""http://schemas.openxmlformats.org/markup-compatibility/2006"" LineHeight=""20"">
                                          <Paragraph>{0}</Paragraph>
                                      </RichTextBlock>", input);
                var p = (RichTextBlock)XamlReader.Load(xaml);
                return p;

            }
            catch (Exception ex)
            {
                _logger.Error("用户动态文本转富文本失败", ex);
                var tx = new RichTextBlock();
                var paragraph = new Paragraph();
                var run = new Run() { Text = txt };
                paragraph.Inlines.Add(run);
                tx.Blocks.Add(paragraph);
                return tx;
            }
        }

        /// <summary>
        /// 处理表情
        /// </summary>
        private static string HandelEmoji(string input, List<DynamicCardDisplayEmojiInfoItemModel> emote)
        {
            if (emote == null) return input;
            //替换表情
            var matchCollection = Regex.Matches(input, @"\[.*?\]");
            foreach (Match item in matchCollection)
            {
                if (emote.Count <= 0) continue;
                var name = item.Groups[0].Value;
                var emoji = emote.FirstOrDefault(x => x.emoji_name.Equals(name));
                if (emoji != null)
                {
                    input = input.Replace(item.Groups[0].Value, string.Format(@"<InlineUIContainer><Border Margin=""0 -4 4 -4""><Image Source=""{0}"" Width=""{1}"" Height=""{1}""/></Border></InlineUIContainer>",
                        emoji.url, 24));
                }
            }
            return input;
        }

        /// <summary>
        /// 处理标签
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string HandelTag(string input)
        {
            //处理话题
            var avMatchCollection = Regex.Matches(input, @"\#(.*?)\#");
            var handel = new List<string>();
            foreach (Match item in avMatchCollection)
            {
                if (handel.Contains(item.Groups[0].Value)) continue;
                var data = @"<InlineUIContainer><HyperlinkButton Command=""{Binding UserDynamicItemDisplayCommands.TagCommand}""  IsEnabled=""True"" Margin=""0 -4 4 -4"" Padding=""0"" " + string.Format(@" Tag=""{1}""  CommandParameter=""{1}"" ><TextBlock>{0}</TextBlock></HyperlinkButton></InlineUIContainer>",
                    item.Groups[0].Value, item.Groups[1].Value);
                handel.Add(item.Groups[0].Value);
                input = input.Replace(item.Groups[0].Value, data);

            }

            return input;
        }

        /// <summary>
        /// 处理URL链接
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string HandelUrl(string input)
        {
            var keyword = new List<string>();
            var urlMatchCollection = Regex.Matches(input, @"(https?|ftp|file)://[-A-Za-z0-9+&@#/%?=~_|!:,.;]+[-A-Za-z0-9+&@#/%=~_|]");
            foreach (Match item in urlMatchCollection)
            {
                if (keyword.Contains(item.Groups[0].Value))
                {
                    continue;
                }
                keyword.Add(item.Groups[0].Value);
                var data = @"<InlineUIContainer><HyperlinkButton x:Name=""btn"" Command=""{Binding UserDynamicItemDisplayCommands.LaunchUrlCommand}""  IsEnabled=""True"" Margin=""0 -4 0 -4"" Padding=""0"" " +
                    string.Format(@" CommandParameter=""{0}"" ><TextBlock>🔗网页链接</TextBlock></HyperlinkButton></InlineUIContainer>", item.Groups[0].Value);
                input = input.Replace(item.Groups[0].Value, data);
            }


            return input;
        }

        /// <summary>
        /// 处理At及投票
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string HandelAtAndVote(string input, string origin_content, JObject extendJson)
        {
            var content = origin_content;
            var ctrls = new List<UserDynamicCtrlItem>();
            if (extendJson.TryGetValue("ctrl", out var ctrl))
            {
                ctrls = JsonConvert.DeserializeObject<List<UserDynamicCtrlItem>>(ctrl.ToString());
            }
            if (extendJson.TryGetValue("at_control", out var atControl))
            {
                ctrls = JsonConvert.DeserializeObject<List<UserDynamicCtrlItem>>(atControl.ToString());
            }
            if (ctrls == null) return input;

            foreach (var item in ctrls)
            {
                //@
                if (item.Type == 1)
                {
                    try
                    {
                        var d = content.Substring(item.Location, item.Length);
                        var index = input.IndexOf(d);
                        input = input.Remove(index, item.Length);
                        var run = @"<InlineUIContainer><HyperlinkButton Command=""{Binding UserDynamicItemDisplayCommands.UserCommand}""  IsEnabled=""True"" Margin=""0 -4 4 -4"" Padding=""0"" " + string.Format(@" Tag=""{1}""  CommandParameter=""{1}"" ><TextBlock>{0}</TextBlock></HyperlinkButton></InlineUIContainer>", d.Replace("@", "^x$%^"), item.Data);
                        input = input.Insert(index, run);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("处理At及投票信息失败", ex);
                    }
                }
                //投票
                if (item.Type == 3)
                {
                    var d = content.Substring(item.Location, content.Length - item.Location);
                    var index = input.IndexOf(d);
                    input = input.Remove(index, content.Length - item.Location);
                    var run = @"<InlineUIContainer><HyperlinkButton Command=""{Binding UserDynamicItemDisplayCommands.VoteCommand}""  IsEnabled=""True"" Margin=""0 -4 4 -4"" Padding=""0"" " + string.Format(@" Tag=""{1}""  CommandParameter=""{1}"" ><TextBlock>{0}</TextBlock></HyperlinkButton></InlineUIContainer>",
                        "📊" + d, extendJson["vote"]?["vote_id"]?.ToInt32() ?? 0);
                    input = input.Insert(index, run);
                }
            }
            return input;
        }

        /// <summary>
        /// 处理抽奖
        /// </summary>
        /// <param name="input"></param>
        /// <param name="extendJson"></param>
        /// <returns></returns>
        private static string HandelLottery(string input, string id, JObject extendJson)
        {
            if (!extendJson.ContainsKey("lott")) return input;

            if (input.IndexOf("互动抽奖") == 1)
            {
                input = input.Remove(1, 4);
            }
            input = input.Insert(0, $@"<InlineUIContainer><HyperlinkButton Command=""{{Binding UserDynamicItemDisplayCommands.LotteryCommand}}""  CommandParameter=""{id}"" IsEnabled=""True"" Margin=""0 -4 4 -4"" Padding=""0"" ><TextBlock>🎁互动抽奖</TextBlock></HyperlinkButton></InlineUIContainer>");
            return input;
        }

        /// <summary>
        /// 处理视频AVID,BVID,CVID
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string HandelVideoID(string input)
        {
            var keyword = new List<string>();
            //如果是链接就不处理了
            if (!Regex.IsMatch(input, @"/[aAbBcC][vV]([a-zA-Z0-9]+)"))
            {
                //处理AV号
                var av = Regex.Matches(input, @"[aA][vV](\d+)");
                foreach (Match item in av)
                {
                    if (keyword.Contains(item.Groups[0].Value))
                    {
                        continue;
                    }
                    keyword.Add(item.Groups[0].Value);
                    var urlPrefix = "bilibili://video/";
                    var data =
                        $"<InlineUIContainer><HyperlinkButton Command=\"{{Binding {nameof(UserDynamicItemDisplayCommands)}.{nameof(UserDynamicItemDisplayCommands.LaunchUrlCommand)}}}\"  IsEnabled=\"True\" Margin=\"0 -4 0 -4\" Padding=\"0\" " +
                        $" CommandParameter=\"{urlPrefix}{item.Groups[0].Value}\" ><TextBlock>{item.Groups[0].Value}</TextBlock></HyperlinkButton></InlineUIContainer>";
                    input = input.Replace(item.Groups[0].Value, data);
                }

                //处理BV号
                var bv = Regex.Matches(input, @"[bB][vV]([a-zA-Z0-9]{8,})");
                foreach (Match item in bv)
                {
                    if (keyword.Contains(item.Groups[0].Value))
                    {
                        continue;
                    }
                    keyword.Add(item.Groups[0].Value);
                    var urlPrefix = "bilibili://video/";
                    var data =
                        $"<InlineUIContainer><HyperlinkButton Command=\"{{Binding {nameof(UserDynamicItemDisplayCommands)}.{nameof(UserDynamicItemDisplayCommands.LaunchUrlCommand)}}}\"  IsEnabled=\"True\" Margin=\"0 -4 0 -4\" Padding=\"0\" " +
                        $" CommandParameter=\"{urlPrefix}{item.Groups[0].Value}\" ><TextBlock>{item.Groups[0].Value}</TextBlock></HyperlinkButton></InlineUIContainer>";
                    input = input.Replace(item.Groups[0].Value, data);
                }

                //处理CV号
                var cv = Regex.Matches(input, @"[cC][vV](\d+)");
                foreach (Match item in cv)
                {
                    if (keyword.Contains(item.Groups[0].Value))
                    {
                        continue;
                    }
                    keyword.Add(item.Groups[0].Value);
                    var urlPrefix = "bilibili://article/";
                    var data =
                        $"<InlineUIContainer><HyperlinkButton Command=\"{{Binding {nameof(UserDynamicItemDisplayCommands)}.{nameof(UserDynamicItemDisplayCommands.LaunchUrlCommand)}}}\"  IsEnabled=\"True\" Margin=\"0 -4 0 -4\" Padding=\"0\" " +
                        $" CommandParameter=\"{urlPrefix}{item.Groups[1].Value}\" ><TextBlock>{item.Groups[0].Value}</TextBlock></HyperlinkButton></InlineUIContainer>";
                    input = input.Replace(item.Groups[0].Value, data);
                }
            }
            keyword.Clear();
            keyword = null;
            return input;
        }
    }
}
