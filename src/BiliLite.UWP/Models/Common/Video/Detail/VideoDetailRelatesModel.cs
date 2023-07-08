namespace BiliLite.Models.Common.Video.Detail
{
    public class VideoDetailRelatesModel
    {
        public string Aid { get; set; }

        public string Pic { get; set; }

        public string Title { get; set; }

        public VideoDetailOwnerModel Owner { get; set; }
        
        public VideoDetailStatModel Stat { get; set; }
    }
}