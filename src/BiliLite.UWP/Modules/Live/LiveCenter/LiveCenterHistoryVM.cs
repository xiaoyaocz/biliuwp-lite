using BiliLite.Models.Requests.Api.Live;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using BiliLite.Extensions;

namespace BiliLite.Modules.Live.LiveCenter
{
    public class LiveCenterHistoryVM : IModules
    {
        readonly LiveCenterAPI liveCenterAPI;

        public LiveCenterHistoryVM()
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

        private ObservableCollection<LiveHistoryItemModel> _Items;
        public ObservableCollection<LiveHistoryItemModel> Items
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
                var api = liveCenterAPI.History(Page);

                var results = await api.Request();
                if (results.status)
                {
                    var data = await results.GetData<ObservableCollection<LiveHistoryItemModel>>();
                    if (data.code == 0)
                    {
                        var items = data.data;
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


                        if (items != null && items.Count > 0)
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
                var handel = HandelError<LiveCenterHistoryVM>(ex);
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
    public class LiveHistoryItemModel
    {
        public int roomid { get; set; }
        public string title { get; set; }
        public string name { get; set; }
        public string cover { get; set; }
        public string tag_name { get; set; }
        public long view_at { get; set; }
        public string uri { get; set; }
        public LiveHistoryItemHistoryModel history { get; set; }
    }
    public class LiveHistoryItemHistoryModel
    {
        public int oid { get; set; }
    }
}
