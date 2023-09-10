using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using AutoMapper;
using BiliLite.Controls.Dynamic;
using BiliLite.Dialogs;
using BiliLite.Extensions;
using BiliLite.Models;
using BiliLite.Models.Builders;
using BiliLite.Models.Common;
using BiliLite.Models.Common.UserDynamic;
using BiliLite.Models.Dynamic;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api.User;
using BiliLite.Models.Responses;
using BiliLite.Modules;
using BiliLite.Modules.User;
using BiliLite.Pages;
using BiliLite.Pages.User;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static BiliLite.Models.Requests.Api.User.DynamicAPI;
using DynamicItemDataTemplateSelector = BiliLite.Controls.DataTemplateSelectors.DynamicItemDataTemplateSelector;

namespace BiliLite.ViewModels.UserDynamic
{
    public class UserDynamicViewModel : BaseViewModel
    {
        #region Fields

        private readonly WatchLaterVM m_watchLaterVm;
        private readonly DynamicAPI m_dynamicApi;
        private DynamicItemDataTemplateSelector m_dynamicItemDataTemplateSelector;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly IMapper m_mapper;

        #endregion

        #region Constructors

        public UserDynamicViewModel()
        {
            m_mapper = App.ServiceProvider.GetRequiredService<IMapper>();
            m_dynamicApi = new DynamicAPI();
            m_watchLaterVm = new WatchLaterVM();
            m_dynamicItemDataTemplateSelector = new DynamicItemDataTemplateSelector();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);

            UserCommand = new RelayCommand<object>(OpenUser);
            LaunchUrlCommand = new RelayCommand<object>(LaunchUrl);
            WebCommand = new RelayCommand<object>(OpenWeb);
            TagCommand = new RelayCommand<object>(OpenTag);
            LotteryCommand = new RelayCommand<object>(OpenLottery);
            VoteCommand = new RelayCommand<object>(OpenVote);
            ImageCommand = new RelayCommand<object>(OpenImage);
            CommentCommand = new RelayCommand<UserDynamicItemDisplayViewModel>(OpenComment);
            DeleteCommand = new RelayCommand<UserDynamicItemDisplayViewModel>(Delete);
            LikeCommand = new RelayCommand<UserDynamicItemDisplayViewModel>(DoLike);
            RepostCommand = new RelayCommand<UserDynamicItemDisplayViewModel>(OpenSendDynamicDialog);
            DetailCommand = new RelayCommand<UserDynamicItemDisplayViewModel>(OpenDetail);
        }

        #endregion

        #region Properties

        public ICommand VoteCommand { get; set; }
        public ICommand UserCommand { get; set; }
        public ICommand LotteryCommand { get; set; }
        public ICommand LaunchUrlCommand { get; set; }
        public ICommand WebCommand { get; set; }
        public ICommand TagCommand { get; set; }
        public ICommand ImageCommand { get; set; }
        public ICommand CommentCommand { get; set; }
        public ICommand RepostCommand { get; set; }
        public ICommand LikeCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand DetailCommand { get; set; }
        public ICommand RefreshCommand { get; private set; }
        public ICommand LoadMoreCommand { get; private set; }
        public bool Loading { get; set; } = true;
        public bool CanLoadMore { get; set; }
        public DynamicType DynamicType { get; set; } = DynamicType.UserDynamic;
        public string Uid { get; set; }
        public UserDynamicType UserDynamicType { get; set; } = UserDynamicType.All;

        public ObservableCollection<UserDynamicItemDisplayViewModel> Items { get; set; }

        #endregion

        #region Events

