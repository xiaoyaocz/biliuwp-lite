using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.Api
{
    public enum EmoteBusiness
    {
        /// <summary>
        /// 评论区
        /// </summary>
        reply,
        /// <summary>
        /// 动态
        /// </summary>
        dynamic
    }

    public class EmoteApi
    {
        
        public ApiModel UserEmote(EmoteBusiness business)
        {
            var type = business.ToString();
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/emote/user/panel/web",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&business={type}"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }
    }
}
