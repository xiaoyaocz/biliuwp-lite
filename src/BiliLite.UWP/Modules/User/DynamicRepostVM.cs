using BiliLite.Controls.Dynamic;
using BiliLite.Models.Dynamic;
using BiliLite.Models.Requests.Api.User;
using BiliLite.Pages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Services;

namespace BiliLite.Modules.User
{
    public class DynamicRepostVM : IModules
    {
        readonly DynamicAPI dynamicAPI;
        public DynamicRepostVM()
        {
            dynamicAPI = new DynamicAPI();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
            UserCommand = new RelayCommand<object>(OpenUser);
        }
        public string ID { get; set; }
        public ICommand RefreshCommand { get; private set; }
        public ICommand LoadMoreCommand { get; private set; }
        public ICommand UserCommand { get; set; }
        private bool _loading = false;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }
        private bool _loadMore = false;
        public bool CanLoadMore
        {
            get { return _loadMore; }
            set { _loadMore = value; DoPropertyChanged("CanLoadMore"); }
        }
        private ObservableCollection<DynamicItemDisplayModel> _Items;
        public ObservableCollection<DynamicItemDisplayModel> Items
        {
            get { return _Items; }
            set { _Items = value; DoPropertyChanged("Items"); }
        }
        string next = "";
        public async Task GetDynamicItems()
        {
            try
            {
                CanLoadMore = false;
                Loading = true;
                var api = dynamicAPI.DynamicRepost(ID, next);

                var results = await api.Request();
                if (results.status)
                {
                    var data = results.GetJObject();
                    if (data["code"].ToInt32() == 0)
                    {
                        var items = JsonConvert.DeserializeObject<List<DynamicCardModel>>(data["data"]?["items"]?.ToString() ?? "[]");

                        ObservableCollection<DynamicItemDisplayModel> _ls = new ObservableCollection<DynamicItemDisplayModel>();
                        foreach (var item in items)
                        {
                            _ls.Add(ConvertToDisplayRepost(item));
                        }
                        if (Items == null)
                        {

                            Items = _ls;
                        }
                        else
                        {
                            foreach (var item in _ls)
                            {
                                Items.Add(item);
                            }
                        }
                        CanLoadMore = (data["data"]?["has_more"]?.ToInt32() ?? 0) == 1;
                        next = data["data"]?["has_more"]?.ToString() ?? "";
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
                var handel = HandelError<DynamicRepostVM>(ex);
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
            next = "";
            Items = null;
            await GetDynamicItems();
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
            var last = Items.LastOrDefault();
            await GetDynamicItems();
        }
        private DynamicItemDisplayModel ConvertToDisplayRepost(DynamicCardModel item)
        {
            var card = JObject.Parse(item.card);
            var data = new DynamicItemDisplayModel()
            {
                Datetime = TimeExtensions.TimestampToDatetime(item.desc.timestamp).ToString(),
                DynamicID = item.desc.dynamic_id,
                Mid = item.desc.uid,
                Time = item.desc.timestamp.HandelTimestamp(),
                UserCommand = UserCommand
            };
            var content = "";
            //内容
            if (card.ContainsKey("item") && card["item"]["content"] != null)
            {
                content = card["item"]["content"]?.ToString();
            }
            data.ContentStr = content;

            if (item.desc.user_profile != null)
            {
                data.UserName = item.desc.user_profile.info.uname;
                data.Photo = item.desc.user_profile.info.face;
                if (item.desc.user_profile.vip != null)
                {
                    data.IsYearVip = item.desc.user_profile.vip.vipStatus == 1 && item.desc.user_profile.vip.vipType == 2;
                }
                switch (item.desc.user_profile.card?.official_verify?.type ?? 3)
                {
                    case 0:
                        data.Verify = Constants.App.VERIFY_PERSONAL_IMAGE;
                        break;
                    case 1:
                        data.Verify = Constants.App.VERIFY_OGANIZATION_IMAGE;
                        break;
                    default:
                        data.Verify = Constants.App.TRANSPARENT_IMAGE;
                        break;
                }
                if (!string.IsNullOrEmpty(item.desc.user_profile.pendant?.image))
                {
                    data.Pendant = item.desc.user_profile.pendant.image;
                }
            }
            return data;
        }
        public void OpenUser(object id)
        {
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Contact,
                page = typeof(UserInfoPage),
                title = "用户中心",
                parameters = id
            });
        }


        public void Clear()
        {
            next = "";
            Items = null;

        }
    }
}
