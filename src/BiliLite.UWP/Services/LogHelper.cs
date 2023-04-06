using BiliLite.Extensions;
using BiliLite.Helpers;
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
    public class LogHelper
    {
        public static LoggingConfiguration config;
        public static Logger logger = LogManager.GetCurrentClassLogger();

        private static bool IsAutoClearLogFile => SettingHelper.GetValue<bool>(SettingHelper.Other.AUTO_CLEAR_LOG_FILE, true);
        private static int AutoClearLogFileDay => SettingHelper.GetValue<int>(SettingHelper.Other.AUTO_CLEAR_LOG_FILE_DAY, 7);
        private static bool IsProtectLogInfo => SettingHelper.GetValue<bool>(SettingHelper.Other.PROTECT_LOG_INFO, true);

        public static void Init()
        {
            config = new LoggingConfiguration();
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
            LogManager.Configuration = config;
            if (IsAutoClearLogFile)
            {
                // todo: add await
                DeleteFile(storageFolder.Path + @"\log\");
            }
        }

        public static async Task DeleteFile(string path)
        {
            string pattern = "yyyyMMdd";
            int days = AutoClearLogFileDay;
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

        public static void Log(string message, LogType type, Exception ex = null, [CallerMemberName] string methodName = null)
        {
            Debug.WriteLine("[" + LogType.INFO.ToString() + "]" + message);
            message = $"[{methodName}]{message}";
            if(IsProtectLogInfo)
                message = message.ProtectValues("access_key", "csrf", "access_token");
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
    }
}
