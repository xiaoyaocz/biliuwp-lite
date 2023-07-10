using System.Collections.Generic;
using System.Linq;
using BiliLite.Modules;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BiliLite.Models.Common.Season
{
    public class SeasonDetailModel
    {
        [JsonProperty("season_id")]
        public int SeasonId { get; set; }

        [JsonProperty("season_title")]
        public string SeasonTitle { get; set; }

        public string Title { get; set; }

        public string Cover { get; set; }

        public string Evaluate { get; set; }

        public string Alias { get; set; }

        public string Badge { get; set; }

        [JsonProperty("badge_type")]
        public int BadgeType { get; set; }

        public int Status { get; set; }

        public string Subtitle { get; set; }

        [JsonProperty("show_badge")]
        public bool ShowBadge => !string.IsNullOrEmpty(Badge);

        public string Link { get; set; }

        [JsonProperty("short_link")]
        public string ShortLink { get; set; }

        [JsonProperty("square_cover")]
        public string SquareCover { get; set; }

        [JsonProperty("media_id")]
        public int MediaId { get; set; }

        public int Mode { get; set; }

        public JArray Modules { get; set; }

        [JsonProperty("up_info")]
        public SeasonDetailUpInfoModel UpInfo { get; set; }

        public SeasonDetailActorModel Actor { get; set; }

        public SeasonDetailActorModel Staff { get; set; }

        public List<SeasonDetailAreaItemModel> Areas { get; set; }

        public string Area
        {
            get
            {
                const string r = "";
                return Areas != null ? Areas.Aggregate(r, (current, item) => current + (item.Name + " ")) : "";
            }
        }

        [JsonProperty("new_ep")]
        public SeasonDetailNewEpModel NewEp { get; set; }

        public List<SeasonDetailEpisodeModel> Episodes { get; set; }

        [JsonProperty("origin_name")]
        public string OriginName { get; set; }

        public SeasonDetailRatingModel Rating { get; set; }

        [JsonProperty("show_rating")]
        public bool ShowRating => Rating != null;

        public SeasonDetailPublishModel Publish { get; set; }

        public List<SeasonDetailSeasonItemModel> Seasons { get; set; }

        [JsonProperty("show_seasons")]
        public bool ShowSeasons => Seasons != null && Seasons.Count > 1;

        [JsonProperty("current_season")]
        public SeasonDetailSeasonItemModel CurrentSeason
        {
            get
            {
                return Seasons?.FirstOrDefault(x => x.SeasonId == SeasonId);
            }
        }

        public List<SeasonDetailStyleItemModel> Styles { get; set; }

        public SeasonDetailStatModel Stat { get; set; }

        public int Total { get; set; }

        public int Type { get; set; }

        [JsonProperty("type_name")]
        public string TypeName { get; set; }

        [JsonProperty("user_status")]
        public SeasonDetailUserStatusModel UserStatus { get; set; }

        public SeasonDetailLimitModel Limit { get; set; }

        public List<SeasonDetailSectionItemModel> Section { get; set; }

        public SeasonDetailPaymentModel Payment { get; set; }

        [JsonProperty("show_payment")]
        public bool ShowPayment => Payment is { Dialog: { } };
    }
}