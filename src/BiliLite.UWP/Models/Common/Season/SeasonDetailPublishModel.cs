using Newtonsoft.Json;

namespace BiliLite.Models.Common.Season
{
    public class SeasonDetailPublishModel
    {
        [JsonProperty("is_finish")]
        public int IsFinish { get; set; }

        [JsonProperty("is_started")]
        public int IsStarted { get; set; }

        public int Weekday { get; set; }

        [JsonProperty("pub_time")]
        public string PubTime { get; set; }

        [JsonProperty("pub_time_show")]
        public string PubTimeShow { get; set; }

        [JsonProperty("release_date_show")]
        public string ReleaseDateShow { get; set; }

        [JsonProperty("time_length_show")]
        public string TimeLengthShow { get; set; }
    }
}