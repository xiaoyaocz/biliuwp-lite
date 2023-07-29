using BiliLite.Models.Common;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BiliLite.Services
{
    public class GlobalLogger : ILogger
    {
        private string m_typeName;

        public void Trace(string message, Exception ex = null, string methodName = null)
        {
            LogService.Log(message, LogType.Trace, ex, methodName, m_typeName);
        }

        public void Info(string message, Exception ex = null, [CallerMemberName] string methodName = null)
        {
            LogService.Log(message, LogType.Info, ex, methodName, m_typeName);
        }

        public void Debug(string message, Exception ex = null, [CallerMemberName] string methodName = null)
        {
            LogService.Log(message, LogType.Debug, ex, methodName, m_typeName);
        }

        public void Error(string message, Exception ex = null, [CallerMemberName] string methodName = null)
        {
            LogService.Log(message, LogType.Error, ex, methodName, m_typeName);
        }

        public void Fatal(string message, Exception ex = null, [CallerMemberName] string methodName = null)
        {
            LogService.Log(message, LogType.Fatal, ex, methodName, m_typeName);
        }

        public void Log(string message, LogType type, Exception ex = null, [CallerMemberName] string methodName = null, string typeName = null)
        {
            var className = m_typeName;
            if (!string.IsNullOrEmpty(typeName))
            {
                className = typeName;
            }
            LogService.Log(message, type, ex, methodName, className);
        }

        public static GlobalLogger FromCurrentType()
        {
            var type = new StackTrace().GetFrame(1).GetMethod().ReflectedType;
            var logger = new GlobalLogger() { m_typeName = type.Name };
            return logger;
        }
    }
}
