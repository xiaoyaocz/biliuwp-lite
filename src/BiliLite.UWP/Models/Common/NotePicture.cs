using Newtonsoft.Json;

namespace BiliLite.Models.Common
{
    /// <summary>
    /// 笔记图片
    /// </summary>
    public class NotePicture
    {
        /// <summary>
        /// 图片地址	
        /// </summary>
        [JsonProperty("img_src")]
        public string ImgSrc { get; set; }

        /// <summary>
        /// 图片宽度	
        /// </summary>
        [JsonProperty("img_width")]
        public float ImgWidth { get; set; }

        /// <summary>
        /// 图片高度	
        /// </summary>
        [JsonProperty("img_height")]
        public float ImgHeight { get; set; }

        /// <summary>
        /// 图片大小	
        /// </summary>
        [JsonProperty("img_size")]
        public float ImgSize { get; set; }
    }
}
