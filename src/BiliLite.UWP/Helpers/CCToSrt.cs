using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.Helpers
{
    /// <summary>
    /// CC字幕转为srt
    /// </summary>
    internal class CCToSrt
    {
        /// <summary>
        /// 转为SRT
        /// </summary>
        /// <param name="json">CC字幕</param>
        /// <param name="toSimplified">转为简体</param>
        /// <returns></returns>
        public string ConvertToSrt(string json,bool toSimplified=false)
        {
            SubtitleModel subtitle=JsonConvert.DeserializeObject<SubtitleModel>(json);
            StringBuilder stringBuilder=new StringBuilder();
            int i = 1;
            foreach (var item in subtitle.body)
            {
                var start = TimeSpan.FromSeconds(item.from);
                var end = TimeSpan.FromSeconds(item.to);
                stringBuilder.AppendLine(i.ToString());
                stringBuilder.AppendLine($"{start.ToString(@"hh\:mm\:ss\,fff")} --> {end.ToString(@"hh\:mm\:ss\,fff")}");
                var content = item.content;
                if (toSimplified)
                {
                    content = Utils.ToSimplifiedChinese(content);
                }
                stringBuilder.AppendLine(content);
                stringBuilder.AppendLine();
                i++;
            }
            return stringBuilder.ToString();
        }




        public class SubtitleModel
        {
            public double font_size { get; set; }
            public string font_color { get; set; }
            public double background_alpha { get; set; }
            public string background_color { get; set; }
            public string Stroke { get; set; }
            public List<SubtitleItemModel> body { get; set; }
        }
        public class SubtitleItemModel
        {
            public double from { get; set; }
            public double to { get; set; }
            public int location { get; set; }
            public string content { get; set; }
        }
    }
}
