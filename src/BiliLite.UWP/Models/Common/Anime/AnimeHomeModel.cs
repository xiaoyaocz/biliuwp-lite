using System.Collections.Generic;
using BiliLite.Modules;

namespace BiliLite.Models.Common.Anime
{
    public class AnimeHomeModel
    {
        public List<AnimeRankModel> Hots { get; set; }

        public List<AnimeBannerModel> Banners { get; set; }

        public List<AnimeRankModel> Ranks { get; set; }

        public List<AnimeTimelineItemModel> Today { get; set; }

        public List<AnimeFallModel> Falls { get; set; }
    }
}