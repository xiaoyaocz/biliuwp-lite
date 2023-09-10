namespace BiliLite.Models.Common.UserDynamic
{
    public class UserDynamicItemDisplayOneRowInfo
    {
        public string Cover { get; set; }

        public string Url { get; set; }

        public string CoverText { get; set; }

        public string CoverParameter { get; set; } = "412w_232h_1c";

        public double CoverWidth { get; set; } = 160;

        public bool ShowCoverText => !string.IsNullOrEmpty(CoverText);

        public string Tag { get; set; }

        public bool ShowTag => !string.IsNullOrEmpty(Tag);

        public string Title { get; set; }

        public string Subtitle { get; set; }

        public string Desc { get; set; }

        public object ID { get; set; }

        public string AID { get; set; }
    }
}