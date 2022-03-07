using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.Api.Live
{
    public class LiveRecommendAPI
    {
        /// <summary>
        /// 直播列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="sort_type">最新开播：live_time，热门：online，互动直播：sort_type_169</param>
        /// <returns></returns>
        public ApiModel LiveRoomList(int page=1,string sort_type= "online")
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://api.live.bilibili.com/room/v3/Area/getRoomList",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&actionKey=appkey&area_id=0&cate_id=0&parent_area_id=0&page={page}&page_size=36&sort_type={sort_type}"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }
    }
}
