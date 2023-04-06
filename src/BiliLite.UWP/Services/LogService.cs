using BiliLite.Models.Common;
using log4net;
using log4net.Config;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace BiliLite.Services
{
    public class LogService
    {
        public static ILog logger = LogManager.GetLogger(typeof(LogHelper));

        public static void Log(string message, LogType type, Exception ex = null, [CallerMemberName] string methodName = null)
        {
            if (!LogManager.GetRepository().Configured)
            {
                var logRepository = LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly());
                XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
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
                    logger.Error(message, ex);
                    break;
                case LogType.FATAL:
                    logger.Fatal(message, ex);
                    break;
                default:
                    break;
            }
        }
        public static void Init()
        {
            var logRepository = LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
        }
    }
}