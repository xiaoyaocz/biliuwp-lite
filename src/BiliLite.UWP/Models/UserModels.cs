using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.Models
{

    public enum LoginStatus
    {
        /// <summary>
        /// 登录成功
        /// </summary>
        Success,
        /// <summary>
        /// 登录失败
        /// </summary>
        Fail,
        /// <summary>
        /// 登录错误
        /// </summary>
        Error,
        /// <summary>
        /// 登录需要验证码
        /// </summary>
        NeedCaptcha,
        /// <summary>
        /// 需要安全认证
        /// </summary>
        NeedValidate
    }
    /// <summary>
    /// V2的登录
    /// </summary>
    public class LoginV2Model
    {

        public long ts { get; set; }
        public int code { get; set; }
        public LoginTokenInfo data { get; set; }
        /// <summary>
        /// 当错误代码为2100会返回一个链接验证
        /// </summary>
        public string url { get; set; }
        public string message { get; set; }
    }

    public class LoginDataV3Model
    {
        public int status { get; set; }
        public LoginTokenInfo token_info { get; set; }
        public LoginCookieInfo cookie_info { get; set; }
        public string url { get; set; }
        public List<string> sso { get; set; }
    }
   
    public class LoginTokenInfo
    {

        public long mid { get; set; }

        public string access_token { get; set; }

        public string refresh_token { get; set; }

        public int expires_in { get; set; }

        public DateTime expires_datetime { get; set; }
    }
    public class LoginCookieInfo
    {
        public List<LoginCookieInfoItem> cookies { get; set; }
        public List<string> domains { get; set; }
    }
    public class LoginCookieInfoItem
    {
        public string name { get; set; }
        public string value { get; set; }
        public int http_only { get; set; }
        public int expires { get; set; }
        public int secure { get; set; }
    }
    public class LoginCallbackModel
    {
        public LoginStatus status { get; set; }
        public string message { get; set; }
        public string url { get; set; }
    }
    public class MyProfileModel
    {
        /// <summary>
        /// Mid
        /// </summary>
        public long mid { get; set; }
        /// <summary>
        /// xiaoyaocz
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 死宅，半个程序猿.....
        /// </summary>
        public string sign { get; set; }
        /// <summary>
        /// Coins
        /// </summary>
        public string coins { get; set; }
        /// <summary>
        /// 1997-09-21
        /// </summary>
        public DateTime birthday { get; set; }
        private string _face;

        public string face
        {
            get
            {
                if (!_face.Contains("@100w.jpg"))
                {
                    return _face + "@100w.jpg";
                }
                else
                {
                    return _face;
                }
            }
            set { _face = value; }
        }

        /// <summary>
        /// Sex
        /// </summary>
        public int sex { get; set; }
        /// <summary>
        /// Level
        /// </summary>
        public int level { get; set; }
        /// <summary>
        /// Rank
        /// </summary>
        public int rank { get; set; }
        /// <summary>
        /// Silence
        /// </summary>
        public int silence { get; set; }
        /// <summary>
        /// Vip
        /// </summary>
        public Vip vip { get; set; }
        /// <summary>
        /// Email_status
        /// </summary>
        public int email_status { get; set; }
        /// <summary>
        /// Tel_status
        /// </summary>
        public int tel_status { get; set; }
        /// <summary>
        /// Official
        /// </summary>
        public Official official { get; set; }


        public string Sex
        {
            get
            {
                switch (sex)
                {
                    case 0:
                        return "保密";
                    case 1:
                        return "男";
                    case 2:
                        return "女";
                    default:
                        return "保密";
                }
            }
        }
    }

    public class Vip
    {
        /// <summary>
        /// Type
        /// </summary>
        public int type { get; set; }
        /// <summary>
        /// Status
        /// </summary>
        public int status { get; set; }
        /// <summary>
        /// Due_date
        /// </summary>
        public string due_date { get; set; }
    }

    public class Official
    {
        /// <summary>
        /// Role
        /// </summary>
        public int role { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string desc { get; set; }
    }
}
