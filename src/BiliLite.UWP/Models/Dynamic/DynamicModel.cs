using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.Models.Dynamic
{
    public class DynamicModel
    {
        public string history_offset { get; set; }
        public string max_dynamic_id { get; set; }
        public List<DynamicCardModel> cards { get; set; }
    }
    public class DynamicCardModel
    {
        public DynamicCardDescModel desc { get; set; }
       // public string card { get; set; }
        public string extend_json { get; set; }
        public string card { get; set; }
        public DynamicCardDisplayModel display { get; set; }
    }
    public class DynamicCardDescModel
    {
        public DynamicCardDescUserProfileModel user_profile { get; set; }
        public long uid { get; set; }
        public int type { get; set; }
        public string rid { get; set; }
        public string dynamic_id_str { get; set; }
        public string dynamic_id { get; set; }
        public string rid_str { get; set; }
        public int r_type { get; set; }
        public string bvid { get; set; }
        public int view { get; set; }
        public int repost { get; set; }
        public int comment { get; set; }
        public int like { get; set; }
        public int is_liked { get; set; }
        public long timestamp { get; set; }
        public string pre_dy_id { get; set; }
        public string orig_dy_id { get; set; }
        public int orig_type { get; set; }
        public int uid_type { get; set; }
        public int status { get; set; }
    }

    public class DynamicCardDisplayModel
    {
        public DynamicCardDisplayModel origin { get; set; }
        public DynamicCardDisplayEmojiInfoModel emoji_info { get; set; }
        public DynamicCardDisplayEmojiInfoModel topic_info { get; set; }
    }

    public class DynamicCardDisplayTopicInfoModel
    {
        public List<DynamicCardDisplayTopicInfoItemModel> topic_details{ get; set; }
    }
    public class DynamicCardDisplayTopicInfoItemModel
    {
        public int topic_id { get; set; }
        public string topic_name { get; set; }
        public int is_activity { get; set; }
        public string topic_link { get; set; }
    }

    public class DynamicCardDisplayEmojiInfoModel
    {
        public List<DynamicCardDisplayEmojiInfoItemModel> emoji_details { get; set; }
    }
    public class DynamicCardDisplayEmojiInfoItemModel
    {
        public int id { get; set; }
        public int package_id { get; set; }
        public string emoji_name { get; set; }
        public string url { get; set; }
        public string text { get; set; }
        public DynamicCardDisplayEmojiInfoEmoteMetaModel meta { get; set; }
    }
    public class DynamicCardDisplayEmojiInfoEmoteMetaModel
    {
        //1=16px,2=16x2 px
        public int size { get; set; }
    }
}
