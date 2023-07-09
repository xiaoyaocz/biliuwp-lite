using System.Collections.Generic;

namespace BiliLite.Models.Common.Video
{
    public static class SoundQualityConstants
    {
        private static Dictionary<int, string> m_dictionary = new Dictionary<int, string>()
        {
            {30216,"64K" },
            {30232,"132K" },
            {30280,"192K" },
            {30250,"杜比" },
            {30251,"无损" }
        };

        public static Dictionary<int, string> Dictionary
        {
            get
            {
                return m_dictionary;
            }
        }
    }
}
