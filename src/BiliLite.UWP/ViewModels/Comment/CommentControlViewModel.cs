using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using BiliLite.ViewModels.Common;
using PropertyChanged;

namespace BiliLite.ViewModels.Comment
{
    public class CommentControlViewModel : BaseViewModel
    {
        public ObservableCollection<CommentViewModel> Comments { get; set; } = new ObservableCollection<CommentViewModel>();

        public bool Loading { get; set; }

        [DependsOn(nameof(Loading))]
        public bool PrLoadVisibility => Loading;

        public bool NoRepostVisibility { get; set; } = false;

        public bool CloseRepostVisibility { get; set; } = false;

        public bool BtnLoadMoreVisibility { get; set; } = true;

        public bool HotCommentsVisibility { get; set; } = true;

        public bool NewCommentVisibility { get; set; } = true;

        public bool IsCommentDialog { private get; set; } = true;

        [DependsOn(nameof(IsCommentDialog))]
        public Thickness BtnRefreshMargin => IsCommentDialog ? new Thickness(0, 0, 40, 0) : new Thickness(0);
    }
}
