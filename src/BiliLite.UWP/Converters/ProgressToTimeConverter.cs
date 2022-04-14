using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace BiliLite.Converters
{
    public class ProgressToTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return "00:00";
            }
            if (value is double)
            {
                TimeSpan ts = TimeSpan.FromSeconds((double)value);
                
                return ts.ToString(ts.TotalHours>=1? @"hh\:mm\:ss" : @"mm\:ss");
            }
            if (value is int)
            {
               
                TimeSpan ts = TimeSpan.FromSeconds(System.Convert.ToDouble(value));
                return ts.ToString(ts.TotalHours >= 1 ? @"hh\:mm\:ss" : @"mm\:ss");
            }
            if (value is TimeSpan)
            {
                var ts= (TimeSpan)value;
                return ts.ToString(ts.TotalHours >= 1 ? @"hh\:mm\:ss" : @"mm\:ss");
            }
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return "00:00";
        }
    }
}
