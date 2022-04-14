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
    public class RankColorConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return new SolidColorBrush(Colors.Black);
            }
            int rank = (int)value;
            Color color = Colors.Gray;
            switch (rank)
            {
                case 1:
                    color = Colors.DarkOrange;
                    break;
                case 2:
                    color = Colors.Silver;
                    break;
                case 3:
                    color = Colors.Orange;
                    break;
                default:
                    color = Colors.Gray;
                    break;
            }
            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
