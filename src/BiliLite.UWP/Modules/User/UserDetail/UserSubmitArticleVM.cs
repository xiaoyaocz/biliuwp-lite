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
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Media;
using BiliLite.Extensions;

namespace BiliLite.Modules.User.UserDetail
{
    /// <summary>
    /// 专栏投稿
    /// </summary>
    public class UserSubmitArticleVM : IModules
    {
        public string mid { get; set; }
        private readonly UserDetailAPI userDetailAPI;
        public UserSubmitArticleVM()
        {
            userDetailAPI = new UserDetailAPI();
            RefreshSubmitArticleCommand = new RelayCommand(Refresh);
            LoadMoreSubmitArticleCommand = new RelayCommand(LoadMore);

        }

        public int SelectOrder { get; set; } = 0;



        private bool _LoadingSubmitArticle = true;
        public bool LoadingSubmitArticle
        {
            get { return _LoadingSubmitArticle; }
            set { _LoadingSubmitArticle = value; DoPropertyChanged("LoadingSubmitArticle"); }
        }
        private bool _SubmitArticleCanLoadMore = false;
        public bool SubmitArticleCanLoadMore
        {
            get { return _SubmitArticleCanLoadMore; }
            set { _SubmitArticleCanLoadMore = value; DoPropertyChanged("SubmitArticleCanLoadMore"); }
        }
        public ICommand RefreshSubmitArticleCommand { get; private set; }
        public ICommand LoadMoreSubmitArticleCommand { get; private set; }

        private ObservableCollection<SubmitArticleItemModel> _SubmitArticleItems;
        public ObservableCollection<SubmitArticleItemModel> SubmitArticleItems
        {
            get { return _SubmitArticleItems; }
            set { _SubmitArticleItems = value; DoPropertyChanged("SubmitArticleItems"); }
        }


        private bool _Nothing = false;

        public bool Nothing
        {
            get { return _Nothing; }
            set { _Nothing = value; DoPropertyChanged("Nothing"); }
        }

        public int SubmitArticlePage { get; set; } = 1;

        public async Task GetSubmitArticle()
        {
            try
            {
                Nothing = false;
                SubmitArticleCanLoadMore = false;
                LoadingSubmitArticle = true;
                var api = userDetailAPI.SubmitArticles(mid, SubmitArticlePage, order: (SubmitArticleOrder)SelectOrder);

                var results = await api.Request();
                if (results.status)
                {
                    var data = await results.GetData<JObject>();
                    if (data.code == 0)
                    {
                        var items = JsonConvert.DeserializeObject<ObservableCollection<SubmitArticleItemModel>>(data.data["articles"]?.ToString() ?? "[]");
                        if (SubmitArticleItems == null)
                        {
                            SubmitArticleItems = items;
                        }
                        else
                        {
                            foreach (var item in items)
                            {
                                SubmitArticleItems.Add(item);
                            }
                        }
                        if (SubmitArticlePage == 1 && (SubmitArticleItems == null || SubmitArticleItems.Count == 0))
                        {
                            Nothing = true;
                        }



                        var count = data.data["count"]?.ToInt32() ?? 0;
                        if (SubmitArticleItems.Count >= count)
                        {
                            SubmitArticleCanLoadMore = false;
                        }
                        else
                        {
                            SubmitArticleCanLoadMore = true;
                            SubmitArticlePage++;
                        }
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
                var handel = HandelError<UserSubmitArticleVM>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                LoadingSubmitArticle = false;
            }
        }
        public async void Refresh()
        {
            if (LoadingSubmitArticle)
            {
                return;
            }
            SubmitArticleItems = null;
            SubmitArticlePage = 1;
            await GetSubmitArticle();
        }
        public async void LoadMore()
        {
            if (LoadingSubmitArticle)
            {
                return;
            }
            if (SubmitArticleItems == null || SubmitArticleItems.Count == 0)
            {
                return;
            }
            await GetSubmitArticle();
        }
    }


    public class SubmitArticleItemModel
    {
        public int id { get; set; }
        public string title { get; set; }
        public string summary { get; set; }
        public List<string> image_urls { get; set; }
        public string cover
        {
            get
            {
                return image_urls?[0] ?? "";
            }
        }

        public long publish_time { get; set; }
        public int words { get; set; }
        public SubmitArticleStatsModel stats { get; set; }
        public SubmitArticleCategoryModel category { get; set; }
    }
    public class SubmitArticleStatsModel
    {
        public int view { get; set; }
        public int favorite { get; set; }
        public int like { get; set; }
        public int reply { get; set; }
        public int share { get; set; }
        public int coin { get; set; }
        public int dynamic { get; set; }
    }
    public class SubmitArticleCategoryModel
    {
        public int id { get; set; }
        public int parent_id { get; set; }
        public string name { get; set; }

    }
}