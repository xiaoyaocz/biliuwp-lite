using BiliLite.Helpers;
using BiliLite.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;

namespace BiliLite.Modules.User
{
    public class LoginVM : IModules
    {
        public Account account;
        Timer smsTimer;
        Timer qrTimer;
        Api.AccountApi accountApi;
        public LoginVM()
        {
            account = new Account();
            accountApi = new Api.AccountApi();
            Countries = new ObservableCollection<CountryItemModel>();
            LoginTypeCommand = new RelayCommand<int>(ChangeLoginType);
            SendSMSCommand = new RelayCommand(SendSMSCode);
            RefreshQRCommand = new RelayCommand(RefreshQRCode);
            smsTimer = new Timer(1000);
            smsTimer.Elapsed += SmsTimer_Elapsed;
        }
        public event EventHandler<Uri> OpenWebView;
        public event EventHandler<bool> SetWebViewVisibility;
        public event EventHandler CloseDialog;
        public ICommand LoginTypeCommand { get; private set; }
        public ICommand SendSMSCommand { get; private set; }
        public ICommand RefreshQRCommand { get; private set; }

        private int loginType = 1;
        /// <summary>
        /// 登录类型
        /// 0=账号密码，1=短信登录，2=二维码登录
        /// </summary>
        public int LoginType
        {
            get { return loginType; }
            set { loginType = value; DoPropertyChanged("LoginType"); }
        }

        private string title = "短信登录";
        public string Title
        {
            get { return title; }
            set { title = value; DoPropertyChanged("Title"); }
        }

        private bool _primaryButtonEnable = true;

        public bool PrimaryButtonEnable
        {
            get { return _primaryButtonEnable; }
            set { _primaryButtonEnable = value; DoPropertyChanged("PrimaryButtonEnable"); }
        }



        public void ChangeLoginType(int type)
        {
            LoginType = type;
           
            switch (type)
            {
                case 0:
                    Title = "密码登录";
                    if (qrTimer != null)
                    {
                        qrTimer.Stop();
                        qrTimer.Dispose();
                    }
                    Utils.ShowMessageToast("为了您的账号安全,建议扫描二维码登录");
                    break;
                case 1:
                    Title = "短信登录";
                    if (qrTimer != null)
                    {
                        qrTimer.Stop();
                        qrTimer.Dispose();
                    }
                    break;
                case 2:
                    Title = "二维码登录";
                    GetQRAuthInfo();
                    break;
                default:
                    break;
            }
        }

        public void DoLogin()
        {
            if (loginType == 1)
            {
                DoSMSLogin();
            }
            if (loginType == 0)
            {
                DoPasswordLogin();
            }
        }

        public async void ValidateLogin(JObject jObject)
        {
            try
            {
                if (jObject["access_token"] != null)
                {
                    var m = await account.SaveLogin(jObject["access_token"].ToString(), jObject["refresh_token"].ToString(), jObject["expires_in"].ToInt32(), Convert.ToInt64(jObject["mid"].ToString()), null);

                    if (m)
                    {
                        CloseDialog?.Invoke(this, null);
                        Utils.ShowMessageToast("登录成功");
                    }
                    else
                    {
                        PrimaryButtonEnable = true;
                        SetWebViewVisibility?.Invoke(this, false);
                        Utils.ShowMessageToast("登录失败,请重试");
                    }
                    //await UserManage.LoginSucess(jObject["access_token"].ToString());
                }
                else
                {
                    PrimaryButtonEnable = true;
                    SetWebViewVisibility?.Invoke(this, false);
                    Utils.ShowMessageToast("登录失败,请重试");
                }

            }
            catch (Exception ex)
            {
                LogHelper.Log("登录二次验证失败", LogType.ERROR, ex);
            }
        }

        #region 短信登录
        private ObservableCollection<CountryItemModel> _countries;
        public ObservableCollection<CountryItemModel> Countries
        {
            get { return _countries; }
            set { _countries = value; DoPropertyChanged("Countries"); }
        }
        private CountryItemModel currentCountry;
        public CountryItemModel CurrentCountry
        {
            get { return currentCountry; }
            set { currentCountry = value; DoPropertyChanged("CurrentCountry"); }
        }
        private string phone;
        public string Phone
        {
            get { return phone; }
            set { phone = value; DoPropertyChanged("Phone"); }
        }

