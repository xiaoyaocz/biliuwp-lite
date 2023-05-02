using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using BiliLite.Modules.Home;
using System.Windows.Input;
using BiliLite.Extensions;
using BiliLite.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using BiliLite.Models.Requests.Api;
using BiliLite.Services;

namespace BiliLite.Modules
{
    public class RegionDetailVM : IModules
    {
        public RegionDetailVM()
        {

        }
        private ObservableCollection<IRegionVM> _Regions;
        public ObservableCollection<IRegionVM> Regions
        {
            get { return _Regions; }
            set { _Regions = value; DoPropertyChanged("Regions"); }
        }
        private IRegionVM _SelectRegion;
        public IRegionVM SelectRegion
        {
            get { return _SelectRegion; }
            set { _SelectRegion = value; DoPropertyChanged("SelectRegion"); }
        }

        public void InitRegion(int id, int tid)
        {
            var ls = new ObservableCollection<IRegionVM>();
            var region = AppHelper.Regions.FirstOrDefault(x => x.tid == id);
            ls.Add(new RegionDetailHomeVM(region));
            Regions = ls;
            foreach (var item in region.children)
            {
                ls.Add(new RegionDetailChildVM(item));
            }
            if (tid == 0)
            {
                SelectRegion = Regions[0];
            }
            else
            {
                SelectRegion = Regions.FirstOrDefault(x => x.ID == tid);
            }
        }

    }
    public interface IRegionVM
    {
        ICommand RefreshCommand { get; set; }
        ICommand LoadMoreCommand { get; set; }
        int ID { get; set; }
        string RegionName { get; set; }
        bool Loading { get; set; }
    }
    public class RegionDetailHomeVM : IModules, IRegionVM
    {
        public int ID { get; set; }
        public string RegionName { get; set; } = "推荐";

        public ICommand RefreshCommand { get; set; }
        public ICommand LoadMoreCommand { get; set; }
        private bool _loading = false;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }

        private List<RegionHomeBannerItemModel> _Banners;

        public List<RegionHomeBannerItemModel> Banners
        {
            get { return _Banners; }
            set { _Banners = value; DoPropertyChanged("Banners"); }
        }

        private ObservableCollection<RegionVideoItemModel> _regionVideos;

        public ObservableCollection<RegionVideoItemModel> Videos
        {
            get { return _regionVideos; }
            set { _regionVideos = value; DoPropertyChanged("Videos"); }
        }


