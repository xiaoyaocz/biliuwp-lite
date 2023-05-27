using Newtonsoft.Json;

namespace BiliLite.Models.Common.Video
{
    public class VideoUgcSeasonSectionEpisode
    {
        public long Id { get; set; }

        public string Aid { get; set; }

        public string Cid { get; set; }

        public string Title { get; set; }

        public string Cover { get; set; }

        [JsonProperty("cover_right_text")]
        public string CovverRightText { get; set; }

        //public int Page { get; set; }

        public string Part { get; set; }

        public long Duration { get; set; }

        public string Vid { get; set; }

        public string Bvid { get; set; }

        public VideoAuthor Author { get; set; }

        [JsonProperty("author_desc")]
        public string AuthorDesc { get; set; }

        [JsonProperty("first_frame")]
        public string FirstFrame { get; set; }
    }
}
