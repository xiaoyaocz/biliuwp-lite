using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BiliLite.Controls.Dynamic;
using BiliLite.Models.Common;

namespace BiliLite.Controls.DataTemplateSelectors
{
    public class UserDynamicItemDataTemplateSelector : DataTemplateSelector
    {
        private static readonly Dictionary<UserDynamicDisplayType, Func<UserDynamicItemDataTemplateSelector, UserDynamicItemDisplayViewModel, DataTemplate>> _dynamicTypeTemplateSelectFuncs;

        static UserDynamicItemDataTemplateSelector()
        {
            DataTemplate SelectRowTemplate(UserDynamicItemDataTemplateSelector selector, UserDynamicItemDisplayViewModel _) => selector.OneRowTemplate;
            _dynamicTypeTemplateSelectFuncs = new Dictionary<UserDynamicDisplayType, Func<UserDynamicItemDataTemplateSelector, UserDynamicItemDisplayViewModel, DataTemplate>>()
            {
                {UserDynamicDisplayType.Repost, (selector,_) => selector.RepostTemplate },
                {UserDynamicDisplayType.Text, (selector,_) => selector.TextTemplate },
                {UserDynamicDisplayType.Photo, (selector,model) =>
                {
                    if (model.ImagesInfo.Count <= 1)
                    {
                        return selector.Photo1x1Template;
                    }
                    return model.ImagesInfo.Count == 4 ? selector.Photo2x2Template : selector.Photo3x3Template;
                } },
                {UserDynamicDisplayType.ShortVideo,(selector,_) => selector.ShortVideoTemplate },
                {UserDynamicDisplayType.Video,SelectRowTemplate},
                {UserDynamicDisplayType.Season,SelectRowTemplate},
                {UserDynamicDisplayType.Music,SelectRowTemplate},
                {UserDynamicDisplayType.Web,SelectRowTemplate},
                {UserDynamicDisplayType.Article,SelectRowTemplate},
                {UserDynamicDisplayType.Live,SelectRowTemplate},
                {UserDynamicDisplayType.LiveShare,SelectRowTemplate},
                {UserDynamicDisplayType.MediaList,SelectRowTemplate},
                {UserDynamicDisplayType.Cheese,SelectRowTemplate},
                {UserDynamicDisplayType.Miss,(selector,_) => selector.MissTemplate },
            };
        }

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
            var model = item as UserDynamicItemDisplayViewModel;
            var success = _dynamicTypeTemplateSelectFuncs.TryGetValue(model.Type, out var selectFunc);
            return success ? selectFunc(this, model) : OtherTemplate;
        }
    }
}
