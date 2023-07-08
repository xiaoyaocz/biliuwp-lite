namespace BiliLite.Models.Common.Video.Detail
{
    public class VideoDetailOwnerExtOfficialVerifyModel
    {
        /// <summary>
        /// 0个人认证,1企业认证
        /// </summary>
        public int Type { get; set; }

        public string Desc { get; set; }
    }
}