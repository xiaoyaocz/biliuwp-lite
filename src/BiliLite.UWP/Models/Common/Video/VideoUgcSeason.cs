using AutoMapper;
using System.Collections.Generic;

namespace BiliLite.Models.Common.Video
{
    public class VideoUgcSeason
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string Cover { get; set; }

        public string Intro { get; set; }

        public List<VideoUgcSeasonSection> Sections { get; set; }
    }
}
