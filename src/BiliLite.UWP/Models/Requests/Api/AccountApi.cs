using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Services;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace BiliLite.Models.Requests.Api
{
    public class AccountApi
    {
        private readonly CookieService m_cookieService;

        public AccountApi()
        {
            m_cookieService = App.ServiceProvider.GetRequiredService<CookieService>();
        }

        /// <summary>
        /// 读取登录密码加密信息
        /// </summary>
        /// <returns></returns>
        public ApiModel GetKey()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = "https://passport.bilibili.com/x/passport-login/web/key"
            };
            return api;
        }

        /// <summary>
        /// 登录API V2
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="captcha">验证码</param>
        /// <returns></returns>
        public ApiModel LoginV2(string username, string password, string captcha = "")
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = "https://passport.bilibili.com/api/oauth2/login",
                body = $"username={Uri.EscapeDataString(username)}&password={Uri.EscapeDataString(password)}{(captcha == "" ? "" : "&captcha=" + captcha)}&" + ApiHelper.MustParameter(ApiHelper.AndroidKey)
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 登录API V3
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="gee_type"></param>
        /// <returns></returns>
        public ApiModel LoginV3(string username, string password)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = "https://passport.bilibili.com/x/passport-login/oauth2/login",
                body = $"actionKey=appkey&channel=bili&device=phone&permission=ALL&subid=1&username={Uri.EscapeDataString(username)}&password={Uri.EscapeDataString(password)}&" + ApiHelper.MustParameter(ApiHelper.LoginKey)
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.LoginKey);
            return api;
        }

        /// <summary>
        /// 获取登录国家地区
        /// </summary>
        /// <returns></returns>
        public ApiModel Country()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = "https://passport.bilibili.com/x/passport-login/country",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey)
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }
        public ApiModel SendSMS(string cid, string phone, string session_id)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = "https://passport.bilibili.com/x/passport-login/sms/send",
                body = $"actionKey=appkey&cid={cid}&tel={phone}&login_session_id={session_id}&" + ApiHelper.MustParameter(ApiHelper.LoginKey)
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.LoginKey);
            return api;
        }

        public ApiModel SendSMSWithCaptcha(string cid, string phone, string session_id, string seccode = "", string validate = "", string challenge = "", string recaptcha_token = "")
        {
            var buvid = ApiHelper.GetBuvid();
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = "https://passport.bilibili.com/x/passport-login/sms/send",
                body = $"buvid={buvid}&actionKey=appkey&cid={cid}&tel={phone}&login_session_id={session_id}&gee_seccode={seccode}&gee_validate={validate}&gee_challenge={challenge}&recaptcha_token={recaptcha_token}&" + ApiHelper.MustParameter(ApiHelper.LoginKey)
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.LoginKey);
            return api;
        }
        public ApiModel SMSLogin(string cid, string phone, string code, string session_id, string captcha_key)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = "https://passport.bilibili.com/x/passport-login/login/sms",
                body = $"actionKey=appkey&cid={cid}&tel={phone}&login_session_id={session_id}&captcha_key={captcha_key}&code={code}&" + ApiHelper.MustParameter(ApiHelper.AndroidKey)
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }
        /// <summary>
        /// SSO
        /// </summary>
        /// <param name="access_key"></param>
        /// <returns></returns>
        public ApiModel SSO(string access_key)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://passport.bilibili.com/api/login/sso",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, false) + $"&access_key={access_key}",
                headers = ApiHelper.GetDefaultHeaders()
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 读取验证码
        /// </summary>
        /// <returns></returns>
        public ApiModel Captcha()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = "https://passport.bilibili.com/captcha",
                headers = ApiHelper.GetDefaultHeaders(),
                parameter = $"ts={TimeExtensions.GetTimestampS()}"
            };
            return api;
        }

        /// <summary>
        /// 读取极验验证码
        /// </summary>
        /// <returns></returns>
        public ApiModel GeetestCaptcha()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = "https://passport.bilibili.com/x/safecenter/captcha/pre"
            };
            return api;
        }

        /// <summary>
        /// 获取带星号的手机号
        /// </summary>
        /// <returns></returns>
        public ApiModel FetchHideTel(string tmp_token)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = "https://api.bilibili.com/x/safecenter/user/info",
                parameter = $"tmp_code={tmp_token}"
            };
            return api;
        }

        /// <summary>
        /// 发送验证短信
        /// </summary>
        /// <returns></returns>
        public ApiModel SendVerifySMS(string tmp_token, string recaptcha_token, string gee_challenge, string gee_gt, string geetest_validate, string geetest_seccode)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = "https://passport.bilibili.com/x/safecenter/common/sms/send",
                body = $"sms_type=loginTelCheck&tmp_code={tmp_token}&recaptcha_token={recaptcha_token}&gee_challenge={gee_challenge}&gee_gt={gee_gt}&gee_validate={geetest_validate}&gee_seccode={geetest_seccode}"
            };
            return api;
        }

        /// <summary>
        /// 提交短信验证码
        /// </summary>
        /// <returns></returns>
        public ApiModel SubmitPwdLoginSMSCheck(string code, string tmp_token, string request_id, string captcha_key)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = "https://passport.bilibili.com/x/safecenter/login/tel/verify",
                body = $"type=loginTelCheck&code={code}&tmp_code={tmp_token}&request_id={request_id}&captcha_key={captcha_key}"
            };
            return api;
        }

        /// <summary>
        /// 交换获取cookie
        /// </summary>
        /// <returns></returns>
        public ApiModel PwdLoginExchangeCookie(string code)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = "https://passport.bilibili.com/x/passport-login/web/exchange_cookie",
                body = $"code={code}"
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
                method = RestSharp.Method.Get,
                baseUrl = "https://app.bilibili.com/x/v2/account/myinfo",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true)
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
                method = RestSharp.Method.Get,
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
                method = RestSharp.Method.Get,
                baseUrl = "http://app.biliapi.net/x/v2/space",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&vmid={mid}"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        public ApiModel History(int pn = 1, int ps = 24)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v2/history",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&pn={pn}&ps={ps}"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }
        public ApiModel DelHistory(string id)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v2/history/delete",
                body = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + $"&kid={id}"
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// tv版二维码登录获取二维码及AuthCode
        /// </summary>
        /// <param name="mid"></param>
        /// <returns></returns>
        public ApiModel QRLoginAuthCodeTV(string local_id)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = "https://passport.bilibili.com/x/passport-tv-login/qrcode/auth_code",
                body = ApiHelper.MustParameter(ApiHelper.AndroidTVKey, false) + $"&local_id={local_id}",
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidTVKey);
            return api;
        }

        /// <summary>
        /// tv版二维码登录轮询
        /// </summary>
        /// <param name="auth_code"></param>
        /// <returns></returns>
        public ApiModel QRLoginPollTV(string auth_code, string local_id)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = "https://passport.bilibili.com/x/passport-tv-login/qrcode/poll",
                body = ApiHelper.MustParameter(ApiHelper.AndroidTVKey, false) + $"&auth_code={auth_code}&guid={Guid.NewGuid().ToString()}&local_id={local_id}",
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidTVKey);
            return api;
        }

        /// <summary>
        /// web二维码登录获取二维码及AuthCode
        /// </summary>
        /// <returns></returns>
        public ApiModel QRLoginAuthCode()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = "https://passport.bilibili.com/x/passport-login/web/qrcode/generate",
            };
            return api;
        }

        /// <summary>
        /// web版二维码登录轮询
        /// </summary>
        /// <param name="auth_code"></param>
        /// <returns></returns>
        public ApiModel QRLoginPoll(string auth_code)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = "https://passport.bilibili.com/x/passport-login/web/qrcode/poll",
                parameter = $"qrcode_key={auth_code}"
            };
            return api;
        }

        /// <summary>
        /// web版登录获取到的Cookie转app令牌
        /// </summary>
        /// <returns></returns>
        public ApiModel GetCookieToAccessKey()
        {
            var apiBody = "api=http://link.acg.tv/forum.php";
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = "https://passport.bilibili.com/login/app/third",
                parameter = $"appkey={ApiHelper.AndroidKey.Appkey}&{apiBody}",
                need_cookie = true,
            };
            api.parameter += ApiHelper.GetSign(apiBody, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// web版登录获取到的Cookie转app令牌
        /// </summary>
        /// <returns></returns>
        public ApiModel GetCookieToAccessKey(string url)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = url,
                need_cookie = true,
                need_redirect = true,
            };
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
                method = RestSharp.Method.Get,
                baseUrl = "https://passport.bilibili.com/api/oauth2/info",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey) + "&access_token=" + SettingService.Account.AccessKey
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
                method = RestSharp.Method.Post,
                baseUrl = "https://passport.bilibili.com/api/oauth2/refreshToken",
                body = ApiHelper.MustParameter(ApiHelper.AndroidKey) + $"&access_token={SettingService.Account.AccessKey}&refresh_token={SettingService.GetValue(SettingConstants.Account.REFRESH_KEY, "")}"
            };
            api.body += ApiHelper.GetSign(api.body, ApiHelper.AndroidKey);
            return api;
        }

        /// <summary>
        /// 检查Cookie是否需要刷新
        /// </summary>
        /// <returns></returns>
        public ApiModel CheckCookies()
        {
            var csrf = m_cookieService.GetCSRFToken();
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = "https://passport.bilibili.com/x/passport-login/web/cookie/info",
                parameter = $"csrf={csrf}",
                need_cookie = true,
            };
            return api;
        }

        /// <summary>
        /// 刷新CSRF
        /// </summary>
        /// <returns></returns>
        public ApiModel RefreshCsrf(string correspondPath)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://www.bilibili.com/correspond/1/{correspondPath}",
                need_cookie = true,
            };
            return api;
        }

        /// <summary>
        /// 刷新Cookie
        /// </summary>
        /// <returns></returns>
        public ApiModel RefreshCookie(string refreshCsrf,string refreshToken)
        {
            var csrf = m_cookieService.GetCSRFToken();
            var api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"https://passport.bilibili.com/x/passport-login/web/cookie/refresh",
                body = $"csrf={csrf}&refresh_csrf={refreshCsrf}&source=main_web&refresh_token={refreshToken}",
                need_cookie = true,
            };
            return api;
        }

        /// <summary>
        /// 确认更新Cookie
        /// </summary>
        /// <returns></returns>
        public ApiModel ConfirmRefreshCookie(string refreshToken)
        {
            var csrf = m_cookieService.GetCSRFToken();
            var api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"https://passport.bilibili.com/x/passport-login/web/confirm/refresh",
                body = $"csrf={csrf}&refresh_token={refreshToken}",
                need_cookie = true,
            };
            return api;
        }

        public ApiModel Nav()
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = "https://api.bilibili.com/x/web-interface/nav"
            };
            return api;
        }
    }
}
