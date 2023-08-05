using System;

namespace BiliLite.Extensions
{
    public static class TimeExtensions
    {
        /// <summary>
        /// 将时间戳转为时间
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        public static DateTime TimestampToDatetime(long ts)
        {
            DateTime dtStart = new DateTime(1970, 1, 1, 8, 0, 0);
            long lTime = long.Parse(ts + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            
            return dtStart.Add(toNow);
        }

        public static string HandelTimestamp(this long ts)
        {
            var dateTime = DateTimeOffset.FromUnixTimeSeconds(ts).ToLocalTime();
            var timeSpan = DateTimeOffset.Now - dateTime;
            if (timeSpan.TotalDays <= 0)
            {
                return "今天" + dateTime.ToString("HH:mm");
            }
            else if (timeSpan.TotalDays >= 1 && timeSpan.TotalDays < 2)
            {
                return "昨天" + dateTime.ToString("HH:mm");
            }
            else
            {
                return dateTime.ToString("yyyy-MM-dd HH:mm");
            }
        }

        public static string ProgressToTime(this long progress)
        {
            var ts = TimeSpan.FromSeconds(progress);
            return ts.ToString(ts.TotalHours >= 1 ? @"hh\:mm\:ss" : @"mm\:ss");
        }

        /// <summary>
        /// 生成时间戳/秒
        /// </summary>
        /// <returns></returns>
        public static long GetTimestampS()
        {
            return Convert.ToInt64((DateTime.Now - new DateTime(1970, 1, 1, 8, 0, 0, 0)).TotalSeconds);
        }

        /// <summary>
        /// 生成时间戳/豪秒
        /// </summary>
        /// <returns></returns>
        public static long GetTimestampMS()
        {
            return Convert.ToInt64((DateTime.Now - new DateTime(1970, 1, 1, 8, 0, 0, 0)).TotalMilliseconds);
        }
    }
}
