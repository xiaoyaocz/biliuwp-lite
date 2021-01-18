using BiliLite.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.Api
{
    public class AccountApi
    {   
        /// <summary>
        /// 读取登录密码加密信息
        /// </summary>
        /// <returns></returns>
        public ApiModel GetKey()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.POST,
                baseUrl = "https://passport.bilibili.com/api/oauth2/getKey",
                body = ApiHelper.MustParameter(ApiHelper.AndroidKey)
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 登录API V2
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="captcha">验证码</param>
        /// <returns></returns>
        public  ApiModel LoginV2(string username, string password, string captcha = "")
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.POST,
                baseUrl = "https://passport.bilibili.com/api/oauth2/login",
                body = $"username={Uri.EscapeDataString(username)}&password={Uri.EscapeDataString(password)}{((captcha == "") ? "" : "&captcha=" + captcha)}&" + ApiHelper.MustParameter(ApiHelper.AndroidKey)
            };
            api.body += ApiHelper.GetSign(api.body,ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 登录API V3
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="gee_type"></param>
        /// <returns></returns>
        public  ApiModel LoginV3(string username, string password, int gee_type = 10)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.POST,
                baseUrl = "https://passport.bilibili.com/api/v3/oauth2/login",
                body = $"username={Uri.EscapeDataString(username)}&password={Uri.EscapeDataString(password)}&gee_type={gee_type}&" + ApiHelper.MustParameter(ApiHelper.AndroidTVKey)
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidTVKey);
            return api;
        }

        /// <summary>
        /// SSO
        /// </summary>
        /// <param name="access_key"></param>
        /// <returns></returns>
        public  ApiModel SSO(string access_key)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = $"https://passport.bilibili.com/api/login/sso",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey,false) + $"&gourl=https%3A%2F%2Faccount.bilibili.com%2Faccount%2Fhome&access_key={access_key}",
                headers = ApiHelper.GetDefaultHeaders()
            };
            api.parameter += ApiHelper.GetSign(api.parameter,ApiHelper.AndroidKey);
            return api;
        }
        
        /// <summary>
        /// 读取验证码
        /// </summary>
        /// <returns></returns>
        public  ApiModel Captcha()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = "https://passport.bilibili.com/captcha",
                headers = ApiHelper.GetDefaultHeaders(),
                parameter = $"ts={Utils.GetTimestampS()}"
            };
            return api;

        }
        /// <summary>
        /// 个人资料
        /// </summary>
        /// <returns></returns>
        public ApiModel UserProfile()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = "https://app.bilibili.com/x/v2/account/myinfo",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey,true)
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }


        /// <summary>
        /// 个人资料
        /// </summary>
        /// <returns></returns>
        public ApiModel MineProfile()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = "https://app.bilibili.com/x/v2/account/mine",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true)
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 个人空间
        /// </summary>
        /// <returns></returns>
        public ApiModel Space(string mid)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = "http://app.biliapi.net/x/v2/space",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true)+ $"&vmid={mid}"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        public ApiModel History(int pn=1,int ps=24)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = "https://api.bilibili.com/x/v2/history",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&pn={pn}&ps={ps}"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }
        public ApiModel DelHistory(string id)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.POST,
                baseUrl = "https://api.bilibili.com/x/v2/history/delete",
                body = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&kid={id}"
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 二维码登录获取二维码及AuthCode
        /// </summary>
        /// <param name="mid"></param>
        /// <returns></returns>
        public ApiModel QRLoginAuthCode(string local_id)
        {
            ApiModel api = new ApiModel()
            {
                method =  RestSharp.Method.POST,
                baseUrl = "https://passport.bilibili.com/x/passport-tv-login/qrcode/auth_code",
                body = ApiHelper.MustParameter(ApiHelper.AndroidTVKey, false) + $"&local_id={local_id}",
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidTVKey);
            return api;
        }

        /// <summary>
        /// 二维码登录轮询
        /// </summary>
        /// <param name="auth_code"></param>
        /// <returns></returns>
        public ApiModel QRLoginPoll(string auth_code, string local_id)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.POST,
                baseUrl = "https://passport.bilibili.com/x/passport-tv-login/qrcode/poll",
                body = ApiHelper.MustParameter(ApiHelper.AndroidTVKey, false) + $"&auth_code={auth_code}&guid={Guid.NewGuid().ToString()}&local_id={local_id}",
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidTVKey);
            return api;
        }


        /// <summary>
        /// 读取oauth2信息
        /// </summary>
        /// <returns></returns>
        public ApiModel GetOAuth2Info()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.GET,
                baseUrl = "https://passport.bilibili.com/api/oauth2/info",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey)+ "&access_token="+SettingHelper.Account.AccessKey
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 刷新Token
        /// </summary>
        /// <returns></returns>
        public ApiModel RefreshToken()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.POST,
                baseUrl = "https://passport.bilibili.com/api/oauth2/refreshToken",
                body = ApiHelper.MustParameter(ApiHelper.AndroidKey)+ $"&access_token={SettingHelper.Account.AccessKey}&refresh_token={SettingHelper.GetValue<string>(SettingHelper.Account.REFRESH_KEY, "")}"
            };
            api.body += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }
    }
}
