using BiliLite.ViewModels.Common;
using PropertyChanged;

namespace BiliLite.ViewModels.Video
{
    public class DanmakuViewModel : BaseViewModel
    {
        [DoNotNotify]
        public bool ShowAreaControl { get; set; }

        [DoNotNotify]
        public bool ShowBoldControl { get; set; }

        [DoNotNotify]
        public bool ShowBoldStyleControl { get; set; }

        public double Area { get; set; }

        public double SizeZoom { get; set; }

        public int Speed { get; set; }

        public double Opacity { get; set; }

        public double MarginTop { get; set; }

        public int Density { get; set; }

        public int BolderStyle { get; set; }

        public bool Bold { get; set; }

        public bool IsHide { get; set; }
    }
}
