using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace BiliLite.Converters
{
    public class CountOrTextConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return "";
            }
            if (value is int || value is long)
            {
                var number = System.Convert.ToDouble(value);
                if (number <= 0)
                {
                    return parameter.ToString();
                }
                if (number >= 10000)
                {
                    return ((double)number / 10000).ToString("0.0") + "万";
                }
               
            }
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return "";
        }
    }
}

