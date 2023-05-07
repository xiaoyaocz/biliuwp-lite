using System;
using System.IO;

namespace BiliLite.Extensions
{
    public static class StreamExtensions
    {
        public static void ReadB(this Stream stream, byte[] buffer, int offset, int count)
        {
            if (offset + count > buffer.Length)
                throw new ArgumentException();
            var read = 0;
            while (read < count)
            {
                var available = stream.Read(buffer, offset, count - read);
                if (available == 0)
                {
                    // throw new ObjectDisposedException(null);
                }
                read += available;
                offset += available;
            }
        }
    }
}
