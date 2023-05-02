using System.Threading.Tasks;

namespace BiliLite.Extensions
{
    public static class TaskExtensions
    {
        public static void RunWithoutAwait(this Task task)
        {
            Task.Run(() => task);
        }
    }
}
