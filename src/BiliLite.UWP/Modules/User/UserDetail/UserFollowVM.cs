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
using BiliLite.Models.Common;

namespace BiliLite.Modules.User.UserDetail
{
    /// <summary>
    /// 关注的人
    /// </summary>
    public class UserFollowVM : IModules
    {
        public string mid { get; set; }
        private readonly UserDetailAPI userDetailAPI;
        readonly bool IsFans = false;
        public UserFollowVM(bool isfans)
        {
            userDetailAPI = new UserDetailAPI();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
            Tlist = new ObservableCollection<FollowTlistItemModel>() {
                new FollowTlistItemModel()
                {
                    name="全部关注",
                    tagid=-1
                }
            };
            SelectTid = Tlist.First();
            IsFans = isfans;
        }

        public int SelectOrder { get; set; } = 0;

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
        private ObservableCollection<FollowTlistItemModel> _tlist;

        public ObservableCollection<FollowTlistItemModel> Tlist
        {
            get { return _tlist; }
            set { _tlist = value; DoPropertyChanged("Tlist"); }
        }

        private FollowTlistItemModel _selectTid;

        public FollowTlistItemModel SelectTid
        {
            get { return _selectTid; }
            set { if (value == null) return; _selectTid = value; }
        }

        private bool _Nothing = false;

        public bool Nothing
        {
            get { return _Nothing; }
            set { _Nothing = value; DoPropertyChanged("Nothing"); }
        }

        public int Page { get; set; } = 1;
        public int CurrentTid { get; set; } = -1;
        public string Keyword { get; set; } = "";

        public async Task GetTags()
        {
            try
            {
                var api = userDetailAPI.FollowingsTag();
                var results = await api.Request();
                if (results.status)
                {
                    var data = await results.GetData<JArray>();
                    var items = JsonConvert.DeserializeObject<ObservableCollection<FollowTlistItemModel>>(data.data.ToString());
                    if (items != null && items.Count > 0)
                    {
                        foreach (var item in items)
                        {
                            Tlist.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("关注分组加载失败:" + ex);
            }
        }

        public async Task Get()
        {
            try
            {
                Nothing = false;
                CanLoadMore = false;
                Loading = true;
                CurrentTid = SelectTid.tagid;
                var api = userDetailAPI.Followings(mid, Page, 30, tid: CurrentTid, keyword: Keyword, (FollowingsOrder)SelectOrder);
                if (IsFans)
                {
                    api = userDetailAPI.Followers(mid, Page);
                }
                var results = await api.Request();
                if (results.status)
                {
                    var data = results.GetJObject();
                    var successful = data["code"].ToInt32() == 0;

                    if (successful)
                    {
                        var listStr = data["data"] is JArray ? data["data"].ToString() : data["data"]["list"].ToString();
                        var items = JsonConvert.DeserializeObject<ObservableCollection<UserFollowItemModel>>(listStr ?? "[]");
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

                        // var count = data.data["total"]?.ToInt32() ?? 0;
                        if (items.Count == 0)
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
                var handel = HandelError<UserFollowVM>(ex);
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
                        return Constants.App.VERIFY_PERSONAL_IMAGE;
                    case 1:
                        return Constants.App.VERIFY_OGANIZATION_IMAGE;
                    default:
                        return Constants.App.TRANSPARENT_IMAGE;
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
    public class FollowTlistItemModel
    {
        public int tagid { get; set; }
        public string name { get; set; }
        public int count { get; set; }
        public string tip { get; set; }
    }
}
