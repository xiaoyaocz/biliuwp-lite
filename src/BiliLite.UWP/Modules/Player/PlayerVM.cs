using BiliLite.Models.Requests.Api;
using BiliLite.Modules.Player.Playurl;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Atelier39;
using Bilibili.Tv.Interfaces.Dm.V1;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Services;
using BiliLite.Models.Common.Video;
using BiliLite.Models.Common.Video.PlayUrlInfos;

namespace BiliLite.Modules
{
    public class PlayerVM : IModules
    {
        private static readonly ILogger logger = GlobalLogger.FromCurrentType();

        readonly gRPC.Api.PlayURL playUrlApi;
        readonly PlayerAPI PlayerAPI;
        readonly BiliPlayUrl biliPlayUrl;
        public PlayerVM()
        {
            playUrlApi = new gRPC.Api.PlayURL();
            PlayerAPI = new PlayerAPI();
            biliPlayUrl = new BiliPlayUrl(false);
            DefaultDanmakuColors = new List<string>() {
                "#FE0302",
                "#FF7204",
                "#FFAA02",
                "#FFD302",
                "#FFFF00",
                "#A0EE00",
                "#00CD00",
                "#019899",
                "#4266BE",
                "#89D5FF",
                "#CC0273",
                "#222222",
                "#9B9B9B",
                "#FFFFFF"
            };
        }
        public PlayerVM(bool isDownload)
        {
            playUrlApi = new gRPC.Api.PlayURL();
            PlayerAPI = new PlayerAPI();
            biliPlayUrl = new BiliPlayUrl(isDownload);
            IsDownload = isDownload;
        }
        private readonly bool IsDownload;
        public List<string> DefaultDanmakuColors { get; set; }

        public async Task<BiliPlayUrlQualitesInfo> GetPlayUrls(PlayInfo playInfo, int qn, int soundQualityId = 0)
        {
            var result = await biliPlayUrl.GetPlayUrl(playInfo, qn, soundQualityId);
            return result;
        }

