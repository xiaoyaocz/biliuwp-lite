using BiliLite.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.Api.User
{
    public class DynamicAPI
    {
        public enum UserDynamicType
        {
            /// <summary>
            /// 全部
            /// </summary>
            All=0,
            /// <summary>
            /// 追番、追剧
            /// </summary>
            Season=2,
            /// <summary>
            /// 视频
            /// </summary>
            Video=1,
            /// <summary>
            /// 专栏
            /// </summary>
            Article=3
        }
        /// <summary>
        /// 读取动态列表
        /// </summary>
        /// <param name="type">268435455=全部,512,4097,4098,4099,4100,4101=番剧，8=视频，64=专栏</param>
        /// <returns></returns>
        public ApiModel DyanmicNew(UserDynamicType type)
        {
            var typeList = "";
            switch (type)
            {
               
                case UserDynamicType.Season:
                    typeList = "512,4097,4098,4099,4100,4101";
                    break;
                case UserDynamicType.Video:
                    typeList = "8";
                    break;
                case UserDynamicType.Article:
                    typeList = "64";
                    break;
                default:
                    typeList = "268435455";
                    //typeList = "268435455";
                    break;
            }
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = "https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/dynamic_new",
                parameter = $"type_list={Uri.EscapeDataString(typeList)}&uid={SettingHelper.Account.UserID}",
            };
            //ApiModel api = new ApiModel()
            //{
            //    method = RestSharp.Method.Get,
            //    baseUrl = "https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/space_history",
            //    parameter = $"host_uid={SettingHelper.Account.UserID}&visitor_uid={SettingHelper.Account.UserID}",
            //};
            //使用Web的API
            if (SettingHelper.Account.Logined)
            {
                api.parameter +=  $"&access_key={SettingHelper.Account.AccessKey}";
            }
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        public ApiModel DynamicDetail(string id)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = "https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/get_dynamic_detail",
                parameter = $"dynamic_id={id}",
            };
            //使用Web的API
            if (SettingHelper.Account.Logined)
            {
                api.parameter += $"&access_key={SettingHelper.Account.AccessKey}";
            }
       
            return api;
        }
        public ApiModel DynamicRepost(string id,string offset="")
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = "https://api.vc.bilibili.com/dynamic_repost/v1/dynamic_repost/repost_detail",
                parameter = $"dynamic_id={id}&offset={offset}",
            };
            //使用Web的API
            if (SettingHelper.Account.Logined)
            {
                api.parameter += $"&access_key={SettingHelper.Account.AccessKey}";
            }

            return api;
        }
        public ApiModel HistoryDynamic(string dynamic_id, UserDynamicType type)
        {
            var typeList = "";
            switch (type)
            {

                case UserDynamicType.Season:
                    typeList = "512,4097,4098,4099,4100,4101";
                    break;
                case UserDynamicType.Video:
                    typeList = "8";
                    break;
                case UserDynamicType.Article:
                    typeList = "64";
                    break;
                default:
                    typeList = "268435455";
                    //typeList = "268435455";
                    break;
            }
            
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/dynamic_history",
                parameter = $"offset_dynamic_id={dynamic_id}&type_list={Uri.EscapeDataString(typeList)}&uid={SettingHelper.Account.UserID}"
            };//使用Web的API
            if (SettingHelper.Account.Logined)
            {
                api.parameter += $"&access_key={SettingHelper.Account.AccessKey}";
            }
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        public ApiModel SpaceHistory(string mid,string dynamic_id="")
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/space_history",
                parameter = $"offset_dynamic_id={dynamic_id}&visitor_uid={SettingHelper.Account.UserID}&host_uid={mid}&need_top=1"
            };
            if (SettingHelper.Account.Logined)
            {
                api.parameter += $"&access_key={SettingHelper.Account.AccessKey}";
            }
           
            return api;
        }
        /// <summary>
        /// 推荐话题
        /// </summary>
        /// <returns></returns>
        public ApiModel RecommendTopic()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = "https://api.vc.bilibili.com/topic_svr/v1/topic_svr/get_rcmd_topic",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) ,
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }


        /// <summary>
        /// 发表图片动态
        /// </summary>
        /// <param name="mid">用户ID</param>
        /// <param name="mode">1为关注，2为取消关注</param>
        /// <returns></returns>
        public ApiModel CreateDynamicPhoto(string imgs, string content, string at_uids, string at_control)
        {
            

            ApiModel api = new ApiModel()
            {
                method =  RestSharp.Method.Post,
                baseUrl = $"https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/create_draw",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true),
                body = $"uid={ SettingHelper.Account.UserID}&category=3&pictures={Uri.EscapeDataString(imgs)}&description={Uri.EscapeDataString(content)}&content={Uri.EscapeDataString(content)}&setting=%7B%22copy_forbidden%22%3A0%7D&at_uids={Uri.EscapeDataString(at_uids)}&at_control={Uri.EscapeDataString(at_control)}&extension=%7B%22emoji_type%22%3A1%7D"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 发表文本动态
        /// </summary>
        /// <param name="mid">用户ID</param>
        /// <param name="mode">1为关注，2为取消关注</param>
        /// <returns></returns>
        public ApiModel CreateDynamicText(string content, string at_uids, string at_control)
        {
            ApiModel api = new ApiModel()
            {
                method =  RestSharp.Method.Post,
                baseUrl = $"https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/create",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true),
                body = $"uid={SettingHelper.Account.UserID}&dynamic_id=0&type=4&content={Uri.EscapeDataString(content)}&setting=%7B%22copy_forbidden%22%3A0%7D&at_uids={Uri.EscapeDataString(at_uids)}&at_control={Uri.EscapeDataString(at_control)}&jumpfrom=110&extension=%7B%22emoji_type%22%3A1%7D"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 转发动态
        /// </summary>
        /// <param name="mid">用户ID</param>
        /// <param name="mode">1为关注，2为取消关注</param>
        /// <returns></returns>
        public ApiModel RepostDynamic(string dynamic_id,string content, string at_uids, string at_control)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"https://api.vc.bilibili.com/dynamic_repost/v1/dynamic_repost/repost",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true),
                body = $"uid={SettingHelper.Account.UserID}&dynamic_id={dynamic_id}&content={Uri.EscapeDataString(content)}&at_uids={Uri.EscapeDataString(at_uids)}&at_control={Uri.EscapeDataString(at_control)}&extension=%7B%22emoji_type%22%3A1%7D"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 点赞动态
        /// </summary>
        /// <param name="dynamic_id">动态ID</param>
        /// <param name="up">点赞=1，取消点赞=2</param>
        /// <returns></returns>
        public ApiModel Like(string dynamic_id,int up)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"https://api.vc.bilibili.com/dynamic_like/v1/dynamic_like/thumb",
                body = ApiHelper.MustParameter(ApiHelper.AndroidKey, true)+$"&dynamic_id={dynamic_id}&uid={SettingHelper.Account.UserID}&up={up}"
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }
        /// <summary>
        /// 删除动态
        /// </summary>
        /// <param name="dynamic_id">动态ID</param>
        /// <returns></returns>
        public ApiModel Delete(string dynamic_id)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/rm_dynamic",
                body = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&dynamic_id={dynamic_id}"
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="dynamic_id">动态ID</param>
        /// <returns></returns>
        public ApiModel UploadImage()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://api.vc.bilibili.com/api/v1/drawImage/upload",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) 
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }
    }
}
