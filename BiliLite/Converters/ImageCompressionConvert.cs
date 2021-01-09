using BiliLite.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace BiliLite.Converters
{
    public class ImageCompressionConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return "ms-appx:///Assets/Thumbnails/Placeholde.png";
            }
            if (SettingHelper.UI.LoadOriginalImage)
            {
                return value;
            }
            if (value.ToString().Contains("@"))
            {
                return value;
            }
            if (parameter == null) return value;
            return value.ToString() + "@" + parameter.ToString() + ".jpg";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
