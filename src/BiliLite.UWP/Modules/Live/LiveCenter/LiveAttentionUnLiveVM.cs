using BiliLite.Models.Requests.Api.Live;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using BiliLite.Extensions;

namespace BiliLite.Modules.Live.LiveCenter
{
    public class LiveAttentionUnLiveVM : IModules
    {
        readonly LiveCenterAPI liveCenterAPI;

        public LiveAttentionUnLiveVM()
        {
            liveCenterAPI = new LiveCenterAPI();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
        }

        private bool _Loading = true;
        public bool Loading
        {
            get { return _Loading; }
            set { _Loading = value; DoPropertyChanged("Loading"); }
        }
        private bool _CanLoadMore = false;
        public bool CanLoadMore
        {
            get { return _CanLoadMore; }
            set { _CanLoadMore = value; DoPropertyChanged("CanLoadMore"); }
        }
        public ICommand RefreshCommand { get; private set; }
        public ICommand LoadMoreCommand { get; private set; }

        private ObservableCollection<LiveFollowUnliveAnchorModel> _Items;
        public ObservableCollection<LiveFollowUnliveAnchorModel> Items
        {
            get { return _Items; }
            set { _Items = value; DoPropertyChanged("Items"); }
        }


        private bool _Nothing = false;

        public bool Nothing
        {
            get { return _Nothing; }
            set { _Nothing = value; DoPropertyChanged("Nothing"); }
        }

        public int Page { get; set; } = 1;

        public async Task Get()
        {
            try
            {
                Nothing = false;
                CanLoadMore = false;
                Loading = true;
                var api = liveCenterAPI.FollowUnLive(Page);

                var results = await api.Request();
                if (results.status)
                {
                    var data = await results.GetData<JObject>();
                    if (data.code == 0)
                    {
                        var items = JsonConvert.DeserializeObject<ObservableCollection<LiveFollowUnliveAnchorModel>>(data.data["rooms"]?.ToString() ?? "[]");
                        if (Items == null)
                        {
                            Items = items;
                        }
                        else
                        {
                            foreach (var item in items)
                            {
                                Items.Add(item);
                            }
                        }
                        if (Page == 1 && (Items == null || Items.Count == 0))
                        {
                            Nothing = true;
                        }

                        var has_more = data.data["has_more"]?.ToInt32() ?? 0;
                        if (has_more == 0)
                        {
                            CanLoadMore = false;
                        }
                        else
                        {
                            CanLoadMore = true;
                            Page++;
                        }
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
                var handel = HandelError<LiveAttentionUnLiveVM>(ex);
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
            Items = null;
            Page = 1;
            await Get();
        }
        public async void LoadMore()
        {
            if (Loading)
            {
                return;
            }
            if (Items == null || Items.Count == 0)
            {
                return;
            }
            await Get();
        }
    }
    public class LiveFollowUnliveAnchorModel
    {
        public int roomid { get; set; }
        public string uid { get; set; }
        public string uname { get; set; }
        public string face { get; set; }
        public string title { get; set; }
        public string area_name { get; set; }
        public string area_v2_name { get; set; }
        public string area_v2_parent_name { get; set; }
        public string cover { get; set; }
        public string pendent_ru { get; set; }
        public string pendent_ru_color { get; set; }
        public string pendent_ru_pic { get; set; }
        public string live_desc { get; set; }
        public string announcement_content { get; set; }
    }
}
