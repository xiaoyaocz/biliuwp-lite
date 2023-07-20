using System;
using System.Threading.Tasks;
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
    }
}
