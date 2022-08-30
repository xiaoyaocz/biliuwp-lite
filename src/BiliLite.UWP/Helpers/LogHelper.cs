using NLog;
using NLog.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BiliLite.Helpers
{
    public enum LogType
    {
        INFO,
        DEBUG,
        ERROR,
        FATAL
    }
    public class LogHelper
    {
        public static LoggingConfiguration config;
        public static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public static void Log(string message, LogType type, Exception ex = null)
        {
            if (config == null)
            {
                config = new NLog.Config.LoggingConfiguration();
                Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                var logfile = new NLog.Targets.FileTarget()
                {
                    Name = "logfile",
                    CreateDirs = true,
                    FileName = storageFolder.Path + @"\log\" + DateTime.Now.ToString("yyyyMMdd") + ".log",
                    Layout = "${longdate}|${level:uppercase=true}|${logger}|${threadid}|${message}|${exception:format=Message,StackTrace}"
                };
                config.AddRule(LogLevel.Info, LogLevel.Info, logfile);
                config.AddRule(LogLevel.Debug, LogLevel.Debug, logfile);
                config.AddRule(LogLevel.Error, LogLevel.Error, logfile);
                config.AddRule(LogLevel.Fatal, LogLevel.Fatal, logfile);
                NLog.LogManager.Configuration = config;
            }
            Debug.WriteLine("[" + LogType.INFO.ToString() + "]" + message);
            switch (type)
            {
                case LogType.INFO:
                    logger.Info(message);
                    break;
                case LogType.DEBUG:
                    logger.Debug(message);
                    break;
                case LogType.ERROR:
                    logger.Error(ex, message);
                    break;
                case LogType.FATAL:
                    logger.Fatal(ex, message);
                    break;
                default:
                    break;
            }
        }
        public static bool IsNetworkError(Exception ex)
        {
            if (ex.HResult == -2147012867 || ex.HResult == -2147012889)
            {
                return true;
            }
            {
                return false;
            }
        }

    }
}
