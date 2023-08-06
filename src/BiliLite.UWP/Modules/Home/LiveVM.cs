using BiliLite.Models;
using BiliLite.Models.Requests.Api.Home;
using BiliLite.Modules.Live.LiveCenter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;
using Windows.UI.Xaml.Media.Imaging;
using BiliLite.Extensions;

namespace BiliLite.Modules
{
    public class LiveVM : IModules
    {
        readonly LiveAPI liveAPI;
        public readonly LiveAttentionVM liveAttentionVM;
        public LiveVM()
        {
            liveAPI = new LiveAPI();
            liveAttentionVM = new LiveAttentionVM();
        }
        private bool _showFollows = false;
        public bool ShowFollows
        {
            get { return _showFollows; }
            set { _showFollows = value; DoPropertyChanged("ShowFollows"); }
        }
        private bool _loading = true;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }
        private bool _loadingFollow = true;
        public bool LoadingFollow
        {
            get { return _loadingFollow; }
            set { _loadingFollow = value; DoPropertyChanged("LoadingFollow"); }
        }

        private ObservableCollection<LiveHomeBannerModel> _banners;

        public ObservableCollection<LiveHomeBannerModel> Banners
        {
            get { return _banners; }
            set { _banners = value; DoPropertyChanged("Banners"); }
        }

        private ObservableCollection<LiveHomeAreaModel> _areas;

        public ObservableCollection<LiveHomeAreaModel> Areas
        {
            get { return _areas; }
            set { _areas = value; DoPropertyChanged("Areas"); }
        }

        //private ObservableCollection<LiveFollowAnchorModel> _Follow;

        //public ObservableCollection<LiveFollowAnchorModel> Follow
        //{
        //    get { return _Follow; }
        //    set { _Follow = value; DoPropertyChanged("Follow"); }
        //}

        private List<LiveHomeItemsModel> _items;
        public List<LiveHomeItemsModel> Items
        {
            get { return _items; }
            set { _items = value; DoPropertyChanged("Items"); }
        }

        public async Task GetLiveHome()
        {
            try
            {
                Loading = true;
                var api = liveAPI.LiveHome();

                var results = await api.Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        if (data.data["banner"].Count() > 0)
                        {
                            Banners = await data.data["banner"][0]["list"].ToString().DeserializeJson<ObservableCollection<LiveHomeBannerModel>>();
                        }
                        Areas = await data.data["area_entrance_v2"][0]["list"].ToString().DeserializeJson<ObservableCollection<LiveHomeAreaModel>>();
                        await GetLiveHomeItems();
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
                var handel = HandelError<LiveVM>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }
        public async Task GetLiveHomeItems()
        {
            try
            {
                Loading = true;
                var api = liveAPI.LiveHomeItems();
                var results = await api.Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        var items = await data.data["room_list"].ToString().DeserializeJson<List<LiveHomeItemsModel>>();

                        Items = items.Where(x => x.list != null && x.list.Count > 0).ToList();
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
                var handel = HandelError<LiveVM>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }



    }
    public class LiveHomeItemsModel
    {
        public LiveHomeItemsModuleInfoModel module_info { get; set; }
        public List<LiveHomeItemsItemModel> list { get; set; }
    }
    public class LiveHomeItemsModuleInfoModel
    {
        public int id { get; set; }
        public string link { get; set; }
        public string pic { get; set; }
        public string title { get; set; }
        public int type { get; set; }
        public int sort { get; set; }
    }
    public class LiveHomeItemsItemModel
    {
        public int area_v2_id { get; set; }
        public int area_v2_parent_id { get; set; }
        public string area_v2_name { get; set; }
        public string area_v2_parent_name { get; set; }
        public string title { get; set; }
        public string cover { get; set; }

        public int online { get; set; }
        public string roomid { get; set; }
        public string uname { get; set; }
        public string face { get; set; }
        public string uid { get; set; }

        public JObject pendant_Info { get; set; }
        public LivePendentItemModel pendent
        {
            get
            {
                if (pendant_Info.ContainsKey("2"))
                {
                    return JsonConvert.DeserializeObject<LivePendentItemModel>(pendant_Info["2"].ToString());
                }
                else
                {
                    return null;
                }
            }
        }
        public bool show_pendent
        {
            get
            {
                return pendent != null;
            }
        }
    }
    public class LiveHomeBannerModel
    {
        public int id { get; set; }
        public string link { get; set; }
        public string pic { get; set; }
        public string title { get; set; }
    }
    public class LiveHomeAreaModel
    {
        public int id { get; set; }
        public int area_v2_id { get; set; }
        public int area_v2_parent_id { get; set; }
        public int tag_type { get; set; }
        public string title { get; set; }
        public string pic { get; set; }
        public string link { get; set; }
    }




    public class LivePendentItemModel
    {
        public string bg_pic { get; set; }
        public string bg_color { get; set; }
        public string text { get; set; }
        public string name { get; set; }
        public string @type { get; set; }
    }

}
