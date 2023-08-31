using System.Collections.Generic;
using BiliLite.Modules;

namespace BiliLite.Models.Common.Recommend
{
    public class RecommendThreePointV2ItemModel
    {
        public string Idx { get; set; }

        public string Title { get; set; }

        public string Type { get; set; }

        public string Subtitle { get; set; }

        public string Url { get; set; }

        public List<RecommendThreePointV2ItemReasonsModel> Reasons { get; set; }
    }
}