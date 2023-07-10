using Newtonsoft.Json;

namespace BiliLite.Models.Common.Season
{
    public class SeasonDetailEpisodeModel
    {
        public string Aid { get; set; }

        public string Cid { get; set; }

        [JsonProperty("badge_type")]
        public int BadgeType { get; set; }

        public string Badge { get; set; }

        public bool ShowBadge => !string.IsNullOrEmpty(Badge);

        public string Cover { get; set; }

        public string Bvid { get; set; }

        private int? m_id;

        public int Id
        {
            get
            {
                if (m_id == null && EpId != null)
                {
                    return EpId.Value;
                }
                return m_id.Value;
            }
            set => m_id = value;
        }

        private int? m_status;

        public int Status
        {
            get
            {
                if (m_status == null && EpisodeStatus != null)
                {
                    return EpisodeStatus.Value;
                }
                return m_status.Value;
            }
            set => m_status = value;
        }

        private string m_title;

        public string Title
        {
            get
            {
                if (m_title == null && Index != null)
                {
                    return Index;
                }
                return m_title;
            }
            set => m_title = value;
        }

        private string m_longTitle;

        [JsonProperty("long_title")]
        public string LongTitle
        {
            get
            {
                if (m_longTitle == null && IndexTitle != null)
                {
                    return IndexTitle;
                }
                return m_longTitle;
            }
            set => m_longTitle = value;
        }

        [JsonProperty("ep_id")]
        public int? EpId { get; set; }

        [JsonProperty("episode_status")]
        public int? EpisodeStatus { get; set; }

        public string Index { get; set; }

        [JsonProperty("index_title")]
        public string IndexTitle { get; set; }

        [JsonProperty("section_type")]
        public int SectionType { get; set; }

        public bool IsPreview => SectionType != 0;
    }
}