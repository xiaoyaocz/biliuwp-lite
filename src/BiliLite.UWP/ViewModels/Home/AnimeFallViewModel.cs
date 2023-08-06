using System.Collections.ObjectModel;
using BiliLite.Models.Common.Anime;
using BiliLite.ViewModels.Common;
using PropertyChanged;

namespace BiliLite.ViewModels.Home
{
    public class AnimeFallViewModel : BaseViewModel
    {
        [DoNotNotify]
        public int Wid { get; set; }

        [DoNotNotify]
        public string Title { get; set; }

        public bool ShowMore { get; set; } = true;

        [DoNotNotify]
        public ObservableCollection<AnimeFallItemModel> Items { get; set; }
    }
}