using System.Collections.Generic;

namespace BiliLite.Models.Common.Season
{
    public class SeasonDetailSectionItemModel
    {
        public string Title { get; set; }

        public int Id { get; set; }

        public int Type { get; set; }

        public List<SeasonDetailEpisodeModel> Episodes { get; set; }
    }
}