using BiliLite.Api;
using BiliLite.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
            LoadShieldSetting();
        }
        /// <summary>
        /// 弹幕屏蔽关键词列表
        /// </summary>
        public ObservableCollection<string> ShieldWords { get; set; }
        public ObservableCollection<string> LiveWords { get; set; }
        public ObservableCollection<string> ShieldUsers { get; set; }
        public ObservableCollection<string> ShieldRegulars { get; set; }

        public List<AppThemeColor> ThemeColors { get; set; }

        public void LoadShieldSetting()
        {
            LiveWords= SettingHelper.GetValue<ObservableCollection<string>>(SettingHelper.Live.SHIELD_WORD, new ObservableCollection<string>() { });
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
                var result = await playerAPI.GetDanmuFilterWords().Request();
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

    }
    public class DanmuFilterItem
    {
        public int id { get; set; }
        public int mid { get; set; }
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
}
