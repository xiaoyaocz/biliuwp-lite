using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Services;

namespace BiliLite.Models.Requests.Api
{
    public class GitApi
    {
        /// <summary>
        /// 检查更新
        /// </summary>
        /// <returns></returns>
        public ApiModel CheckUpdate()
        {
            var m_updateJsonAddress = SettingService.GetValue(SettingConstants.Other.DEFAULT_UPDATE_JSON_ADDRESS, 
                                                              DefaultUpdateJsonAddressOptions.DEFAULT_UPDATE_JSON_ADDRESS);
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{m_updateJsonAddress}/document/new_version.json",
                parameter = $"ts={TimeExtensions.GetTimestampS()}"
            };
            return api;
        }

        /// <summary>
        /// 发现页入口
        /// </summary>
        /// <returns></returns>
        public ApiModel FindMoreEntrance()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.GIT_RAW_URL}/document/entrance.json",
                parameter = $"ts={TimeExtensions.GetTimestampS()}"
            };
            return api;
        }
    }
}
