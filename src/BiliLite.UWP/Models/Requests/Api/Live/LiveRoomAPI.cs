using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Services;
using System;

namespace BiliLite.Models.Requests.Api.Live
{
    public class LiveRoomAPI
    {
        /// <summary>
        /// 直播间信息
        /// </summary>
        /// <param name="roomid"></param>
        /// <returns></returns>
        public ApiModel LiveRoomInfo(string roomid)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://api.live.bilibili.com/xlive/app-room/v1/index/getInfoByRoom",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&actionKey=appkey&room_id={roomid}&device=android"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }
        /// <summary>
        /// 钱包
        /// </summary>
        /// <returns></returns>
        public ApiModel MyWallet()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://api.live.bilibili.com/pay/v2/Pay/myWallet",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&actionKey=appkey"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 直播头衔列表
        /// </summary>
        /// <returns></returns>
        public ApiModel LiveTitles()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://api.live.bilibili.com/rc/v1/Title/getTitle",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&actionKey=appkey"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 直播礼物列表
        /// </summary>
        /// <returns></returns>
        public ApiModel GiftList(int area_v2_id, int area_v2_parent_id, int roomId)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://api.live.bilibili.com/gift/v4/Live/giftConfig",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&actionKey=appkey&area_v2_id={area_v2_id}&area_v2_parent_id={area_v2_parent_id}&roomid={roomId}"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 直播背包
        /// </summary>
        /// <returns></returns>
        public ApiModel BagList(int roomId)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://api.live.bilibili.com/xlive/app-room/v1/gift/bag_list",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&actionKey=appkey&roomid={roomId}"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 直播房间可用礼物列表
        /// </summary>
        /// <returns></returns>
        public ApiModel RoomGifts(int area_v2_id, int area_v2_parent_id, int roomId)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://api.live.bilibili.com/gift/v3/live/room_gift_list",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&actionKey=appkey&area_v2_id={area_v2_id}&area_v2_parent_id={area_v2_parent_id}&roomid={roomId}"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 免费瓜子（宝箱）
        /// </summary>
        /// <returns></returns>
        public ApiModel FreeSilverTime()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://api.live.bilibili.com/mobile/freeSilverCurrentTask",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&actionKey=appkey"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }
        /// <summary>
        /// 领取免费瓜子（宝箱）
        /// </summary>
        /// <returns></returns>
        public ApiModel GetFreeSilver()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://api.live.bilibili.com/mobile/freeSilverAward",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&actionKey=appkey"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 赠送背包礼物
        /// </summary>
        /// <returns></returns>
        public ApiModel SendBagGift(long ruid, int gift_id, int num, int bag_id, int roomId)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"https://api.live.bilibili.com/gift/v2/live/bag_send",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&actionKey=appkey",
                body = $"uid={SettingService.Account.UserID}&ruid={ruid}&send_ruid=0&gift_id={gift_id}&gift_num={num}&bag_id={bag_id}&biz_id={roomId}&rnd={new Random().Next(1000, 999999).ToString("000000")}&biz_code=live&data_behavior_id=&data_source_id="
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 赠送礼物
        /// </summary>
        /// <returns></returns>
        public ApiModel SendGift(long ruid, int gift_id, int num, int roomId, string coin_type, int price)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"https://api.live.bilibili.com/gift/v2/live/send",
                body = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&actionKey=appkey",
            };
            api.body += $"&biz_code=live&biz_id={roomId}&coin_type={coin_type}&gift_id={gift_id}&gift_num={num}&mobi_app=android&platform=android&price={price}&rnd={TimeExtensions.GetTimestampMS()}&ruid={ruid}&uid={SettingService.Account.UserID}";
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 发送弹幕
        /// </summary>
        /// <returns></returns>
        public ApiModel SendDanmu(string text, int roomId)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"https://api.live.bilibili.com/api/sendmsg",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&actionKey=appkey",
            };
            api.body = $"cid={roomId}&mid={SettingService.Account.UserID}&msg={Uri.EscapeDataString(text)}&rnd={TimeExtensions.GetTimestampMS()}&mode=1&pool=0&type=json&color=16777215&fontsize=25&playTime=0.0";
            api.parameter += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 主播详细信息
        /// </summary>
        /// <returns></returns>
        public ApiModel AnchorProfile(long uid)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://api.live.bilibili.com/live_user/v1/card/card_up",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&actionKey=appkey&uid={uid}",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }


        /// <summary>
        /// 舰队列表
        /// </summary>
        /// <param name="ruid">主播ID</param>
        /// <param name="roomId">房间号</param>
        /// <param name="page">页码</param>
        /// <returns></returns>
        public ApiModel GuardList(long ruid, int roomId, int page)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://api.live.bilibili.com/xlive/app-room/v1/guardTab/topList",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&actionKey=appkey&page={page}&page_size=20&roomid={roomId}&ruid={ruid}",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 粉丝榜
        /// </summary>
        /// <param name="ruid">主播ID</param>
        /// <param name="roomId">房间号</param>
        /// <param name="page">页码</param>
        /// <returns></returns>
        public ApiModel FansList(long ruid, int roomId, int page)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://api.live.bilibili.com/rankdb/v2/RoomRank/mobileMedalRank",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&actionKey=appkey&page={page}&roomid={roomId}&ruid={ruid}",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }


        /// <summary>
        /// 房间榜单
        /// </summary>
        /// <param name="ruid">主播ID</param>
        /// <param name="roomId">房间号</param>
        /// <param name="rank_type"></param>
        /// <param name="next_offset">gold-rank=金瓜子排行，today-rank=今日礼物排行，seven-rank=7日礼物排行</param>
        /// <returns></returns>
        public ApiModel RoomRankList(long ruid, int roomId, string rank_type, int next_offset = 0)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://api.live.bilibili.com/rankdb/v1/RoomRank/tabRanks",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&actionKey=appkey&next_offset={next_offset}&room_id={roomId}&ruid={ruid}&rank_type={rank_type}",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 直播间抽奖信息
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public ApiModel RoomLotteryInfo(int roomId)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://api.live.bilibili.com/xlive/lottery-interface/v1/lottery/getLotteryInfo",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&actionKey=appkey&roomid={roomId}",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 直播间抽奖信息
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public ApiModel RoomSuperChat(int roomId)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://api.live.bilibili.com/av/v1/SuperChat/getMessageList",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&actionKey=appkey&room_id={roomId}",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }
        /// <summary>
        /// 进入房间
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public ApiModel RoomEntryAction(int roomId)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"https://api.live.bilibili.com/room/v1/Room/room_entry_action",
                body = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&actionKey=appkey&room_id={roomId}",
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }

        public ApiModel GetDanmukuInfo(int roomId)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://api.live.bilibili.com/xlive/web-room/v1/index/getDanmuInfo",
                parameter = $"?id={roomId}",
                need_cookie = true
            };
            return api;
        }

        public ApiModel GetBuvid()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = "https://api.bilibili.com/x/frontend/finger/spi",
                need_cookie = true
            };
            return api;
        }
    }
}
