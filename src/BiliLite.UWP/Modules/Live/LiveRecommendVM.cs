using BiliLite.Models.Requests.Api.Live;
using Microsoft.Toolkit.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Media.Imaging;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Services;

namespace BiliLite.Modules.Live
{
    public class LiveRecommendVM : IModules
    {
        public List<LiveRecommendItem> Items { get; set; }
        private LiveRecommendItem _current;
        public LiveRecommendItem Current
        {
            get { return _current; }
            set { _current = value; DoPropertyChanged("Current"); }
        }
        public LiveRecommendVM()
        {
            Items = new List<LiveRecommendItem>() {
                new LiveRecommendItem("最热直播","online"),
                new LiveRecommendItem("互动直播","sort_type_169"),
                new LiveRecommendItem("最新开播","live_time")
            };
            Current = Items[0];
        }
    }

    public class LiveRecommendItem : IModules
    {
        public string Title { get; set; }
        public string SortType { get; set; }
        readonly LiveRecommendAPI recommendAPI;

        //private IncrementalLoadingCollection<LiveRecommendItemSource, LiveRecommendItemModel> _items;
        //public IncrementalLoadingCollection<LiveRecommendItemSource, LiveRecommendItemModel> Items
        //{
        //    get { return _items; }
        //    set { _items = value; DoPropertyChanged("Items"); }
        //}
        public ObservableCollection<LiveRecommendItemModel> Items { get; set; }

        public ICommand RefreshCommand { get; private set; }
        public ICommand LoadMoreCommand { get; private set; }
        public LiveRecommendItem(string title, string sort)
        {
            Title = title;
            SortType = sort;
            recommendAPI = new LiveRecommendAPI();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
            Items = new ObservableCollection<LiveRecommendItemModel>();
        }
        private bool _loading = false;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }

        private bool _CanLoadMore = false;
        public bool CanLoadMore
        {
            get { return _CanLoadMore; }
            set { _CanLoadMore = value; DoPropertyChanged("CanLoadMore"); }
        }

        public int Page { get; set; } = 1;
        public async Task GetItems()
        {
            try
            {
                Loading = true;
                CanLoadMore = false;
                var results = await recommendAPI.LiveRoomList(Page, SortType).Request();
                if (results.status)
                {
                    var data = results.GetJObject();
                    if (data["code"].ToInt32() == 0)
                    {
                        var items = JsonConvert.DeserializeObject<List<LiveRecommendItemModel>>(data["data"]["list"].ToString());
                        if (items != null)
                        {
                            foreach (var item in items)
                            {
                                Items.Add(item);
                            }
                            //Items = new IncrementalLoadingCollection<LiveRecommendItemSource, LiveRecommendItemModel>(new LiveRecommendItemSource(items, SortType), 30);
                            if (Items.Count < data["data"]["count"].ToInt32())
                            {
                                Page++;
                                CanLoadMore = true;
                            }
                        }
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
                var handel = HandelError<AnimeHomeModel>(ex);
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
            Items.Clear();
            Page = 1;
            await GetItems();
        }
        public async void LoadMore()
        {
            if (Loading)
            {
                return;
            }
            await GetItems();
        }

    }

    public class LiveRecommendItemSource : IIncrementalSource<LiveRecommendItemModel>
    {
        private static readonly ILogger logger = GlobalLogger.FromCurrentType();

        readonly LiveRecommendAPI recommendAPI;
        public LiveRecommendItemSource(List<LiveRecommendItemModel> items, string sort)
        {
            recommendAPI = new LiveRecommendAPI();
            sort_type = sort;
            recommends = items;
        }
        string sort_type = "";
        List<LiveRecommendItemModel> recommends;
        public async Task<List<LiveRecommendItemModel>> GetRecommend(int page)
        {
            try
            {
                var result = await recommendAPI.LiveRoomList(page, sort_type).Request();
                if (result.status)
                {
                    var obj = await result.GetData<JObject>();
                    if (obj.code == 0)
                    {
                        var items = JsonConvert.DeserializeObject<List<LiveRecommendItemModel>>(obj.data["list"].ToString());

                        return items;
                    }
                    else
                    {
                        Notify.ShowMessageToast(obj.message);
                        return new List<LiveRecommendItemModel>();
                    }
                }
                else
                {
                    Notify.ShowMessageToast(result.message);
                    return new List<LiveRecommendItemModel>();
                }
            }
            catch (Exception ex)
            {
                logger.Log("加载直播推荐信息失败", LogType.Error, ex);
                Notify.ShowMessageToast("加载直播推荐信息失败");
                return new List<LiveRecommendItemModel>();
            }
        }
        public async Task<IEnumerable<LiveRecommendItemModel>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
        {
            if (pageIndex == 0)
            {
                return recommends;
            }

            return await GetRecommend(pageIndex + 1);
        }
    }

    public class LiveRecommendItemModel
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

        public string pendent_ru { get; set; }
        public string pendent_ru_color { get; set; }
        public string pendent_ru_pic { get; set; }
        public BitmapImage pendent_pic
        {
            get
            {
                if (string.IsNullOrEmpty(pendent_ru_pic))
                {
                    return new BitmapImage();
                }
                else
                {
                    return new BitmapImage(new Uri(pendent_ru_pic));
                }
            }
        }
        public bool show_pendent
        {
            get
            {
                return !string.IsNullOrEmpty(pendent_ru);
            }
        }

    }
}
