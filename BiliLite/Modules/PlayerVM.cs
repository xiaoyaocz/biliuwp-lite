using BiliLite.Api;
using BiliLite.Controls;
using BiliLite.Helpers;
using BiliLite.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BiliLite.Modules
{
    public class PlayerVM:IModules
    {
        readonly PlayerAPI PlayerAPI;
        public PlayerVM()
        {
            PlayerAPI = new PlayerAPI();
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
       
        public List<string> DefaultDanmakuColors { get; set; }

        public async Task<ReturnModel<PlayUrlReturnInfo>> GetPlayUrls(PlayInfo playInfo,int qn)
        {
            try
            {
                var mode= SettingHelper.GetValue<int>(SettingHelper.Player.DEFAULT_VIDEO_TYPE, 1);
                if (mode == 0)
                {
                    var data = await HandelFlv(playInfo, qn);
                    if (data.success)
                    {
                        return data;
                    }
                    else
                    {
                        return await HandelDash(playInfo, qn,mode);
                    }
                }
                else
                {
                    var data = await HandelDash(playInfo, qn, mode);
                    if (data.success)
                    {
                        return data;
                    }
                    else
                    {
                        return await HandelFlv(playInfo, qn);
                    }
                }
            }
            catch (Exception ex)
            {
                return HandelError<PlayUrlReturnInfo>(ex);
            }
        }
        private async Task<ReturnModel<PlayUrlReturnInfo>> HandelFlv(PlayInfo playInfo, int qn)
        {
            var noVIP = !SettingHelper.Account.Logined || (SettingHelper.Account.Logined && SettingHelper.Account.Profile.vip != null && SettingHelper.Account.Profile.vip.type == 0);
            var data = await GetBiliBiliFlv(playInfo, qn);
            if (data.code == 0)
            {
               
                List<QualityWithPlayUrlInfo> qualityWithPlayUrlInfos = new List<QualityWithPlayUrlInfo>();
                for (int i = 0; i < data.data.accept_description.Count; i++)
                {
                    qualityWithPlayUrlInfos.Add(new QualityWithPlayUrlInfo()
                    {
                        quality = data.data.accept_quality[i],
                        quality_description = data.data.accept_description[i]
                    });
                }
                var index = data.data.accept_quality.IndexOf(data.data.quality);
                qualityWithPlayUrlInfos[index].playUrlInfo = new PlayUrlInfo()
                {
                    multi_flv_url = data.data.durl,
                    mode = data.data.durl.Count > 1 ? VideoPlayMode.MultiFlv : VideoPlayMode.SingleFlv,
                    codec_name = "h264_flv"
                };
                if (noVIP)
                {
                    qualityWithPlayUrlInfos = qualityWithPlayUrlInfos.Where(x => x.quality != 74 && x.quality <= 80).ToList();
                }
                return new ReturnModel<PlayUrlReturnInfo>()
                {
                    success = true,
                    data = new PlayUrlReturnInfo()
                    {
                        quality = qualityWithPlayUrlInfos,
                        current = qualityWithPlayUrlInfos[index]
                    }
                };
            }
            else
            {
                return new ReturnModel<PlayUrlReturnInfo>() {
                    success=false,
                    message= data.message
                };
            }
        }
        private async Task<ReturnModel<PlayUrlReturnInfo>> HandelDash(PlayInfo playInfo, int qn,int mode)
        {
            var noVIP = !SettingHelper.Account.Logined || (SettingHelper.Account.Logined && SettingHelper.Account.Profile.vip != null && SettingHelper.Account.Profile.vip.type == 0);
            var data = await GetBiliBiliDash(playInfo, qn);
            if (data.code == 0&&data.data.dash!=null)
            {
                var codecid = (mode == 0) ? 7 : 12;
                var h264 = data.data.dash.video.Where(x => x.codecid == 7);
                var h265 = data.data.dash.video.Where(x => x.codecid == 12);
                if(qn> data.data.accept_quality.Max())
                {
                    qn = data.data.accept_quality.Max();
                }
                if (!data.data.accept_quality.Contains(qn))
                {
                    qn = data.data.accept_quality.Max();
                }
                List<QualityWithPlayUrlInfo> qualityWithPlayUrlInfos = new List<QualityWithPlayUrlInfo>();
                for (int i = 0; i < data.data.accept_description.Count; i++)
                {
                    PlayUrlInfo info=null;
                    var video = h264.FirstOrDefault(x => x.id == data.data.accept_quality[i]);

                    if (mode == 2)
                    {
                        var h265_video = h265.FirstOrDefault(x => x.id == data.data.accept_quality[i]);
                        if (h265_video!=null)
                        {
                            video = h265_video;
                        }
                    }
                    if (video != null)
                    {
                        DashItemModel audio = null;
                        var audios = data.data.dash.audio.Where(x=>x.mimeType== "audio/mp4" || x.mime_type== "audio/mp4").OrderBy(x => x.bandwidth);
                        if (qn > 64)
                        {
                            audio = audios.LastOrDefault();
                        }
                        else
                        {
                            audio = audios.FirstOrDefault();
                        }
                        info = new PlayUrlInfo()
                        {
                            codec_name = video.codecid == 7 ? "h264_m4s" : "h265_m4s",
                            dash_video_url = video,
                            dash_audio_url = audio,
                            mode = VideoPlayMode.Dash
                        };
                    }
                    qualityWithPlayUrlInfos.Add(new QualityWithPlayUrlInfo()
                    {
                        quality = data.data.accept_quality[i],
                        quality_description = data.data.accept_description[i],
                        playUrlInfo = info
                    });
                }
                var current = qualityWithPlayUrlInfos.FirstOrDefault(x => x.quality == qn);
                if (current == null)
                {
                    current = qualityWithPlayUrlInfos.OrderByDescending(x=>x.quality).FirstOrDefault(x => x.playUrlInfo != null);
                }
                if (noVIP)
                {
                    qualityWithPlayUrlInfos = qualityWithPlayUrlInfos.Where(x => x.quality != 74 && x.quality <= 80).ToList();
                }
                return new ReturnModel<PlayUrlReturnInfo>()
                {
                    success = true,
                    data = new PlayUrlReturnInfo()
                    {
                        quality = qualityWithPlayUrlInfos,
                        current = current
                    }
                };
            }
            else
            {
                return new ReturnModel<PlayUrlReturnInfo>()
                {
                    success = false,
                    message = data.message
                };
            }
        }
        private async Task<ApiDataModel<FlvModel>> GetBiliBiliFlv(PlayInfo playInfo,int qn)
        {
            try
            {
                var api = PlayerAPI.VideoPlayUrl(aid: playInfo.avid,cid: playInfo.cid,qn: qn,false);
                if (playInfo.play_mode== VideoPlayType.Season)
                {
                    api = PlayerAPI.SeasonPlayUrl(aid: playInfo.avid, cid: playInfo.cid, qn: qn, season_type: playInfo.season_type, false);
                }
                var result = await api.Request();
                if (result.status)
                {
                    var obj = result.GetJObject();
                    FlvModel flvData = null;
                    if (obj["code"].ToInt32()!=0 || result.results.Contains("8986943"))
                    {
                        var bp = await GetBiliPlusFlv(playInfo, qn);
                        return new ApiDataModel<FlvModel>()
                        {
                            code = bp.code,
                            message = bp.message,
                            data = bp.data
                        };
                    }
                    if (obj["data"]!=null)
                    {
                        flvData = JsonConvert.DeserializeObject<FlvModel>(obj["data"].ToString());
                    }
                    if (obj["result"] != null&&obj["result"].ToString()!= "suee")
                    {
                        flvData = JsonConvert.DeserializeObject<FlvModel>(obj["result"].ToString());
                    }
                    if (obj["durl"] != null)
                    {
                        flvData = JsonConvert.DeserializeObject<FlvModel>(obj.ToString());
                    }
                    return new ApiDataModel<FlvModel>()
                    {
                        code=0,
                        message="",
                        data= flvData
                    };
                    //var data = await result.GetJson<ApiDataModel<FlvModel>>();
                    //if (data.code != 0|| result.results.Contains("8986943"))
                    //{
                       
                    //    //return await GetBiliPlusFlv(playInfo, qn);
                    //}
                    //if (data.data == null)
                    //{
                    //    data.data = await result.GetJson<FlvModel>();
                    //}
                    //return data;
                }
                else
                {
                    if (playInfo.play_mode== VideoPlayType.Season)
                    { 
                        var bp = await GetBiliPlusFlv(playInfo, qn);
                        return new ApiDataModel<FlvModel>() { 
                            code=bp.code,
                            message=bp.message,
                            data= bp.data
                        };
                    }
                    return new ApiDataModel<FlvModel>()
                    {
                        code = -998,
                        message = result.message
                    };
                }
            }
            catch (Exception ex)
            {
                var data = HandelError<object>(ex);
                return new ApiDataModel<FlvModel>() { 
                    code=-999,
                    message=data.message
                };
            }
        }
        private async Task<ApiDataModel<FlvModel>> GetBiliPlusFlv(PlayInfo playInfo, int qn)
        {
            try
            {
                var api = PlayerAPI.SeasonPlayUrlBiliPlus(aid: playInfo.avid, cid: playInfo.cid, qn: qn, season_type: playInfo.season_type, false);
                var result = await api.Request();
                if (result.status)
                {
                    var data =await result.GetJson<ApiDataModel<FlvModel>>();
                    if (data.code == 0)
                    {
                        data.data = await result.GetJson<FlvModel>();
                    }
                    foreach (var item in data.data.durl)
                    {
                        item.url = "http://bilibili.iill.moe/"+Uri.EscapeDataString(item.url);
                    }
                    return data;
                }
                else
                {
                    return new ApiDataModel<FlvModel>()
                    {
                        code = -998,
                        message = result.message
                    };
                }
            }
            catch (Exception ex)
            {
                var data = HandelError<object>(ex);
                return new ApiDataModel<FlvModel>()
                {
                    code = -999,
                    message = data.message
                };
            }
        }
        private async Task<ApiDataModel<DashModel>> GetBiliBiliDash(PlayInfo playInfo, int qn)
        {
            try
            {
                var api = PlayerAPI.VideoPlayUrl(aid: playInfo.avid, cid: playInfo.cid, qn: qn, true);
                if (playInfo.play_mode == VideoPlayType.Season)
                {
                    api = PlayerAPI.SeasonPlayUrl(aid: playInfo.avid, cid: playInfo.cid, qn: qn, season_type: playInfo.season_type, true);
                }
                var result = await api.Request();
                if (result.status)
                {
                    DashModel dashModel=null;
                    var obj = result.GetJObject();
                    if (obj["code"].ToInt32()!= 0 || result.results.Contains("8986943"))
                    {
                        return await GetBiliPlusDash(playInfo, qn);
                    }
                    if (obj.ContainsKey("data"))
                    {
                        dashModel=  JsonConvert.DeserializeObject<DashModel>(obj["data"].ToString());
                    }
                    if(obj.ContainsKey("result")&& obj["result"].ToString()!= "suee")
                    {
                        dashModel = JsonConvert.DeserializeObject<DashModel>(obj["result"].ToString());
                    }
                    if (obj.ContainsKey("dash"))
                    {
                        dashModel = JsonConvert.DeserializeObject<DashModel>(obj.ToString());
                    }
                    //var data = await result.GetJson<ApiDataModel<DashModel>>();
                    //if (data.code != 0 || result.results.Contains("8986943"))
                    //{
                    //    return await GetBiliPlusDash(playInfo, qn);
                    //}

                    if (dashModel == null)
                    {
                        dashModel = await result.GetJson<DashModel>();
                    }
                    if (dashModel.dash == null)
                    {
                        return new ApiDataModel<DashModel>()
                        {
                            code = -910,
                            message = "需要使用FLV"
                        };
                    }
                    return new ApiDataModel<DashModel>() { 
                        code=0,
                        data= dashModel
                    };
                }
                else
                {
                    if (playInfo.play_mode == VideoPlayType.Season)
                    {
                        return await GetBiliPlusDash(playInfo, qn);
                    }
                    return new ApiDataModel<DashModel>()
                    {
                        code = -998,
                        message = result.message
                    };
                }
            }
            catch (Exception ex)
            {
                var data = HandelError<object>(ex);
                return new ApiDataModel<DashModel>()
                {
                    code = -999,
                    message = data.message
                };
            }
        }
        private async Task<ApiDataModel<DashModel>> GetBiliPlusDash(PlayInfo playInfo, int qn)
        {
            try
            {
                var api = PlayerAPI.SeasonPlayUrlBiliPlus(aid: playInfo.avid, cid: playInfo.cid, qn: qn, season_type: playInfo.season_type, true);
                var result = await api.Request();
                if (result.status)
                {
                    var data = await result.GetJson<ApiDataModel<DashModel>>();
                    if (data.code == 0)
                    {
                        data.data = await result.GetJson<DashModel>();
                    }
                    foreach (var item in data.data.dash.video)
                    {
                        item.baseUrl = "http://bilibili.iill.moe/" + Uri.EscapeDataString(item.baseUrl);
                        item.base_url = "http://bilibili.iill.moe/" + Uri.EscapeDataString(item.base_url);
                    }
                    foreach (var item in data.data.dash.audio)
                    {
                        item.baseUrl = "http://bilibili.iill.moe/" + Uri.EscapeDataString(item.baseUrl);
                        item.base_url = "http://bilibili.iill.moe/" + Uri.EscapeDataString(item.base_url);
                    }
                    return data;
                }
                else
                {
                    return new ApiDataModel<DashModel>()
                    {
                        code = -998,
                        message = result.message
                    };
                }
            }
            catch (Exception ex)
            {
                var data = HandelError<object>(ex);
                return new ApiDataModel<DashModel>()
                {
                    code = -999,
                    message = data.message
                };
            }
        }
        public async Task ReportHistory(PlayInfo playInfo,int progress)
        {
            try
            {
                var api = PlayerAPI.SeasonHistoryReport(playInfo.avid, playInfo.cid, progress,playInfo.season_id,playInfo.ep_id, playInfo.play_mode== VideoPlayType.Video?3:4);
                await api.Request();
                Debug.WriteLine(progress);
            }
            catch (Exception ex)
            {
                var handel = HandelError<AnimeHomeModel>(ex);
                Utils.ShowMessageToast(handel.message);
            }
        }
        public async Task<HasSubtitleModel> GetSubtitles(string aid,string cid)
        {
            try
            {
                var api = PlayerAPI.GetPlayerInfo(aid: aid, cid: cid);
                var result = await api.Request();
                if (result.status)
                {
                    if (result.results.Contains("subtitle"))
                    {
                        var json = Regex.Match(result.results, @"<subtitle>(.*?)</subtitle>").Groups[1].Value;
                        return JsonConvert.DeserializeObject<HasSubtitleModel>(json);
                    }
                    else
                    {
                        return new HasSubtitleModel() { allow_submit = false };
                    }
                }
                else
                {
                    return new HasSubtitleModel() { allow_submit = false };
                }
            }
            catch (Exception ex)
            {
                var data = HandelError<object>(ex);
                return new HasSubtitleModel() { allow_submit = false };
            }
        }
        public async Task<SubtitleModel> GetSubtitle(string url)
        {
            try
            {
                if (!url.Contains("http:") || !url.Contains("https:"))
                {
                    url = "https:" + url;
                }
                var results = await HttpHelper.GetString(url);
                return JsonConvert.DeserializeObject<SubtitleModel>(results);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> SendDanmaku(string aid,string cid,string text,int mode,string color)
        {
            try
            {
                if (!SettingHelper.Account.Logined && !await Utils.ShowLoginDialog())
                {
                    Utils.ShowMessageToast("请先登录");
                    return false;
                }
                if (text == null || text.Trim().Length == 0)
                {
                    Utils.ShowMessageToast("弹幕文本不能为空");
                    return false;
                }
                var result = await PlayerAPI.SendDanmu(aid, cid, color, text, mode).Request();
                if (result.status)
                {
                    var obj = result.GetJObject();
                    if (obj["code"].ToInt32() == 0)
                    {
                        Utils.ShowMessageToast("弹幕成功发射");
                        return true;
                    }
                    else
                    {
                        Utils.ShowMessageToast("弹幕发送失败" + obj["message"].ToString());
                        return false;
                    }
                }
                else
                {
                    Utils.ShowMessageToast("弹幕发送失败" + result.message);
                    return false;
                }
            }
            catch (Exception ex)
            {
                var result= HandelError<object>(ex);
                Utils.ShowMessageToast(result.message);
                return false;
            }
            
        }


    }
    public enum VideoPlayMode
    {
        /// <summary>
        /// 单个flv
        /// </summary>
        SingleFlv,
        /// <summary>
        /// 多段flv
        /// </summary>
        MultiFlv,
        /// <summary>
        /// 音视频分离
        /// </summary>
        Dash,
        /// <summary>
        /// 单个MP4
        /// </summary>
        SingleMp4
    }
    public class PlayUrlReturnInfo
    {
        public QualityWithPlayUrlInfo current { get; set; }
        public List<QualityWithPlayUrlInfo> quality { get; set; }
    }
    public class QualityWithPlayUrlInfo
    {
        public int quality { get; set; }
        public string quality_description { get; set; }
        public PlayUrlInfo playUrlInfo { get; set; }
    }
    public class PlayUrlInfo
    {
        public VideoPlayMode mode { get; set; }
        public string url { get; set; }
        public List<FlvDurlModel> multi_flv_url { get; set; }
        public DashItemModel dash_video_url { get; set; }
        public DashItemModel dash_audio_url { get; set; }
        public string codec_name { get; set; }
      
    }

    public class FlvModel
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<string> accept_description { get; set; }
        public List<int> accept_quality { get; set; }
        public int quality { get; set; }
        public int timelength { get; set; }
        public int video_codecid { get; set; }
        public List<FlvDurlModel> durl { get; set; }
    }
    public class FlvDurlModel
    {
        public List<string> backup_url { get; set; }
        public string url { get; set; }
        public int order { get; set; }
        public long size { get; set; }
        public int length { get; set; }
    }
    public class DashModel
    {
        public string format { get; set; }
        public List<string> accept_description { get; set; }
        public List<int> accept_quality { get; set; }
        /// <summary>
        /// 时长，毫秒
        /// </summary>
        public int timelength { get; set; }
        public int video_codecid { get; set; }
        public DashDashModel dash { get; set; }
    }
    public class DashDashModel
    {
        public List<DashItemModel> video { get; set; }
        public List<DashItemModel> audio { get; set; }
        /// <summary>
        /// 时长，秒
        /// </summary>
        public int duration { get; set; }
    
    }
    public class DashItemModel
    {
        public int id { get; set; }
        public int bandwidth { get; set; }
        public string baseUrl { get; set; }
        public string base_url { get; set; }
        public List<string> backupUrl { get; set; }
        public List<string> backup_url { get; set; }
        public string mime_type { get; set; }
        public string mimeType { get; set; }
        public string codecs { get; set; }
        public int codecid { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string frameRate { get; set; }
        public string frame_rate { get; set; }
        /// <summary>
        /// 计算平均帧数
        /// </summary>
        public string fps
        {
            get
            {
                try
                {
                    if (!string.IsNullOrEmpty(frameRate))
                    {
                        var values = frameRate.Split('/');
                        if (values.Length == 1)
                        {
                            return frameRate;
                        }
                        double r = Convert.ToDouble(values[0]);
                        double d = Convert.ToDouble(values[1]);
                        return (r / d).ToString("0.0");
                    }
                    else if (!string.IsNullOrEmpty(frame_rate))
                    {
                        var values = frame_rate.Split('/');
                        if (values.Length == 1)
                        {
                            return frame_rate;
                        }
                        double r = Convert.ToDouble(values[0]);
                        double d = Convert.ToDouble(values[1]);
                        return (r / d).ToString("0.0");
                    }
                    else
                    {
                        return "0";
                    }
                }
                catch (Exception)
                {
                    return "0";
                }
                
            }
        }

        public SegmentBase SegmentBase { get; set; }
        public SegmentBase segment_base { get; set; }
    }

    public class SegmentBase
    {
        public string Initialization { get; set; }
        public string indexRange { get; set; }

        public string initialization { get; set; }
        public string index_range { get; set; }
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
