using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace BiliLite.Converters
{
    public class TimeSpanToDoubleConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var ts = (TimeSpan)value;
            return ts.TotalSeconds;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return TimeSpan.FromSeconds((double)value);
        }
    }
}
