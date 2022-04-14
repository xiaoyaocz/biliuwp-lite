using BiliLite.Api.Live;
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
using System.Windows.Input;
using Windows.ApplicationModel.Appointments;
using Windows.UI.Xaml.Media.Imaging;


namespace BiliLite.Modules.Live.LiveCenter
{
    public class LiveCenterVM:IModules
    {
        readonly LiveCenterAPI liveCenterAPI;
        public LiveCenterVM()
        {
            liveCenterAPI = new LiveCenterAPI();
            SignCommand = new RelayCommand(DoSign);
            TitleCommand = new RelayCommand(OpenTitle);
        }
        private SignInfoModel _SignInfo;

        public SignInfoModel SignInfo
        {
            get { return _SignInfo; }
            set { _SignInfo = value; DoPropertyChanged("SignInfo"); }
        }
        public ICommand SignCommand { get; private set; }
        public ICommand TitleCommand { get; private set; }
        public async void GetUserInfo()
        {
            try
            {
                var result = await liveCenterAPI.SignInfo().Request();

                if (result.status)
                {
                    var data = await result.GetData<SignInfoModel>();
                    if (data.success)
                    {

                        SignInfo = data.data;
                    }
                    else
                    {
                        Utils.ShowMessageToast(data.message);
                    }

                }
                else
                {
                    Utils.ShowMessageToast(result.message);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log("读取签到信息失败", LogType.ERROR, ex);
                Utils.ShowMessageToast("读取签到信息失败");
            }
        }
       
        public async void DoSign()
        {
            if (!SettingHelper.Account.Logined && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请先登录后再操作");
                return;
            }

            try
            {
                var results = await liveCenterAPI.DoSign().Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        SignInfo.is_signed =true;
                        Utils.ShowMessageToast(data.data["text"].ToString());
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
                var handel = HandelError<object>(ex);
                Utils.ShowMessageToast(handel.message);
            }



        }

        public void OpenTitle()
        {
            MessageCenter.NavigateToPage(this,new NavigationInfo() { 
                icon= Windows.UI.Xaml.Controls.Symbol.World,
                title="佩戴中心",
                page=typeof(Pages.WebPage),
                parameters= "https://link.bilibili.com/p/center/index#/user-center/wearing-center/my-medal"
            });
        }

    }
    public class SignInfoModel:IModules
    {
        private bool _is_signed;

        public bool is_signed
        {
            get { return _is_signed; }
            set { _is_signed = value; DoPropertyChanged("is_signed"); }
        }

        public int days { get; set; }
        public int sign_days { get; set; }
        public string h5_url { get; set; }
        public List<SignInfoAwardModel> days_award { get; set; }
        public List<SignInfoAwardModel> awards { get; set; }
    }
    public class SignInfoAwardModel
    {
       
        public int count { get; set; }
        public string award { get; set; }
        public string text { get; set; }
        public SignInfoAwardImageModel img { get; set; }
    }
    public class SignInfoAwardImageModel
    {

        public int width { get; set; }
        public string src { get; set; }
        public int height { get; set; }
    }
}
