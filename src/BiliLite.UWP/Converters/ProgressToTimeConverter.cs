using System;
using Windows.UI.Xaml.Data;

namespace BiliLite.Converters
{
    public class ProgressToTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch (value)
            {
                case null:
                    return "00:00";
                case double second:
                {
                    var ts = TimeSpan.FromSeconds(second);
                
                    return ts.ToString(ts.TotalHours>=1? @"hh\:mm\:ss" : @"mm\:ss");
                }
                case int _:
                {
                    var ts = TimeSpan.FromSeconds(System.Convert.ToDouble(value));
                    return ts.ToString(ts.TotalHours >= 1 ? @"hh\:mm\:ss" : @"mm\:ss");
                }
                case TimeSpan secondSpan:
                    return secondSpan.ToString(secondSpan.TotalHours >= 1 ? @"hh\:mm\:ss" : @"mm\:ss");
                case string secondStr:
                {
                    var ts = TimeSpan.FromSeconds(long.Parse(secondStr));
                    return ts.ToString(ts.TotalHours >= 1 ? @"hh\:mm\:ss" : @"mm\:ss");
                }
                default:
                    return value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return "00:00";
        }
    }
}
