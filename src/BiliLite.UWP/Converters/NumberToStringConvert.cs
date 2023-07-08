using System;
using Windows.UI.Xaml.Data;

namespace BiliLite.Converters
{
    public class NumberToStringConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Convert(value, parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        public static string Convert(object value, object parameter)
        {
            if (value == null)
            {
                return "";
            }

            if (value is int || value is long)
            {
                var number = System.Convert.ToDouble(value);
                if (parameter != null)
                {
                    return number.ToString(parameter.ToString());
                }
                if (number >= 10000)
                {
                    return ((double)number / 10000).ToString("0.0") + "万";
                }
            }
            if (value is double)
            {
                var number = (double)value;
                if (parameter != null)
                {
                    return number.ToString(parameter.ToString());
                }
                return number.ToString("0.00");
            }
            if (value is string)
            {
                if (double.TryParse(value.ToString(), out var num))
                {
                    if (num >= 10000)
                    {
                        return (num / 10000).ToString("0.0") + "万";
                    }
                }

            }
            return value.ToString();
        }
    }
}
