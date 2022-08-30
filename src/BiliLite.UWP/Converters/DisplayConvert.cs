using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace BiliLite.Converters
{
    public class DisplayConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value==null)
            {
                return Visibility.Collapsed;
            }
            //如果是bool，反转下结果返回
            if(value is bool)
            {
                if ((bool)value)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
            //如果是Visibility，反转下结果返回
            if (value is Visibility)
            {
                if ((Visibility)value== Visibility.Collapsed)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
            //如果是数字，则内容与parameter相等时返回显示
            if (value is int||value is long)
            {
                if (value.ToString()== parameter.ToString())
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
           return  Visibility.Visible;
        }
    }
}
