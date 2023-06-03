namespace BiliLite.Models.Common.Account
{
    public class CheckCookieResult
    {
        /// <summary>
        /// 是否应该刷新 cookie
        /// </summary>
        public bool Refresh { get; set; }

        /// <summary>
        /// 当前毫秒时间戳
        /// </summary>
        public long Timestamp { get; set; }
    }
}
