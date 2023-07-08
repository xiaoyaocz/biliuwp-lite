namespace BiliLite.Models.Common.Video.Detail
{
    public class VideoDetailRightsModel
    {
        public int Bp { get; set; }

        /// <summary>
        /// 能不能充电
        /// </summary>
        public int Elec { get; set; }

        /// <summary>
        /// 能不能下载
        /// </summary>
        public int Download { get; set; }

        /// <summary>
        /// 是不是电影
        /// </summary>
        public int Movie { get; set; }

        /// <summary>
        /// 是不是付费
        /// </summary>
        public int Pay { get; set; }
    }
}