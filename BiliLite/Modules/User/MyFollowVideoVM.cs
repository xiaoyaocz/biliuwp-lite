using BiliLite.Helpers;
using BiliLite.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BiliLite.Modules
{
    public class MyFollowVideoVM : IModules
    {
        readonly Api.User.FollowAPI followAPI;
        public MyFollowVideoVM()
        {
            followAPI = new Api.User.FollowAPI();
            RefreshCommand = new RelayCommand(Refresh);
        }
        private bool _loading = false;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }
        public ICommand RefreshCommand { get; private set; }
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
        public async Task LoadFavorite()
        {
            try
            {
                Loading = true;
               
                var results = await followAPI.MyFavorite().Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JArray>>();
                    if (data.success)
                    {
                        if (data.data[0]["mediaListResponse"]!=null)
                        {
                            MyFavorite =await data.data[0]["mediaListResponse"]["list"].ToString().DeserializeJson<ObservableCollection<FavoriteItemModel>>();
                        }
                        if (data.data[1]["mediaListResponse"]!=null)
                        {
                            CollectFavorite = await data.data[1]["mediaListResponse"]["list"].ToString().DeserializeJson<ObservableCollection<FavoriteItemModel>>();
                        }
                    }
                    else
                    {
                        Utils.ShowMessageToast(data.message);
                    }
                }
                else
                {
                    Utils.ShowMessageToast(results.message);
                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<AnimeHomeModel>(ex);
                Utils.ShowMessageToast(handel.message);
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
                return attr==2;
            }
        }
   
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
                if (value) {
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
