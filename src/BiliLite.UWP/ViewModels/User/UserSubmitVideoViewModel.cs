using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AutoMapper;
using BiliLite.Extensions;
using BiliLite.Models.Common.User;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api.User;
using BiliLite.Modules;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BiliLite.ViewModels.User
{
    /// <summary>
    /// 视频投稿
    /// </summary>
    public class UserSubmitVideoViewModel : BaseViewModel
    {
        #region Fields

        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly UserDetailAPI m_userDetailApi;
        private SubmitVideoTlistItemModel m_selectTid;
        private readonly GrpcService m_grpcService;
        private readonly IMapper m_mapper;
        private string m_lastAid;

        #endregion

        #region Constructors

        public UserSubmitVideoViewModel(GrpcService grpcService, IMapper mapper)
        {
            m_grpcService = grpcService;
            m_mapper = mapper;
            m_userDetailApi = new UserDetailAPI();
            RefreshSubmitVideoCommand = new RelayCommand(Refresh);
            LoadMoreSubmitVideoCommand = new RelayCommand(LoadMore);
            Tlist = new ObservableCollection<SubmitVideoTlistItemModel>() {
                new SubmitVideoTlistItemModel()
                {
                    Name="全部视频",
                    Tid=0
                }
            };
            SelectTid = Tlist[0];
        }

        #endregion

        #region Properties

        public ICommand RefreshSubmitVideoCommand { get; private set; }
        public ICommand LoadMoreSubmitVideoCommand { get; private set; }

        public string Mid { get; set; }

        public int SelectOrder { get; set; } = 0;

        public bool LoadingSubmitVideo { get; set; } = true;

        public bool SubmitVideoCanLoadMore { get; set; }

        public ObservableCollection<SubmitVideoItemModel> SubmitVideoItems { get; set; }

        public ObservableCollection<SubmitVideoTlistItemModel> Tlist { get; set; }

        public SubmitVideoTlistItemModel SelectTid
        {
            get => m_selectTid;
            set { if (value == null) return; m_selectTid = value; }
        }

        public bool Nothing { get; set; }

        public int SubmitVideoPage { get; set; } = 1;

        public int CurrentTid { get; set; } = 0;

        public string Keyword { get; set; } = "";

        #endregion

        #region Private Methods

        private void GetSubmitVideoCore(JObject data) {
            var cursorItems = JsonConvert.DeserializeObject<List<SubmitVideoCursorItem>>(
                data["data"]["item"].ToString());
            var count = data["data"]["count"].ToInt32();
            var items = m_mapper.Map<List<SubmitVideoItemModel>>(cursorItems);
            AttachSubmitVideoItems(items, count);
            m_lastAid = cursorItems?.LastOrDefault()?.Aid;
        }

        private void AttachSubmitVideoItems(List<SubmitVideoItemModel> submitVideoItems, int count)
        {
            if (SubmitVideoItems == null)
            {
                var observableSubmitVideoItems = new ObservableCollection<SubmitVideoItemModel>(submitVideoItems);
                SubmitVideoItems = observableSubmitVideoItems;
            }
            else
            {
                foreach (var item in submitVideoItems)
                {
                    SubmitVideoItems.Add(item);
                }
            }

            if (SubmitVideoPage == 1 && (SubmitVideoItems == null || SubmitVideoItems.Count == 0))
            {
                Nothing = true;
            }
            if (SubmitVideoItems.Count >= count)
            {
                SubmitVideoCanLoadMore = false;
            }
            else
            {
                SubmitVideoCanLoadMore = true;
                SubmitVideoPage++;
            }
        }

        #endregion

        #region Public Methods

        public async Task GetSubmitVideo()
        {
            try
            {
                Nothing = false;
                SubmitVideoCanLoadMore = false;
                LoadingSubmitVideo = true;
                if (string.IsNullOrEmpty(Keyword))
                {
                    var api = m_userDetailApi.SubmitVideosCursor(Mid, order: (SubmitVideoOrder)SelectOrder, cursor: m_lastAid);
                    CurrentTid = SelectTid.Tid;
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

                    GetSubmitVideoCore(data);
                }
                else
                {
                    var searchArchive = await m_grpcService.SearchSpaceArchive(Mid, SubmitVideoPage, keyword: Keyword);

                    var submitVideoItems = m_mapper.Map<List<SubmitVideoItemModel>>(searchArchive.Archives);
                    AttachSubmitVideoItems(submitVideoItems, (int)searchArchive.Total);
                }
            }
            catch (Exception ex)
            {
                if (ex is CustomizedErrorException)
                {
                    Notify.ShowMessageToast(ex.Message);
                }
                else
                {
                    var handel = HandelError<UserSubmitVideoViewModel>(ex);
                    Notify.ShowMessageToast(handel.message);
                }

                _logger.Error("获取用户投稿失败", ex);
            }
            finally
            {
                LoadingSubmitVideo = false;
            }
        }

        public async void Refresh()
        {
            if (LoadingSubmitVideo)
            {
                return;
            }
            SubmitVideoItems = null;
            SubmitVideoPage = 1;
            m_lastAid = null;
            await GetSubmitVideo();
        }

        public async void LoadMore()
        {
            if (LoadingSubmitVideo)
            {
                return;
            }
            if (SubmitVideoItems == null || SubmitVideoItems.Count == 0)
            {
                return;
            }
            await GetSubmitVideo();
        }

        #endregion
    }
}
