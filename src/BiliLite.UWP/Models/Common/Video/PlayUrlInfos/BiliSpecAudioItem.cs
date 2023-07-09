using System.Collections.Generic;
using BiliLite.Modules.Player.Playurl;

namespace BiliLite.Models.Common.Video.PlayUrlInfos
{
    public class BiliFlacItem
    {
        public bool Display { get; set; }

        public DashItemModel Audio { get; set; }
    }

    public class BiliDolbyItem
    {
        public int Type { get; set; }

        public List<DashItemModel> Audio { get; set; }
    }
}
