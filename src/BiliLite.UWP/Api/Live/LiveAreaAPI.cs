using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.Api.Live
{
    public class LiveAreaAPI
    {
        public ApiModel LiveAreaList()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://api.live.bilibili.com/room/v1/Area/getList",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&actionKey=appkey&need_entrance=1&parent_id=0"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }
        public ApiModel LiveAreaRoomList(int area_id=0,int parent_area_id=0, int page = 1, string sort_type = "online")
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://api.live.bilibili.com/room/v3/Area/getRoomList",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&actionKey=appkey&area_id={area_id}&cate_id=0&parent_area_id={parent_area_id}&page={page}&page_size=36&sort_type={sort_type}"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }
    }
}
