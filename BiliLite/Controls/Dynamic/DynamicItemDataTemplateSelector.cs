using BiliLite.Modules.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BiliLite.Controls.Dynamic
{
    public class DynamicItemDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate RepostTemplate { get; set; }
        public DataTemplate MissTemplate { get; set; }
        public DataTemplate TextTemplate { get; set; }
        public DataTemplate Photo2x2Template { get; set; }
        public DataTemplate Photo3x3Template { get; set; }
        public DataTemplate ShortVideoTemplate { get; set; }
        public DataTemplate Photo1x1Template { get; set; }
        public DataTemplate OneRowTemplate { get; set; }
        public DataTemplate OtherTemplate { get; set; }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            
           var model = item as DynamicItemDisplayModel;
            switch (model.Type)
            {
                case Dynamic.DynamicDisplayType.Repost:
                    return RepostTemplate;
                case Dynamic.DynamicDisplayType.Text:
                    return TextTemplate;
                case Dynamic.DynamicDisplayType.Photo:
                    {
                        if (model.ImagesInfo.Count <= 1)
                        {
                            return Photo1x1Template;
                        }
                        if (model.ImagesInfo.Count ==4)
                        {
                            return Photo2x2Template;
                        }
                        return Photo3x3Template;
                    }
                   
                case Dynamic.DynamicDisplayType.ShortVideo:
                    return ShortVideoTemplate;
                case Dynamic.DynamicDisplayType.Video:
                case Dynamic.DynamicDisplayType.Season:
                case Dynamic.DynamicDisplayType.Music:
                case Dynamic.DynamicDisplayType.Web:
                case Dynamic.DynamicDisplayType.Article:
                case Dynamic.DynamicDisplayType.Live:
                case Dynamic.DynamicDisplayType.LiveShare:
                case Dynamic.DynamicDisplayType.MediaList:
                case Dynamic.DynamicDisplayType.Cheese:
                    return OneRowTemplate;
                case DynamicDisplayType.Miss:
                    return MissTemplate;
                default:
                    return OtherTemplate;
            }
        }
    }
}
