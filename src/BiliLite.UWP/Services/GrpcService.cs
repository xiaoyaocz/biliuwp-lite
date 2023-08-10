using System;
using System.Threading.Tasks;
using Bilibili.App.Dynamic.V2;
using Bilibili.App.Interface.V1;
using BiliLite.gRPC.Api;

namespace BiliLite.Services
{
    public class GrpcService
    {
        public async Task<SearchArchiveReply> SearchSpaceArchive(string mid, int page = 1, int pageSize = 30, string keyword = "")
        {
            var message = new SearchArchiveReq()
            {
                Keyword = keyword,
                Mid = long.Parse(mid),
                Pn = page,
                Ps = pageSize,
            };
            var accessKey = SettingService.Account.AccessKey;

            var result = await GrpcRequest.Instance.SendMessage("https://grpc.biliapi.net:443/bilibili.app.interface.v1.Space/SearchArchive", message, accessKey);
            if (result.status)
            {
                var reply = SearchArchiveReply.Parser.ParseFrom(result.results);
                return reply;
            }
            else
            {
                throw new Exception(result.message);
            }
        }

        public async Task<DynAllReply> GetDynAll(int page = 1)
        {
            var message = new DynAllReq()
            {
                Page = page
            };
            var accessKey = SettingService.Account.AccessKey;

            var result = await GrpcRequest.Instance.SendMessage("https://grpc.biliapi.net:443/bilibili.app.dynamic.v2.Dynamic/DynAll", message, accessKey);
            if (result.status)
            {
                var reply = DynAllReply.Parser.ParseFrom(result.results);
                return reply;
            }
            else
            {
                throw new Exception(result.message);
            }
        }

        public async Task<DynVideoReply> GetDynVideo(int page, string historyOffset, string updateBaseline)
        {

            var message = new DynVideoReq()
            {
                LocalTime = 8,
            };
            if (page > 1)
            {
                message.Offset = historyOffset;
                message.UpdateBaseline = updateBaseline;
                message.Page = page;
                message.RefreshType = Refresh.History;
            }
            var accessKey = SettingService.Account.AccessKey;

            var result = await GrpcRequest.Instance.SendMessage("https://grpc.biliapi.net:443/bilibili.app.dynamic.v2.Dynamic/DynVideo", message, accessKey);
            if (result.status)
            {
                var reply = DynVideoReply.Parser.ParseFrom(result.results);
                return reply;
            }
            else
            {
                throw new Exception(result.message);
            }
        }
    }
}
