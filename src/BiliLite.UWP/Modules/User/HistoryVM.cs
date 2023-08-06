using BiliLite.Models;
using BiliLite.Models.Requests.Api;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using BiliLite.Extensions;

namespace BiliLite.Modules.User
{
    public class HistoryVM : IModules
    {
        AccountApi accountApi;
        public HistoryVM()
        {
            accountApi = new AccountApi();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
        }
        public int Page { get; set; } = 1;
        private bool _loading = false;

        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }
        public ICommand LoadMoreCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }
        private bool _Nothing = false;
        public bool Nothing
        {
            get { return _Nothing; }
            set { _Nothing = value; DoPropertyChanged("Nothing"); }
        }

        private bool _ShowLoadMore = false;
        public bool ShowLoadMore
        {
            get { return _ShowLoadMore; }
            set { _ShowLoadMore = value; DoPropertyChanged("ShowLoadMore"); }
        }
        private ObservableCollection<HistoryItemModel> _videos;
        public ObservableCollection<HistoryItemModel> Videos
        {
            get { return _videos; }
            set { _videos = value; DoPropertyChanged("Videos"); }
        }

        public async Task LoadHistory()
        {
            try
            {
                ShowLoadMore = false;
                Loading = true;
                Nothing = false;
                var results = await accountApi.History(Page, 24).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<ObservableCollection<HistoryItemModel>>>();
                    if (data.success)
                    {
                        if (Page == 1)
                        {
                            if (data.data == null || data.data.Count == 0)
                            {
                                Nothing = true;
                                return;
                            }
                            Videos = data.data;

                        }
                        else
                        {
                            if (data.data != null)
                            {
                                foreach (var item in data.data)
                                {
                                    Videos.Add(item);
                                }
                            }
                        }
                        if (data.data != null && data.data.Count != 0)
                        {
                            ShowLoadMore = true;
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
                var handel = HandelError<HistoryVM>(ex);
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
            Page = 1;
            Videos = null;
            await LoadHistory();
        }
        public async void LoadMore()
        {
            if (Loading)
            {
                return;
            }
            if (Videos == null || Videos.Count == 0)
            {
                return;
            }
            await LoadHistory();
        }
        public async void Del(HistoryItemModel item)
        {
            try
            {
                var results = await accountApi.DelHistory(item.business + "_" + item.kid).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        Videos.Remove(item);
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
                var handel = HandelError<HistoryVM>(ex);
                Notify.ShowMessageToast(handel.message);
            }
        }
    }
    public class HistoryItemModel
    {
        public string aid { get; set; }
        public string cover { get; set; }
        public HistoryItemOwnerModel owner { get; set; }
        public string name { get; set; }
        public string pic { get; set; }
        public string title { get; set; }
        public long view_at { get; set; }
        public string tname { get; set; }
        public string kid { get; set; }
        public string business { get; set; }
    }

    public class HistoryItemOwnerModel
    {
        public long mid { get; set; }
        public string name { get; set; }
        public string face { get; set; }
    }
}
