using BiliLite.Models.Common.Video.PlayUrlInfos;
using BiliLite.ViewModels.Common;
using PropertyChanged;
using System.Collections.Generic;
using BiliLite.Models.Common;

namespace BiliLite.ViewModels.Download
{
    public class DownloadDialogViewModel : BaseViewModel
    {
        public DefaultVideoTypeOption SelectedVideoType { get; set; }

        public BiliPlayUrlInfo SelectedQuality { get; set; }

        public int SelectedQualityIndex { get; set; }

        public List<BiliPlayUrlInfo> Qualities { get; set; }

        public List<BiliDashAudioPlayUrlInfo> AudioQualities { get; set; }

        public BiliDashAudioPlayUrlInfo SelectedAudioQuality { get; set; }

        [DependsOn(nameof(SelectedVideoType))]
        public bool ShowAudioQualityComboBox { get => SelectedVideoType.Value > 0; }
    }
}
