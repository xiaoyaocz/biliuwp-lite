using System;
using System.IO;
using Windows.Storage;

namespace BiliLite.Services
{
    public static class SqlHelper
    {
        /// <summary>
        /// 数据库文件所在路径
        /// </summary>
        private readonly static string DbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "bili.db");

        public async static void InitDB()
        {
            await ApplicationData.Current.LocalFolder.CreateFileAsync("bili.db", CreationCollisionOption.OpenIfExists);
        }
    }
}
