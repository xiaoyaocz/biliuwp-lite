using System;

namespace BiliLite.Models.Common.Video.PlayUrlInfos
{
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
}
