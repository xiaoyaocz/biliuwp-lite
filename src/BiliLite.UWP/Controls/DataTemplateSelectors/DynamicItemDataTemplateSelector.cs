using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BiliLite.Models.Common.Dynamic;

namespace BiliLite.Controls.DataTemplateSelectors
{
    public class DynamicItemDataTemplateSelector : DataTemplateSelector
    {
        public ResourceDictionary Resource { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is DynamicItemModel card && card.Desc.Type == 8)
            {
                return Resource["DynamicVideo"] as DataTemplate;
            }
            else
            {
                return Resource["DynamicSeason"] as DataTemplate;
            }
        }
    }
}