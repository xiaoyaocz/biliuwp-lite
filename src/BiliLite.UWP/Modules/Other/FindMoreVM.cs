using BiliLite.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.Modules.Other
{
    public class FindMoreVM : IModules
    {
        readonly Api.GitApi gitApi;
        public FindMoreVM()
        {
            gitApi = new Api.GitApi();
        }
        private bool _loading = true;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }
        private List<FindMoreEntranceModel> _Items;
        public List<FindMoreEntranceModel> Items
        {
            get { return _Items; }
            set { _Items = value; DoPropertyChanged("Items"); }
        }
        public async void LoadEntrance()
        {
            try
            {
                Loading = true;
                var results = await gitApi.FindMoreEntrance().Request();
                if (results.status)
                {
                    var data = await results.GetJson<List<FindMoreEntranceModel>>();
                    await Task.Delay(2000);
                    Items=data;
                }
                else
                {
                    Utils.ShowMessageToast(results.message);

                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<List<RankRegionModel>>(ex);
                Utils.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }

    }
    public class FindMoreEntranceModel
    {
        public string name { get; set; }
        public string desc { get; set; }
        public int type { get; set; }
        public string link { get; set; }
        public string pic { get; set; }
    }
}
