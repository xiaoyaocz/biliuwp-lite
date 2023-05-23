using NSDanmaku.Model;
using System.Collections.Generic;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Models.Common.Danmaku
{
    public class CompareDanmakuModel : IEqualityComparer<DanmakuModel>
    {
        public bool Equals(DanmakuModel x, DanmakuModel y)
        {
            return x.text == y.text;
        }
        public int GetHashCode(DanmakuModel obj)
        {
            return obj.text.GetHashCode();
        }
    }
}
