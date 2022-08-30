using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.Modules.Player.Playurl
{
    public enum BiliPlayUrlType
    {
        /// <summary>
        /// 单段FLV
        /// </summary>
        SingleFLV,
        /// <summary>
        /// 多段FLV
        /// </summary>
        MultiFLV,
        /// <summary>
        /// 音视频分离DASH流
        /// </summary>
        DASH
    }
    public enum BiliPlayUrlVideoCodec
    {
        AVC = 7,
        HEVC = 12,
        AV1 = 13,
    }
    public class BiliPlayUrlQualitesInfo
    {
        public static BiliPlayUrlQualitesInfo Failure(string message) => new BiliPlayUrlQualitesInfo()
        {
            Success = false,
            Message = message,
        };
        /// <summary>
        /// 请求是否成功
        /// </summary>
        public bool Success { get; set; } = true;
        /// <summary>
        /// 请求失败附带信息
        /// </summary>
        public string Message { get; set; } = "";
        /// <summary>
        /// 清晰度列表
        /// </summary>
        public List<BiliPlayUrlInfo> Qualites { get; set; }
        /// <summary>
        /// 当前清晰度
        /// </summary>
        public BiliPlayUrlInfo CurrentQuality { get; set; }
    }
    public class BiliPlayUrlInfo
    {
        /// <summary>
        /// 是否包含有播放链接
        /// </summary>
        public bool HasPlayUrl { get; set; }
        /// <summary>
        /// 清晰度ID
        /// </summary>
        public int QualityID { get; set; }
        /// <summary>
        /// 清晰度名称
        /// </summary>
        public string QualityName { get; set; }
        /// <summary>
        /// 播放链接的类型
        /// </summary>
        public BiliPlayUrlType PlayUrlType { get; set; }
        /// <summary>
        /// HTTP请求头-UserAgent
        /// </summary>
        public string UserAgent { get; set; } = "";
        /// <summary>
        /// HTTP请求头-Referer
        /// </summary>
        public string Referer { get; set; } = "https://www.bilibili.com/";
        /// <summary>
        /// 视频编码
        /// </summary>
        public BiliPlayUrlVideoCodec Codec { get; set; } = BiliPlayUrlVideoCodec.AVC;
        /// <summary>
        /// 时长，毫秒
        /// </summary>
        public long Timelength { get; set; }
        /// <summary>
        /// DASH播放信息
        /// </summary>
        public BiliDashPlayUrlInfo DashInfo { get; set; }
        /// <summary>
        /// FLV信息
        /// </summary>
        public List<BiliFlvPlayUrlInfo> FlvInfo { get; set; }
        public IDictionary<string, string> GetHttpHeader()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            if (UserAgent != null && UserAgent.Length > 0)
            {
                headers.Add("User-Agent", UserAgent);
            }
            if (Referer != null && Referer.Length > 0)
            {
                headers.Add("Referer", Referer);
            }
            return headers;
        }
    }
    public class BiliDashPlayUrlInfo
    {
        /// <summary>
        /// 时长，秒
        /// </summary>
        public int Duration { get; set; }
        /// <summary>
        /// 最小缓冲时间
        /// </summary>
        public double MinBufferTime { get; set; } = 1.5;
        /// <summary>
        /// 视频
        /// </summary>
        public BiliDashItem Video { get; set; }
        /// <summary>
        /// 音频
        /// </summary>
        public BiliDashItem Audio { get; set; }

    }
    public class BiliDashItem
    {
        /// <summary>
        /// 是否视频，false是为音频
        /// </summary>
        public bool IsVideo { get; set; } = true;
        /// <summary>
        /// 替换过CDN
        /// </summary>
        public bool ReplaceCDN { get; set; } = false;
        public int ID { get; set; }
        public int CodecID { get; set; }
        public string Url { get; set; }

        public string Host
        {
            get
            {
                if (Url != null && Url.Contains("http"))
                {
                    var uri = new Uri(Url);
                    return uri.Host;
                }
                else
                {
                    return "";
                }
            }
        }
        public int BandWidth { get; set; }
        public string MimeType { get; set; }
        public string Codecs { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string FrameRate { get; set; }
        public string Sar { get; set; }
        public int StartWithSap { get; set; } = 1;
        public string SegmentBaseInitialization { get; set; }
        public string SegmentBaseIndexRange { get; set; }

    }
    public class BiliFlvPlayUrlInfo
    {
        /// <summary>
        /// 排序
        /// </summary>
        public int Order { get; set; }
        /// <summary>
        /// 链接
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 时长,毫秒
        /// </summary>
        public long Length { get; set; }
        /// <summary>
        /// 文件大小
        /// </summary>
        public long Size { get; set; }
    }
}
