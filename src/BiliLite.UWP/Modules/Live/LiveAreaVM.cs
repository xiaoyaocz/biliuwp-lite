using BiliLite.Models.Requests.Api.Live;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BiliLite.Extensions;

namespace BiliLite.Modules.Live
{
    public class LiveAreaVM : IModules
    {
        readonly LiveAreaAPI liveAreaAPI;
        public LiveAreaVM()
        {
            liveAreaAPI = new LiveAreaAPI();
        }
        private bool _loading = false;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }
        private List<LiveAreaModel> _items;

        public List<LiveAreaModel> Items
        {
            get { return _items; }
            set { _items = value; DoPropertyChanged("Items"); }
        }


        public async Task GetItems()
        {
            try
            {
                Loading = true;
                var results = await liveAreaAPI.LiveAreaList().Request();
                if (results.status)
                {
                    var data = await results.GetData<List<LiveAreaModel>>();
                    if (data.success)
                    {
                        Items = data.data;
                    }
                    else
                    {
                        Notify.ShowMessageToast(data.message);
                    }
                }
                else
                {
                    Notify.ShowMessageToast(results.message);

                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<LiveAreaVM>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }


    }
    public class LiveAreaModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public List<LiveAreaItemModel> list { get; set; }
    }
    public class LiveAreaItemModel
    {
        public int id { get; set; }
        public int parent_id { get; set; }
        public string pic { get; set; }
        public string parent_name { get; set; }
        public string name { get; set; }
    }

}