        public async Task ReportHistory(PlayInfo playInfo, double progress)
        {
            try
            {
                var api = PlayerAPI.SeasonHistoryReport(playInfo.avid, playInfo.cid, Math.Floor(progress).ToInt32(), playInfo.season_id, playInfo.ep_id, playInfo.play_mode == VideoPlayType.Video ? 3 : 4);
                await api.Request();
                Debug.WriteLine(progress);
            }
            catch (Exception ex)
            {
                var handel = HandelError<PlayerVM>(ex);
                Notify.ShowMessageToast(handel.message);
            }
        }
        public async Task<PlayerInfo> GetPlayInfo(string aid, string cid)
        {
            var playerInfo = new PlayerInfo();
            try
            {
                var api = await PlayerAPI.GetPlayerInfo(aid: aid, cid: cid, "");
                var result = await api.Request();
                if (result.status)
                {
                    var data = await result.GetData<PlayerInfo>();
                    if (data.code == 0)
                    {
                        playerInfo = data.data;
                    }
                    return playerInfo;

                }
                else
                {
                    return playerInfo;
                }
            }
            catch (Exception ex)
            {
                var data = HandelError<object>(ex);
                return playerInfo;
            }
        }
        public async Task<SubtitleModel> GetSubtitle(string url)
        {
            try
            {
                if (!url.Contains("//"))
                {
                    var jsonFile = await StorageFile.GetFileFromPathAsync(url);

                    return JsonConvert.DeserializeObject<SubtitleModel>(await FileIO.ReadTextAsync(jsonFile));
                }
                if (!url.Contains("http:") || !url.Contains("https:"))
                {
                    url = "https:" + url;
                }
                var results = await url.GetString();
                return JsonConvert.DeserializeObject<SubtitleModel>(results);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<DanmakuItem>> GetDanmakuForDanmakuFrostMaster(string cid, int segment_index = 1)
        {
            var danmuList = new List<DanmakuItem>();
            try
            {
#if DEBUG
                var sw = Stopwatch.StartNew();
                sw.Start();
#endif

                var data = await PlayerAPI.SegDanmaku(cid, segment_index).url.GetStream();
                var result = DmSegMobileReply.Parser.ParseFrom(data);

                danmuList = result.Elems.MapToDanmakuItems();
#if DEBUG
                sw.Stop();
                Debug.WriteLine($"获取弹幕耗时：{sw.ElapsedMilliseconds}ms");
#endif
            }
            catch (Exception ex)
            {
                Notify.ShowMessageToast("弹幕加载失败:" + ex.Message);
                logger.Log("grpc弹幕加载失败", LogType.Fatal, ex);
            }
            return danmuList;
        }

        public async Task<List<NSDanmaku.Model.DanmakuModel>> GetDanmaku(string cid, int segment_index = 1)
        {
            List<NSDanmaku.Model.DanmakuModel> danmuList = new List<NSDanmaku.Model.DanmakuModel>();
            try
            {
#if DEBUG
                Stopwatch sw = new Stopwatch();
                sw.Start();
#endif

                var data = await PlayerAPI.SegDanmaku(cid, segment_index).url.GetStream();
                var result = DmSegMobileReply.Parser.ParseFrom(data);

                foreach (var item in result.Elems)
                {
                    NSDanmaku.Model.DanmakuLocation location = NSDanmaku.Model.DanmakuLocation.Scroll;
                    if (item.Mode == 4)
                    {
                        location = NSDanmaku.Model.DanmakuLocation.Bottom;
                    }
                    if (item.Mode == 5)
                    {
                        location = NSDanmaku.Model.DanmakuLocation.Top;
                    }
                    danmuList.Add(new NSDanmaku.Model.DanmakuModel()
                    {
                        color = item.Color.ToString().StrToColor(),
                        fromSite = NSDanmaku.Model.DanmakuSite.Bilibili,
                        location = location,
                        pool = item.Pool.ToString(),
                        rowID = item.IdStr,
                        sendID = item.MidHash,
                        size = item.Fontsize,
                        weight = item.Weight,
                        text = item.Content,
                        sendTime = item.Ctime.ToString(),
                        time = item.Progress / 1000d,
                        time_s = item.Progress / 1000
                    });
                }
#if DEBUG
                sw.Stop();
                Debug.WriteLine($"获取弹幕耗时：{sw.ElapsedMilliseconds}ms");
#endif
            }
            catch (Exception ex)
            {
                Notify.ShowMessageToast("弹幕加载失败:" + ex.Message);
                logger.Log("grpc弹幕加载失败", LogType.Fatal, ex);
            }
            return danmuList;
        }

        public async Task<bool> SendDanmaku(string aid, string cid, string text, int position, int mode, string color)
        {
            try
            {
                if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
                {
                    Notify.ShowMessageToast("请先登录");
                    return false;
                }
                if (text == null || text.Trim().Length == 0)
                {
                    Notify.ShowMessageToast("弹幕文本不能为空");
                    return false;
                }
                var result = await PlayerAPI.SendDanmu(aid, cid, color, text, position, mode).Request();
                if (result.status)
                {
                    var obj = result.GetJObject();
                    if (obj["code"].ToInt32() == 0)
                    {
                        Notify.ShowMessageToast("弹幕成功发射");
                        return true;
                    }
                    else
                    {
                        Notify.ShowMessageToast("弹幕发送失败" + obj["message"].ToString());
                        return false;
                    }
                }
                else
                {
                    Notify.ShowMessageToast("弹幕发送失败" + result.message);
                    return false;
                }
            }
            catch (Exception ex)
            {
                var result = HandelError<object>(ex);
                Notify.ShowMessageToast(result.message);
                return false;
            }

        }

        public async Task<string> GetOnline(string aid, string cid)
        {

            try
            {
                var api = PlayerAPI.GetPlayerOnline(aid: aid, cid: cid, "");
                var result = await api.Request();
                if (result.status)
                {
                    var data = await result.GetData<PlayerOnlineInfo>();
                    if (data.code == 0)
                    {
                        return $"{data.data.total}人正在看";
                    }
                }
            }
            catch (Exception ex)
            {
                HandelError<object>(ex);
            }
            return "";
        }
    }


    public class PlayerInfo
    {
        /// <summary>
        /// 字幕信息
        /// </summary>
        public HasSubtitleModel subtitle { get; set; }
        /// <summary>
        /// 互动视频信息
        /// </summary>
        public InteractionModel interaction { get; set; }
    }
    public class PlayerOnlineInfo
    {
        public string total { get; set; }
        public string count { get; set; }
    }
    public class InteractionHistoryNodeModel
    {
        public int node_id { get; set; }
        public string title { get; set; }
        public int cid { get; set; }
    }

    public class InteractionModel
    {
        public InteractionHistoryNodeModel history_node { get; set; }
        public int graph_version { get; set; }
        public string msg { get; set; }
        public string error_toast { get; set; }
        public int mark { get; set; }
        public int need_reload { get; set; }
    }




    public class HasSubtitleModel
    {
        public bool allow_submit { get; set; }
        public List<HasSubtitleItemModel> subtitles { get; set; }
    }

    public class HasSubtitleItemModel
    {
        public long id { get; set; }
        public string lan { get; set; }
        public string lan_doc { get; set; }
        public string subtitle_url { get; set; }
    }
    public class SubtitleModel
    {
        public double font_size { get; set; }
        public string font_color { get; set; }
        public double background_alpha { get; set; }
        public string background_color { get; set; }
        public string Stroke { get; set; }

        public List<SubtitleItemModel> body { get; set; }
    }
    public class SubtitleItemModel
    {
        public double from { get; set; }
        public double to { get; set; }
        public int location { get; set; }
        public string content { get; set; }
    }



}
