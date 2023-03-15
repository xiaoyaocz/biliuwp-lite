using System.Collections.Generic;

namespace BiliLite.Models.Common
{
    public class Subtitle
    {
        public double font_size { get; set; }
        public string font_color { get; set; }
        public double background_alpha { get; set; }
        public string background_color { get; set; }
        public string Stroke { get; set; }
        public List<SubtitleItem> body { get; set; }
    }

    public class SubtitleItem
    {
        public double from { get; set; }
        public double to { get; set; }
        public int location { get; set; }
        public string content { get; set; }
    }
}
