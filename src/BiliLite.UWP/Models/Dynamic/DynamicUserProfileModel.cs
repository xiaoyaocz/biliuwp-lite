using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.Models.Dynamic
{
   
    public class DynamicCardDescUserProfileInfoModel
    {
        public long uid { get; set; }
        public string uname { get; set; }
        public string face { get; set; }
    }

    public class DynamicCardDescUserProfileOfficialVerifyModel
    {
        public int type { get; set; }
        public string desc { get; set; }
    }

    public class DynamicCardDescUserProfileCardModel
    {
        public DynamicCardDescUserProfileOfficialVerifyModel official_verify { get; set; }
    }


    public class DynamicCardDescUserProfileVipModel
    {
        public int vipType { get; set; }
        public long vipDueDate { get; set; }
        public string dueRemark { get; set; }
        public int accessStatus { get; set; }
        public int vipStatus { get; set; }
        public string vipStatusWarn { get; set; }
        public int themeType { get; set; }
       
    }

    public class DynamicCardDescUserProfilePendantModel
    {
        public int pid { get; set; }
        public string name { get; set; }
        public string image { get; set; }
        public int expire { get; set; }
        public string image_enhance { get; set; }
    }

    public class DynamicCardDescUserProfileLevelInfoModel
    {
        public int current_level { get; set; }
        public int current_min { get; set; }
        public int current_exp { get; set; }
        public string next_exp { get; set; }
    }
    public class DynamicCardDescUserProfileDecorateCardModel
    {
        public long id { get; set; }
        public string name { get; set; }
        public string big_card_url { get; set; }
        public string jump_url { get; set; }
        public string card_url { get; set; }
        public int card_type { get; set; }
        public DynamicCardDescUserProfileDecorateCardFanModel fan { get; set; }
    }
    public class DynamicCardDescUserProfileDecorateCardFanModel
    {
        public int is_fan { get; set; }
        public int number { get; set; }
        public string color { get; set; }
        public string num_desc { get; set; }
      
    }
    public class DynamicCardDescUserProfileModel
    {
        public DynamicCardDescUserProfileDecorateCardModel decorate_card { get; set; }
        public DynamicCardDescUserProfileInfoModel info { get; set; }
        public DynamicCardDescUserProfileCardModel card { get; set; }
        public DynamicCardDescUserProfileVipModel vip { get; set; }
        public DynamicCardDescUserProfilePendantModel pendant { get; set; }
        public string rank { get; set; }
        public string sign { get; set; }
        public DynamicCardDescUserProfileLevelInfoModel level_info { get; set; }
    }


}
