using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace BiliLite.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value switch
            {
                null => Visibility.Collapsed,
                bool b when b => Visibility.Visible,
                bool _ => Visibility.Collapsed,
                _ => Visibility.Visible
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Visibility.Visible;
        }

        public static Visibility AntiConvert(bool value)
        {
            return value switch
            {
                true => Visibility.Collapsed,
                false => Visibility.Visible,
            };
        }
    }
}
