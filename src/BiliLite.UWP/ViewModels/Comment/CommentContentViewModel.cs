using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json.Linq;
using PropertyChanged;

namespace BiliLite.ViewModels.Comment
{
    public class CommentContentViewModel : BaseViewModel
    {
        public List<NotePicture> Pictures { get; set; }
        public string Message { get; set; }
        public int Plat { get; set; }

        [DependsOn(nameof(Plat))]
        public string PlatStr
        {
            get
            {
                return Plat switch
                {
                    2 => "来自 Android",
                    3 => "来自 IOS",
                    4 => "来自 WindowsPhone",
                    6 => "来自 Windows",
                    _ => ""
                };
            }
        }
        public string Device { get; set; }

        [DependsOn(nameof(Message))]
        public RichTextBlock Text => Message.ToRichTextBlock(Emote);

        public JObject Emote { get; set; }
    }
}
