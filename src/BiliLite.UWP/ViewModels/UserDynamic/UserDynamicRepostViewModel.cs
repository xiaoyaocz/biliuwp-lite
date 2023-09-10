using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using BiliLite.Controls.Dynamic;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Models.Common.UserDynamic;
using BiliLite.Models.Dynamic;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api.User;
using BiliLite.Modules;
using BiliLite.Pages;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PropertyChanged;

namespace BiliLite.ViewModels.UserDynamic
{
    public class UserDynamicRepostViewModel : BaseViewModel
    {
        #region Fields

        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly DynamicAPI m_dynamicApi;
        private string m_next = "";

        #endregion

        #region Constructors

        public UserDynamicRepostViewModel()
        {
            m_dynamicApi = new DynamicAPI();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
            UserCommand = new RelayCommand<object>(OpenUser);
        }

        #endregion

        #region Properties

        public ICommand RefreshCommand { get; private set; }
        public ICommand LoadMoreCommand { get; private set; }
        public ICommand UserCommand { get; set; }

        [DoNotNotify]
        public string ID { get; set; }

        public bool Loading { get; set; }

        public bool CanLoadMore { get; set; }

        public ObservableCollection<UserDynamicItemDisplayViewModel> Items { get; set; }

        #endregion

        #region Public Methods

        public async Task GetDynamicItems()
        {
            try
            {
                CanLoadMore = false;
                Loading = true;
                await GetDynamicItemsCore();
            }
            catch (CustomizedErrorException ex)
            {
                Notify.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message);
            }
            catch (Exception ex)
            {
                var handel = HandelError<UserDynamicRepostViewModel>(ex);
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
            m_next = "";
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
            m_next = "";
            Items = null;
        }

        #endregion

        #region Private Methods

        private async Task GetDynamicItemsCore()
        {
            var api = m_dynamicApi.DynamicRepost(ID, m_next);

            var results = await api.Request();
            if (!results.status)
            {
                throw new CustomizedErrorException(results.message);
            }

            var data = results.GetJObject();
            if (data["code"].ToInt32() != 0)
            {
                throw new CustomizedErrorException(data["message"].ToString());
            }

            var items = JsonConvert.DeserializeObject<List<DynamicCardModel>>(
                data["data"]?["items"]?.ToString() ?? "[]");

            var dynamicItemDisplayModels =
                new ObservableCollection<UserDynamicItemDisplayViewModel>();
            foreach (var item in items)
            {
                dynamicItemDisplayModels.Add(ConvertToDisplayRepost(item));
            }

            if (Items == null)
            {
                Items = dynamicItemDisplayModels;
            }
            else
            {
                foreach (var item in dynamicItemDisplayModels)
                {
                    Items.Add(item);
                }
            }

            CanLoadMore = (data["data"]?["has_more"]?.ToInt32() ?? 0) == 1;
            m_next = data["data"]?["has_more"]?.ToString() ?? "";
        }

        private UserDynamicItemDisplayViewModel ConvertToDisplayRepost(DynamicCardModel item)
        {
            var card = JObject.Parse(item.card);
            var data = new UserDynamicItemDisplayViewModel()
            {
                Datetime = TimeExtensions.TimestampToDatetime(item.desc.timestamp).ToString(),
                DynamicID = item.desc.dynamic_id,
                Mid = item.desc.uid,
                Time = item.desc.timestamp.HandelTimestamp(),
                UserDynamicItemDisplayCommands = new UserDynamicItemDisplayCommands()
                {
                    UserCommand = UserCommand
                }
            };
            var content = "";
            //内容
            if (card.ContainsKey("item") && card["item"]["content"] != null)
            {
                content = card["item"]["content"]?.ToString();
            }
            data.ContentStr = content;

            if (item.desc.user_profile == null) return data;
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
            return data;
        }

        #endregion
    }
}
