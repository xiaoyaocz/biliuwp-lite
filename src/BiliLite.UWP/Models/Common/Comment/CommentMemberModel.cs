using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using BiliLite.Controls;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Comment
{
    public class CommentMemberModel
    {
        public string Mid { get; set; }
        public string Uname { get; set; }
        public string Sex { get; set; }
        public string Sign { get; set; }

        //public string avatar { get; set; }
        private string m_avatar;
        public string Avatar { get { return m_avatar; } set { m_avatar = value + "@64w_64h.jpg"; } }

        [JsonProperty("level_info")]
        public CommentMemberModel LevelInfo { get; set; }

        [JsonProperty("fans_detail")]
        public CommentMemberFansDetailModel FansDetail { get; set; }

        [JsonProperty("user_sailing")]
        public CommentMemberUserSailingModel UserSailing { get; set; }
        public bool ShowFansDetail => FansDetail != null;

        public bool ShowCardBg => UserSailing != null && UserSailing.Cardbg != null && UserSailing.Cardbg.Fan != null;

        [JsonProperty("current_level")]
        public int CurrentLevel { get; set; }
        public string LV
        {
            get
            {
                switch (LevelInfo.CurrentLevel)
                {
                    case 0:
                        return "ms-appx:///Assets/Icon/lv0.png";
                    case 1:
                        return "ms-appx:///Assets/Icon/lv1.png";
                    case 2:
                        return "ms-appx:///Assets/Icon/lv2.png";
                    case 3:
                        return "ms-appx:///Assets/Icon/lv3.png";
                    case 4:
                        return "ms-appx:///Assets/Icon/lv4.png";
                    case 5:
                        return "ms-appx:///Assets/Icon/lv5.png";
                    case 6:
                        return "ms-appx:///Assets/Icon/lv6.png";
                    default:
                        return Constants.App.TRANSPARENT_IMAGE;
                }
            }
        }
        
        public string PendantStr
        {
            get
            {
                if (Pendant != null)
                {
                    if (Pendant.Image == "")
                    {
                        return Constants.App.TRANSPARENT_IMAGE;
                    }
                    return Pendant.Image;
                }
                else
                {
                    return Constants.App.TRANSPARENT_IMAGE;
                }
            }
        }

        public CommentMemberModel Pendant { get; set; }

        public int Pid { get; set; }

        public string Name { get; set; }

        public string Image { get; set; }

        [JsonProperty("official_verify")]
        public CommentMemberModel OfficialVerify { get; set; }

        public int Type { get; set; }

        public string Desc { get; set; }

        public string Verify
        {
            get
            {
                if (OfficialVerify == null)
                {
                    return "";
                }
                switch (OfficialVerify.Type)
                {
                    case 0:
                        return Constants.App.VERIFY_PERSONAL_IMAGE;
                    case 1:
                        return Constants.App.VERIFY_OGANIZATION_IMAGE;
                    default:
                        return Constants.App.TRANSPARENT_IMAGE;
                }
            }
        }

        public CommentMemberModel Vip { get; set; }

        public int VipType { get; set; }

        public SolidColorBrush VipCo
        {
            get
            {
                if (Vip.VipType == 2)
                {
                    return new SolidColorBrush(Colors.DeepPink);
                }
                else
                {
                    return new SolidColorBrush((Color)Application.Current.Resources["TextColor"]);
                }
            }
        }
    }
}