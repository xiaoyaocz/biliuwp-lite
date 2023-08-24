using System;
using Windows.UI.Xaml.Data;

namespace BiliLite.Converters
{
    public class BooleanConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value==null|| parameter==null)
            {
                return false;
            }
            return value.ToString() == parameter.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return System.Convert.ToInt32(parameter);
        }
    }
}
