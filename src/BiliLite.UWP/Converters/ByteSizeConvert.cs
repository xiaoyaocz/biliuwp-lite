using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace BiliLite.Converters
{
    public class ByteSizeConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var par = new string[] { "MB", "--" };

            if (parameter != null)
            {
                par = parameter.ToString().Split(",");
            }
            var unit = par[0];
            var defaultVal = par[1];
            var size = (ulong)value;
            if (size == 0)
            {
                return defaultVal;
            }
            if (unit == "GB")
            {
                return (size / 1024d / 1024d / 1024d).ToString("0.00")+ unit;
            }
            else
            if (unit == "MB")
            {
                return (size / 1024d / 1024d ).ToString("0.00") + unit;
            }
            else
            {
                return (size / 1024d).ToString("0.00") + unit;
            }
           
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
