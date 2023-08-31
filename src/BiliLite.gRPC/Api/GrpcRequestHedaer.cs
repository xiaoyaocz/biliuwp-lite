using System;
using Bilibili.App.View.V1;
using Bilibili.Metadata;
using Bilibili.Metadata.Device;
using Bilibili.Metadata.Fawkes;
using Bilibili.Metadata.Locale;
using Bilibili.Metadata.Network;
using Google.Protobuf;

namespace BiliLite.gRPC.Api
{
    public class GrpcRequestHeaderConfig
    {
        public GrpcRequestHeaderConfig(string accessKey)
        {
            AccessKey = accessKey;
        }
        public string AccessKey { get; set; }

        public static string dalvik_ver = "2.1.0";
        public static string os_ver = "9";
        public static string brand = "Xiaomi";
        public static string model = "MIUI";
        public static string app_ver = "6.7.0";
        public static int build = 6070600;
        public static string channel = "bilibili140";
        public static int network_type = 1;
        public static int network_tf = 0;
        public static string network_oid = "46007";
        public static string cronet = "1.21.0";

        public static string buvid = "XZFD48CFF1E68E637D0DF11A562468A8DC314";
        public static string mobiApp = "android";
        public static string platform = "android";
        public static string env = "prod";
        public static int appId = 1;
        public static string region = "CN";
        public static string language = "zh";


        public string GetFawkesreqBin()
        {
            var msg = new FawkesReq();
            msg.Appkey = mobiApp;
            msg.Env = env;
            return ToBase64(msg.ToByteArray());
        }
        public string GetMetadataBin()
        {
            var msg = new Metadata();
            msg.AccessKey = AccessKey;
            msg.MobiApp = mobiApp;
            //msg.device = "";
            msg.Build = build;
            msg.Channel = channel;
            msg.Buvid = buvid;
            msg.Platform = platform;
            return ToBase64(msg.ToByteArray());
        }
        public string GetDeviceBin()
        {
            var msg = new Device();
            msg.AppId = appId;
            msg.MobiApp = mobiApp;
            //没有的值不要做操作
            //msg.device = "";
            msg.Build = build;
            msg.Channel = channel;
            msg.Buvid = buvid;
            msg.Platform = platform;
            msg.Brand = brand;
            msg.Model = model;
            msg.Osver = os_ver;
            return ToBase64(msg.ToByteArray());
        }

        public string GetNetworkBin()
        {
            var msg = new Network();
            msg.Type = NetworkType.Wifi;
            msg.Oid = network_oid;
            return ToBase64(msg.ToByteArray());
        }

        public string GetRestrictionBin()
        {
            var msg = new Restriction();

            return ToBase64(msg.ToByteArray());
        }
        public string GetLocaleBin()
        {
            var msg = new Locale();
            msg.CLocale = new LocaleIds();
            msg.SLocale = new LocaleIds();
            msg.CLocale.Language = language;
            msg.CLocale.Region = region;
            msg.SLocale.Language = language;
            msg.SLocale.Region = region;
            return ToBase64(msg.ToByteArray());
        }
        public string ToBase64(byte[] data)
        {
            return Convert.ToBase64String(data).TrimEnd('=');
        }
    }
}
