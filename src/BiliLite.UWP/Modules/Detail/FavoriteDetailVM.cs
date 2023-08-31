using BiliLite.Models;
using BiliLite.Models.Requests.Api.User;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using BiliLite.Extensions;
using BiliLite.Services;

namespace BiliLite.Modules
{
    public class FavoriteDetailVM : IModules
    {
        readonly FavoriteApi favoriteApi;
        public int Page { get; set; } = 1;
        public string Keyword { get; set; } = "";
        public string Id { get; set; }
        public int Type { get; set; }
        public FavoriteDetailVM()
        {
            favoriteApi = new FavoriteApi();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
            CollectCommand = new RelayCommand(DoCollect);
            CancelCollectCommand = new RelayCommand(DoCancelCollect);
            SelectCommand = new RelayCommand<object>(SetSelectMode);
        }
        private bool _loading = false;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }
        private FavoriteInfoModel _FavoriteInfo;
        public FavoriteInfoModel FavoriteInfo
        {
            get { return _FavoriteInfo; }
            set { _FavoriteInfo = value; DoPropertyChanged("FavoriteInfo"); }
        }
        private ObservableCollection<FavoriteInfoVideoItemModel> _videos;
        public ObservableCollection<FavoriteInfoVideoItemModel> Videos
        {
            get { return _videos; }
            set { _videos = value; DoPropertyChanged("Videos"); }
        }

        private ListViewSelectionMode _selectionMode = ListViewSelectionMode.None;
        public ListViewSelectionMode SelectionMode
        {
            get { return _selectionMode; }
            set { _selectionMode = value; DoPropertyChanged("SelectionMode"); }
        }

