using BiliLite.Api;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace BiliLite.Helpers
{
    public static class AppHelper
    {
        
        /// <summary>
        /// 透明图片
        /// </summary>
        public const string TRANSPARENT_IMAGE = "ms-appx:///Assets/MiniIcon/transparent.png";
        public const string VERIFY_PERSONAL_IMAGE = "ms-appx:///Assets/Icon/verify0.png";
        public const string VERIFY_OGANIZATION_IMAGE = "ms-appx:///Assets/Icon/verify1.png";
        public const string BACKGROUND_IAMGE_URL = "ms-appx:///Assets/Image/background.jpg";
        public static List<Modules.Home.RegionItem> Regions { get; set; }
        private static RegionAPI regionAPI=new RegionAPI();
        public async static Task<List<Modules.Home.RegionItem>> GetDefaultRegions()
        {
            try
            {
                var str = await FileIO.ReadTextAsync(await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Text/regions.json")));
                return JsonConvert.DeserializeObject<List<Modules.Home.RegionItem>>(str);
            }
            catch (Exception ex)
            {
                LogHelper.Log("读取默认分区失败！"+ex.Message, LogType.ERROR, ex);
                return new List<Modules.Home.RegionItem>();
            }
            
        }
        public static async Task SetRegions()
        {
            try
            {
                var results = await regionAPI.Regions().Request();
                if (results.status)
                {
                    var data = results.GetJObject();
                    if (data["code"].ToInt32() == 0)
                    {
                        var ls = JsonConvert.DeserializeObject<List<Modules.Home.RegionItem>>(data["data"].ToString().Replace("goto", "_goto"));
                        foreach (var item in ls.Where(x => string.IsNullOrEmpty(x.uri) || x.name == "会员购" || x.name == "漫画" || x.name == "游戏中心" || x.name == "话题中心" || x.name == "音频" || x.name == "原创排行榜").ToList())
                        {
                            ls.Remove(item);
                        }
                        Regions = ls;
                    }
                    else
                    {
                        //var str =await FileIO.ReadTextAsync(await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Text/regions.json")));
                        Regions = await AppHelper.GetDefaultRegions();
                    }
                }
                else
                {
                    Regions = await AppHelper.GetDefaultRegions();

                }
            }
            catch (Exception ex)
            {
                Regions = await AppHelper.GetDefaultRegions();
                LogHelper.Log("读取分区失败" + ex.Message, LogType.ERROR, ex);
            }
        }

        public  static async Task LaunchConverter(string title,List<string> inputFiles, string outFile, List<string> subtitle,bool isDash)
        {
            ApplicationData.Current.LocalSettings.Values["VideoConverterInfo"] =JsonConvert.SerializeObject( new {
                title= title,
                inputFiles= inputFiles,
                outFile = outFile,
                subtitle= subtitle ,
                isDash=isDash
            });
            await Windows.ApplicationModel.FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
        }

    }
}
