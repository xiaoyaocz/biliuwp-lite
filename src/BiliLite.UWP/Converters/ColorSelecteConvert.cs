using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
namespace BiliLite.Converters
{
   public class ColorSelecteConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value==null)
            {
                return new SolidColorBrush((Color)App.Current.Resources["TextColor"]);
            }
            if (value.ToString()== parameter.ToString())
            {
                return new SolidColorBrush((Color)App.Current.Resources["HighLightColor"]);
            }
            else
            {
                return (SolidColorBrush)App.Current.Resources["DefaultTextColor"];
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Colors.Black;
        }
    }
}
