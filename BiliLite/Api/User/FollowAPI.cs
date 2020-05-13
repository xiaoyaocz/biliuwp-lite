using BiliLite.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.Api.User
{
    public class FollowAPI
    {
        /// <summary>
        /// 我的追番
        /// </summary>
        /// <param name="page">页数</param>
        /// <param name="status">0=全部，1=想看，2=在看，3=看过</param>
        /// <param name="pagesize">每页数量</param>
        /// <returns></returns>
        public ApiModel MyFollowBangumi(int page=1, int status = 0,int pagesize=20)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = "https://api.bilibili.com/pgc/app/follow/v2/bangumi",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&pn={page}&ps={pagesize}&status={status}",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }
        /// <summary>
        /// 我的追剧
        /// </summary>
        /// <param name="page">页数</param>
        /// <param name="status">0=全部，1=想看，2=在看，3=看过</param>
        /// <param name="pagesize">每页数量</param>
        /// <returns></returns>
        public ApiModel MyFollowCinema(int page = 1, int status = 0, int pagesize = 20)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = "https://api.bilibili.com/pgc/app/follow/v2/cinema",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&pn={page}&ps={pagesize}&status={status}",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }
        /// <summary>
        /// 收藏番剧
        /// </summary>
        /// <returns></returns>
        public ApiModel FollowSeason(string season_id)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.POST,
                baseUrl = $"https://api.bilibili.com/pgc/app/follow/add",
                body = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&season_id={season_id}"
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }
        /// <summary>
        /// 取消收藏番剧
        /// </summary>
        /// <returns></returns>
        public ApiModel CancelFollowSeason(string season_id)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.POST,
                baseUrl = $"https://api.bilibili.com/pgc/app/follow/del",
                body = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&season_id={season_id}"
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 设置状态
        /// </summary>
        /// <returns></returns>
        public ApiModel SetSeasonStatus(string season_id,int status)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.POST,
                baseUrl = "https://api.bilibili.com/pgc/app/follow/status/update",
                body = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&season_id={season_id}&status={status}"
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 我的收藏夹/收藏的收藏夹
        /// </summary>
        /// <returns></returns>
        public ApiModel MyFavorite()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = "https://api.bilibili.com/medialist/gateway/base/space",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&up_mid={SettingHelper.Account.Profile.mid}"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 我创建的收藏夹
        /// </summary>
        /// <returns></returns>
        public ApiModel MyCreatedFavorite(string aid)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = "https://api.bilibili.com/medialist/gateway/base/created",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&rid={aid}&up_mid={SettingHelper.Account.Profile.mid}&type=2&pn=1&ps=100"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 添加到收藏夹
        /// </summary>
        /// <returns></returns>
        public ApiModel AddFavorite(List<string> fav_ids,string avid)
        {
            var ids = "";
            foreach (var item in fav_ids)
            {
                ids += item + ",";
            }
            ids = Uri.EscapeDataString(ids.TrimEnd(','));
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.POST,
                baseUrl = "https://api.bilibili.com/medialist/gateway/coll/resource/deal",
                body = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&add_media_ids={ids}&rid={avid}&type=2"
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }


        /// <summary>
        /// 创建收藏夹
        /// </summary>
        /// <returns></returns>
        public ApiModel CreateFavorite(string title, bool privacy)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.POST,
                baseUrl = "https://api.bilibili.com/medialist/gateway/base/add",
                body = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"privacy={(privacy?1:0)}&title={Uri.EscapeDataString(title)}"
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }


        /// <summary>
        /// 收藏夹信息，不含视频
        /// </summary>
        /// <returns></returns>
        public ApiModel FavoriteInfo(string fid,string keyword,int page=1)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = "https://api.bilibili.com/medialist/gateway/base/detail",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&media_id={fid}&mid={SettingHelper.Account.Profile.mid}&keyword={Uri.EscapeDataString(keyword)}&pn={page}&ps=20"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }


       

    }
}
