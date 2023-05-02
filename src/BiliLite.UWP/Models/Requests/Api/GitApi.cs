using BiliLite.Extensions;
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
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.GIT_RAW_URL}/document/new_version.json",
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
