using Newtonsoft.Json;

namespace BiliLite.Models.Common.User
{
    public class SubmitVideoItemModel
    {
        public int Comment { get; set; }

        public string Play { get; set; }

        public string Pic { get; set; }

        public string Description { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public string Length { get; set; }

        public string Aid { get; set; }

        public long Created { get; set; }

        [JsonProperty("video_review")]
        public int VideoReview { get; set; }

        public string RedirectUrl { get; set; }
    }
}