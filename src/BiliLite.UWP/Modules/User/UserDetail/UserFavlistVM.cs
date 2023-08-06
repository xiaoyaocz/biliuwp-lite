using BiliLite.Models;
using BiliLite.Models.Requests.Api.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Media;
using BiliLite.Extensions;

namespace BiliLite.Modules.User.UserDetail
{
    public class UserFavlistVM : IModules
    {
        public string mid { get; set; }
        private readonly UserDetailAPI userDetailAPI;
        public UserFavlistVM()
        {
            userDetailAPI = new UserDetailAPI();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);


        }

        private bool _Loading = true;
        public bool Loading
        {
            get { return _Loading; }
            set { _Loading = value; DoPropertyChanged("Loading"); }
        }

        public ICommand RefreshCommand { get; private set; }
        public ICommand LoadMoreCommand { get; private set; }

        private ObservableCollection<FavFolderItemModel> _Items;
        public ObservableCollection<FavFolderItemModel> Items
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


        public async Task Get()
        {
            try
            {
                Nothing = false;
                Loading = true;
                var api = userDetailAPI.Favlist(mid);

                var results = await api.Request();
                if (results.status)
                {
                    var data = await results.GetData<JObject>();
                    if (data.code == 0)
                    {
                        if (data.data != null)
                        {
                            var items = JsonConvert.DeserializeObject<ObservableCollection<FavFolderItemModel>>(data.data["list"]?.ToString() ?? "[]");
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
                        }

                        if ((Items == null || Items.Count == 0))
                        {
                            Nothing = true;
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
                var handel = HandelError<UserFavlistVM>(ex);
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

    public class FavFolderItemModel
    {
        public long id { get; set; }
        public int fid { get; set; }
        public long mid { get; set; }
        public int media_count { get; set; }
        public string title { get; set; }
    }

}
