using System;

namespace BiliLite.Extensions
{
    public static class TimeExtensions
    {
        public static string DateTimeToDisplayTimeText(this DateTime dateTime)
        {
            var span = DateTime.Now - dateTime;
            if (span.TotalDays > 7)
            {
                return dateTime.ToString("yyyy-MM-dd");
            }
            else
            if (span.TotalDays > 1)
            {
                return $"{(int)Math.Floor(span.TotalDays)}天前";
            }
            else
            if (span.TotalHours > 1)
            {
                return $"{(int)Math.Floor(span.TotalHours)}小时前";
            }
            else
            if (span.TotalMinutes > 1)
            {
                return $"{(int)Math.Floor(span.TotalMinutes)}分钟前";
            }
            else
            if (span.TotalSeconds >= 1)
            {
                return $"{(int)Math.Floor(span.TotalSeconds)}秒前";
            }
            else
            {
                return "1秒前";
            }
        }

        /// <summary>
        /// 将时间戳转为时间
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        public static DateTime TimestampToDatetime(this long ts)
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
