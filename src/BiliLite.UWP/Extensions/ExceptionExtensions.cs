using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.Extensions
{
    public static class ExceptionExtensions
    {
        public static bool IsNetworkError(this Exception ex)
        {
            return ex.HResult == -2147012867 || ex.HResult == -2147012889;
        }
    }
}
