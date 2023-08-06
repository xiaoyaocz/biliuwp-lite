using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BiliLite.Controls.DataTemplateSelectors;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Dynamic;
using BiliLite.Models.Requests.Api.User;
using BiliLite.Models.Responses;
using BiliLite.Modules;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BiliLite.ViewModels.Home
{
    public class DynamicPageViewModel : BaseViewModel
    {
        #region Fields

        private readonly DynamicAPI m_dynamicApi;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        #endregion

        #region Constructors

        public DynamicPageViewModel()
        {
            m_dynamicApi = new DynamicAPI();
            DynamicItemDataTemplateSelector = new DynamicItemDataTemplateSelector();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
        }

        #endregion

        #region Properties

        public ICommand RefreshCommand { get; private set; }

        public ICommand LoadMoreCommand { get; private set; }

        public DynamicItemDataTemplateSelector DynamicItemDataTemplateSelector { get; set; }

        public bool Loading { get; set; } = true;

        public ObservableCollection<DynamicItemModel> Items { get; set; }

        #endregion

        #region Private Methods

        private async void LoadMore()
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
            await GetDynamicItems(last.Desc.DynamicId);
        }

        private JObject HandleRequestDynamicItemsResult(HttpResults results)
        {
            if (!results.status)
            {
                throw new CustomizedErrorException(results.message);
            }
            var data = results.GetJObject();
            if (data["code"].ToInt32() != 0)
            {
                throw new CustomizedErrorException(data["message"].ToString());
            }

            return data;
        }

        private void HandleDynamicItems(ObservableCollection<DynamicItemModel> items)
        {
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

        #endregion

        #region Public Methods

        public async Task GetDynamicItems(string idx = "")
        {
            try
            {
                Loading = true;
                var api = m_dynamicApi.DyanmicNew(DynamicAPI.UserDynamicType.Video);
                if (idx != "")
                {
                    api = m_dynamicApi.HistoryDynamic(idx, DynamicAPI.UserDynamicType.Video);
                }
                var results = await api.Request();

                // 处理http结果，可能抛出CustomizedErrorException异常
                var data = HandleRequestDynamicItemsResult(results);

                var items =
                    JsonConvert.DeserializeObject<ObservableCollection<DynamicItemModel>>(data["data"]["cards"]
                        .ToString());
                HandleDynamicItems(items);
            }
            catch (Exception ex)
            {
                if (ex is CustomizedErrorException)
                {
                    _logger.Error(ex.Message, ex);
                    Notify.ShowMessageToast(ex.Message);
                    return;
                }
                var handel = HandelError<DynamicPageViewModel>(ex);
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
            await GetDynamicItems();
        }

        #endregion
    }
}
