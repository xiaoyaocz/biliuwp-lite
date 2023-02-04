using BiliLite.Api;
using BiliLite.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http;

namespace BiliLite.Modules
{
    public class SettingVM : IModules
    {
        PlayerAPI playerAPI;
        public SettingVM()
        {
            playerAPI = new PlayerAPI();
            ThemeColors = new List<AppThemeColor>()
            {
                new AppThemeColor()
                {
                    use_system_color=true,
                    name="跟随系统"
                },
                new AppThemeColor()
                {
                    color="#FFDF85A0",
                    name="少女粉",
                    theme="Pink",
                },
                new AppThemeColor()
                {
                    color="#FFF70D0D",
                    name="姨妈红",
                    theme="Red",
                },
                new AppThemeColor()
                {
                    color="#FFF9EF23",
                    name="咸蛋黄",
                    theme="Yellow",
                },
                new AppThemeColor()
                {
                    color="#FF71F923",
                    name="早苗绿",
                    theme="Green",
                },
                new AppThemeColor()
                {
                    color="#FF18BDFB",
                    name="胖次蓝",
                    theme="Blue",
                },
                new AppThemeColor()
                {
                    color="#FFB92CBF",
                    name="基佬紫",
                    theme="Purple",
                }
            };
            CDNServers = new List<CDNServerItem>()
            {
                new CDNServerItem("upos-sz-mirrorhwo1.bilivideo.com","华为云"),
                new CDNServerItem("upos-sz-mirrorcos.bilivideo.com","腾讯云"),
                new CDNServerItem("upos-sz-mirrorali.bilivideo.com","阿里云"),
                new CDNServerItem("upos-sz-mirrorhw.bilivideo.com","华为云"),
                new CDNServerItem("upos-sz-mirrorks3.bilivideo.com","金山云"),
                new CDNServerItem("upos-tf-all-js.bilivideo.com","JS"),
                new CDNServerItem("cn-hk-eq-bcache-01.bilivideo.com","香港"),
                new CDNServerItem("cn-hk-eq-bcache-16.bilivideo.com","香港"),
                new CDNServerItem("upos-hz-mirrorakam.akamaized.net","Akamaized"),
            };
            LoadShieldSetting();
        }

        /// <summary>
        /// 弹幕屏蔽关键词列表
        /// </summary>
        public ObservableCollection<string> ShieldWords { get; set; }
        public ObservableCollection<string> LiveWords { get; set; }
        public ObservableCollection<string> ShieldUsers { get; set; }
        public ObservableCollection<string> ShieldRegulars { get; set; }
        public List<CDNServerItem> CDNServers { get; set; }
        public List<AppThemeColor> ThemeColors { get; set; }

        public void LoadShieldSetting()
        {
            LiveWords = SettingHelper.GetValue<ObservableCollection<string>>(SettingHelper.Live.SHIELD_WORD, new ObservableCollection<string>() { });
            ShieldWords = SettingHelper.GetValue<ObservableCollection<string>>(SettingHelper.VideoDanmaku.SHIELD_WORD, new ObservableCollection<string>() { });

            //正则关键词
            ShieldRegulars = SettingHelper.GetValue<ObservableCollection<string>>(SettingHelper.VideoDanmaku.SHIELD_REGULAR, new ObservableCollection<string>() { });

            //用户
            ShieldUsers = SettingHelper.GetValue<ObservableCollection<string>>(SettingHelper.VideoDanmaku.SHIELD_USER, new ObservableCollection<string>() { });
        }

