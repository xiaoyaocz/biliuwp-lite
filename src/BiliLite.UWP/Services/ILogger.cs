using BiliLite.Models.Common;
using System;
using System.Runtime.CompilerServices;

namespace BiliLite.Services
{
    public interface ILogger
    {
        public void Trace(string message, Exception ex = null, [CallerMemberName] string methodName = null);

        public void Info(string message, Exception ex = null, [CallerMemberName] string methodName = null);

        public void Debug(string message, Exception ex = null, [CallerMemberName] string methodName = null);

        public void Error(string message, Exception ex = null, [CallerMemberName] string methodName = null);

        public void Fatal(string message, Exception ex = null, [CallerMemberName] string methodName = null);

        public void Log(string message, LogType type, Exception ex = null, [CallerMemberName] string methodName = null, string typeName = null);
    }
}
