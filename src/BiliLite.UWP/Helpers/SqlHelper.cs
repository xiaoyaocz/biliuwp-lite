
//using SQLite;
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
        private readonly static string DbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "bili.db");

        //private static SQLiteConnection _connection;

        //public static SQLiteConnection Connection
        //{
        //    get
        //    {
        //        if (_connection == null)
        //        {
        //            _connection = new SQLiteConnection(DbPath);
        //        }
        //        return _connection;
        //    }
        //    set { _connection = value; }
        //}


        public async static void InitDB()
        {
            await ApplicationData.Current.LocalFolder.CreateFileAsync("bili.db", CreationCollisionOption.OpenIfExists);

            //Connection.CreateTable<LocalHistory>();

        }
        //public static List<LocalHistory> GetHistory(int page = 1)
        //{

        //    var list = Connection.Table<LocalHistory>()
        //         .OrderByDescending(x => x.WatchTime)
        //         .Skip((page - 1) * 30)
        //         .Take(30).ToList();
        //    return list;

        //}

        //public static int UpdateOrInsertHistory(LocalHistory localHistory)
        //{

        //    var value = GetHistoryItem(localHistory.VideoID);
        //    if (value == null)
        //    {
        //        return Connection.Insert(localHistory);
        //    }
        //    else
        //    {
        //        return Connection.Update(localHistory);
        //    }

        //}

        //public static LocalHistory GetHistoryItem(string videoId)
        //{
        //    var value = Connection.Table<LocalHistory>().FirstOrDefault(x => x.VideoID == videoId);
        //    return value;
        //}
    }
    //public class LocalHistory
    //{
    //    [PrimaryKey]
    //    public string VideoID { get; set; }
    //    public string Title { get; set; }
    //    public string Cover { get; set; }
    //    public string EpisodeID { get; set; }
    //    public string EpisodeTitle { get; set; }
    //    public double Position { get; set; }
    //    public DateTime WatchTime { get; set; }
    //}
}
