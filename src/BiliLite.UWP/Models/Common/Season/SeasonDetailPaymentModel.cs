using Newtonsoft.Json;

namespace BiliLite.Models.Common.Season
{
    public class SeasonDetailPaymentModel
    {
        public string Price { get; set; }

        [JsonProperty("tv_price")]
        public string TvPrice { get; set; }

        public SeasonDetailPaymentDialogModel Dialog { get; set; }
    }
}