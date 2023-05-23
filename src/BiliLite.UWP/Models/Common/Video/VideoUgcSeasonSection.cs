using System.Collections.Generic;

namespace BiliLite.Models.Common.Video
{
    public class VideoUgcSeasonSection
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public int Type { get; set; }

        public List<VideoUgcSeasonSectionEpisode> Episodes { get; set; }
    }
}