        public event EventHandler<UserDynamicItemDisplayViewModel> OpenCommentEvent;

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
            await GetDynamicItems(last.DynamicID);
        }

        private void OpenUser(object id)
        {
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Contact,
                page = typeof(UserInfoPage),
                title = "用户中心",
                parameters = id
            });
        }

        private async void LaunchUrl(object url)
        {
            var result = await MessageCenter.HandelUrl(url.ToString());
            if (!result)
            {
                Notify.ShowMessageToast("无法打开Url");
            }
        }

        private void OpenWeb(object url)
        {
            MessageCenter.NavigateToPage(null, new NavigationInfo()
            {
                icon = Symbol.World,
                page = typeof(WebPage),
                title = "加载中...",
                parameters = url
            });
        }

        private void OpenImage(object data)
        {
            var info = data as UserDynamicItemDisplayImageInfo;
            MessageCenter.OpenImageViewer(info.AllImages, info.Index);
        }

        private void OpenTag(object name)
        {
            //TODO 打开话题
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.World,
                page = typeof(WebPage),
                title = name.ToString(),
                parameters = "https://t.bilibili.com/topic/name/" + Uri.EscapeDataString(name.ToString())
            });
        }

        private async void OpenLottery(object id)
        {
            var contentDialog = new ContentDialog()
            {
                IsPrimaryButtonEnabled = true,
                Title = "抽奖",
                PrimaryButtonText = "关闭"
            };
            contentDialog.PrimaryButtonClick += new TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs>((sender, e) =>
            {
                contentDialog.Hide();
            });
            contentDialog.Content = new WebView()
            {
                Width = 500,
                Height = 500,
                Source = new Uri($"https://t.bilibili.com/lottery/h5/index/#/result?business_id={id.ToString()}&business_type=1&isWeb=1"),
            };
            await contentDialog.ShowAsync();
        }

        private void OpenVote(object id)
        {
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Document,
                page = typeof(WebPage),
                title = "投票",
                parameters = $"https://t.bilibili.com/vote/h5/index/#/result?vote_id={id.ToString()}"
            });
        }

        private void OpenComment(UserDynamicItemDisplayViewModel data)
        {
            OpenCommentEvent?.Invoke(this, data);
        }

        private void OpenDetail(UserDynamicItemDisplayViewModel data)
        {
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Document,
                page = typeof(DynamicDetailPage),
                title = "动态详情",
                parameters = data.DynamicID
            });
        }

        private async Task<bool> ConfirmDeleteDialog()
        {
            var messageDialog = new MessageDialog("确定要删除动态吗?", "删除动态");
            messageDialog.Commands.Add(new UICommand("确定", cmd => { }, commandId: 0));
            messageDialog.Commands.Add(new UICommand("取消", cmd => { }, commandId: 1));
            var result = await messageDialog.ShowAsync();
            return result.Id.ToInt32() != 1;
        }

        private async Task DeleteCore(UserDynamicItemDisplayViewModel item)
        {
            var results = await m_dynamicApi.Delete(item.DynamicID).Request();
            if (!results.status)
            {
                throw new CustomizedErrorException(results.message);
            }

            var data = await results.GetJson<ApiDataModel<object>>();
            if (!data.success)
            {
                throw new CustomizedErrorException(data.message);
            }

            Items.Remove(item);
        }

        private async void Delete(UserDynamicItemDisplayViewModel item)
        {
            if (!await BiliExtensions.ActionCheckLogin()) return;

            if (!await ConfirmDeleteDialog()) return;
            try
            {
                await DeleteCore(item);
            }
            catch (CustomizedErrorException ex)
            {
                Notify.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message);
            }
            catch (Exception ex)
            {
                var handel = HandelError<object>(ex);
                Notify.ShowMessageToast(handel.message);
            }
        }

        private async Task DoLikeCore(UserDynamicItemDisplayViewModel item)
        {
            var results = await m_dynamicApi.Like(item.DynamicID, item.Liked ? 2 : 1).Request();
            if (!results.status)
            {
                throw new CustomizedErrorException(results.message);
            }

            var data = await results.GetJson<ApiDataModel<object>>();
            if (!data.success)
            {
                throw new CustomizedErrorException(data.message);
            }

            if (item.Liked)
            {
                item.Liked = false;
                item.LikeCount -= 1;
            }
            else
            {
                item.Liked = true;
                item.LikeCount += 1;
            }
        }

        private async void DoLike(UserDynamicItemDisplayViewModel item)
        {
            if (!await BiliExtensions.ActionCheckLogin()) return;

            try
            {
                await DoLikeCore(item);
            }
            catch (CustomizedErrorException ex)
            {
                Notify.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message);
            }
            catch (Exception ex)
            {
                var handel = HandelError<object>(ex);
                Notify.ShowMessageToast(handel.message);
            }
        }

        private async void OpenSendDynamicDialog(UserDynamicItemDisplayViewModel data)
        {
            if (!await BiliExtensions.ActionCheckLogin()) return;

            var sendDynamicDialog = new SendDynamicDialog();
            if (data != null)
            {
                sendDynamicDialog = new SendDynamicDialog(data);
            }
            await sendDynamicDialog.ShowAsync();
        }

        private UserDynamicItemDisplayViewModel ConvertToDisplay(DynamicCardModel item)
        {
            try
            {
                var card = JObject.Parse(item.card);
                var extendJson = JObject.Parse(item.extend_json);
                var dynDisplayModel = new DynamicItemDisplayModelBuilder()
                    .Init(m_mapper, item, card)
                    .SetCommands(new UserDynamicItemDisplayCommands()
                    {
                        UserCommand = UserCommand,
                        LaunchUrlCommand = LaunchUrlCommand,
                        WebCommand = WebCommand,
                        TagCommand = TagCommand,
                        LotteryCommand = LotteryCommand,
                        VoteCommand = VoteCommand,
                        ImageCommand = ImageCommand,
                        CommentCommand = CommentCommand,
                        LikeCommand = LikeCommand,
                        DeleteCommand = DeleteCommand,
                        RepostCommand = RepostCommand,
                        DetailCommand = DetailCommand,
                        WatchLaterCommand = m_watchLaterVm.AddCommand,
                    })
                    .SwitchType(m_mapper, card)
                    .SetCommentCount(card)
                    .SetContent(card, extendJson)
                    .SetSeasonInfo(card)
                    .SetUserProfile()
                    .Build();

                return dynDisplayModel;
            }
            catch (DynamicDisplayTypeUnsupportedException ex)
            {
                return ex.ViewModel;
            }
            catch (Exception ex)
            {
                _logger.Error("动态加载失败", ex);
                var model = new UserDynamicItemDisplayViewModel
                {
                    Type = UserDynamicDisplayType.Other,
                    IntType = 999,
                    DynamicID = item.desc.dynamic_id,
                    ContentStr = "动态加载失败:\r\n" + JsonConvert.SerializeObject(item)
                };
                return model;
            }
        }

        private async Task<HttpResults> RequestGetDynamic(string idx = "")
        {
            var api = m_dynamicApi.DyanmicNew(UserDynamicType);
            // TODO: switch改字典存储方法调用
            switch (DynamicType)
            {
                case DynamicType.UserDynamic:
                    if (idx != "")
                    {
                        api = m_dynamicApi.HistoryDynamic(idx, UserDynamicType);
                    }

                    break;
                case DynamicType.Topic:
                    break;
                case DynamicType.Space:
                    api = m_dynamicApi.SpaceHistory(Uid, idx);
                    break;
                default:
                    break;
            }

            var results = await api.Request();
            return results;
        }

        private void HandleDynamicResults(HttpResults results)
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

            var items = JsonConvert.DeserializeObject<List<DynamicCardModel>>(
                data["data"]?["cards"]?.ToString() ?? "[]");
            if (items.Count > 0)
            {
                CanLoadMore = true;
            }

            var displayModels =
                new ObservableCollection<UserDynamicItemDisplayViewModel>();
            foreach (var item in items)
            {
                displayModels.Add(ConvertToDisplay(item));
            }

            if (Items == null)
            {
                Items = displayModels;
            }
            else
            {
                foreach (var item in displayModels)
                {
                    Items.Add(item);
                }
            }
        }

        private async Task GetDynamicDetailCore(string id)
        {
            var api = m_dynamicApi.DynamicDetail(id);

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

            var items = JsonConvert.DeserializeObject<DynamicCardModel>(data["data"]["card"].ToString());
            var displayModels =
                new ObservableCollection<UserDynamicItemDisplayViewModel>();
            displayModels.Add(ConvertToDisplay(items));
            Items = displayModels;
        }

        #endregion

        #region Public Methods

        public async void Refresh()
        {
            if (Loading)
            {
                return;
            }
            Items = null;
            await GetDynamicItems();
        }

        public async Task GetDynamicItems(string idx = "")
        {
            try
            {
                CanLoadMore = false;
                Loading = true;

                var results = await RequestGetDynamic(idx);

                HandleDynamicResults(results);
            }
            catch (CustomizedErrorException ex)
            {
                Notify.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message);
            }
            catch (Exception ex)
            {
                var handel = HandelError<UserDynamicViewModel>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }

        public async Task GetDynamicDetail(string id)
        {
            try
            {
                CanLoadMore = false;
                Loading = true;
                await GetDynamicDetailCore(id);
            }
            catch (CustomizedErrorException ex)
            {
                Notify.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message);
            }
            catch (Exception ex)
            {
                var handel = HandelError<UserDynamicViewModel>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }

        #endregion
    }
}
