namespace BiliLite.Models.Common.Video.PlayUrlInfos
{
    public class BiliFlvPlayUrlInfo
    {
        /// <summary>
        /// 排序
        /// </summary>
        public int Order { get; set; }
        /// <summary>
        /// 链接
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 时长,毫秒
        /// </summary>
        public long Length { get; set; }
        /// <summary>
        /// 文件大小
        /// </summary>
        public long Size { get; set; }
    }
}
