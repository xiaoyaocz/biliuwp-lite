using BiliLite.Models;
using BiliLite.Models.Requests.Api.User;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using BiliLite.Extensions;

namespace BiliLite.Modules.User
{
    public class AtVM : IModules
    {
        readonly AtApi atApi;
        public AtVM()
        {
            atApi = new AtApi();
            SearchCommand = new RelayCommand<string>(Search);
            LoadMoreCommand = new RelayCommand(LoadMore);
            Users = new ObservableCollection<AtUserModel>();
        }
        public ICommand LoadMoreCommand { get; private set; }
        public ICommand SearchCommand { get; private set; }


        private ObservableCollection<AtUserModel> _user;

        public ObservableCollection<AtUserModel> Users
        {
            get { return _user; }
            set { _user = value; DoPropertyChanged("Users"); }
        }

        private bool _loading = true;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }
        public int Page { get; set; } = 1;
        public string Keyword { get; set; }
        public async Task GetUser()
        {
            try
            {
                Loading = true;
                var api = atApi.RecommendAt(Page);
                if (!string.IsNullOrEmpty(Keyword))
                {
                    api = atApi.SearchUser(Keyword, Page);
                }
                if (Page == 1)
                {
                    Users.Clear();
                }
                var results = await api.Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        if (!string.IsNullOrEmpty(Keyword))
                        {
                            if (data.data.ContainsKey("items"))
                            {
                                foreach (var item in data.data["items"])
                                {
                                    Users.Add(new AtUserModel()
                                    {
                                        Face = item["face"].ToString(),
                                        UserName = item["name"].ToString(),
                                        ID = item["mid"].ToInt32(),
                                    });
                                }
                            }

                        }
                        else
                        {
                            foreach (var item in data.data["recent_attention"]["info"])
                            {
                                Users.Add(new AtUserModel()
                                {
                                    Face = item["face"].ToString(),
                                    UserName = item["uname"].ToString(),
                                    ID = item["uid"].ToInt32(),
                                });
                            }
                        }
                        Page++;
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
                var handel = HandelError<AtVM>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }

        public async void LoadMore()
        {
            if (Loading)
            {
                return;
            }

            await GetUser();
        }
        public async void Search(string keyword)
        {

            if (Loading)
            {
                return;
            }
            Keyword = keyword;
            Page = 1;
            await GetUser();
        }
    }
}
public class AtUserModel
{
    public int ID { get; set; }
    public string UserName { get; set; }
    public string Face { get; set; }
    public string Display { get { return "@" + UserName; } }
}