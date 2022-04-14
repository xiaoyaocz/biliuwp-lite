using BiliLite.Helpers;
using BiliLite.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.Modules.Season
{
    public class SeasonRankVM:IModules
    {
        readonly Api.RankAPI rankAPI;
        public SeasonRankVM()
        {
            rankAPI = new Api.RankAPI();
           
        }
        private bool _loading = true;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }
        private SeasonRankModel _current;

        public SeasonRankModel Current
        {
            get { return _current; }
            set { _current = value; DoPropertyChanged("Current"); }
        }
        private List<SeasonRankModel> _RegionItems;
        public List<SeasonRankModel> RegionItems
        {
            get { return _RegionItems; }
            set { _RegionItems = value; DoPropertyChanged("RegionItems"); }
        }

        public void LoadRankRegion(int type = 1)
        {
            RegionItems = new List<SeasonRankModel>()
            {
                new SeasonRankModel()
                {
                    name="热门番剧",
                    type=1
                },
                new SeasonRankModel()
                {
                    name="热门国创",
                    type=4
                },
                new SeasonRankModel()
                {
                    name="热门电影",
                    type=2
                },
                new SeasonRankModel()
                {
                    name="热门纪录片",
                    type=3
                },
                new SeasonRankModel()
                {
                    name="热门电视剧",
                    type=5
                },
                new SeasonRankModel()
                {
                    name="热门综艺",
                    type=7
                },
            };
            Current = RegionItems.FirstOrDefault(x=>x.type.Equals(type));
        }

        public async Task LoadRankDetail(SeasonRankModel region)
        {
            try
            {
                Loading = true;
                var results = await rankAPI.SeasonRank(region.type).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        var result = await Utils.DeserializeJson<List<SeasonRankItemModel>>(data.data["list"].ToString());
                        region.Items = result;
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
                var handel = HandelError<ApiDataModel<List<RankRegionModel>>>(ex);
                Utils.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }
    }
    public class SeasonRankModel : IModules
    {
        public string name { get; set; }
        public int type { get; set; }

        private List<SeasonRankItemModel> _Items;
        public List<SeasonRankItemModel> Items
        {
            get { return _Items; }
            set { _Items = value; DoPropertyChanged("Items"); }
        }
    }
    public class SeasonRankItemModel
    {
        public int rank { get; set; }
        public string badge { get; set; }
        public string desc { get; set; }
        public string season_id { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public string cover { get; set; }
        public int badge_type { get; set; }
        public int pts { get; set; }
        public bool show_badge { get { return !string.IsNullOrEmpty(badge); } }
        public bool show_danmaku { get { return stat!=null&&stat.danmaku!=0; } }
        public SeasonRankItemStatModel stat { get; set; }
        public SeasonRankItemNewEPModel new_ep { get; set; }
    }
    public class SeasonRankItemStatModel
    {
        public int danmaku { get; set; }
        public int follow { get; set; }
        public int view { get; set; }
    }
    public class SeasonRankItemNewEPModel
    {
        public string cover { get; set; }
        public string index_show { get; set; }
    }
}
