using System;
using System.Collections.Generic;
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
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BiliLite.ViewModels.Home
{
    public class DynamicPageViewModel : BaseViewModel
    {
        #region Fields

        private readonly DynamicAPI m_dynamicApi;
        private readonly GrpcService m_grpcService;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private string m_historyOffset = "";
        private string m_updateBaseline = "";
        private int m_page = 1;

        #endregion

        #region Constructors

        public DynamicPageViewModel()
        {
            m_grpcService = App.ServiceProvider.GetService<GrpcService>();
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

        private bool FirstGrpc => SettingService.GetValue<bool>(SettingConstants.Other.FIRST_GRPC_REQUEST_DYNAMIC, true);

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
            m_page++;
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

        private void HandleDynamicItems(List<DynamicItemModel> items)
        {
            if (Items == null)
            {
                Items = new ObservableCollection<DynamicItemModel>(items);
            }
            else
            {
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }
        }

        private async Task<List<DynamicItemModel>> GetDynItemsWithGrpc()
        {
            var result = await m_grpcService.GetDynVideo(m_page, m_historyOffset, m_updateBaseline);
            m_historyOffset = result.DynamicList.HistoryOffset;
            m_updateBaseline = result.DynamicList.UpdateBaseline;
            var items = result.DynamicList.List.MapToDynamicItemModels();
            return items;
        }

        private async Task<List<DynamicItemModel>> GetDynItems(string idx)
        {
            var api = m_dynamicApi.DyanmicNew(DynamicAPI.UserDynamicType.Video);
            if (idx != "")
            {
                api = m_dynamicApi.HistoryDynamic(idx, DynamicAPI.UserDynamicType.Video);
            }

            var results = await api.Request();

            // 处理http结果，可能抛出CustomizedErrorException异常
           var data = HandleRequestDynamicItemsResult(results);

            var items =
                JsonConvert.DeserializeObject<List<DynamicItemModel>>(data["data"]["cards"]
                    .ToString());
            return items;
        }

        #endregion

        #region Public Methods

        public async Task GetDynamicItems(string idx = "")
        {
            try
            {
                Loading = true;
                if (string.IsNullOrEmpty(idx))
                {
                    m_historyOffset = "";
                    m_updateBaseline = "";
                    m_page = 1;
                }

                List<DynamicItemModel> items;

                try
                {
                    if (FirstGrpc)
                    {
                        items = await GetDynItemsWithGrpc();
                    }
                    else
                    {
                        items = await GetDynItems(idx);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message, ex);
                    if (FirstGrpc)
                    {
                        items = await GetDynItems(idx);
                    }
                    else
                    {
                        items = await GetDynItemsWithGrpc();
                    }
                }

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
