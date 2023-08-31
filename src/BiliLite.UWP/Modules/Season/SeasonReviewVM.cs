using BiliLite.Models;
using BiliLite.Models.Requests.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using BiliLite.Extensions;
using BiliLite.Services;

namespace BiliLite.Modules.Season
{
    public class SeasonReviewVM : IModules
    {
        readonly SeasonApi seasonApi;

        public SeasonReviewVM()
        {

            Items = new ObservableCollection<SeasonShortReviewItemModel>();
            seasonApi = new SeasonApi();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
        }
        public ObservableCollection<SeasonShortReviewItemModel> Items { get; set; }
        public int MediaID { get; set; }
        public ICommand RefreshCommand { get; private set; }
        public ICommand LoadMoreCommand { get; private set; }
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

        public string Next { get; set; } = "";
        public async Task GetItems()
        {
            try
            {
                if (MediaID == 0) { return; }
                Loading = true;
                CanLoadMore = false;
                var results = await seasonApi.ShortReview(MediaID, Next).Request();
                if (results.status)
                {
                    var data = results.GetJObject();
                    if (data["code"].ToInt32() == 0)
                    {
                        var items = JsonConvert.DeserializeObject<List<SeasonShortReviewItemModel>>(data["data"]["list"].ToString());
                        if (items != null)
                        {
                            foreach (var item in items)
                            {
                                Items.Add(item);
                            }
                            //Items = new IncrementalLoadingCollection<LiveRecommendItemSource, LiveRecommendItemModel>(new LiveRecommendItemSource(items, SortType), 30);
                            if (Items.Count < data["data"]["total"].ToInt32())
                            {
                                Next = data["data"]["next"].ToString();
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
                var handel = HandelError<SeasonReviewVM>(ex);
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
            Next = "";
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

        public async void Like(SeasonShortReviewItemModel item)
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录后再操作");
                return;
            }
            try
            {
                var api = seasonApi.LikeReview(MediaID, item.review_id, ReviewType.Short);
                var results = await api.Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiResultModel<JObject>>();
                    if (data.success)
                    {
                        item.stat.liked = data.result["status"].ToInt32();
                        if (item.stat.liked == 1)
                        {
                            item.stat.likes += 1;
                        }
                        else
                        {
                            item.stat.likes -= 1;
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
                var handel = HandelError<object>(ex);
                Notify.ShowMessageToast(handel.message);
            }

        }

        public async void Dislike(SeasonShortReviewItemModel item)
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录后再操作");
                return;
            }
            try
            {
                var api = seasonApi.DislikeReview(MediaID, item.review_id, ReviewType.Short);
                var results = await api.Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiResultModel<JObject>>();
                    if (data.success)
                    {
                        item.stat.disliked = data.result["status"].ToInt32();
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
                Notify.ShowMessageToast(handel.message);
            }

        }

        public async Task<bool> SendShortReview(string content, bool share, int score)
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录后再操作");
                return false;
            }
            try
            {
                var api = seasonApi.SendShortReview(MediaID, content, share, score);
                var results = await api.Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiResultModel<JObject>>();
                    if (data.success)
                    {
                        Notify.ShowMessageToast("发表成功");
                        return true;
                    }
                    else
                    {
                        Notify.ShowMessageToast(data.message);
                        return false;
                    }
                }
                else
                {
                    Notify.ShowMessageToast(results.message);
                    return false;
                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<object>(ex);
                Notify.ShowMessageToast(handel.message);
                return false;
            }

        }
    }


    public class SeasonShortReviewItemModel
    {
        public long ctime { get; set; }
        public long mid { get; set; }
        public int review_id { get; set; }
        public string content { get; set; }
        public string progress { get; set; }
        public int score { get; set; }
        /// <summary>
        /// 评分，转为5分制
        /// </summary>
        public int score_5 { get { return score / 2; } }
        public SeasonShortReviewItemAuthorModel author { get; set; }
        public SeasonShortReviewItemStatModel stat { get; set; }
    }
    public class SeasonShortReviewItemAuthorModel
    {
        public string avatar { get; set; }
        public string uname { get; set; }
        public long mid { get; set; }
        public SeasonShortReviewItemVIPModel vip { get; set; }
    }
    public class SeasonShortReviewItemVIPModel
    {
        public int vipType { get; set; }
        public int vipStatus { get; set; }
    }
    public class SeasonShortReviewItemStatModel : IModules
    {
        private int _disliked;
        /// <summary>
        /// 是否已经点踩👎
        /// </summary>
        public int disliked
        {
            get { return _disliked; }
            set { _disliked = value; DoPropertyChanged("disliked"); }
        }

        private int _liked;
        /// <summary>
        /// 是否已经点赞👍
        /// </summary>
        public int liked
        {
            get { return _liked; }
            set { _liked = value; DoPropertyChanged("liked"); }
        }

        private int _likes;
        /// <summary>
        /// 点赞数量
        /// </summary>
        public int likes
        {
            get { return _likes; }
            set { _likes = value; DoPropertyChanged("likes"); }
        }

    }
}
