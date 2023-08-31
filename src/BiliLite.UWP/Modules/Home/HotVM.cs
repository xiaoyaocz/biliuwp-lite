using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.Windows.Input;
using BiliLite.Extensions;
using BiliLite.Models.Common.Recommend;
using BiliLite.Models.Requests.Api.Home;

namespace BiliLite.Modules
{
    public class HotVM : IModules
    {
        readonly HotAPI hotAPI;
        public HotVM()
        {
            hotAPI = new HotAPI();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
        }
        public ICommand RefreshCommand { get; private set; }
        public ICommand LoadMoreCommand { get; private set; }

        private bool _loading = true;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }

        private ObservableCollection<HotDataItemModel> _hotItems;

        public ObservableCollection<HotDataItemModel> HotItems
        {
            get { return _hotItems; }
            set { _hotItems = value; DoPropertyChanged("HotItems"); }
        }
        private List<HotTopItemModel> _topItems;

        public List<HotTopItemModel> TopItems
        {
            get { return _topItems; }
            set { _topItems = value; DoPropertyChanged("TopItems"); }
        }

        public async Task GetPopular(string idx = "0", string last_param = "")
        {
            try
            {
                Loading = true;

                var results = await hotAPI.Popular(idx, last_param).Request();
                if (results.status)
                {
                    var data = results.GetJObject();
                    if (data["code"].ToInt32() == 0)
                    {
                        if (TopItems == null)
                        {
                            TopItems = JsonConvert.DeserializeObject<List<HotTopItemModel>>(data["config"]["top_items"].ToString());
                        }
                        var items = JsonConvert.DeserializeObject<ObservableCollection<HotDataItemModel>>(data["data"].ToString());
                        for (int i = items.Count - 1; i >= 0; i--)
                        {
                            if (items[i].card_goto != "av")
                                items.Remove(items[i]);
                        }
                        if (HotItems == null)
                        {
                            HotItems = items;
                        }
                        else
                        {
                            foreach (var item in items)
                            {
                                HotItems.Add(item);
                            }
                        }
                    }
                    else
                    {
                        Notify.ShowMessageToast(data["message"].ToString());
                    }
                }
                else
                {
                    Notify.ShowMessageToast(results.message);

                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<HotVM>(ex);
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
            TopItems = null;
            HotItems = null;
            await GetPopular();
        }
        public async void LoadMore()
        {
            if (Loading)
            {
                return;
            }
            if (HotItems == null || HotItems.Count == 0)
            {
                return;
            }
            var last = HotItems.LastOrDefault();
            await GetPopular(last.idx, last.param);
        }
    }
    public class HotTopItemModel
    {
        public int entrance_id { get; set; }
        public string icon { get; set; }
        public string module_id { get; set; }
        public string uri { get; set; }
        public string title { get; set; }
    }
    public class HotDataItemModel
    {
        public string card_type { get; set; }
        public string card_goto { get; set; }
        public string param { get; set; }
        public string cover { get; set; }
        public string title { get; set; }
        public string idx { get; set; }
        public string uri { get; set; }
        public string cover_right_text_1 { get; set; }
        public string right_desc_1 { get; set; }
        public string right_desc_2 { get; set; }
        public RecommendRcmdReasonStyleModel rcmd_reason_style { get; set; }
    }
}
