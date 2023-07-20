using System;
using System.Threading.Tasks;
using Bilibili.App.Playurl.V1;

namespace BiliLite.gRPC.Api
{
    public class PlayURL
    {
        /// <summary>
        /// 视频播放地址
        /// </summary>
        /// <param name="aid">AV号</param>
        /// <param name="cid">CID</param>
        /// <param name="qn">清晰度</param>
        /// <param name="fnval">0、2=flv，16=dash</param>
        /// <param name="codeType">编码，支持h264及h265</param>
        /// <param name="access_key">登录后access_key</param>
        /// <returns></returns>
        public async Task<PlayViewReply> VideoPlayView(long aid, long cid,int qn,int fnval , CodeType codeType, string access_key="")
        {
            var message = new PlayViewReq() { 
                Aid= aid,
                Cid=cid,
                Qn=qn,
                Fnval= fnval,
                Fourk=true,
                PreferCodecType= codeType
            };
            var result=await  GrpcRequest.Instance.SendMessage("https://app.bilibili.com/bilibili.app.playurl.v1.PlayURL/PlayView", message, access_key);
            if (result.status)
            {
                return PlayViewReply.Parser.ParseFrom(result.results);
            }
            else
            {
                throw new Exception(result.message);
            }
        }
        /// <summary>
        /// 番剧播放地址
        /// </summary>
        /// <param name="epid">剧集epid</param>
        /// <param name="cid">CID</param>
        /// <param name="qn">清晰度</param>
        /// <param name="fnval">0、2=flv，16=dash</param>
        /// <param name="codeType">编码，支持h264及h265</param>
        /// <param name="access_key">登录后access_key</param>
        /// <returns></returns>
        public async Task<PlayViewReply> BangumiPlayView(long epid, long cid, int qn, int fnval, CodeType codeType, string access_key = "")
        {
            var message = new PlayViewReq()
            {
                Aid = epid,
                Cid = cid,
                Qn = qn,
                Fnval = fnval,
                Fourk = true,
                PreferCodecType = codeType
            };
            var result = await GrpcRequest.Instance.SendMessage("https://app.bilibili.com/bilibili.pgc.gateway.player.v1.PlayURL/PlayView", message, access_key);
            if (result.status)
            {
                return PlayViewReply.Parser.ParseFrom(result.results);
            }
            else
            {
                throw new Exception(result.message);
            }
        }
    }
}
