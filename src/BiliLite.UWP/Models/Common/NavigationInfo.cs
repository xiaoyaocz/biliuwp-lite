using System;
using Windows.UI.Xaml.Controls;

namespace BiliLite.Models.Common
{
    public class NavigationInfo
    {
        public Symbol icon { get; set; } = Symbol.Document;
        public Type page { get; set; }
        public string title { get; set; }
        public object parameters { get; set; }
        public bool dontGoTo { get; set; } = false;
    }
}
