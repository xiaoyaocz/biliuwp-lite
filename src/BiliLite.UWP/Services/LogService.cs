using BiliLite.Extensions;
using BiliLite.Models.Common;
using NLog;
using NLog.Config;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;

namespace BiliLite.Services
{
    public class LogService
    {
        public static LoggingConfiguration config;
        public static Logger logger = LogManager.GetCurrentClassLogger();

        private static bool IsAutoClearLogFile => SettingService.GetValue<bool>(SettingConstants.Other.AUTO_CLEAR_LOG_FILE, true);
        private static int AutoClearLogFileDay => SettingService.GetValue<int>(SettingConstants.Other.AUTO_CLEAR_LOG_FILE_DAY, 7);
        private static bool IsProtectLogInfo => SettingService.GetValue<bool>(SettingConstants.Other.PROTECT_LOG_INFO, true);

        public static void Init()
        {
            config = new LoggingConfiguration();
            var storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var logfile = new NLog.Targets.FileTarget()
            {
                Name = "logfile",
                CreateDirs = true,
                FileName = storageFolder.Path + @"\log\" + DateTime.Now.ToString("yyyyMMdd") + ".log",
                Layout = "${longdate}|${level:uppercase=true}|${threadid}|${event-properties:item=type}.${event-properties:item=method}|${message}|${exception:format=Message,StackTrace}"
            };
            config.AddRule(LogLevel.Info, LogLevel.Info, logfile);
            config.AddRule(LogLevel.Debug, LogLevel.Debug, logfile);
            config.AddRule(LogLevel.Error, LogLevel.Error, logfile);
            config.AddRule(LogLevel.Fatal, LogLevel.Fatal, logfile);
            LogManager.Configuration = config;
        }

        public static async Task DeleteExpiredLogFile()
        {
            var storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            if (IsAutoClearLogFile)
            {
                await DeleteFile(storageFolder.Path + @"\log\");
            }
        }

        private static async Task DeleteFile(string path)
        {
            var pattern = "yyyyMMdd";
            var days = AutoClearLogFileDay;
            var folder = await StorageFolder.GetFolderFromPathAsync(path);

            var files = await folder.GetFilesAsync();

            foreach (var file in files)
            {
                var fileName = file.DisplayName;
                if (!DateTimeOffset.TryParseExact(fileName, pattern, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fileDate))
                {
                    continue;
                }
                if (fileDate < DateTimeOffset.Now.AddDays(-days))
                {
                    File.Delete(file.Path);
                }
            }
        }

        public static void Log(string message, LogType type, Exception ex = null, [CallerMemberName] string methodName = null, string typeName = "unknowType")
        {
            Debug.WriteLine("[" + LogType.INFO.ToString() + "]" + message);
            if (IsProtectLogInfo)
                message = message.ProtectValues("access_key", "csrf", "access_token", "sign");

            var logEvent = new LogEventInfo(LogLevel.Info, null, message);
            switch (type)
            {
                case LogType.INFO:
                    logEvent.Level = LogLevel.Info;
                    break;
                case LogType.DEBUG:
                    logEvent.Level = LogLevel.Debug;
                    break;
                case LogType.ERROR:
                    logEvent.Level = LogLevel.Error;
                    logEvent.Exception = ex;
                    break;
                case LogType.FATAL:
                    logEvent.Level = LogLevel.Fatal;
                    logEvent.Exception = ex;
                    break;
                default:
                    break;
            }
            logEvent.Properties["type"] = typeName;
            logEvent.Properties["method"] = methodName;
            logger.Log(logEvent);
        }
    }
}