        private RegionItem _region;
        RegionAPI regionAPI;
        public RegionDetailHomeVM(RegionItem regionItem)
        {
            regionAPI = new RegionAPI();
            _region = regionItem;
            ID = regionItem.tid;
            //RegionName = regionItem.name;
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
        }
        private string next_id = "";
        public async Task LoadHome()
        {
            try
            {
                Loading = true;
                var api = regionAPI.RegionDynamic(ID);
                if (next_id != "")
                {
                    api = regionAPI.RegionDynamic(ID, next_id);
                }
                var results = await api.Request();

                if (results.status)
                {
                    var data = results.GetJObject();
                    if (data["code"].ToInt32() == 0)
                    {
                        var ls = JsonConvert.DeserializeObject<ObservableCollection<RegionVideoItemModel>>(data["data"]["new"].ToString());
                        if (next_id == "")
                        {
                            var recommend = JsonConvert.DeserializeObject<ObservableCollection<RegionVideoItemModel>>(data["data"]["recommend"]?.ToString() ?? "[]");
                            foreach (var item in recommend)
                            {
                                ls.Insert(0, item);
                            }
                            Banners = JsonConvert.DeserializeObject<List<RegionHomeBannerItemModel>>(data["data"]["banner"]["top"].ToString());
                            Videos = ls;
                        }
                        else
                        {
                            foreach (var item in ls)
                            {
                                Videos.Add(item);
                            }
                        }

                        next_id = data["data"]["cbottom"].ToString();
                    }
                    else
                    {
                        Notify.ShowMessageToast(data["message"].ToString());
                    }
                }
                else
                {
                    Notify.ShowMessageToast(results.message);

                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<ApiDataModel<List<RankRegionModel>>>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }

        public async void Refresh()
        {
            next_id = "";
            await LoadHome();
        }
        public async void LoadMore()
        {
            await LoadHome();
        }
    }
    public class RegionDetailChildVM : IModules, IRegionVM
    {
        public string RegionName { get; set; }
        public int ID { get; set; }
        private bool _loading = false;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }
        public ICommand RefreshCommand { get; set; }
        public ICommand LoadMoreCommand { get; set; }
        private RegionChildrenItem _region;
        RegionAPI regionAPI;
        public RegionDetailChildVM(RegionChildrenItem regionItem)
        {
            regionAPI = new RegionAPI();
            Orders = new List<RegionChildOrderModel>() {
                //new RegionChildOrderModel("默认排序",""),
                new RegionChildOrderModel("最新视频","senddate"),
                new RegionChildOrderModel("最多播放","view"),
                new RegionChildOrderModel("评论最多","reply"),
                new RegionChildOrderModel("弹幕最多","danmaku"),
                new RegionChildOrderModel("最多收藏","favorite")
            };
            SelectOrder = Orders[0];
            _region = regionItem;
            ID = regionItem.tid;
            RegionName = regionItem.name;
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
        }
        public List<RegionChildOrderModel> Orders { get; set; }

        private RegionChildOrderModel _SelectOrder;

        public RegionChildOrderModel SelectOrder
        {
            get { return _SelectOrder; }
            set
            {
                if (value != null)
                {
                    _SelectOrder = value;
                }
            }
        }

        private RegionTagItemModel _SelectTag;

        public RegionTagItemModel SelectTag
        {
            get { return _SelectTag; }
            set
            {
                if (value != null)
                {
                    _SelectTag = value;
                }

            }
        }


        private List<RegionTagItemModel> _tag;
        public List<RegionTagItemModel> Tasgs
        {
            get { return _tag; }
            set { _tag = value; DoPropertyChanged("Tasgs"); }
        }

        private ObservableCollection<RegionVideoItemModel> _regionVideos;
        public ObservableCollection<RegionVideoItemModel> Videos
        {
            get { return _regionVideos; }
            set { _regionVideos = value; DoPropertyChanged("Videos"); }
        }
        public string next_id = "";

        public async Task LoadHome()
        {
            try
            {
                Loading = true;
                var api = regionAPI.RegionChildDynamic(ID, (SelectTag == null) ? 0 : SelectTag.tid);
                if (next_id != "")
                {
                    api = regionAPI.RegionChildDynamic(ID, next_id, (SelectTag == null) ? 0 : SelectTag.tid);
                }

                var results = await api.Request();
                if (results.status)
                {
                    var data = results.GetJObject();
                    if (data["code"].ToInt32() == 0)
                    {
                        var ls = JsonConvert.DeserializeObject<ObservableCollection<RegionVideoItemModel>>(data["data"]["new"].ToString());
                        if (next_id == "")
                        {
                            var tags = JsonConvert.DeserializeObject<List<RegionTagItemModel>>(data["data"]["top_tag"]?.ToString() ?? "[]");
                            tags.Insert(0, new RegionTagItemModel()
                            {
                                tid = 0,
                                tname = "全部标签"
                            });
                            if (Tasgs == null || Tasgs.Count == 0)
                            {
                                Tasgs = tags;
                                SelectTag = Tasgs[0];
                            }

                            Videos = ls;
                        }
                        else
                        {
                            foreach (var item in ls)
                            {
                                Videos.Add(item);
                            }
                        }
                        next_id = data["data"]["cbottom"]?.ToString() ?? "";
                    }
                    else
                    {
                        Notify.ShowMessageToast(data["message"].ToString());
                    }
                }
                else
                {
                    Notify.ShowMessageToast(results.message);

                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<ApiDataModel<List<RankRegionModel>>>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }
        public int page = 1;
        public async Task LoadList()
        {
            try
            {
                Loading = true;
                var api = regionAPI.RegionChildList(ID, SelectOrder.order, page, SelectTag.tid);
                var results = await api.Request();
                if (results.status)
                {
                    var data = results.GetJObject();
                    if (data["code"].ToInt32() == 0)
                    {
                        var ls = JsonConvert.DeserializeObject<ObservableCollection<RegionVideoItemModel>>(data["data"].ToString());
                        if (page == 1)
                        {
                            Videos = ls;
                        }
                        else
                        {
                            foreach (var item in ls)
                            {
                                Videos.Add(item);
                            }
                        }
                        page++;
                    }
                    else
                    {
                        Notify.ShowMessageToast(data["message"].ToString());
                    }
                }
                else
                {
                    Notify.ShowMessageToast(results.message);

                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<ApiDataModel<List<RankRegionModel>>>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }

        public async void Refresh()
        {
            if (Loading)
            {
                return;
            }
            if (SelectOrder == null || SelectOrder.order == "")
            {
                next_id = "";
                await LoadHome();
            }
            else
            {
                page = 1;
                await LoadList();
            }

        }
        public async void LoadMore()
        {
            if (Loading)
            {
                return;
            }
            if (SelectOrder == null || SelectOrder.order == "")
            {
                await LoadHome();
            }
            else
            {
                await LoadList();
            }
        }

    }


    public class RegionHomeBannerItemModel
    {
        public int id { get; set; }
        public string title { get; set; }
        public string image { get; set; }
        public string uri { get; set; }
    }
    public class RegionChildOrderModel
    {
        public RegionChildOrderModel(string name, string order)
        {
            this.name = name;
            this.order = order;
        }
        public string name { get; set; }
        public string order { get; set; }
    }
    public class RegionTagItemModel
    {
        public int tid { get; set; }
        public int rid { get; set; }
        public int reid { get; set; }
        public string tname { get; set; }
        public string rname { get; set; }
        public string rename { get; set; }
    }
    public class RegionVideoItemModel
    {
        public int id { get; set; }
        public string title { get; set; }
        public string cover { get; set; }
        public string param { get; set; }
        public string name { get; set; }
        public int play { get; set; }
        public int danmaku { get; set; }
        public string rname { get; set; }
        public int duration { get; set; }
    }
}
