using BiliLite.Models.Requests.Api.Live;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using BiliLite.Extensions;

namespace BiliLite.Modules.Live
{
    public class LiveAreaDetailVM : IModules
    {
        public int AreaID { get; set; }
        public int ParentAreaID { get; set; }


        readonly LiveAreaAPI liveAreaAPI;
        public LiveAreaDetailVM(int area_id, int parent_id)
        {
            liveAreaAPI = new LiveAreaAPI();
            AreaID = area_id;
            ParentAreaID = parent_id;
            Items = new ObservableCollection<LiveRecommendItemModel>();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
        }

        private bool _loading = false;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }
        private bool _CanLoadMore = false;
        public bool CanLoadMore
        {
            get { return _CanLoadMore; }
            set { _CanLoadMore = value; DoPropertyChanged("CanLoadMore"); }
        }
        public ICommand LoadMoreCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }
        private List<LiveTagItemModel> _tags;
        public List<LiveTagItemModel> Tags
        {
            get { return _tags; }
            set { _tags = value; DoPropertyChanged("Tags"); }
        }

        private LiveTagItemModel _selectTag;
        public LiveTagItemModel SelectTag
        {
            get { return _selectTag; }
            set { _selectTag = value; DoPropertyChanged("SelectTag"); }
        }
        public int Page { get; set; } = 1;
        public ObservableCollection<LiveRecommendItemModel> Items { get; set; }
        public async Task GetItems()
        {
            try
            {
                Loading = true;
                CanLoadMore = false;
                var results = await liveAreaAPI.LiveAreaRoomList(AreaID, ParentAreaID, Page, SelectTag?.sort_type ?? "").Request();
                if (results.status)
                {
                    var data = results.GetJObject();
                    if (data["code"].ToInt32() == 0)
                    {
                        if (Tags == null)
                        {
                            Tags = JsonConvert.DeserializeObject<List<LiveTagItemModel>>(data["data"]["new_tags"].ToString());
                            SelectTag = Tags[0];
                            SelectTag.Select = true;
                        }

                        var items = JsonConvert.DeserializeObject<List<LiveRecommendItemModel>>(data["data"]["list"].ToString());
                        if (items != null)
                        {
                            foreach (var item in items)
                            {
                                Items.Add(item);
                            }
                            if (Items.Count < data["data"]["count"].ToInt32())
                            {
                                Page++;
                                CanLoadMore = true;
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
                var handel = HandelError<LiveAreaDetailVM>(ex);
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
            Items.Clear();
            Page = 1;
            await GetItems();
        }
        public async void LoadMore()
        {
            if (Loading)
            {
                return;
            }
            await GetItems();
        }

    }
    public class LiveTagItemModel : INotifyPropertyChanged
    {
        public int id { get; set; }
        public string name { get; set; }
        public string sort_type { get; set; }
        public string sort { get; set; }

        private bool _select;
        public bool Select
        {
            get { return _select; }
            set { _select = value; DoPropertyChanged("Select"); }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void DoPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

}
