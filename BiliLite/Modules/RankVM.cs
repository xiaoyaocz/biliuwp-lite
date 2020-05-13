using BiliLite.Helpers;
using BiliLite.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.Modules
{
    public class RankVM:IModules
    {
        readonly Api.RankAPI rankAPI;
        public RankVM()
        {
            rankAPI = new Api.RankAPI();
        }
        private bool _loading = true;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }
        private RankRegionModel _current;

        public RankRegionModel Current
        {
            get { return _current; }
            set { _current = value; DoPropertyChanged("Current"); }
        }
        private List<RankRegionModel> _RegionItems;
        public List<RankRegionModel> RegionItems
        {
            get { return _RegionItems; }
            set { _RegionItems = value; DoPropertyChanged("RegionItems"); }
        }
        public async Task LoadRankRegion(int rid=0)
        {
            try
            {
                Loading = true;
                var results = await rankAPI.RankRegion().Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<List<RankRegionModel>>>();
                    if (data.success)
                    {
                        RegionItems = data.data;
                        Current = RegionItems.FirstOrDefault(x => x.rid == rid);
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
        public async Task LoadRankDetail(RankRegionModel region , int type,int day=3)
        {
            try
            {
                Loading = true;
                var results = await rankAPI.Rank(region.rid, type,day).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        var result =await Utils.DeserializeJson<List<RankItemModel>>(data.data["list"].ToString());
                        int i = 1;
                        result = result.Take(36).ToList();
                        foreach (var item in result)
                        {
                            item.rank = i;
                            i++;
                        }
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
    public class RankRegionModel : IModules
    {
        public string name { get; set; }
        public int rid { get; set; }

        private List<RankItemModel> _Items;
        public List<RankItemModel> Items
        {
            get { return _Items; }
            set { _Items = value; DoPropertyChanged("Items"); }
        }
    }
    public class RankItemModel
    {
        public int rank { get; set; }
        public string aid { get; set; }
        public string author { get; set; }
        public string mid { get; set; }
        public int coins { get; set; }
        public int pts { get; set; }
        public string duration { get; set; }
        public string pic { get; set; }
        public string title { get; set; }
        public int video_review { get; set; }
        public int play { get; set; }
    }



}
