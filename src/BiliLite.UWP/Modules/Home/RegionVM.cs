using BiliLite.Api.Home;
using BiliLite.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using BiliLite.Api;
using Windows.Storage;

namespace BiliLite.Modules.Home
{
    public class RegionVM : IModules
    {
        readonly RegionAPI regionAPI;
        public RegionVM()
        {
            regionAPI = new RegionAPI();
        }
        private bool _loading = true;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }
        private List<RegionItem> _Regions;
        public List<RegionItem> Regions
        {
            get { return _Regions; }
            set { _Regions = value; DoPropertyChanged("Regions"); }
        }
        public async Task GetRegions()
        {
            try
            {
                Loading = true;
                if (AppHelper.Regions == null || AppHelper.Regions.Count == 0)
                {
                    await AppHelper.SetRegions();
                }
                Regions = AppHelper.Regions;
            }
            catch (Exception ex)
            {
                Regions = await AppHelper.GetDefaultRegions();
                var handel = HandelError<AnimeHomeModel>(ex);
                Utils.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }

        
    }

    public class RegionItem
    {
        public int tid { get; set; }
        public int reid { get; set; }
        public string name { get; set; }
        public string logo { get; set; }
        public string uri { get; set; }
        public int type { get; set; }
        public int is_bangumi { get; set; }
        public string _goto { get; set; }
        public List<RegionChildrenItem> children { get; set; }
    }
    public class RegionChildrenItem
    {
        public int tid { get; set; }
        public int reid { get; set; }
        public string name { get; set; }
        public string logo { get; set; }
        public int type { get; set; }

        public string _goto { get; set; }
        public string param { get; set; }
    }
}
