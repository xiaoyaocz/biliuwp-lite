using System.Collections.ObjectModel;

namespace BiliLite.Models.Common.Anime
{
    public class AnimeFallModel
    {
        public int Wid { get; set; }
        
        public string Title { get; set; }

        public ObservableCollection<AnimeFallItemModel> Items { get; set; }
    }
}