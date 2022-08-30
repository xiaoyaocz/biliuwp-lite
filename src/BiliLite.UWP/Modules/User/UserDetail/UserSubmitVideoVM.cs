using BiliLite.Api.User;
using BiliLite.Helpers;
using BiliLite.Models;
using Newtonsoft.Json;
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
    /// 视频投稿
    /// </summary>
    public class UserSubmitVideoVM : IModules
    {
        public string mid { get; set; }
        private readonly UserDetailAPI userDetailAPI;
        public UserSubmitVideoVM()
        {
            userDetailAPI = new UserDetailAPI();
            RefreshSubmitVideoCommand = new RelayCommand(Refresh);
            LoadMoreSubmitVideoCommand = new RelayCommand(LoadMore);
            Tlist = new ObservableCollection<SubmitVideoTlistItemModel>() {
                new SubmitVideoTlistItemModel()
                {
                    name="全部视频",
                    tid=0
                }
            };
            SelectTid = Tlist[0];
        }

        public int SelectOrder { get; set; } = 0;



        private bool _LoadingSubmitVideo = true;
        public bool LoadingSubmitVideo
        {
            get { return _LoadingSubmitVideo; }
            set { _LoadingSubmitVideo = value; DoPropertyChanged("LoadingSubmitVideo"); }
        }
        private bool _SubmitVideoCanLoadMore = false;
        public bool SubmitVideoCanLoadMore
        {
            get { return _SubmitVideoCanLoadMore; }
            set { _SubmitVideoCanLoadMore = value; DoPropertyChanged("SubmitVideoCanLoadMore"); }
        }
        public ICommand RefreshSubmitVideoCommand { get; private set; }
        public ICommand LoadMoreSubmitVideoCommand { get; private set; }
        private ObservableCollection<SubmitVideoItemModel> _SubmitVideoItems;
        public ObservableCollection<SubmitVideoItemModel> SubmitVideoItems
        {
            get { return _SubmitVideoItems; }
            set { _SubmitVideoItems = value; DoPropertyChanged("SubmitVideoItems"); }
        }

        private ObservableCollection<SubmitVideoTlistItemModel> _tlist;

        public ObservableCollection<SubmitVideoTlistItemModel> Tlist
        {
            get { return _tlist; }
            set { _tlist = value; DoPropertyChanged("Tlist"); }
        }

        private SubmitVideoTlistItemModel _selectTid;

        public SubmitVideoTlistItemModel SelectTid
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

        public int SubmitVideoPage { get; set; } = 1;
        public int CurrentTid { get; set; } = 0;
        public string Keyword { get; set; } = "";

        public async Task GetSubmitVideo()
        {
            try
            {
                Nothing = false;
                SubmitVideoCanLoadMore = false;
                LoadingSubmitVideo = true;
                var api = userDetailAPI.SubmitVideos(mid, SubmitVideoPage, keyword: Keyword, tid: SelectTid.tid, order: (SubmitVideoOrder)SelectOrder);
                CurrentTid = SelectTid.tid;
                var results = await api.Request();
                if (results.status)
                {
                    var data = results.GetJObject();
                    if (data["code"].ToInt32() == 0)
                    {
                        if (Tlist.Count == 1)
                        {
                            ObservableCollection<SubmitVideoTlistItemModel> _tlist = new ObservableCollection<SubmitVideoTlistItemModel>();
                            foreach (var item in data["data"]["list"]["tlist"])
                            {
                                Tlist.Add(JsonConvert.DeserializeObject<SubmitVideoTlistItemModel>(item.First.ToString()));
                            }


                        }

                        var items = JsonConvert.DeserializeObject<ObservableCollection<SubmitVideoItemModel>>(data["data"]["list"]["vlist"].ToString());
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
                    else
                    {
                        Utils.ShowMessageToast(data["message"].ToString());
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
    }
    public class SubmitVideoTlistItemModel
    {
        public int tid { get; set; }
        public string name { get; set; }
        public int count { get; set; }
    }
    public class SubmitVideoItemModel
    {
        public int comment { get; set; }
        public string play { get; set; }

        public string pic { get; set; }
        public string description { get; set; }
        public string title { get; set; }
        public string author { get; set; }
        public string length { get; set; }
        public string aid { get; set; }
        public long created { get; set; }
        public int video_review { get; set; }
    }

}
