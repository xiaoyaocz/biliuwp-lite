using BiliLite.Modules;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Dynamic
{
    public class DynamicItemModel
    {
        /// <summary>
        /// json字符串
        /// </summary>
        [JsonProperty("extend_json")]
        public string ExtendJson { get; set; }

        /// <summary>
        /// json字符串,根据desc里的type，获得数据
        /// </summary>
        public string Card { get; set; }

        public DynamicDescModel Desc { get; set; }

        public DynamicVideoCardModel Video => Desc is { Type: 8 } ? Newtonsoft.Json.JsonConvert.DeserializeObject<DynamicVideoCardModel>(Card) : null;

        public DynamicSeasonCardModel Season => Desc is { Type: 512 } ? Newtonsoft.Json.JsonConvert.DeserializeObject<DynamicSeasonCardModel>(Card) : null;
    }
}