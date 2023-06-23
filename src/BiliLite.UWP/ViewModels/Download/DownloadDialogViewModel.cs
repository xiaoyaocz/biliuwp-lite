using BiliLite.Models.Common.Video.PlayUrlInfos;
using BiliLite.ViewModels.Common;
using PropertyChanged;

namespace BiliLite.ViewModels.Download
{
    public class DownloadDialogViewModel:BaseViewModel
    {
        public int VideoTypeSelectedIndex { get; set; }

        public BiliPlayUrlInfo SelectedQuality { get; set; }

        public BiliDashAudioPlayUrlInfo SelectedAudioQuality { get; set; }

        [DependsOn(nameof(VideoTypeSelectedIndex))]
        public bool ShowAudioQualityComboBox { get => VideoTypeSelectedIndex > 0; }
    }
}
