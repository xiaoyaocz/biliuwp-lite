using BiliLite.Helpers;
using Microsoft.Toolkit.Collections;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BiliLite.Modules
{
    public class DynamicVM : IModules
    {
        readonly Api.User.DynamicAPI dynamicAPI;
        public DynamicVM()
        {
            dynamicAPI = new Api.User.DynamicAPI();
            dynamicItemDataTemplateSelector = new DynamicItemDataTemplateSelector();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
        }
        public DynamicItemDataTemplateSelector dynamicItemDataTemplateSelector { get; set; }
        private bool _loading = true;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }
        public ICommand RefreshCommand { get; private set; }
        public ICommand LoadMoreCommand { get; private set; }
        private ObservableCollection<DynamicItemModel> _Items;

        public ObservableCollection<DynamicItemModel> Items
        {
            get { return _Items; }
            set { _Items = value; DoPropertyChanged("Items"); }
        }

        public async Task GetDynamicItems(string idx = "")
        {
            try
            {
                Loading = true;
                var api = dynamicAPI.DyanmicNew(Api.User.DynamicAPI.UserDynamicType.Video);
                if (idx != "")
                {
                    api = dynamicAPI.HistoryDynamic(idx, Api.User.DynamicAPI.UserDynamicType.Video);
                }
                var results = await api.Request();
                if (results.status)
                {
                    var data = results.GetJObject();
                    if (data["code"].ToInt32() == 0)
                    {
                        var items = JsonConvert.DeserializeObject<ObservableCollection<DynamicItemModel>>(data["data"]["cards"].ToString());
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
            await GetDynamicItems(last.desc.dynamic_id);
        }



      
    }
    public class DynamicItemModel
    {
        /// <summary>
        /// json字符串
        /// </summary>
        public string extend_json { get; set; }
        /// <summary>
        /// json字符串,根据desc里的type，获得数据
        /// </summary>
        public string card { get; set; }
        public DynamicDescModel desc { get; set; }
        public DynamicVideoCardModel video
        {
            get
            {
                if (desc != null && desc.type == 8)
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<DynamicVideoCardModel>(card);
                }
                return null;
            }
        }
        public DynamicSeasonCardModel season
        {
            get
            {
                if (desc != null && desc.type == 512)
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<DynamicSeasonCardModel>(card);
                }
                return null;
            }
        }
    }
    public class DynamicItemDataTemplateSelector : DataTemplateSelector
    {
        public ResourceDictionary resource;
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var card = item as DynamicItemModel;
            if (card.desc.type == 8)
            {
                return resource["DynamicVideo"] as DataTemplate;
            }
            else
            {
                return resource["DynamicSeason"] as DataTemplate;
            }
        }
    }
    public class DynamicDescModel
    {
        public string uid { get; set; }
        /// <summary>
        /// 8=视频，512=番剧
        /// </summary>
        public int type { get; set; }
        public string rid { get; set; }
        public int view { get; set; }
        public int like { get; set; }
        public int comment { get; set; }
        public int is_liked { get; set; }
        public string dynamic_id_str { get; set; }
        public string dynamic_id { get; set; }
        public int status { get; set; }
        public long timestamp { get; set; }
    }
    public class DynamicVideoCardModel
    {
        public string aid { get; set; }
        public int attribute { get; set; }
        public string cid { get; set; }
        public long ctime { get; set; }
        public string desc { get; set; }
        public int duration { get; set; }
        public string dynamic { get; set; }
        public string jump_url { get; set; }
        public DynamicVideoCardOwnerModel owner { get; set; }
        public string pic { get; set; }
        public long pubdate { get; set; }
        public string title { get; set; }
        public DynamicVideoCardStatModel stat { get; set; }
    }
    public class DynamicVideoCardStatModel
    {
        public int coin { get; set; }
        public int danmaku { get; set; }
        public int favorite { get; set; }
        public int like { get; set; }
        public int reply { get; set; }
        public int share { get; set; }
        public int view { get; set; }
    }
    public class DynamicVideoCardOwnerModel
    {
        public string face { get; set; }
        public string mid { get; set; }
        public string name { get; set; }
    }
    public class DynamicSeasonCardModel
    {
        public string aid { get; set; }
        public string cover { get; set; }
        public string index_title { get; set; }
        public string index { get; set; }
        public string new_desc { get; set; }
        public string url { get; set; }
        public int play_count { get; set; }
        public int reply_count { get; set; }
        public int bullet_count { get; set; }
        public int episode_id { get; set; }
        public DynamicSeasonCardApiSeasonInfoModel season { get; set; }
    }
    public class DynamicSeasonCardApiSeasonInfoModel
    {
        public string type_name { get; set; }
        public string cover { get; set; }
        public string title { get; set; }
        public int is_finish { get; set; }
        public int season_id { get; set; }
    }
}
