using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using BiliLite.Extensions;
using Newtonsoft.Json.Linq;

namespace BiliLite.Models.Common.Comment
{
    public class CommentContentModel
    {
        public List<NotePicture> Pictures { get; set; }
        public string Message { get; set; }
        public int Plat { get; set; }
        public string Device { get; set; }

        public JObject Emote { get; set; }
    }
}