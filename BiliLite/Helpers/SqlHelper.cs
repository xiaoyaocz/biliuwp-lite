using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace BiliLite.Helpers
{
    public static class SqlHelper
    {
        /// <summary>
        /// 数据库文件所在路径
        /// </summary>
        public readonly static string DbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "bili.db");



    }
}
