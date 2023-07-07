using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BiliLite.ViewModels.Common;
using PropertyChanged;

namespace BiliLite.ViewModels.Comment
{
    public class CommentControlViewModel : BaseViewModel
    {
        public ObservableCollection<CommentViewModel> Comments { get; set; } =
            new ObservableCollection<CommentViewModel>();

        public bool Loading { get; set; }

        [DependsOn(nameof(Loading))] public bool PrLoadVisibility => Loading;

        public bool NoRepostVisibility { get; set; } = false;

        public bool CloseRepostVisibility { get; set; } = false;

        public bool BtnLoadMoreVisibility { get; set; } = true;

        public bool HotCommentsVisibility { get; set; } = true;

        public bool NewCommentVisibility { get; set; } = true;

        public bool IsCommentDialog { private get; set; } = true;

        [DependsOn(nameof(IsCommentDialog))]
        public Thickness BtnRefreshMargin => IsCommentDialog ? new Thickness(0, 0, 40, 0) : new Thickness(0);

        public double Width { get; set; } = 320;

        [DependsOn(nameof(Width))] public bool IsNarrowMode => Width < 300;

        [DependsOn(nameof(Width))] public bool IsWideMode => Width >= 300;

        [DependsOn(nameof(Width))] public bool IsNarrow2Mode => Width < 200;

        [DependsOn(nameof(IsNarrow2Mode))]
        public Orientation CommentActionOrientation => IsNarrow2Mode ? Orientation.Vertical : Orientation.Horizontal;

        [DependsOn(nameof(IsWideMode))]
        public Thickness ListViewCommentsPadding => IsWideMode ? new Thickness(0, 0, 12, 0) : new Thickness(0);
    }
}
