using BiliLite.Api.User;
using BiliLite.Helpers;
using BiliLite.Models;
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

namespace BiliLite.Modules.User.UserDetail
{
    /// <summary>
    /// 关注的人
    /// </summary>
    public class UserFollowVM : IModules
    {
        public string mid { get; set; }
        private readonly UserDetailAPI userDetailAPI;
        readonly bool IsFans=false; 
        public UserFollowVM(bool isfans)
        {
            userDetailAPI = new UserDetailAPI();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);

            IsFans = isfans;
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

        private ObservableCollection<UserFollowItemModel> _Items;
        public ObservableCollection<UserFollowItemModel> Items
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
                var api = userDetailAPI.Followings(mid, Page);
                if (IsFans)
                {
                    api = userDetailAPI.Followers(mid, Page);
                }
                var results = await api.Request();
                if (results.status)
                {
                    var data = await results.GetData<JObject>();
                    if (data.code == 0)
                    {
                        var items = JsonConvert.DeserializeObject<ObservableCollection<UserFollowItemModel>>(data.data["list"]?.ToString() ?? "[]");
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



                        var count = data.data["total"]?.ToInt32() ?? 0;
                        if (Items.Count >= count)
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


    public class UserFollowItemModel
    {
        public string mid { get; set; }
        public string uname { get; set; }
        public string face { get; set; }

      
        public UserFollowOfficialVerifyItem official_verify { get; set; }
        public string Verify
        {
            get
            {
                if (official_verify == null)
                {
                    return "";
                }
                switch (official_verify.type)
                {
                    case 0:
                        return AppHelper.VERIFY_PERSONAL_IMAGE;
                    case 1:
                        return AppHelper.VERIFY_OGANIZATION_IMAGE;
                    default:
                        return AppHelper.TRANSPARENT_IMAGE;
                }
            }
        }
        public string sign { get; set; }
        public string usign
        {
            get
            {
                if (official_verify != null && !string.IsNullOrEmpty(official_verify.desc))
                {
                    return official_verify.desc;
                }
                return sign;
            }
        }
    }
  
    public class UserFollowOfficialVerifyItem
    {
        public string desc { get; set; }
        public int type { get; set; }
    }
}
