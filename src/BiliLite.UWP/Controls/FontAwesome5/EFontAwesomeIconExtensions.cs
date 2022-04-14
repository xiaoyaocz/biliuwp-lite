using System;
using System.Linq;
using System.Reflection;
using Windows.UI.Xaml.Media;

namespace FontAwesome5.Extensions
{
    /// <summary>
    /// EFontAwesomeIcon extensions
    /// </summary>
    public static class EFontAwesomeIconExtensions
    {
        /// <summary>
        /// Get the Font Awesome label of an icon
        /// </summary>
        public static string GetLabel(this EFontAwesomeIcon icon)
        {
            var info = icon.GetInformationAttribute<FontAwesomeInformationAttribute>();
            if (info == null)
                return null;

            return info.Label;
        }
        /// <summary>
        /// Get the FontFamily of an icon
        /// </summary>
        public static FontFamily GetFontFamily(this EFontAwesomeIcon icon)
        {
            var info = icon.GetInformationAttribute<FontAwesomeInformationAttribute>();
            if (info == null)
                return Fonts.RegularFontFamily;

            switch (info.Style)
            {
                case EFontAwesomeStyle.Regular: return Fonts.RegularFontFamily;
                case EFontAwesomeStyle.Solid: return Fonts.SolidFontFamily;
                case EFontAwesomeStyle.Brands: return Fonts.BrandsFontFamily;
            }

            return null;
        }
        /// <summary>
        /// Get the Font Awesome Style of an icon
        /// </summary>
        public static EFontAwesomeStyle GetStyle(this EFontAwesomeIcon icon)
        {
            var info = icon.GetInformationAttribute<FontAwesomeInformationAttribute>();
            if (info == null)
                return EFontAwesomeStyle.None;

            return info.Style;
        }

        /// <summary>
        /// Get the SVG path of an icon
        /// </summary>
        public static bool GetSvg(this EFontAwesomeIcon icon, out string path, out int width, out int height)
        {
            path = string.Empty;
            width = -1;
            height = -1;


            var svgInfo = icon.GetInformationAttribute<FontAwesomeSvgInformationAttribute>();
            if (svgInfo == null)
                return false;

            path = svgInfo.Path;
            width = svgInfo.Width;
            height = svgInfo.Height;

            return true;
        }

        /// <summary>
        /// Get the Unicode of an icon
        /// </summary>
        public static string GetUnicode(this EFontAwesomeIcon icon)
        {
            var info = icon.GetInformationAttribute<FontAwesomeInformationAttribute>();
            if (info == null)
                return char.ConvertFromUtf32(0);

            return char.ConvertFromUtf32(info.Unicode);
        }

        public static T GetInformationAttribute<T>(this EFontAwesomeIcon icon) where T : class
        {
            if (icon == EFontAwesomeIcon.None)
                return null;

            var memInfo = typeof(EFontAwesomeIcon).GetMember(icon.ToString());
            if (memInfo.Length == 0)
                throw new Exception("EFontAwesomeIcon not found.");

            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false)
                .ToList();

            if (!attributes.Any())
                throw new Exception("FontAwesomeInformationAttribute not found.");

            return attributes[0] as T;
        }
    }
}
