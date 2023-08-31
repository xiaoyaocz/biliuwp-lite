using System.Linq;

namespace BiliLite.Models.Common
{
    public static class DefaultVideoTypeOptions
    {
        public static DefaultVideoTypeOption[] Options = new[]
        {
            new DefaultVideoTypeOption(){Name = "AVC/H.264",Value = PlayUrlCodecMode.DASH_H264},
            new DefaultVideoTypeOption(){Name = "HEVC/H.265",Value = PlayUrlCodecMode.DASH_H265},
            new DefaultVideoTypeOption(){Name = "AV1",Value = PlayUrlCodecMode.DASH_AV1},
        };

        public const PlayUrlCodecMode DEFAULT_VIDEO_TYPE = PlayUrlCodecMode.DASH_H264;

        public static DefaultVideoTypeOption GetOption(PlayUrlCodecMode type)
        {
            return Options.FirstOrDefault(x => x.Value == type);
        }
    }

    public class DefaultVideoTypeOption
    {
        public string Name { get; set; }

        public PlayUrlCodecMode Value { get; set; }
    }
}
