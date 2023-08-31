using BiliLite.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.Web.Http.Filters;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Models.Responses;
using BiliLite.Models.Requests.Api;
using BiliLite.Services;
using BiliLite.Models.Common.Account;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;

namespace BiliLite.Modules
{
    public class Account
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        private readonly CookieService m_cookieService;
        private SettingVM settingVM;
        public AccountApi accountApi;
        string guid = "";
        public Account()
        {
            m_cookieService = App.ServiceProvider.GetRequiredService<CookieService>();
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
        /// 安全验证后保存状态
        /// </summary>
        /// <param name="access_key"></param>
        /// <param name="refresh_token"></param>
        /// <param name="expires"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public async Task<bool> SaveLogin(string access_key, string refresh_token, int expires, long userid, List<string> sso, LoginCookieInfo cookie)
        {
            try
            {
                //设置登录状态

                SettingService.SetValue(SettingConstants.Account.ACCESS_KEY, access_key);
                SettingService.SetValue(SettingConstants.Account.USER_ID, userid);
                SettingService.SetValue(SettingConstants.Account.ACCESS_KEY_EXPIRE_DATE, DateTime.Now.AddSeconds(expires));
                SettingService.SetValue(SettingConstants.Account.REFRESH_KEY, refresh_token);
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
                    if (cookie != null)
                    {
                        var cookies = new List<HttpCookieItem>();
                        cookies.AddRange(cookie.cookies.Select(cookieItem => new HttpCookieItem()
                        {
                            Domain = Constants.BILIBILI_DOMAIN,
                            Expires = TimeExtensions.TimestampToDatetime(cookieItem.expires),
                            HttpOnly = cookieItem.http_only == 1,
                            Name = cookieItem.name,
                            Secure = cookieItem.secure == 1,
                            Value = cookieItem.value,
                        }));
                        m_cookieService.Cookies = cookies;
                    }
                    //执行SSO
                    //await accountApi.SSO(access_key).Request();
                }
                catch (Exception ex)
                {
                    _logger.Log($"SSO失败", LogType.Error, ex);
                }

                //读取个人资料
                await GetProfile();
                MessageCenter.SendLogined();
                return true;

            }
            catch (Exception ex)
            {
                _logger.Log("安全验证后设置保存信息失败", LogType.Error, ex);
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
                    SettingService.SetValue(SettingConstants.Account.USER_PROFILE, data);
                    return data;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.Log("读取个人资料失败", LogType.Error, ex);
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
                var space_api = accountApi.Space(SettingService.Account.UserID.ToString());
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
                    data.pendant = Constants.App.TRANSPARENT_IMAGE;
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
        public async Task<ReturnModel<QRAuthInfoWeb>> GetQRAuthInfo()
        {
            try
            {
                var result = await accountApi.QRLoginAuthCode().Request();
                var data = await result.GetData<QRAuthInfoWeb>();
                return new ReturnModel<QRAuthInfoWeb>()
                {
                    success = true,
                    data = data.data
                };
            }
            catch (Exception ex)
            {
                return new ReturnModel<QRAuthInfoWeb>()
                {
                    success = false,
                    message = ex.Message
                };
            }
        }

        public async void LoginByCookie(List<HttpCookieItem> cookies, string refresh_token)
        {
            m_cookieService.Cookies = cookies;
            var cookieToAccessKeyConfirmUrl = await GetCookieToAccessKeyConfirmUrl();
            var accessKey = await GetAccessKey(cookieToAccessKeyConfirmUrl);
            // var expires = result.cookies[0].Expires;
            var userId = cookies.FirstOrDefault(x => x.Name == "DedeUserID").Value;
            SettingService.SetValue(SettingConstants.Account.IS_WEB_LOGIN, true);
            await SaveLogin(accessKey, refresh_token, 3600 * 240, long.Parse(userId), null, null);
        }

        /// <summary>
        /// 轮询二维码扫描信息
        /// </summary>
        /// <returns></returns>
        public async Task<LoginCallbackModel> PollQRAuthInfo(string auth_code)
        {
            try
            {
                var result = await accountApi.QRLoginPoll(auth_code).Request();
                if (result.status)
                {
                    var data = await result.GetData<LoginDataWebModel>();
                    if (data.data.code == LoginQRStatusCode.Success)
                    {
                        LoginByCookie(result.cookies, data.data.refresh_token);
                        return new LoginCallbackModel()
                        {
                            status = LoginStatus.Success,
                            message = "登录成功"
                        };
                    }
                    return new LoginCallbackModel()
                    {
                        status = LoginStatus.Fail,
                        message = data.message
                    };
                }
                return new LoginCallbackModel()
                {
                    status = LoginStatus.Fail,
                    message = result.message
                };
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

        public async Task<string> GetCookieToAccessKeyConfirmUrl()
        {
            try
            {
                var result = await accountApi.GetCookieToAccessKey().Request();
                if (result.status)
                {
                    var data = await result.GetData<LoginAppThirdResponse>();
                    return data.data.confirm_uri;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return null;
        }

        public async Task<string> GetAccessKey(string url)
        {
            try
            {
                var result = await accountApi.GetCookieToAccessKey(url).Request();
                if (!result.status) return null;
                var uri = new Uri(result.results);
                var queries = HttpUtility.ParseQueryString(uri.Query);
                var accessKey = queries.Get("access_key");
                return accessKey;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取二维码登录信息
        /// </summary>
        /// <returns></returns>
        public async Task<ReturnModel<QRAuthInfo>> GetQRAuthInfoTV()
        {
            try
            {
                var result = await accountApi.QRLoginAuthCodeTV(guid).Request();
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
        public async Task<LoginCallbackModel> PollQRAuthInfoTV(string auth_code)
        {
            try
            {
                var result = await accountApi.QRLoginPollTV(auth_code, guid).Request();
                if (result.status)
                {
                    var data = await result.GetData<LoginDataV3Model>();
                    if (data.success)
                    {
                        SettingService.SetValue(SettingConstants.Account.IS_WEB_LOGIN, false);
                        await SaveLogin(data.data.token_info.access_token, data.data.token_info.refresh_token, data.data.token_info.expires_in, data.data.token_info.mid, data.data.sso, data.data.cookie_info);
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
                        await SaveLogin(data.access_token, data.refresh_token, data.expires_in, data.mid, null, null);
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

        /// <summary>
        /// 检查与更新Cookies
        /// </summary>
        /// <returns></returns>
        public async Task CheckUpdateCookies()
        {
            if (!SettingService.GetValue(SettingConstants.Account.IS_WEB_LOGIN, false)) return;
            var checkResult = await CheckCookies();
            if (!checkResult.Refresh)
            {
                return;
            }
            try
            {
                var correspondPath = await GetCorrespondPath(checkResult.Timestamp);
                var newCsrf = await GetCsrfRefresh(correspondPath);
                var refreshToken = SettingService.GetValue(SettingConstants.Account.REFRESH_KEY, "");
                await RefreshCookies(newCsrf, refreshToken);
                await ConfirmRefreshCookies(refreshToken);
            }
            catch (Exception ex)
            {
                _logger.Error("刷新Cookie失败", ex);
                Notify.ShowMessageToast("刷新Cookie失败，建议手动重新登录");
            }
        }

        private async Task<CheckCookieResult> CheckCookies()
        {
            var api = new AccountApi().CheckCookies();
            var result = await api.Request();
            var checkCookieResult = await result.GetData<CheckCookieResult>();
            return checkCookieResult.data;
        }

        private async Task<string> GetCorrespondPath(long timestamp)
        {
            var baseUrl = SettingService.GetValue(SettingConstants.Other.BILI_LITE_WEB_API_BASE_URL, ApiConstants.BILI_LITE_WEB_API_DEFAULT_BASE_URL);
            var apiUrl = $"{baseUrl}{ApiConstants.Utils.GET_CORRESPOND_PATH}";
            var result = await $"{apiUrl}?timestamp={timestamp}".GetString();
            return result;
        }

        private async Task RefreshCookies(string csrf, string refreshToken)
        {
            var api = new AccountApi().RefreshCookie(csrf, refreshToken);
            var result = await api.Request();
            var data = await result.GetData<LoginDataWebModel>();
            LoginByCookie(result.cookies, data.data.refresh_token);
        }

        private async Task ConfirmRefreshCookies(string refreshToken)
        {
            var api = new AccountApi().ConfirmRefreshCookie(refreshToken);
            var result = await api.Request();
        }

        ///// <summary>
        ///// 构造CorrespondPath，.net standard2.1以上可用，以后更新WinUI再启用
        ///// </summary>
        ///// <param name="timestamp"></param>
        ///// <returns></returns>
        //private string BuildCorrespondPath(long timestamp)
        //{
        //    // 定义公钥
        //    string publicKey = "-----BEGIN PUBLIC KEY-----\nMIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDLgd2OAkcGVtoE3ThUREbio0Eg\nUc/prcajMKXvkCKFCWhJYJcLkcM2DKKcSeFpD/j6Boy538YXnR6VhcuUJOhH2x71\nnzPjfdTcqMz7djHum0qSZA0AyCBDABUqCrfNgCiJ00Ra7GmRj+YCK1NJEuewlb40\nJNrRuoEUXpabUzGB8QIDAQAB\n-----END PUBLIC KEY-----";
        //    // 定义字符串payload
        //    string payload = $"refresh_{timestamp}";

        //    // 将字符串payload转换为字节数组
        //    byte[] data = Encoding.UTF8.GetBytes(payload);

        //    RSAEncryptionPadding oaepsha256 = RSAEncryptionPadding.OaepSHA256;
        //    using RSA rsaImpl = new RSACng();

        //    rsaImpl.ImportFromPem(publicKey);
        //    var ciphertext = rsaImpl.Encrypt(data, oaepsha256);

        //    // 将字节数组转换为十六进制字符串
        //    var hex = BitConverter.ToString(ciphertext);

        //    // 去掉分隔符
        //    hex = hex.Replace("-", "");
        //    return hex.ToLower();
        //}

        private async Task<string> GetCsrfRefresh(string correspondPath)
        {
            var result = await new AccountApi().RefreshCsrf(correspondPath).Request();
            var html = result.results;

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            // 使用XPath查询找到id为1-name的标签
            var node = doc.DocumentNode.SelectSingleNode("//div[@id='1-name']");
            // 获取标签中的文本内容
            var text = node.InnerText;
            return text;
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

    public class QRAuthInfoWeb
    {
        public string url { get; set; }
        public string qrcode_key { get; set; }
    }
}
