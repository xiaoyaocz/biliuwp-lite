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
        public string PlatStr
        {
            get
            {
                switch (Plat)
                {
                    case 2:
                        return "来自 Android";
                    case 3:
                        return "来自 IOS";
                    case 4:
                        return "来自 WindowsPhone";
                    case 6:
                        return "来自 Windows";
                    default:
                        return "";
                }
            }
        }
        public string Device { get; set; }
        public RichTextBlock Text
        {
            get
            {
                //var tx = new RichTextBlock();
                //Paragraph paragraph = new Paragraph();
                //Run run = new Run() { Text = message };
                //paragraph.Inlines.Add(run);
                //tx.Blocks.Add(paragraph);
                //return tx;

                return Message.ToRichTextBlock(Emote);
            }

        }

        public JObject Emote { get; set; }
    }
}