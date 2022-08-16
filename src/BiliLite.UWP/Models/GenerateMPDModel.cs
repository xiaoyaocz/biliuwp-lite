using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.Models
{
    public class GenerateMPDModel
    {
        public long Duration { get; set; }
        public long DurationMS { get; set; }
        public string AudioID { get; set; }
        public string AudioBandwidth { get; set; }
        public string AudioCodec { get; set; }
        public string AudioUrl { get; set; }

        public string VideoID { get; set; }
        public string VideoBandwidth { get; set; }
        public string VideoCodec { get; set; }
        public string VideoUrl { get; set; }
        public string VideoFrameRate { get; set; }
        public int VideoWidth { get; set; }
        public int VideoHeight { get; set; }

    }
}
