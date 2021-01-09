using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json.Linq;
using Windows.UI.Xaml.Media.Imaging;
using System.Text.RegularExpressions;
using BiliLite.Helpers;
using BiliLite.Modules;
using BiliLite.Models;
using System.Threading.Tasks;
using System.Timers;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BiliLite.Dialogs
{
    public sealed partial class LoginDialog : ContentDialog
    {
        Account account;
        public LoginDialog()
        {
            this.InitializeComponent();
            account = new Account();
            _biliapp.CloseBrowserEvent += _biliapp_CloseBrowserEvent;
            _biliapp.ValidateLoginEvent += _biliapp_ValidateLoginEvent;
            _secure.CloseCaptchaEvent += _biliapp_CloseCaptchaEvent;
            _secure.CaptchaEvent += _biliapp_CaptchaEvent;
        }

        private void _biliapp_CaptchaEvent(object sender, string e)
        {
            //throw new NotImplementedException();
        }

        private void _biliapp_CloseCaptchaEvent(object sender, string e)
        {
            //throw new NotImplementedException();
        }

        JSBridge.biliapp _biliapp = new JSBridge.biliapp();
        JSBridge.secure _secure = new JSBridge.secure();
        private void _biliapp_CloseBrowserEvent(object sender, string e)
        {
            this.Hide();
        }

        private async void _biliapp_ValidateLoginEvent(object sender, string e)
        {
            try
            {
                JObject jObject = JObject.Parse(e);
                if (jObject["access_token"] != null)
                {
                    var m = await account.SaveLogin(jObject["access_token"].ToString(), jObject["refresh_token"].ToString(), jObject["expires_in"].ToInt32(), Convert.ToInt64(jObject["mid"].ToString()));
                    
                    if (m)
                    {
                        this.Hide();
                        Utils.ShowMessageToast("登录成功");
                    }
                    else
                    {
                        Title = "登录";
                        IsPrimaryButtonEnabled = true;
                        webView.Visibility = Visibility.Collapsed;
                        Utils.ShowMessageToast("登录失败,请重试");
                    }
                    //await UserManage.LoginSucess(jObject["access_token"].ToString());
                }
                else
                {
                    Title = "登录";
                    IsPrimaryButtonEnabled = true;
                    webView.Visibility = Visibility.Collapsed;
                    Utils.ShowMessageToast("登录失败,请重试");
                }

            }
            catch (Exception ex)
            {
                LogHelper.Log("登录二次验证失败", LogType.ERROR, ex);
            }

        }
        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
            if (txt_Username.Text.Length == 0)
            {
                txt_Username.Focus(FocusState.Pointer);
                Utils.ShowMessageToast("请输入用户名");
                return;
            }
            if (txt_Password.Password.Length == 0)
            {
                txt_Password.Focus(FocusState.Pointer);
                Utils.ShowMessageToast("请输入密码");
                return;
            }
            if (chatcha.Visibility == Visibility.Visible && txt_captcha.Text.Length == 0)
            {
                txt_Password.Focus(FocusState.Pointer);
                Utils.ShowMessageToast("请输入验证码");
                return;
            }
            IsPrimaryButtonEnabled = false;
            //var results = await account.LoginV3(txt_Username.Text, txt_Password.Password);
            var results = await account.LoginV3(txt_Username.Text, txt_Password.Password);
            switch (results.status)
            {
                case LoginStatus.Success:
                    this.Hide();
                    break;
                case LoginStatus.Fail:
                case LoginStatus.Error:
                    IsPrimaryButtonEnabled = true;
                    break;
                case LoginStatus.NeedCaptcha:
                    //V2
                    //chatcha.Visibility = Visibility.Visible;
                    //IsPrimaryButtonEnabled = true;
                    //GetCaptcha();
                    Utils.ShowMessageToast("登录需要验证码，请扫描二维码登录");
                    break;
                case LoginStatus.NeedValidate:
                    Title = "安全验证";
                    webView.Visibility = Visibility.Visible;
                    webView.Source = new Uri(results.url.Replace("&ticket=1", ""));
                    break;
                default:
                    break;
            }
            Utils.ShowMessageToast(results.message);
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void txt_Password_GotFocus(object sender, RoutedEventArgs e)
        {
            hide.Visibility = Visibility.Visible;
        }
        private void txt_Password_LostFocus(object sender, RoutedEventArgs e)
        {
            hide.Visibility = Visibility.Collapsed;
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            GetCaptcha();
        }
        private async void GetCaptcha()
        {
            try
            {
                var steam = await HttpHelper.GetStream(account.accountApi.Captcha().url);
                var img = new BitmapImage();
                await img.SetSourceAsync(steam.AsRandomAccessStream());
                img_Captcha.Source = img;
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("无法加载验证码");
            }


        }

        private async void webView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if (args.Uri.AbsoluteUri.Contains("access_key="))
            {
                var access = Regex.Match(args.Uri.AbsoluteUri, "access_key=(.*?)&").Groups[1].Value;
                var mid = Regex.Match(args.Uri.AbsoluteUri, "mid=(.*?)&").Groups[1].Value;
                await account.SaveLogin(access,"",0, long.Parse(mid));
                this.Hide();
                return;
            }
            try
            {
                this.webView.AddWebAllowedObject("biliapp", _biliapp);
                this.webView.AddWebAllowedObject("secure", _secure);
            }
            catch (Exception ex)
            {
                LogHelper.Log("注入JS对象失败", LogType.ERROR, ex);
            }

        }

        private void webView_ScriptNotify(object sender, NotifyEventArgs e)
        {

        }

        private void BtnWebLogin_Click(object sender, RoutedEventArgs e)
        {
            Title = "网页登录";
            webView.Visibility = Visibility.Visible;
            webView.Source = new Uri("https://passport.bilibili.com/ajax/miniLogin/minilogin");
            IsPrimaryButtonEnabled = false;
            webView.Width = 440;
            webView.Height = 480;
        }

        private async void WebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (args.Uri.AbsoluteUri == "https://passport.bilibili.com/ajax/miniLogin/redirect"||args.Uri.AbsoluteUri== "https://www.bilibili.com/")
            {
                var results = await HttpHelper.GetString("https://passport.bilibili.com/login/app/third?appkey=27eb53fc9058f8c3&api=http%3A%2F%2Flink.acg.tv%2Fforum.php&sign=67ec798004373253d60114caaad89a8c");
                var obj = JObject.Parse(results);
                if (obj["code"].ToInt32() == 0)
                {
                    webView.Navigate(new Uri(obj["data"]["confirm_uri"].ToString()));
                }
                else
                {
                    Utils.ShowMessageToast("登录失败，请重试");
                }
                return;
            }
        }

        private async void btnQRLogin_Click(object sender, RoutedEventArgs e)
        {
            pwdLogin.Visibility = Visibility.Collapsed;
            qrLogin.Visibility = Visibility.Visible;
            await GetQRAuthInfo();
        }
        bool qr_loading = false;
        QRAuthInfo authInfo;
        Timer timer;
        private async Task GetQRAuthInfo()
        {
            try
            {
                qr_loading = true;
                if (timer != null)
                {
                    timer.Stop();
                    timer.Dispose();
                }
                var result = await account.GetQRAuthInfo();
                if (result.success)
                {
                    authInfo = result.data;
                    ZXing.BarcodeWriter barcodeWriter = new ZXing.BarcodeWriter();
                    barcodeWriter.Format = ZXing.BarcodeFormat.QR_CODE;
                    barcodeWriter.Options = new ZXing.Common.EncodingOptions()
                    {
                        Margin = 1,
                        Height = 200,
                        Width = 200
                    };
                    var img = barcodeWriter.Write(authInfo.url);
                    imgQR.Source = img;
                    timer = new Timer();
                    timer.Interval = 3000;
                    timer.Elapsed += Timer_Elapsed;
                    timer.Start();
                }
                else
                {
                    Utils.ShowMessageToast(result.message);
                }
                qr_loading = false;
            }
            catch (Exception ex)
            {
                LogHelper.Log("读取和加载登录二维码失败", LogType.ERROR, ex);
                Utils.ShowMessageToast("加载二维码失败");
            }

        }

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {

            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
               async () =>
               {
                   var result = await account.PollQRAuthInfo(authInfo.auth_code);
                   if (result.status == LoginStatus.Success)
                   {
                       timer.Stop();
                       this.Hide();
                   }
               });

        }

        private void btnPasswordLogin_Click(object sender, RoutedEventArgs e)
        {
            pwdLogin.Visibility = Visibility.Visible;
            qrLogin.Visibility = Visibility.Collapsed;
        }

        private async void btnRefreshQR_Click(object sender, RoutedEventArgs e)
        {
            if (qr_loading)
                return;
            await GetQRAuthInfo();
        }


    }
}
