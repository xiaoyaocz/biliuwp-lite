using BiliLite.Api;
using BiliLite.Helpers;
using BiliLite.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.Web.Http.Filters;

namespace BiliLite.Modules
{
    public class Account
    {
        private SettingVM settingVM;
        public AccountApi accountApi;
        string guid = "";
        public Account()
        {
            accountApi = new AccountApi();
            settingVM = new SettingVM();
            guid = Guid.NewGuid().ToString();
        }
        public async Task<string> EncryptedPassword(string passWord)
        {
            string base64String;
            try
            {
                HttpBaseProtocolFilter httpBaseProtocolFilter = new HttpBaseProtocolFilter();
                httpBaseProtocolFilter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Expired);
                httpBaseProtocolFilter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Untrusted);
                var jObjects = (await accountApi.GetKey().Request()).GetJObject();
                string str = jObjects["data"]["hash"].ToString();
                string str1 = jObjects["data"]["key"].ToString();
                string str2 = string.Concat(str, passWord);
                string str3 = Regex.Match(str1, "BEGIN PUBLIC KEY-----(?<key>[\\s\\S]+)-----END PUBLIC KEY").Groups["key"].Value.Trim();
                byte[] numArray = Convert.FromBase64String(str3);
                AsymmetricKeyAlgorithmProvider asymmetricKeyAlgorithmProvider = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithmNames.RsaPkcs1);
                CryptographicKey cryptographicKey = asymmetricKeyAlgorithmProvider.ImportPublicKey(WindowsRuntimeBufferExtensions.AsBuffer(numArray), 0);
                IBuffer buffer = CryptographicEngine.Encrypt(cryptographicKey, WindowsRuntimeBufferExtensions.AsBuffer(Encoding.UTF8.GetBytes(str2)), null);
                base64String = Convert.ToBase64String(WindowsRuntimeBufferExtensions.ToArray(buffer));
            }
            catch (Exception)
            {
                base64String = passWord;
            }
            return base64String;
        }

        /// <summary>
        /// 登录V2
        /// </summary>
        /// <returns></returns>
        public async Task<LoginCallbackModel> LoginV2(string username, string password, string captcha = "")
        {
            try
            {

                var results = await accountApi.LoginV2(username, await EncryptedPassword(password), captcha).Request();
                var m = await results.GetJson<LoginV2Model>();
                if (m.code == 0)
                {
                    m.data.expires_datetime = Utils.TimestampToDatetime(m.ts).AddSeconds(m.data.expires_in);
                    //设置登录状态
                    SettingHelper.SetValue(SettingHelper.Account.ACCESS_KEY, m.data.access_token);
                    SettingHelper.SetValue(SettingHelper.Account.USER_ID, m.data.mid);
                    SettingHelper.SetValue(SettingHelper.Account.ACCESS_KEY_EXPIRE_DATE, DateTime.Now.AddSeconds(m.data.expires_in));
                    SettingHelper.SetValue(SettingHelper.Account.REFRESH_KEY, m.data.refresh_token);

                    //执行SSO
                    //await accountApi.SSO(m.data.access_token).Request();
                    //读取个人资料
                    await GetProfile();

                    //发送登录成功事件
                    MessageCenter.SendLogined();

                    return new LoginCallbackModel()
                    {
                        status = LoginStatus.Success,
                        message = "登录成功"
                    };
                }
                else if (m.code == -2100)
                {
                    return new LoginCallbackModel()
                    {
                        status = LoginStatus.NeedValidate,
                        url = m.url,
                        message = "登录需要验证"
                    };
                }
                else if (m.code == -105)
                {
                    return new LoginCallbackModel()
                    {
                        status = LoginStatus.NeedCaptcha,
                        message = "登录需要验证码"
                    };
                }
                else
                {
                    return new LoginCallbackModel()
                    {
                        status = LoginStatus.Fail,
                        message = m.message
                    };
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log("登录错误", LogType.ERROR, ex);
                return new LoginCallbackModel()
                {
                    status = LoginStatus.Error,
                    message = "登录出现小问题,请重试"
                };
            }
        }

        /// <summary>
        /// 登录V3
        /// </summary>
        /// <returns></returns>
        public async Task<LoginCallbackModel> LoginV3(string username, string password, string seccode = "", string validate = "", string recaptcha_token = "", string challenge = "", int gee_type = 10)
        {
            try
            {

                var results = await accountApi.LoginV3(username, await EncryptedPassword(password), seccode, validate, challenge, recaptcha_token).Request();
                if (results.status)
                {
                    var m = await results.GetData<LoginDataV3Model>();
                    if (m.code == 0)
                    {
                        if (m.data.status == 0)
                        {
                            await SaveLogin(m.data.token_info.access_token, m.data.token_info.refresh_token, m.data.token_info.expires_in, m.data.token_info.mid, m.data.sso,m.data.cookie_info);
                            return new LoginCallbackModel()
                            {
                                status = LoginStatus.Success,
                                message = "登录成功"
                            };
                        }
                        if (m.data.status == 1 || m.data.status == 2)
                        {
                            return new LoginCallbackModel()
                            {
                                status = LoginStatus.NeedValidate,
                                message = "本次登录需要安全验证",
                                url = m.data.url
                            };
                        }
                        return new LoginCallbackModel()
                        {
                            status = LoginStatus.Fail,
                            message = m.message
                        };
                    }
                    else if (m.code == -105)
                    {
                        return new LoginCallbackModel()
                        {
                            status = LoginStatus.NeedCaptcha,
                            url = m.data.url,
                            message = "登录需要验证码"
                        };
                    }
                    else
                    {
                        return new LoginCallbackModel()
                        {
                            status = LoginStatus.Fail,
                            message = m.message
                        };
                    }
                }
                else
                {
                    LogHelper.Log("登录V3请求失败错误", LogType.ERROR);
                    return new LoginCallbackModel()
                    {
                        status = LoginStatus.Error,
                        message = results.message
                    };
                }

            }
            catch (Exception ex)
            {
                LogHelper.Log("登录V3错误", LogType.ERROR, ex);
                return new LoginCallbackModel()
                {
                    status = LoginStatus.Error,
                    message = "登录出现小问题,请重试"
                };
            }
        }


        /// <summary>
        /// 安全验证后保存状态
        /// </summary>
        /// <param name="access_key"></param>
        /// <param name="refresh_token"></param>
        /// <param name="expires"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public async Task<bool> SaveLogin(string access_key, string refresh_token, int expires, long userid, List<string> sso,LoginCookieInfo cookie)
        {
            try
            {
                //设置登录状态

                SettingHelper.SetValue(SettingHelper.Account.ACCESS_KEY, access_key);
                SettingHelper.SetValue(SettingHelper.Account.USER_ID, userid);
                SettingHelper.SetValue(SettingHelper.Account.ACCESS_KEY_EXPIRE_DATE, DateTime.Now.AddSeconds(expires));
                SettingHelper.SetValue(SettingHelper.Account.REFRESH_KEY, refresh_token);
                var data = new LoginTokenInfo()
                {
                    access_token = access_key,
                    expires_datetime = DateTime.Now.AddSeconds(expires),
                    expires_in = expires,
                    mid = userid,
                    refresh_token = refresh_token
                };
                // 好像没啥用...
                if (sso == null)
                {
                    sso = new List<string>() {
                        "https://passport.bilibili.com/api/v2/sso",
                        "https://passport.biligame.com/api/v2/sso",
                        "https://passport.bigfunapp.cn/api/v2/sso",
                    };
                }
                try
                {
                    //设置Cookie
                    if (cookie!=null)
                    {
                        HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter();
                        foreach (var item in cookie.domains)
                        {
                            foreach (var cookieItem in cookie.cookies)
                            {
                                filter.CookieManager.SetCookie(new Windows.Web.Http.HttpCookie(cookieItem.name, item,"/")
                                {
                                    HttpOnly=cookieItem.http_only==1,
                                    Secure=cookieItem.secure==1,
                                    Expires=Utils.TimestampToDatetime(cookieItem.expires),
                                    Value=cookieItem.value,
                                });
                            }
                        }
                    }
                    //执行SSO
                    //await accountApi.SSO(access_key).Request();
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"SSO失败", LogType.ERROR, ex);
                }

                //读取个人资料
                await GetProfile();
                MessageCenter.SendLogined();
                return true;

            }
            catch (Exception ex)
            {
                LogHelper.Log("安全验证后设置保存信息失败", LogType.ERROR, ex);
                return false;
            }
        }

        public async Task<MyProfileModel> GetProfile()
        {
            try
            {
                var req = await accountApi.UserProfile().Request();
                var obj = req.GetJObject();
                if (req.status && obj["code"].ToInt32() == 0)
                {
                    var data = JsonConvert.DeserializeObject<MyProfileModel>(obj["data"].ToString());
                    SettingHelper.SetValue(SettingHelper.Account.USER_PROFILE, data);
                    return data;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log("读取个人资料失败", LogType.ERROR, ex);
                return null;
            }
        }

        public async Task<HomeUserCardModel> GetHomeUserCard()
        {

            try
            {
                var mine_api = accountApi.MineProfile();
                var mine_result = await mine_api.Request();
                if (!mine_result.status)
                {
                    return null;
                }
                var mine_obj = mine_result.GetJObject();
                if (mine_obj["code"].ToInt32() != 0)
                {
                    return null;
                }
                var space_api = accountApi.Space(SettingHelper.Account.UserID.ToString());
                var space_result = await space_api.Request();
                if (!space_result.status)
                {
                    return null;
                }
                var space_obj = space_result.GetJObject();
                if (space_obj["code"].ToInt32() != 0)
                {
                    return null;
                }
                var data = new HomeUserCardModel();
                data.current_exp = space_obj["data"]["card"]["level_info"]["current_exp"].ToInt32();
                data.next_exp = space_obj["data"]["card"]["level_info"]["next_exp"].ToInt32();
                data.dynamic = mine_obj["data"]["dynamic"].ToInt32();
                data.face = mine_obj["data"]["face"].ToString();
                data.fans = mine_obj["data"]["follower"].ToInt32();
                data.follow = mine_obj["data"]["following"].ToInt32();
                data.level = mine_obj["data"]["level"].ToInt32();
                data.mid = mine_obj["data"]["mid"].ToString();
                data.name = mine_obj["data"]["name"].ToString();
                data.pendant = space_obj["data"]["card"]["pendant"]["image"].ToString();
                if (data.pendant == "")
                {
                    data.pendant = AppHelper.TRANSPARENT_IMAGE;
                }
                data.vip_type = mine_obj["data"]["vip_type"].ToInt32();
                return data;

            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取二维码登录信息
        /// </summary>
        /// <returns></returns>
        public async Task<ReturnModel<QRAuthInfo>> GetQRAuthInfo()
        {
            try
            {
                var result = await accountApi.QRLoginAuthCode(guid).Request();
                if (result.status)
                {
                    var data = await result.GetData<QRAuthInfo>();
                    if (data.success)
                    {
                        return new ReturnModel<QRAuthInfo>()
                        {
                            success = true,
                            data = data.data
                        };
                    }
                    else
                    {
                        return new ReturnModel<QRAuthInfo>()
                        {
                            success = false,
                            message = data.message
                        };

                    }
                }
                else
                {
                    return new ReturnModel<QRAuthInfo>()
                    {
                        success = false,
                        message = result.message
                    };
                }
            }
            catch (Exception ex)
            {
                return new ReturnModel<QRAuthInfo>()
                {
                    success = false,
                    message = ex.Message
                };
            }
        }
        /// <summary>
        /// 轮询二维码扫描信息
        /// </summary>
        /// <returns></returns>
        public async Task<LoginCallbackModel> PollQRAuthInfo(string auth_code)
        {
            try
            {
                var result = await accountApi.QRLoginPoll(auth_code, guid).Request();
                if (result.status)
                {
                    var data = await result.GetData<LoginDataV3Model>();
                    if (data.success)
                    {
                        
                        await SaveLogin(data.data.token_info.access_token, data.data.token_info.refresh_token, data.data.token_info.expires_in, data.data.token_info.mid, data.data.sso,data.data.cookie_info);
                        return new LoginCallbackModel()
                        {
                            status = LoginStatus.Success,
                            message = "登录成功"
                        };
                    }
                    else
                    {
                        return new LoginCallbackModel()
                        {
                            status = LoginStatus.Fail,
                            message = data.message
                        };

                    }
                }
                else
                {
                    return new LoginCallbackModel()
                    {
                        status = LoginStatus.Fail,
                        message = result.message
                    };
                }
            }
            catch (Exception ex)
            {
                return new LoginCallbackModel()
                {
                    status = LoginStatus.Fail,
                    message = ex.Message
                };
            }
        }

        /// <summary>
        /// 检查登录状态
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CheckLoginState()
        {
            try
            {
                var req = await accountApi.GetOAuth2Info().Request();
                if (req.status)
                {
                    var obj = req.GetJObject();
                    return obj["code"].ToInt32() == 0;
                }
                else
                {
                    throw new Exception(req.message);
                    //return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
                //LogHelper.Log("读取access_key信息失败", LogType.ERROR, ex);
                //return LogHelper.IsNetworkError(ex);
            }
        }
        /// <summary>
        /// 刷新Token
        /// </summary>
        /// <returns></returns>
        public async Task<bool> RefreshToken()
        {
            try
            {
                var req = await accountApi.RefreshToken().Request();

                if (req.status)
                {
                    var obj = req.GetJObject();
                    if (obj["code"].ToInt32() == 0)
                    {
                        var data = JsonConvert.DeserializeObject<LoginTokenInfo>(obj["data"].ToString());
                        await SaveLogin(data.access_token, data.refresh_token, data.expires_in, data.mid, null,null);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    throw new Exception(req.message);
                    // return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
                //LogHelper.Log("读取access_key信息失败", LogType.ERROR, ex);
                //return false;
            }
        }
    }

    public class HomeUserCardModel
    {
        public string name { get; set; }
        public string face { get; set; }
        public string mid { get; set; }
        public int fans { get; set; }
        public int vip_type { get; set; }
        public int follow { get; set; }
        public int dynamic { get; set; }
        public string pendant { get; set; }

        public int level { get; set; }
        public int current_exp { get; set; }
        public int next_exp { get; set; }

        public bool show_vip
        {
            get
            {
                return vip_type != 0;
            }
        }

        public string vip
        {
            get
            {

                return vip_type == 2 ? "年度大会员" : "月度大会员";
            }
        }
    }

    public class QRAuthInfo
    {
        public string url { get; set; }
        public string auth_code { get; set; }
    }

}
