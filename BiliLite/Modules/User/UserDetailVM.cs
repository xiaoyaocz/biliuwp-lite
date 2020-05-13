using BiliLite.Api.User;
using BiliLite.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BiliLite.Modules.User
{
    public class UserDetailVM:IModules
    {
        public string mid { get; set; }
        private readonly UserDetailAPI userDetailAPI;
        public UserDetailVM()
        {
            userDetailAPI = new UserDetailAPI();
            RefreshSubmitVideoCommand = new RelayCommand(Refresh);
            LoadMoreSubmitVideoCommand = new RelayCommand(LoadMore);
        }


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
        public int SubmitVideoPage { get; set; } = 1;
   
        public async Task GetSubmitVideo()
        {
            try
            {
                SubmitVideoCanLoadMore = false;
                LoadingSubmitVideo = true;
                var api = userDetailAPI.SubmitVideos(mid, SubmitVideoPage);
               
                var results = await api.Request();
                if (results.status)
                {
                    var data = results.GetJObject();
                    if (data["code"].ToInt32() == 0)
                    {
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
                        var count = data["data"]["page"]["count"].ToInt32();
                        if (SubmitVideoItems.Count>= count)
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
   
    public class SubmitVideoItemModel
    {
        public int comment { get; set; }
        public string play { get; set; }

        private string _pic;

        public string pic
        {
            get { return _pic.Replace("//","http://"); }
            set { _pic = value; }
        }
        public string description { get; set; }
        public string title { get; set; }
        public string author { get; set; }
        public string length { get; set; }
        public string aid { get; set; }
        public long created { get; set; }
        public int video_review { get; set; }
    }

}