        private bool _IsItemClickEnabled = true;
        public bool IsItemClickEnabled
        {
            get { return _IsItemClickEnabled; }
            set { _IsItemClickEnabled = value; DoPropertyChanged("IsItemClickEnabled"); }
        }
        public ICommand CollectCommand { get; private set; }
        public ICommand CancelCollectCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }
        public ICommand LoadMoreCommand { get; private set; }
        public ICommand SelectCommand { get; private set; }
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
        private bool _isSelf = false;
        public bool IsSelf
        {
            get { return _isSelf; }
            set { _isSelf = value; DoPropertyChanged("IsSelf"); }
        }

        private bool _showCollect = false;
        public bool ShowCollect
        {
            get { return _showCollect; }
            set { _showCollect = value; DoPropertyChanged("ShowCollect"); }
        }
        private bool _showCancelCollect = false;
        public bool ShowCancelCollect
        {
            get { return _showCancelCollect; }
            set { _showCancelCollect = value; DoPropertyChanged("ShowCancelCollect"); }
        }
        public async Task LoadFavoriteInfo()
        {
            try
            {
                ShowLoadMore = false;
                Loading = true;
                Nothing = false;
                var api = favoriteApi.FavoriteInfo(Id, Keyword, Page);
                if (Type == 21)
                {
                    api = favoriteApi.FavoriteSeasonInfo(Id, Keyword, Page);
                }
                var results = await api.Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<FavoriteDetailModel>>();
                    if (data.success)
                    {
                        if (Page == 1)
                        {
                            FavoriteInfo = data.data.info;
                            IsSelf = FavoriteInfo.mid == SettingService.Account.UserID.ToString();
                            if (!IsSelf)
                            {
                                ShowCollect = FavoriteInfo.fav_state != 1;
                                ShowCancelCollect = !ShowCollect;
                            }

                            if (data.data.medias == null || data.data.medias.Count == 0)
                            {
                                Nothing = true;
                                return;
                            }
                            Videos = data.data.medias;

                        }
                        else
                        {
                            if (data.data.medias != null)
                            {
                                foreach (var item in data.data.medias)
                                {
                                    Videos.Add(item);
                                }
                            }
                        }
                        if (Videos.Count != FavoriteInfo.media_count)
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
                var handel = HandelError<FavoriteDetailVM>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }
        public async Task Delete(List<FavoriteInfoVideoItemModel> items)
        {
            try
            {
                if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
                {
                    Notify.ShowMessageToast("请先登录后再操作");
                    return;
                }
                var results = await favoriteApi.Delete(Id, items.Select(x => x.id).ToList()).Request();
                if (results.status)
                {
                    var data = await results.GetData<object>();
                    if (data.success)
                    {
                        foreach (var item in items)
                        {
                            Videos.Remove(item);
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
                var handel = HandelError<FavoriteDetailVM>(ex);
                Notify.ShowMessageToast(handel.message);
            }

        }
        public async Task Clean()
        {
            try
            {
                if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
                {
                    Notify.ShowMessageToast("请先登录后再操作");
                    return;
                }
                var results = await favoriteApi.Clean(Id).Request();
                if (results.status)
                {
                    var data = await results.GetData<object>();
                    if (data.success)
                    {
                        Refresh();
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
                var handel = HandelError<FavoriteDetailVM>(ex);
                Notify.ShowMessageToast(handel.message);
            }

        }
        public async void Refresh()
        {
            if (Loading)
            {
                return;
            }
            Page = 1;
            FavoriteInfo = null;
            Videos = null;
            await LoadFavoriteInfo();
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
            await LoadFavoriteInfo();
        }
        public async void Search(string keyword)
        {
            if (Loading)
            {
                return;
            }
            Keyword = keyword;
            Page = 1;
            FavoriteInfo = null;
            Videos = null;
            await LoadFavoriteInfo();
        }
        private void SetSelectMode(object data)
        {
            if (data == null)
            {
                IsItemClickEnabled = true;
                SelectionMode = ListViewSelectionMode.None;
            }
            else
            {
                IsItemClickEnabled = false;
                SelectionMode = ListViewSelectionMode.Multiple;
            }
        }
        public async void DoCollect()
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录后再操作");
                return;
            }
            try
            {
                var results = await favoriteApi.CollectFavorite(FavoriteInfo.id).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<object>>();
                    if (data.success)
                    {
                        ShowCancelCollect = true;
                        ShowCollect = false;
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
                var handel = HandelError<object>(ex);
                Notify.ShowMessageToast(handel.message);
            }


        }
        public async void DoCancelCollect()
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录后再操作");
                return;
            }
            try
            {
                var results = await favoriteApi.CacnelCollectFavorite(FavoriteInfo.id).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<object>>();
                    if (data.success)
                    {
                        ShowCancelCollect = false;
                        ShowCollect = true;
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
                var handel = HandelError<object>(ex);
                Notify.ShowMessageToast(handel.message);
            }


        }
    }
    public class FavoriteDetailModel
    {
        public FavoriteInfoModel info { get; set; }
        public ObservableCollection<FavoriteInfoVideoItemModel> medias { get; set; }
    }

    public class FavoriteInfoModel
    {
        public string cover { get; set; }
        public int attr { get; set; }
        public bool privacy
        {
            get
            {
                return attr == 2;
            }
        }
        public string fid { get; set; }
        public string id { get; set; }
        public int like_state { get; set; }
        public int fav_state { get; set; }
        public string mid { get; set; }
        public string title { get; set; }
        public int type { get; set; }
        public int media_count { get; set; }
        public FavoriteInfoVideoItemUpperModel upper { get; set; }
    }

    public class FavoriteInfoVideoItemModel
    {
        public string id { get; set; }
        public string cover { get; set; }
        public string title { get; set; }
        public FavoriteInfoVideoItemUpperModel upper { get; set; }
        public FavoriteInfoVideoItemStatModel cnt_info { get; set; }
    }
    public class FavoriteInfoVideoItemUpperModel
    {
        public string face { get; set; }
        public string name { get; set; }
        public string mid { get; set; }
    }
    public class FavoriteInfoVideoItemStatModel
    {
        public int coin { get; set; }
        public int collect { get; set; }
        public int danmaku { get; set; }
        public int play { get; set; }
        public int reply { get; set; }
        public int share { get; set; }
    }
}
