using System;

namespace BiliLite.Models.Events
{
    public class AysncUnhandledExceptionEventArgs : EventArgs
    {
        public bool Handled { get; set; }
        public Exception Exception { get; set; }
    }
}