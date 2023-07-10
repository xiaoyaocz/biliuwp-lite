namespace BiliLite.Models.Common.Season
{
    public class SeasonDetailRatingModel
    {
        public int Count { get; set; }
        
        public double Score { get; set; }

        public double Score5 => Score / 2;
    }
}