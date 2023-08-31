using System.Collections.Generic;
using Atelier39;

namespace BiliLite.Models.Common.Danmaku
{
    public class CompareDanmakuItem : IEqualityComparer<DanmakuItem>
    {
        public bool Equals(DanmakuItem x, DanmakuItem y)
        {
            return x.Text == y.Text;
        }
        public int GetHashCode(DanmakuItem obj)
        {
            return obj.Text.GetHashCode();
        }
    }
}
