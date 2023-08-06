using BiliLite.Models;
using BiliLite.Models.Requests.Api.User;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BiliLite.Extensions;

namespace BiliLite.Modules
{
    public class MyFollowVideoVM : IModules
    {
        readonly FavoriteApi favoriteAPI;
        public MyFollowVideoVM()
        {
            favoriteAPI = new FavoriteApi();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
        }
        private bool _loading = false;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }
        public ICommand RefreshCommand { get; private set; }
        public ICommand LoadMoreCommand { get; private set; }
        private ObservableCollection<FavoriteItemModel> _myFavorite;
        public ObservableCollection<FavoriteItemModel> MyFavorite
        {
            get { return _myFavorite; }
            set { _myFavorite = value; DoPropertyChanged("MyFavorite"); }
        }

        private ObservableCollection<FavoriteItemModel> _collectFavorite;
        public ObservableCollection<FavoriteItemModel> CollectFavorite
        {
            get { return _collectFavorite; }
            set { _collectFavorite = value; DoPropertyChanged("CollectFavorite"); }
        }
        private bool _hasMore = false;
        public bool HasMore
        {
            get { return _hasMore; }
            set { _hasMore = value; DoPropertyChanged("HasMore"); }
        }
        private int Page = 1;
        public async Task LoadFavorite()
        {
            try
            {
                Loading = true;
                HasMore = false;
                Page = 1;
                var results = await favoriteAPI.MyFavorite().Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        if (data.data["space_infos"][0]["mediaListResponse"] != null)
                        {
                            MyFavorite = await data.data["space_infos"][0]["mediaListResponse"]["list"].ToString().DeserializeJson<ObservableCollection<FavoriteItemModel>>();
                            if (MyFavorite == null)
                            {
                                MyFavorite = new ObservableCollection<FavoriteItemModel>();
                            }
                            MyFavorite.Insert(0, await data.data["default_folder"]["folder_detail"].ToString().DeserializeJson<FavoriteItemModel>());
                            HasMore = (bool)data.data["space_infos"][0]["mediaListResponse"]["has_more"];
                            Page++;
                        }
                        if (data.data["space_infos"][1]["mediaListResponse"] != null)
                        {
                            CollectFavorite = await data.data["space_infos"][1]["mediaListResponse"]["list"].ToString().DeserializeJson<ObservableCollection<FavoriteItemModel>>();
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
                var handel = HandelError<MyFollowVideoVM>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }
        public async void LoadMore()
        {
            await LoadCreateList();
        }

        public async Task LoadCreateList()
        {
            try
            {
                HasMore = false;
                var results = await favoriteAPI.MyCreatedFavoriteList(Page).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        var ls = await data.data["list"].ToString().DeserializeJson<List<FavoriteItemModel>>();
                        foreach (var item in ls)
                        {
                            MyFavorite.Add(item);
                        }
                        HasMore = (bool)data.data["has_more"];
                        Page++;
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
                var handel = HandelError<MyFollowVideoVM>(ex);
                Notify.ShowMessageToast(handel.message);
            }
        }


        public async Task<bool> DelFavorite(string id)
        {
            try
            {
                var results = await favoriteAPI.DelFavorite(id).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<object>>();
                    if (data.success)
                    {
                        return true;
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
                var handel = HandelError<MyFollowVideoVM>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            return false;
        }
        public async void Refresh()
        {
            if (Loading)
            {
                return;
            }
            MyFavorite = null;
            CollectFavorite = null;
            await LoadFavorite();
        }
    }

    public class FavoriteItemModel : INotifyPropertyChanged
    {
        public string cover { get; set; }
        public int attr { get; set; }
        public bool privacy
        {
            get
            {
                //attr单数为私密，双数为公开
                return attr % 2 != 0;
            }
        }
        public string intro { get; set; }
        public string fid { get; set; }
        public string id { get; set; }
        public int like_state { get; set; }

        public string mid { get; set; }
        public string title { get; set; }
        public int type { get; set; }


        private int _media_count;
        public int media_count
        {
            get { return _media_count; }
            set { _media_count = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("media_count")); }
        }
        public int fav_state { get; set; }
        public bool is_fav
        {
            get
            {
                return fav_state == 1;
            }
            set
            {
                if (value)
                {
                    fav_state = 1;
                }
                else
                {
                    fav_state = 0;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("is_fav"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("fav_state"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

}