        public async Task SyncDanmuFilter()
        {
            try
            {
                var result = await playerAPI.GetDanmuFilterWords().Request();
                if (!result.status)
                {
                    Utils.ShowMessageToast(result.message);
                    return;
                }
                var obj = result.GetJObject();
                if (obj["code"].ToInt32() == 0)
                {
                    var items = JsonConvert.DeserializeObject<List<DanmuFilterItem>>(obj["data"]["rule"].ToString());
                    {
                        var words = items.Where(x => x.type == 0).Select(x => x.filter).ToList();
                        var ls = ShieldWords.Union(words);
                        ShieldWords.Clear();
                        foreach (var item in ls)
                        {
                            ShieldWords.Add(item);
                        }
                        SettingHelper.SetValue(SettingHelper.VideoDanmaku.SHIELD_WORD, ShieldWords);
                    }
                    {
                        var users = items.Where(x => x.type == 1).Select(x => x.filter).ToList();
                        var ls = ShieldRegulars.Union(users);
                        ShieldRegulars.Clear();
                        foreach (var item in ls)
                        {
                            ShieldRegulars.Add(item);
                        }
                        SettingHelper.SetValue(SettingHelper.VideoDanmaku.SHIELD_REGULAR, ShieldRegulars);
                    }
                    {
                        var users = items.Where(x => x.type == 2).Select(x => x.filter).ToList();
                        var ls = ShieldUsers.Union(users);
                        ShieldUsers.Clear();
                        foreach (var item in ls)
                        {
                            ShieldUsers.Add(item);
                        }
                        SettingHelper.SetValue(SettingHelper.VideoDanmaku.SHIELD_USER, ShieldUsers);
                    }
                }
                else
                {
                    Utils.ShowMessageToast(obj["message"].ToString());
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log("读取弹幕屏蔽词失败", LogType.ERROR, ex);
            }
        }

        public async Task<bool> AddDanmuFilterItem(string word, int type)
        {
            try
            {
                var result = await playerAPI.AddDanmuFilterWord(word:word,type:type).Request();
                if (!result.status)
                {
                    return false;
                }
                var obj = result.GetJObject();
                return obj["code"].ToInt32() == 0;
            }
            catch (Exception ex)
            {
                LogHelper.Log("添加弹幕屏蔽词失败", LogType.ERROR, ex);
                return false;
            }
        }
        /// <summary>
        /// CDN延迟测试
        /// </summary>
        public async void CDNServerDelayTest()
        {
            foreach (var item in CDNServers)
            {
                var time = await GetDelay(item.Server);
                item.Delay = time;
            }
        }
        private HttpClient _httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(2)
        };
        private async Task<long> GetDelay(string server)
        {
            //随便整个链接
            var testUrl = $"https://{server}/upgcxcode/76/62/729206276/729206276_nb2-1-30112.m4s";

            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                var res = await _httpClient.GetAsync(new Uri(testUrl));
                sw.Stop();
                return sw.ElapsedMilliseconds;
            }
            catch (Exception)
            {
                return -1;
            }

        }

        //傻逼微软，UWP连个Ping都不支持
        //private async Task<long> GetPing(string address)
        //{
        //    try
        //    {
        //        Ping ping = new Ping();
        //        PingOptions options = new PingOptions();
        //        options.DontFragment = true;
        //        byte[] buffer = new byte[32];
        //        for (int i = 0; i < 32; i++)
        //        {
        //            buffer[i] = 0;
        //        }
        //        int timeout = 3000;
        //        PingReply reply = await ping.SendPingAsync(address, timeout, buffer, options);
        //        if (reply.Status == IPStatus.Success)
        //        {
        //            return reply.RoundtripTime;
        //        }
        //        else
        //        {
        //            return -1;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex);
        //        return -1;
        //    }


        //}

    }
    public class DanmuFilterItem
    {
        public int id { get; set; }
        public long mid { get; set; }
        public int type { get; set; }
        public string filter { get; set; }
        public string comment { get; set; }
        public long ctime { get; set; }
        public long mtime { get; set; }

    }
    public class AppThemeColor
    {
        public bool use_system_color { get; set; } = false;
        public string color { get; set; }
        public string name { get; set; }
        public string theme { get; set; }
    }
    public class CDNServerItem : IModules
    {
        public CDNServerItem(string server, string remark)
        {
            this.Server = server;
            this.Remark = remark;
        }
        public string Server { get; set; }
        public string Remark { get; set; }

        public bool ShowDelay
        {
            get
            {
                return Delay > 0;
            }
        }
        public bool ShowTimeOut
        {
            get
            {
                return Delay < 0;
            }
        }
        private long _Delay = 0;
        public long Delay
        {
            get { return _Delay; }
            set
            {
                _Delay = value;
                DoPropertyChanged("Delay");
                DoPropertyChanged("ShowDelay");
                DoPropertyChanged("ShowTimeOut");
            }
        }

    }
}
