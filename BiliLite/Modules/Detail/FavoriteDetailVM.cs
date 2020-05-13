using BiliLite.Helpers;
using BiliLite.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BiliLite.Modules
{
    public class FavoriteDetailVM:IModules
    {
        readonly Api.User.FollowAPI followAPI;
        public int Page { get; set; } = 1;
        public string Keyword { get; set; } = "";
        public string Fid { get; set; }
        public FavoriteDetailVM()
        {
            followAPI = new Api.User.FollowAPI();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
        }
        private bool _loading = false;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }
        private FavoriteInfoModel _FavoriteInfo;
        public FavoriteInfoModel FavoriteInfo
        {
            get { return _FavoriteInfo; }
            set { _FavoriteInfo = value; DoPropertyChanged("FavoriteInfo"); }
        }
        private ObservableCollection<FavoriteInfoVideoItemModel> _videos;
        public ObservableCollection<FavoriteInfoVideoItemModel> Videos
        {
            get { return _videos; }
            set { _videos = value; DoPropertyChanged("Videos"); }
        }
      
        public ICommand RefreshCommand { get; private set; }
        public ICommand LoadMoreCommand { get; private set; }
        private bool _Nothing = false;
        public bool Nothing
        {
            get { return _Nothing; }
            set { _Nothing = value; DoPropertyChanged("Nothing"); }
        }

        private bool _ShowLoadMore = false;
        public bool ShowLoadMore
        {
            get { return _ShowLoadMore; }
            set { _ShowLoadMore = value; DoPropertyChanged("ShowLoadMore"); }
        }

        public async Task LoadFavoriteInfo()
        {
            try
            {
                ShowLoadMore = false;
                Loading = true;
                Nothing = false;
                var results = await followAPI.FavoriteInfo(Fid, Keyword,Page).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<FavoriteDetailModel>>();
                    if (data.success)
                    {
                        if (Page == 1)
                        {
                            FavoriteInfo = data.data.info;
                            if (data.data.medias==null|| data.data.medias.Count==0)
                            {
                                Nothing = true;
                                return;
                            }
                            Videos = data.data.medias;
                           
                        }
                        else
                        {
                            if (data.data.medias != null)
                            {
                                foreach (var item in data.data.medias)
                                {
                                    Videos.Add(item);
                                }
                            }
                        }
                        if (Videos.Count != FavoriteInfo.media_count)
                        {
                            ShowLoadMore = true;
                            Page++;
                        }
                    }
                    else
                    {
                        Utils.ShowMessageToast(data.message);
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
                Loading = false;
            }
        }
        public async void Refresh()
        {
            if (Loading)
            {
                return;
            }
            Page = 1;
            FavoriteInfo = null;
            Videos = null;
            await LoadFavoriteInfo();
        }
        public async void LoadMore()
        {
            if (Loading)
            {
                return;
            }
            if (Videos == null || Videos.Count == 0)
            {
                return;
            }
            await LoadFavoriteInfo();
        }
        public async void Search(string keyword)
        {
            if (Loading)
            {
                return;
            }
            Keyword = keyword;
            Page = 1;
            FavoriteInfo = null;
            Videos = null;
            await LoadFavoriteInfo();
        }
    }
    public class FavoriteDetailModel
    {
        public FavoriteInfoModel info { get; set; }
        public ObservableCollection<FavoriteInfoVideoItemModel> medias { get; set; }
    }

    public class FavoriteInfoModel
    {
        public string cover { get; set; }
        public int attr { get; set; }
        public bool privacy
        {
            get
            {
                return attr == 2;
            }
        }
        public string fid { get; set; }
        public string id { get; set; }
        public int like_state { get; set; }
        public string mid { get; set; }
        public string title { get; set; }
        public int type { get; set; }
        public int media_count { get; set; }
        public FavoriteInfoVideoItemUpperModel upper { get; set; }
    }

    public class FavoriteInfoVideoItemModel
    {
        public string id { get; set; }
        public string cover { get; set; }
        public string title { get; set; }
        public FavoriteInfoVideoItemUpperModel upper { get; set; }
        public FavoriteInfoVideoItemStatModel cnt_info { get; set; }
    }
    public class FavoriteInfoVideoItemUpperModel
    {
        public string face { get; set; }
        public string name { get; set; }
        public string mid { get; set; }
    }
    public class FavoriteInfoVideoItemStatModel
    {
        public int coin { get; set; }
        public int collect { get; set; }
        public int danmaku { get; set; }
        public int play { get; set; }
        public int reply { get; set; }
        public int share { get; set; }
    }
}
