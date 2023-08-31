using Windows.UI;

namespace BiliLite.Extensions
{
    public static class ColorExtensions
    {
        public static Color ParseColor(this uint colorValue)
        {
            colorValue = colorValue & 0xFFFFFF; // Ingore alpha
            var b = 0xFF & colorValue;
            var g = (0xFF00 & colorValue) >> 8;
            var r = (0xFF0000 & colorValue) >> 16;
            return Color.FromArgb(byte.MaxValue, (byte)r, (byte)g, (byte)b);
        }
    }
}
