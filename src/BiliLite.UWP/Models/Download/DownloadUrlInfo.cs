using System.Collections.Generic;

namespace BiliLite.Models.Download
{
    public class DownloadUrlInfo
    {
        /// <summary>
        /// 链接
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 保存文件名称
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// Http请求头
        /// </summary>
        public IDictionary<string, string> HttpHeader { get; set; }
    }
}