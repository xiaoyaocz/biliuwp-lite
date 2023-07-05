using BiliLite.Models.Common.Video.PlayUrlInfos;
using BiliLite.ViewModels.Common;
using PropertyChanged;
using System.Collections.Generic;

namespace BiliLite.ViewModels.Download
{
    public class DownloadDialogViewModel : BaseViewModel
    {
        public int VideoTypeSelectedIndex { get; set; }

        public BiliPlayUrlInfo SelectedQuality { get; set; }

        public int SelectedQualityIndex { get; set; }

        public List<BiliPlayUrlInfo> Qualities { get; set; }

        public List<BiliDashAudioPlayUrlInfo> AudioQualities { get; set; }

        public BiliDashAudioPlayUrlInfo SelectedAudioQuality { get; set; }

        [DependsOn(nameof(VideoTypeSelectedIndex))]
        public bool ShowAudioQualityComboBox { get => VideoTypeSelectedIndex > 0; }
    }
}
