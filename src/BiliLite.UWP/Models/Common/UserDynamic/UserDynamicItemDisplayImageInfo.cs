using System.Collections.Generic;
using System.Windows.Input;

namespace BiliLite.Models.Common.UserDynamic
{
    public class UserDynamicItemDisplayImageInfo
    {
        public ICommand ImageCommand { get; set; }

        public int Index { get; set; }

        public string ImageUrl { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public List<string> AllImages { get; set; }

        public bool LongImage => Height > (Width * 2);

        public string ImageUrlWithPar
        {
            get
            {
                if (Height > (Width * 2))
                {
                    return ImageUrl + "@240w_320h_!header.webp";
                }
                else if (Height > (Width * 1.3))
                {
                    return ImageUrl + "@240w_320h_1e_1c.jpg";
                }
                else
                {
                    return ImageUrl + "@400w.jpg";
                }
            }
        }
    }
}