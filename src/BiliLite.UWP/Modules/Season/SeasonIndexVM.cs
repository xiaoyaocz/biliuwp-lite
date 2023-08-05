using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.Windows.Input;
using BiliLite.Extensions;
using BiliLite.Models.Requests.Api;

namespace BiliLite.Modules
{
    public enum IndexSeasonType
    {
        Anime = 1,
        Movie = 2,
        Documentary = 3,
        Guochuang = 4,
        TV = 5,
        Variety = 7
    }
    public class SeasonIndexParameter
    {
        public IndexSeasonType type { get; set; } = IndexSeasonType.Anime;
        public string area { get; set; } = "-1";
        public string style { get; set; } = "-1";
        public string year { get; set; } = "-1";
        public string month { get; set; } = "-1";
        public string order { get; set; } = "3";
    }
    public class SeasonIndexVM : IModules
    {
        readonly SeasonIndexAPI seasonIndexAPI;
        public SeasonIndexVM()
        {
            seasonIndexAPI = new SeasonIndexAPI();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
        }
        public SeasonIndexParameter Parameter { get; set; }

        public ICommand RefreshCommand { get; private set; }
        public ICommand LoadMoreCommand { get; private set; }

        private bool _loading = true;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }


        private bool CanLoadMore = true;


        private bool _Conditionsloading = true;
        public bool ConditionsLoading
        {
            get { return _Conditionsloading; }
            set { _Conditionsloading = value; DoPropertyChanged("ConditionsLoading"); }
        }

        private ObservableCollection<SeasonIndexConditionFilterModel> _Conditions;
        public ObservableCollection<SeasonIndexConditionFilterModel> Conditions
        {
            get { return _Conditions; }
            set { _Conditions = value; DoPropertyChanged("Conditions"); }
        }

        private ObservableCollection<SeasonIndexResultItemModel> _result;
        public ObservableCollection<SeasonIndexResultItemModel> Result
        {
            get { return _result; }
            set { _result = value; DoPropertyChanged("Result"); }
        }

        private int _page = 1;
        public int Page
        {
            get { return _page; }
            set { _page = value; }
        }

        public async Task LoadConditions()
        {
            try
            {
                ConditionsLoading = true;
                var results = await seasonIndexAPI.Condition((int)Parameter.type).Request();
                if (results.status)
                {
                    var data = results.GetJObject();
                    if (data["code"].ToInt32() == 0)
                    {
                        var items = JsonConvert.DeserializeObject<ObservableCollection<SeasonIndexConditionFilterModel>>(data["data"]["filter"].ToString());
                        foreach (var item in items)
                        {
                            if (item.field == "style_id")
                            {
                                item.current = item.values.FirstOrDefault(x => x.keyword == Parameter.style);
                            }
                            else if (item.field == "area")
                            {
                                item.current = item.values.FirstOrDefault(x => x.keyword == Parameter.area);
                            }
                            else if (item.field == "pub_date")
                            {
                                item.current = item.values.FirstOrDefault(x => x.keyword == Parameter.year);
                            }
                            else if (item.field == "season_month")
                            {
                                item.current = item.values.FirstOrDefault(x => x.keyword == Parameter.month);
                            }
                            else
                            {
                                item.current = item.values.FirstOrDefault();
                            }
                        }
                        var orders = new List<SeasonIndexConditionFilterItemModel>();

                        foreach (var item in data["data"]["order"])
                        {
                            orders.Add(new SeasonIndexConditionFilterItemModel()
                            {
                                keyword = item["field"].ToString(),
                                name = item["name"].ToString()
                            });
                        }

                        items.Insert(0, new SeasonIndexConditionFilterModel()
                        {
                            name = "排序",
                            values = orders,
                            field = "order",
                            current = orders.FirstOrDefault(x => x.name == Parameter.order) ?? orders[0],
                        });
                        Conditions = items;
                    }
                    else
                    {
                        Notify.ShowMessageToast(data["message"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<SeasonIndexConditionFilterModel>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                ConditionsLoading = false;
            }
        }

        public async Task LoadResult()
        {
            try
            {
                if (Loading) return;

                if (Page == 1)
                {
                    CanLoadMore = true;
                    Result = null;
                }
                else
                {
                    if (!CanLoadMore)
                    {
                        Loading = false;
                        return;
                    }
                }
                Loading = true;
                var con = "";
                foreach (var item in Conditions)
                {
                    con += $"&{item.field}={Uri.EscapeDataString(item.current.keyword)}";
                }
                con += $"&sort=0";
                var results = await seasonIndexAPI.Result(Page, (int)Parameter.type, con).Request();
                if (results.status)
                {
                    var data = results.GetJObject();
                    if (data["code"].ToInt32() == 0)
                    {
                        var items = JsonConvert.DeserializeObject<ObservableCollection<SeasonIndexResultItemModel>>(data["data"]["list"]?.ToString() ?? "[]");
                        if (items != null && items.Count != 0)
                        {
                            if (Page == 1)
                            {
                                Result = items;
                            }
                            else
                            {
                                foreach (var item in items)
                                {
                                    Result.Add(item);
                                }
                            }
                            Page++;
                        }
                        else
                        {

                            CanLoadMore = false;
                            Notify.ShowMessageToast("加载完了");
                        }
                    }
                    else
                    {
                        Notify.ShowMessageToast(data["message"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<SeasonIndexConditionFilterModel>(ex);
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
            Page = 1;
            if (Conditions == null)
            {
                await LoadConditions();
            }
            if (Conditions != null)
            {
                await LoadResult();
            }
        }
        public async void LoadMore()
        {
            if (Loading)
            {
                return;
            }
            if (Conditions == null || Conditions.Count == 0 || Result == null || Result.Count == 0)
            {
                return;
            }
            await LoadResult();
        }

    }

    public class SeasonIndexConditionFilterModel : IModules
    {
        public string field { get; set; }
        public string name { get; set; }

        private SeasonIndexConditionFilterItemModel _current;
        public SeasonIndexConditionFilterItemModel current
        {
            get { return _current; }
            set { _current = value; }
        }
        public List<SeasonIndexConditionFilterItemModel> values { get; set; }
    }
    public class SeasonIndexConditionFilterItemModel
    {
        public string keyword { get; set; }
        public string name { get; set; }

    }

    public class SeasonIndexResultItemModel
    {
        public int season_id { get; set; }
        public string title { get; set; }
        public string badge { get; set; }
        public int badge_type { get; set; }
        public bool show_badge
        {
            get
            {
                return !string.IsNullOrEmpty(badge);
            }
        }
        public string cover { get; set; }
        public string index_show { get; set; }
        public int is_finish { get; set; }
        public string link { get; set; }
        public int media_id { get; set; }
        public string order { get; set; }
        public string order_type { get; set; }
        public bool show_score
        {
            get
            {
                return order_type == "score";
            }
        }

        //public SeasonIndexResultItemOrderModel order { get; set; }
    }
    public class SeasonIndexResultItemOrderModel
    {
        public string follow { get; set; }
        public string play { get; set; }
        public string score { get; set; }
        public long pub_date { get; set; }
        public long pub_real_time { get; set; }
        public long renewal_time { get; set; }
        public string type { get; set; }
        public string bottom_text
        {
            get
            {
                if (type == "follow")
                {
                    return follow;
                }
                else
                {
                    return renewal_time.HandelTimestamp() + "更新";
                }
            }
        }
        public bool show_score
        {
            get
            {
                return type == "score";
            }
        }
    }
}
