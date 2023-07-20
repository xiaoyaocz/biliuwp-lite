using Google.Protobuf;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace BiliLite.gRPC.Api
{
    public class GrpcRequest
    {
        private static GrpcRequest _instance;
        public static GrpcRequest Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GrpcRequest();
                }
                return _instance;
            }
        }

        /// <summary>
        /// 发送请求
        /// </summary>
        /// <param name="path">路径/URL</param>
        /// <param name="message">请求头</param>
        /// <param name="access_key">access_key</param>
        /// <returns>处理后的byte[]</returns>
        public async Task<GrpcResult> SendMessage(string url, IMessage message, string access_key = "")
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                var config = new GrpcRequestHeaderConfig(access_key);
                //httpClient.DefaultRequestHeaders.Add("User-Agent", "Dalvik/2.1.0 (Linux; U; Android 5.1.1; ELE-AL00 Build/LMY48Z) 6.12.0 os/android model/ELE-AL00 mobi_app/android build/6120400 channel/bili innerVer/6120400 osVer/5.1.1 network/2");
                var ua = $"Dalvik/{GrpcRequestHeaderConfig.dalvik_ver} "
                + $"(Linux; U; Android {GrpcRequestHeaderConfig.os_ver}; {GrpcRequestHeaderConfig.brand} {GrpcRequestHeaderConfig.model}) {GrpcRequestHeaderConfig.app_ver} "
                + $"os/android model/{GrpcRequestHeaderConfig.model} mobi_app/android build/{GrpcRequestHeaderConfig.build} "
                + $"channel/{GrpcRequestHeaderConfig.channel} innerVer/{GrpcRequestHeaderConfig.build} osVer/{GrpcRequestHeaderConfig.os_ver} "
                + $"network/{GrpcRequestHeaderConfig.network_type}";
                httpClient.DefaultRequestHeaders.Add("User-Agent", ua);
                httpClient.DefaultRequestHeaders.Add("APP-KEY", "android");
                httpClient.DefaultRequestHeaders.Add("x-bili-metadata-bin", config.GetMetadataBin());
                httpClient.DefaultRequestHeaders.Add("authorization", "identify_v1 " + access_key);
                httpClient.DefaultRequestHeaders.Add("x-bili-device-bin", config.GetDeviceBin());
                httpClient.DefaultRequestHeaders.Add("x-bili-network-bin", config.GetNetworkBin());
                httpClient.DefaultRequestHeaders.Add("x-bili-restriction-bin", "");
                httpClient.DefaultRequestHeaders.Add("x-bili-locale-bin", config.GetLocaleBin());
                httpClient.DefaultRequestHeaders.Add("x-bili-fawkes-req-bin", config.GetFawkesreqBin());
                //httpClient.DefaultRequestHeaders.Add("grpc-encoding", "gzip");
                httpClient.DefaultRequestHeaders.Add("grpc-accept-encoding", "identity");
                httpClient.DefaultRequestHeaders.Add("grpc-timeout", "17985446u");
                httpClient.DefaultRequestHeaders.Add("env", "prod");
                httpClient.DefaultRequestHeaders.Add("Transfer-Encoding", "chunked");
                httpClient.DefaultRequestHeaders.Add("TE", "trailers");
                //httpClient.DefaultRequestVersion = new Version(1, 1);

                var messageBytes = message.ToByteArray();
                //校验用?第五位为数组长度
                var stateBytes = new byte[] { 0, 0, 0, 0, (byte)messageBytes.Length };
                //合并两个字节数组
                byte[] bodyBytes = new byte[5 + messageBytes.Length];
                stateBytes.CopyTo(bodyBytes, 0);
                messageBytes.CopyTo(bodyBytes, 5);

                ByteArrayContent byteArrayContent = new ByteArrayContent(bodyBytes);
                byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/grpc");
                byteArrayContent.Headers.ContentLength = bodyBytes.Length;
                var response = await httpClient.PostAsync(url, byteArrayContent);
                var data = await response.Content.ReadAsByteArrayAsync();
                if (data.Length > 5)
                {
                    return new GrpcResult()
                    {
                        code = 0,
                        status = true,
                        message = "请求成功",
                        results = data.Skip(5).ToArray()
                    };
                }
                else
                {
                    return new GrpcResult()
                    {
                        code = -102,
                        status = false,
                        message = "请求失败,没有数据返回",
                    };
                }
            }

            catch (Exception ex)
            {
                return new GrpcResult()
                {
                    code = ex.HResult,
                    status = false,
                    message = "发送gRPC请求失败" + ex.Message
                };
            }
        }

        public async Task<GrpcResult> Get(string url)
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    var data = await httpClient.GetByteArrayAsync(url);
                    // var data = await response.Content.ReadAsByteArrayAsync();
                    if (data.Length > 5)
                    {
                        return new GrpcResult()
                        {
                            code = 0,
                            status = true,
                            message = "请求成功",
                            results = data.Skip(5).ToArray()
                        };
                    }
                    else
                    {
                        return new GrpcResult()
                        {
                            code = -102,
                            status = false,
                            message = "请求失败,没有数据返回",
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new GrpcResult()
                {
                    code = ex.HResult,
                    status = false,
                    message = "发送Http请求失败" + ex.Message
                };
            }
        }
    }

    public class GrpcResult
    {
        public int code { get; set; }
        public string message { get; set; }
        public byte[] results { get; set; }
        public bool status { get; set; }
    }
}
