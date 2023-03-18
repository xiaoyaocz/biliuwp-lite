using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BiliLite.Extensions
{
    public static class ObservableCollectionExtension
    {
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> range)
        {
            if (range == null) return;
            foreach (var item in range)
            {
                collection.Add(item);
            }
        }

        public static void ReplaceRange<T>(this ObservableCollection<T> collection, IEnumerable<T> range)
        {
            if (range == null) return;
            collection.Clear();
            foreach (var item in range)
            {
                collection.Add(item);
            }
        }
    }
}
