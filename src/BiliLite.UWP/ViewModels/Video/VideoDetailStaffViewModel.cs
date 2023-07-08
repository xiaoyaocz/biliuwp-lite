using BiliLite.Models.Common.Video.Detail;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json;

namespace BiliLite.ViewModels.Video
{
    public class VideoDetailStaffViewModel : BaseViewModel
    {
        public string Mid { get; set; }

        public string Title { get; set; }

        public string Face { get; set; }

        public string Name { get; set; }

        public VideoDetailOwnerExtVipModel Vip { get; set; }

        [JsonProperty("official_verify")]
        public VideoDetailOwnerExtOfficialVerifyModel OfficialVerify { get; set; }

        public int Attention { get; set; }
    }
}