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
    public class ColorConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return new SolidColorBrush(Colors.Transparent);
            }
            Color color = new Color();
            try
            {
                var obj = value.ToString().Replace("#", "");
                if (long.TryParse(obj, out var c))
                {
                    obj = c.ToString("X2");
                }
                
                if (obj.Length<=6)
                {
                    obj = obj.PadLeft(6,'0');
                    color.R = byte.Parse(obj.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                    color.G = byte.Parse(obj.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                    color.B = byte.Parse(obj.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                    color.A = 255;
                }
                else
                {
                    obj = obj.PadLeft(8, '0');
                    color.R = byte.Parse(obj.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                    color.G = byte.Parse(obj.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                    color.B = byte.Parse(obj.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
                    color.A = byte.Parse(obj.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                }
               
            }
            catch (Exception)
            {
                color = Colors.Transparent;
            }
            if (parameter !=null)
            {
                return color;
            }
            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
