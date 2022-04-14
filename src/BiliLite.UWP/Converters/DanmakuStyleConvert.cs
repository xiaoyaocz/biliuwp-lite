using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace BiliLite.Converters
{
    public class DanmakuStyleConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var style = (NSDanmaku.Model.DanmakuBorderStyle)value;
            return (int)style;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
