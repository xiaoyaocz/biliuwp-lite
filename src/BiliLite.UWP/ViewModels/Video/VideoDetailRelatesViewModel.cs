using BiliLite.Models.Common.Video.Detail;

namespace BiliLite.ViewModels.Video
{
    public class VideoDetailRelatesViewModel
    {
        public string Aid { get; set; }

        public string Pic { get; set; }

        public string Title { get; set; }

        public VideoDetailOwnerModel Owner { get; set; }

        public VideoDetailStatViewModel Stat { get; set; }
    }
}
