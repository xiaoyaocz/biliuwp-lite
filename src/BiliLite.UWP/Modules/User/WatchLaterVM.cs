using BiliLite.Models;
using BiliLite.Models.Requests.Api.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Services;

namespace BiliLite.Modules.User
{
    /// <summary>
    /// 稍后再看
    /// </summary>
    public class WatchLaterVM : IModules
    {
        private static WatchLaterVM _watchLaterVM;
        public static WatchLaterVM Instance
        {
            get
            {
                if (_watchLaterVM == null)
                {
                    _watchLaterVM = new WatchLaterVM();
                }
                return _watchLaterVM;
            }
        }


        readonly WatchLaterAPI watchLaterAPI;
        public WatchLaterVM()
        {
            watchLaterAPI = new WatchLaterAPI();
            AddCommand = new RelayCommand<string>(AddToWatchlater);
            RefreshCommand = new RelayCommand(Refresh);
            CleanCommand = new RelayCommand(Clear);
            DeleteCommand = new RelayCommand<WatchlaterItemModel>(Del);
            CleanViewedCommand = new RelayCommand(ClearViewed);
        }

        public ICommand AddCommand { get; private set; }
        public ICommand CleanCommand { get; private set; }
        public ICommand CleanViewedCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }
        public async void AddToWatchlater(string aid)
        {
            try
            {
                if (!SettingService.Account.Logined && await Notify.ShowLoginDialog())
                {
                    Notify.ShowMessageToast("请先登录");
                    return;
                }
                var results = await watchLaterAPI.Add(aid).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<object>>();
                    if (data.success)
                    {
                        Notify.ShowMessageToast("已添加到稍后再看");
                    }
                    else
                    {
                        Notify.ShowMessageToast(data.message);
                    }
                }
                else
                {
                    Notify.ShowMessageToast(results.message);
                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<object>(ex);
                Notify.ShowMessageToast("添加失败");
            }

        }

        private bool _loading = false;

        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }

        private bool _Nothing = false;
        public bool Nothing
        {
            get { return _Nothing; }
            set { _Nothing = value; DoPropertyChanged("Nothing"); }
        }


        private ObservableCollection<WatchlaterItemModel> _videos;
        public ObservableCollection<WatchlaterItemModel> Videos
        {
            get { return _videos; }
            set { _videos = value; DoPropertyChanged("Videos"); }
        }

        public async Task LoadData()
        {
            try
            {

                Loading = true;
                Nothing = false;
                var results = await watchLaterAPI.Watchlater().Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        var ls = JsonConvert.DeserializeObject<ObservableCollection<WatchlaterItemModel>>(data.data["list"].ToString());
                        if (ls == null || ls.Count == 0)
                        {
                            Nothing = true;

                        }
                        else
                        {
                            foreach (var item in ls)
                            {
                                item.DeleteCommand = DeleteCommand;
                            }
                        }
                        Videos = ls;


                    }
                    else
                    {
                        Notify.ShowMessageToast(data.message);
                    }
                }
                else
                {
                    Notify.ShowMessageToast(results.message);
                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<WatchLaterVM>(ex);
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
            Videos = null;
            await LoadData();
        }

        public async void Clear()
        {
            try
            {

                if (!await Notify.ShowDialog("清空稍后再看", "确定要清空全部视频吗?")) return;
                var results = await watchLaterAPI.Clear().Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        Videos.Clear();
                    }
                    else
                    {
                        Notify.ShowMessageToast(data.message);
                    }
                }
                else
                {
                    Notify.ShowMessageToast(results.message);
                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<WatchLaterVM>(ex);
                Notify.ShowMessageToast(handel.message);
            }
        }
        public async void ClearViewed()
        {
            try
            {
                if (!await Notify.ShowDialog("清除已观看", "确定要清空已观看视频吗?")) return;
                var results = await watchLaterAPI.Del().Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        Refresh();
                    }
                    else
                    {
                        Notify.ShowMessageToast(data.message);
                    }
                }
                else
                {
                    Notify.ShowMessageToast(results.message);
                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<WatchLaterVM>(ex);
                Notify.ShowMessageToast(handel.message);
            }
        }

        public async void Del(WatchlaterItemModel item)
        {
            try
            {

                var results = await watchLaterAPI.Del(item.aid).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        Videos.Remove(item);
                    }
                    else
                    {
                        Notify.ShowMessageToast(data.message);
                    }
                }
                else
                {
                    Notify.ShowMessageToast(results.message);
                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<WatchLaterVM>(ex);
                Notify.ShowMessageToast(handel.message);
            }
        }
    }

    public class WatchlaterItemModel
    {
        public ICommand DeleteCommand { get; set; }
        public string aid { get; set; }
        public int videos { get; set; }
        public string tname { get; set; }
        public string pic { get; set; }
        public string title { get; set; }
        public string desc { get; set; }
        public string dynamic { get; set; }
        public long cid { get; set; }
        public long add_at { get; set; }

        public WatchlaterOwnerModel owner { get; set; }

        public List<WatchlaterPagesModel> pages { get; set; }

        public int progress { get; set; }
        public int duration { get; set; }

        public string display
        {
            get
            {
                var ts = TimeSpan.FromSeconds(duration);
                return $"{videos}P {ts.ToString("c")}";
            }
        }

        public string state
        {
            get
            {
                if (progress == -1)
                {
                    return "已看完";
                }
                else
                {
                    if (progress != 0)
                    {
                        return "看到 " + TimeSpan.FromSeconds(progress).ToString(@"mm\:ss");
                    }
                    else
                    {
                        return "尚未观看";
                    }

                }
            }
        }

    }
    public class WatchlaterOwnerModel
    {
        public string face { get; set; }
        public long mid { get; set; }
        public string name { get; set; }
    }
    public class WatchlaterPagesModel
    {
        public long cid { get; set; }
        public string from { get; set; }
        public string has_alias { get; set; }
        public string link { get; set; }
        public string page { get; set; }
        public string part { get; set; }
        public int duration { get; set; }
        public string rich_vid { get; set; }
        public string vid { get; set; }
        public string weblink { get; set; }


    }
}
