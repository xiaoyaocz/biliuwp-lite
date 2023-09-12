using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Services;
using System;
using System.Threading.Tasks;

namespace BiliLite.Models.Requests.Api
{
    public class PlayerAPI
    {
        public ApiModel VideoPlayUrl(string aid, string cid, int qn, bool dash, bool proxy = false, string area = "")
        {
            var baseUrl = ApiHelper.API_BASE_URL;

            if (proxy)
            {
                baseUrl = area.ChooseProxyServer();
            }
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{baseUrl}/x/player/playurl",
                parameter = $"avid={aid}&cid={cid}&qn={qn}&type=&otype=json&mid={(SettingService.Account.Logined ? SettingService.Account.Profile.mid.ToString() : "")}",
                need_cookie = true,
            };
            api.parameter += "&fourk=1&fnver=0&fnval=4048";
            
            if (proxy)
            {
                api.parameter += $"&area={area}";
            }
            return api;
        }

        public ApiModel SeasonPlayUrl(string aid, string cid, string ep_id, int qn, int season_type, bool dash, bool proxy = false, string area = "")
        {
            var baseUrl = ApiHelper.API_BASE_URL;
            if (proxy)
            {
                baseUrl = area.ChooseProxyServer();
            }
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{baseUrl}/pgc/player/web/playurl",
                parameter = $"appkey={ApiHelper.AndroidKey.Appkey}&cid={cid}&ep_id={ep_id}&qn={qn}&type=&otype=json&module=bangumi&season_type={season_type}"
            };
            if (SettingService.Account.Logined)
            {
                api.parameter += $"&access_key={SettingService.Account.AccessKey}&mid={SettingService.Account.Profile.mid}";
            }
            api.parameter += "&fourk=1&fnver=0&fnval=4048";
            
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            if (proxy)
            {
                api.parameter += $"&area={area}";
            }
            return api;
        }

        public ApiModel SeasonAndroidPlayUrl(string aid, string cid, int qn, int season_type, bool dash)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/pgc/player/web/playurl",
                parameter = $"appkey={ApiHelper.AndroidKey.Appkey}&cid={cid}&qn={qn}&type=&otype=json&module=bangumi&season_type={season_type}"
            };
            if (SettingService.Account.Logined)
            {
                api.parameter += $"&access_key={SettingService.Account.AccessKey}&mid={SettingService.Account.Profile.mid}";
            }
            api.parameter += "&fourk=1&fnver=0&fnval=4048";
            
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.WebVideoKey);
            return api;
        }

        public ApiModel LivePlayUrl(string room_id, int qn = 0)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://api.live.bilibili.com/xlive/web-room/v2/index/getRoomPlayInfo",
                parameter = $"room_id={room_id}&qn={qn}&protocol=0,1&format=0,2&codec=0,1"  //"cid={cid}&qn={qn}&platform=web"
            };
            //api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidVideoKey);
            return api;
        }

        /// <summary>
        /// 互动视频信息
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="graph_version"></param>
        /// <param name="edge_id"></param>
        /// <returns></returns>
        public ApiModel InteractionEdgeInfo(string aid, int graph_version, int edge_id = 0)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/stein/edgeinfo_v2",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&aid={aid}&graph_version={graph_version}&edge_id={edge_id}"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 番剧播放记录上传
        /// </summary>
        /// <param name="aid">AVID</param>
        /// <param name="cid">CID</param>
        /// <param name="sid">SID</param>
        /// <param name="epid">EPID</param>
        /// <param name="type">类型 3=视频，4=番剧</param>
        /// <param name="progress">进度/秒</param>
        /// <returns></returns>
        public ApiModel SeasonHistoryReport(string aid, string cid, int progress, int sid = 0, string epid = "0", int type = 3)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v2/history/report",
                body = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&aid={aid}&cid={cid}&epid={epid}&sid={sid}&progress={progress}&realtime={progress}&sub_type=1&type={type}"
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 发送弹幕
        /// </summary>
        /// <param name="aid">AV</param>
        /// <param name="cid">CID</param>
        /// <param name="color">颜色(10进制)</param>
        /// <param name="msg">内容</param>
        /// <param name="position">位置</param>
        /// <param name="mode">类型</param>
        /// <param name="plat">平台</param>
        /// <returns></returns>
        public ApiModel SendDanmu(string aid, string cid, string color, string msg, int position, int mode = 1, int plat = 2)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v2/dm/post",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&aid={aid}",
                body = $"msg={Uri.EscapeDataString(msg)}&mode={mode}&screen_state=1&color={color}&pool=0&progress={Convert.ToInt32(position * 1000)}&fontsize=25&rnd={TimeExtensions.GetTimestampS()}&from=7&oid={cid}&plat={plat}&type=1"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 读取播放信息
        /// </summary>
        /// <param name="aid">AV</param>
        /// <param name="cid">CID</param>
        /// <returns></returns>
        public async Task<ApiModel> GetPlayerInfo(string aid, string cid, string bvid)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/player/wbi/v2",
                parameter = $"cid={cid}&aid={aid}&bvid={bvid}",
                need_cookie = true,
            };
            api.parameter = await ApiHelper.GetWbiSign(api.parameter);
            return api;
        }

        /// <summary>
        /// 读取视频在线人数
        /// </summary>
        /// <param name="aid">AV</param>
        /// <param name="cid">CID</param>
        /// <returns></returns>
        public ApiModel GetPlayerOnline(string aid, string cid, string bvid)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/player/online/total",
                parameter = $"cid={cid}&aid={aid}&bvid={bvid}",
            };
            return api;
        }

        /// <summary>
        /// 弹幕关键词
        /// </summary>
        /// <returns></returns>
        public ApiModel GetDanmuFilterWords()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/dm/filter/user",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true)
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 添加弹幕屏蔽关键词
        /// </summary>
        /// <param name="word">关键词</param>
        /// <param name="type">类型，0=关键字，1=正则，2=用户</param>
        /// <returns></returns>
        public ApiModel AddDanmuFilterWord(string word, int type)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/dm/filter/user/add",
                body = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&filter={Uri.EscapeDataString(word)}&type={type}"
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 分段弹幕
        /// </summary>
        /// <param name="oid"></param>
        /// <param name="segment_index"></param>
        /// <returns></returns>
        public ApiModel SegDanmaku(string oid, int segment_index)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"http://api.bilibili.com/x/v2/dm/list/seg.so",
                parameter = $"type=1&oid={oid}&segment_index={segment_index}&{ApiHelper.MustParameter(ApiHelper.AndroidKey, true)}",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 生成一个MPD文件链接
        /// </summary>
        /// <param name="generate"></param>
        /// <returns></returns>
        public string GenerateMPD(GenerateMPDModel generate)
        {
            var baseUrl = SettingService.GetValue(SettingConstants.Other.BILI_LITE_WEB_API_BASE_URL, ApiConstants.BILI_LITE_WEB_API_DEFAULT_BASE_URL);
            var par = Newtonsoft.Json.JsonConvert.SerializeObject(generate);
            return $"{baseUrl}api/player/generatempd?par={Uri.EscapeDataString(par)}";
        }
    }
}