        private string code;
        public string Code
        {
            get { return code; }
            set { code = value; DoPropertyChanged("Code"); }
        }

        private bool enableSendSMS = true;
        public bool EnableSendSMS
        {
            get { return enableSendSMS; }
            set { enableSendSMS = value; DoPropertyChanged("EnableSendSMS"); }
        }


        private int _SMSCountDown = 60;
        public int SMSCountDown
        {
            get { return _SMSCountDown; }
            set { _SMSCountDown = value; DoPropertyChanged("SMSCountDown"); }
        }


        public async Task LoadCountry()
        {
            try
            {

                var results = await accountApi.Country().Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        var ls = JsonConvert.DeserializeObject<ObservableCollection<CountryItemModel>>(data.data["list"].ToString());
                        if (ls != null && ls.Count > 0)
                        {
                            Countries = ls;
                            CurrentCountry = Countries.First();
                        }
                    }
                    else
                    {
                        Utils.ShowMessageToast(data.message);
                    }
                }
                else
                {
                    Utils.ShowMessageToast(results.message);
                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<AnimeHomeModel>(ex);
                Utils.ShowMessageToast(handel.message);
            }

        }
        private async void SmsTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (SMSCountDown <= 1)
                {
                    EnableSendSMS = true;
                    smsTimer.Stop();
                }
                else
                {
                    SMSCountDown--;
                }
            });
        }
        string sessionId = "";
        string captchaKey = "";
        public async void SendSMSCode()
        {
            if (CurrentCountry == null)
            {
                Utils.ShowMessageToast("请选择国家/地区");
                return;
            }
            if (Phone.Length == 0)
            {
                Utils.ShowMessageToast("请输入手机号");
                return;
            }

            try
            {
                sessionId = Guid.NewGuid().ToString().Replace("-", "");
                var results = await accountApi.SendSMS(CurrentCountry.country_code, Phone, sessionId).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<SMSResultModel>>();
                    if (data.success)
                    {
                        if (data.data.recaptcha_url != null && data.data.recaptcha_url.Length > 0)
                        {
                            var uri = new Uri(data.data.recaptcha_url);
                            SetWebViewVisibility?.Invoke(this, true);
                            OpenWebView?.Invoke(this, new Uri("https://l78z.nsapps.cn/bili_gt.html" + uri.Query + "&app=uwp"));
                        }
                        else
                        {
                            EnableSendSMS = false;
                            //验证码发送成功，倒计时
                            SMSCountDown = 60;
                            smsTimer.Start();
                        }
                    }
                    else
                    {
                        Utils.ShowMessageToast(data.message);
                    }
                }
                else
                {
                    Utils.ShowMessageToast(results.message);
                }

            }
            catch (Exception ex)
            {
                Utils.ShowMessageToast(ex.Message);
                EnableSendSMS = true;
            }
        }
        public async void SendSMSCodeWithCaptcha(string seccode = "", string validate = "", string challenge = "", string recaptcha_token = "")
        {

            try
            {
                var results = await accountApi.SendSMSWithCaptcha(CurrentCountry.country_code, Phone, sessionId, seccode, validate, challenge, recaptcha_token).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<SMSResultModel>>();
                    if (data.success)
                    {
                        if (data.data.recaptcha_url != null && data.data.recaptcha_url.Length > 0)
                        {
                            var uri = new Uri(data.data.recaptcha_url);
                            SetWebViewVisibility?.Invoke(this, true);
                            OpenWebView?.Invoke(this, new Uri("https://l78z.nsapps.cn/bili_gt.html" + uri.Query + "&app=uwp"));

                        }
                        else
                        {
                            captchaKey = data.data.captcha_key;
                            //验证码发送成功，倒计时
                            EnableSendSMS = false;
                            SMSCountDown = 60;
                            smsTimer.Start();
                        }
                    }
                    else
                    {
                        Utils.ShowMessageToast(data.message);
                    }
                }
                else
                {
                    Utils.ShowMessageToast(results.message);
                }

            }
            catch (Exception ex)
            {
                Utils.ShowMessageToast(ex.Message);
                EnableSendSMS = true;
            }
        }

        public async void DoSMSLogin()
        {
            if (CurrentCountry == null)
            {
                Utils.ShowMessageToast("请选择国家/地区");
                return;
            }
            if (Phone.Length == 0)
            {
                Utils.ShowMessageToast("请输入手机号");
                return;
            }
            if (Code.Length == 0)
            {
                Utils.ShowMessageToast("请输入验证码");
                return;
            }
            try
            {

                var results = await accountApi.SMSLogin(CurrentCountry.country_code, Phone, Code, sessionId, captchaKey).Request();
                if (results.status)
                {
                    var data = await results.GetData<LoginResultModel>();
                    var result = await HandelLoginResult(data.code, data.message, data.data);
                    HnadelResult(result);
                }
                else
                {
                    Utils.ShowMessageToast(results.message);
                }

            }
            catch (Exception ex)
            {
                Utils.ShowMessageToast(ex.Message);
                EnableSendSMS = true;
            }
        }


        #endregion

        #region 密码登录

        private string userName;

        public string UserName
        {
            get { return userName; }
            set { userName = value; DoPropertyChanged("UserName"); }
        }

        private string password;
        public string Password
        {
            get { return password; }
            set { password = value; DoPropertyChanged("Password"); }
        }
        string pwdSessionId = "";
        public async void DoPasswordLogin(string seccode = "", string validate = "", string challenge = "", string recaptcha_token = "")
        {
            PrimaryButtonEnable = false;
            if (UserName.Length == 0)
            {
                Utils.ShowMessageToast("请输入用户名");
                return;
            }
            if (Password.Length == 0)
            {
                Utils.ShowMessageToast("请输入密码");
                return;
            }
            try
            {
                if (seccode == "")
                {
                    pwdSessionId = Guid.NewGuid().ToString().Replace("-", "");
                }


                var pwd = await account.EncryptedPassword(Password);
                var results = await accountApi.LoginV3(UserName, pwd, pwdSessionId, seccode, validate, challenge, recaptcha_token).Request();
                if (results.status)
                {
                    var data = await results.GetData<LoginResultModel>();
                    var result = await HandelLoginResult(data.code, data.message, data.data);

                    HnadelResult(result);
                }
                else
                {
                    Utils.ShowMessageToast(results.message);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowMessageToast(ex.Message);

            }
            finally
            {
                PrimaryButtonEnable = true;
            }
        }

        #endregion

        #region 二维码登录

        private bool qrLoadding;

        public bool QRLoadding
        {
            get { return qrLoadding; }
            set { qrLoadding = value; DoPropertyChanged("QRLoadding"); }
        }

        private Windows.UI.Xaml.Media.ImageSource qrImageSource;

        public Windows.UI.Xaml.Media.ImageSource QRImageSource
        {
            get { return qrImageSource; }
            set { qrImageSource = value; DoPropertyChanged("QRImageSource"); }
        }


        QRAuthInfo qrAuthInfo;
        private async void GetQRAuthInfo()
        {
            try
            {
                QRLoadding = true;
                if (qrTimer != null)
                {
                    qrTimer.Stop();
                    qrTimer.Dispose();
                }
                var result = await account.GetQRAuthInfo();
                if (result.success)
                {
                    qrAuthInfo = result.data;
                    ZXing.BarcodeWriter barcodeWriter = new ZXing.BarcodeWriter();
                    barcodeWriter.Format = ZXing.BarcodeFormat.QR_CODE;
                    barcodeWriter.Options = new ZXing.Common.EncodingOptions()
                    {
                        Margin = 1,
                        Height = 200,
                        Width = 200
                    };
                    var img = barcodeWriter.Write(qrAuthInfo.url);
                    QRImageSource = img;
                    qrTimer = new Timer();
                    qrTimer.Interval = 3000;
                    qrTimer.Elapsed += QrTimer_Elapsed;
                    qrTimer.Start();
                }
                else
                {
                    Utils.ShowMessageToast(result.message);
                }

            }
            catch (Exception ex)
            {
                LogHelper.Log("读取和加载登录二维码失败", LogType.ERROR, ex);
                Utils.ShowMessageToast("加载二维码失败");
            }
            finally
            {
                QRLoadding = false;
            }

        }

        private void RefreshQRCode()
        {
            if (QRLoadding)
                return;
            GetQRAuthInfo();
        }


        private async void QrTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                var result = await account.PollQRAuthInfo(qrAuthInfo.auth_code);
                  if (result.status == LoginStatus.Success)
                  {
                      qrTimer.Stop();
                    CloseDialog?.Invoke(this, null);
                  }
              });
        }

        #endregion

        private async Task<LoginCallbackModel> HandelLoginResult(int code, string message, LoginResultModel result)
        {
            if (code == 0)
            {
                if (result.status == 0)
                {
                    await account.SaveLogin(result.token_info.access_token, result.token_info.refresh_token, result.token_info.expires_in, result.token_info.mid, result.sso);
                    return new LoginCallbackModel()
                    {
                        status = LoginStatus.Success,
                        message = "登录成功"
                    };
                }
                if (result.status == 1 || result.status == 2)
                {
                    return new LoginCallbackModel()
                    {
                        status = LoginStatus.NeedValidate,
                        message = "本次登录需要安全验证",
                        url = result.url
                    };
                }
                return new LoginCallbackModel()
                {
                    status = LoginStatus.Fail,
                    message = result.message
                };
            }
            else if (code == -105)
            {
                return new LoginCallbackModel()
                {
                    status = LoginStatus.NeedCaptcha,
                    url = result.url,
                    message = "登录需要验证码"
                };
            }
            else
            {
                return new LoginCallbackModel()
                {
                    status = LoginStatus.Fail,
                    message = message
                };
            }

        }
        private void HnadelResult(LoginCallbackModel result)
        {
            switch (result.status)
            {
                case LoginStatus.Success:
                    CloseDialog?.Invoke(this, null);
                    break;
                case LoginStatus.Fail:
                case LoginStatus.Error:
                    PrimaryButtonEnable = true;
                    Utils.ShowMessageToast(result.message);
                    break;
                case LoginStatus.NeedCaptcha:
                    var uri = new Uri(result.url);
                    SetWebViewVisibility?.Invoke(this, true);
                    //验证码重定向
                    //源码:https://github.com/xiaoyaocz/some_web
                    OpenWebView?.Invoke(this, new Uri("https://l78z.nsapps.cn/bili_gt.html" + uri.Query + "&app=uwp"));
                    break;
                case LoginStatus.NeedValidate:
                    SetWebViewVisibility?.Invoke(this, true);
                    OpenWebView?.Invoke(this, new Uri(result.url));
                    break;
                default:
                    break;
            }
        }

    }
    public class CountryItemModel
    {
        public int id { get; set; }
        public string country_code { get; set; }
        public string countryCode { get { return country_code; } }
        public string cname { get; set; }
    }
    public class SMSResultModel
    {
        public bool is_new { get; set; }
        public string captcha_key { get; set; }

        public string recaptcha_url { get; set; }
    }

    public class LoginResultModel
    {
        public int status { get; set; }
        public string message { get; set; }
        public List<string> sso { get; set; }
        public string url { get; set; }
        public LoginResultTokenModel token_info { get; set; }
        public LoginResultCookieModel cookie_info { get; set; }
    }
    public class LoginResultTokenModel
    {
        public long mid { get; set; }
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
    }
    public class LoginResultCookieModel
    {
        public List<LoginResultCookieItemModel> cookies { get; set; }
        public List<string> domains { get; set; }
    }
    public class LoginResultCookieItemModel
    {
        public string name { get; set; }
        public string value { get; set; }
        public int http_only { get; set; }
        public int expires { get; set; }
    }
}