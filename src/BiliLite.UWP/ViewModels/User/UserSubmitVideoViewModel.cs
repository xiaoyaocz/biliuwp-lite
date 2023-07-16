using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Models.Common.User;
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

        #endregion

        #region Constructors

        public UserSubmitVideoViewModel()
        {
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

        private void GetSubmitVideoCore(JObject data)
        {
            if (Tlist.Count == 1)
            {
                foreach (var item in data["data"]["list"]["tlist"])
                {
                    Tlist.Add(JsonConvert.DeserializeObject<SubmitVideoTlistItemModel>(item.First.ToString()));
                }
            }
            var items = JsonConvert.DeserializeObject<ObservableCollection<SubmitVideoItemModel>>(
                data["data"]["list"]["vlist"].ToString());
            if (SubmitVideoItems == null)
            {
                SubmitVideoItems = items;
            }
            else
            {
                foreach (var item in items)
                {
                    SubmitVideoItems.Add(item);
                }
            }

            if (SubmitVideoPage == 1 && (SubmitVideoItems == null || SubmitVideoItems.Count == 0))
            {
                Nothing = true;
            }
            var count = data["data"]["page"]["count"].ToInt32();
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
                var api = await m_userDetailApi.SubmitVideos(Mid, SubmitVideoPage, keyword: Keyword, tid: SelectTid.Tid,
                    order: (SubmitVideoOrder)SelectOrder);
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
            catch (Exception ex)
            {
                if (ex is CustomizedErrorException)
                {
                    Notify.ShowMessageToast(ex.Message);
                }
                else
                {
                    var handel = HandelError<AnimeHomeModel>(ex);
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
