using System;
using Windows.UI.Xaml.Data;
using BiliLite.Extensions;

namespace BiliLite.Converters
{
    public class DatetimeConvert:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value==null)
            {
                return "";
            }
            var ts = value.ToString();
            if (ts.Length == 10)
            {
                ts += "0000000";
            }
            DateTime dtStart = new DateTime(1970, 1, 1, 0, 0, 0);
            long lTime = long.Parse(ts);
            TimeSpan toNow = new TimeSpan(lTime);
            DateTime dt = dtStart.Add(toNow).ToLocalTime();
            var parse = "yyyy-MM-dd HH:mm:ss";
            if (parameter!=null)
            {
                if (parameter.ToString() == "ts")
                {
                    return dt.DateTimeToDisplayTimeText();
                }
                parse = parameter.ToString();
            }
           

            return dt.ToString(parse);
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }

}
