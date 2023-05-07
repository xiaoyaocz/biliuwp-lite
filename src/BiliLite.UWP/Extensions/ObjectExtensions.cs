using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace BiliLite.Extensions
{
    public static class ObjectExtensions
    {
        public static int ToInt32(this object obj)
        {

            if (int.TryParse(obj.ToString(), out var value))
            {
                return value;
            }
            else
            {
                return 0;
            }
        }

        public static string ToCountString(this object obj)
        {
            if (obj == null) return "0";
            if (double.TryParse(obj.ToString(), out var number))
            {

                if (number >= 10000)
                {
                    return ((double)number / 10000).ToString("0.0") + "万";
                }
                return obj.ToString();
            }
            else
            {
                return obj.ToString();
            }
        }

        public static T ObjectClone<T>(this T obj)
        {
            var type = typeof(T);

            if (!type.IsSerializable)
                return default;

            if (ReferenceEquals(obj, null))
                return default;

            IFormatter format = new BinaryFormatter();

            using MemoryStream ms = new MemoryStream();
            try
            {
                format.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                return (T)format.Deserialize(ms);
            }
            catch (Exception e)
            {
                return default;
            }
        }
    }
}
