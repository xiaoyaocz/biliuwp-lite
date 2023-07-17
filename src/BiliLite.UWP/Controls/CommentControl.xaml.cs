using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using BiliLite.Pages;
using static BiliLite.Models.Requests.Api.CommentApi;
using BiliLite.Modules;
using BiliLite.Dialogs;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Comment;
using BiliLite.Models.Requests.Api;
using BiliLite.Services;
using BiliLite.ViewModels.Comment;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.UI.Controls;
using IMapper = AutoMapper.IMapper;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls
{
    public sealed partial class CommentControl : UserControl
    {
        #region Fields

        private readonly CommentApi m_commentApi;
        EmoteVM emoteVM;
        private bool m_disableShowPicture = false;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private CommentCursor m_nextCursor;
        private readonly IMapper m_mapper;
        private readonly CommentControlViewModel m_viewModel;

        private int m_page = 1;
        private LoadCommentInfo m_loadCommentInfo;

        private CommentViewModel m_selectComment;

        #endregion

        #region Constructors

        public CommentControl()
        {
            m_mapper = App.ServiceProvider.GetService<IMapper>();
            m_viewModel = App.ServiceProvider.GetService<CommentControlViewModel>();
            DataContext = m_viewModel;
            this.InitializeComponent();
            m_commentApi = new CommentApi();
            emoteVM = new EmoteVM();
        }

        #endregion

        #region Properties

        public int CommentCount => ListViewComments.Items.Count;

        #endregion

        #region Private Methods

        private void BtnUser_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Account,
                title = "用户信息",
                page = typeof(UserInfoPage),
                parameters = (sender as HyperlinkButton)?.Tag.ToString()
            });
        }

        private async Task GetComment()
        {
            if (m_page == 1)
            {
                m_viewModel.NoRepostVisibility = false;
                m_viewModel.CloseRepostVisibility = false;
                m_viewModel.Comments.Clear();
                m_nextCursor = null;
            }
            try
            {
                m_viewModel.BtnLoadMoreVisibility = false;
                m_viewModel.Loading = true;

                var result = await m_commentApi.Comment(m_loadCommentInfo.Oid, m_loadCommentInfo.CommentSort, m_page, m_loadCommentInfo.CommentMode).Request();
                var errorCheck = await result.GetResult<object>();
                if (!result.status || errorCheck.code < 0)
                {
                    result = await m_commentApi.CommentV2(m_loadCommentInfo.Oid, m_loadCommentInfo.CommentSort, m_page,
                        m_loadCommentInfo.CommentMode, offsetStr: m_nextCursor?.PaginationReply?.NextOffset).Request();
                }
                if (!result.status)
                {
                    throw new CustomizedErrorException("加载评论失败");
                }
                var dataCommentModel = JsonConvert.DeserializeObject<DataCommentModel>(result.results);
                if (dataCommentModel.Code == 0)
                {

                    HandleCommentsNormal(dataCommentModel);
                }
                else
                {
                    HandleCommentsAbnormalSituation(dataCommentModel);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                Notify.ShowMessageToast(ex.Message);
            }
            finally
            {
                m_viewModel.Loading = false;
            }
        }

        // 处理评论列表正常情况
        private void HandleCommentsNormal(DataCommentModel model)
        {
            if (model.Data.Replies != null && model.Data.Replies.Count != 0)
            {
                HandleCommentsNormalHasReply(model);
            }
            else
            {
                HandleCommentsNormalNoReply();
            }
        }

        // 处理评论列表正常有评论情况
        private void HandleCommentsNormalHasReply(DataCommentModel model)
        {
            var dataCommentViewModel = m_mapper.Map<DataCommentViewModel>(model);
            if (m_page == 1)
            {
                if (dataCommentViewModel.Data.Upper.Top != null)
                {
                    dataCommentViewModel.Data.Upper.Top.ShowTop = true;
                    dataCommentViewModel.Data.Replies.Insert(0, dataCommentViewModel.Data.Upper.Top);
                }

                m_viewModel.Comments.AddRange(dataCommentViewModel.Data.Replies);
            }
            else
            {
                foreach (var item in dataCommentViewModel.Data.Replies)
                {
                    m_viewModel.Comments?.Add(item);
                }
            }
            m_page++;

            if (model.Data.Replies.Count >= 20)
            {
                m_viewModel.BtnLoadMoreVisibility = true;
            }

            m_nextCursor = model.Data.Cursor;
        }

        // 处理评论列表正常没有评论情况
        private void HandleCommentsNormalNoReply()
        {
            if (m_page == 1)
            {
                m_viewModel.NoRepostVisibility = true;
                m_viewModel.BtnLoadMoreVisibility = false;
            }
            else
            {
                Notify.ShowMessageToast("全部加载完了...");
            }
        }

        // 处理评论列表异常情况
        private void HandleCommentsAbnormalSituation(DataCommentModel model)
        {
            if (model.Code == 12002)
            {
                m_viewModel.CloseRepostVisibility = true;
                m_viewModel.BtnLoadMoreVisibility = false;
            }
            else
            {
                throw new CustomizedErrorException($"加载评论失败:{model.Message}");
            }
        }

        private async Task GetReply(CommentViewModel data)
        {
            try
            {
                data.Replies ??= new ObservableCollection<CommentViewModel>();
                data.ShowReplyMore = false;
                data.ShowLoading = true;
                var result = await m_commentApi.Reply(m_loadCommentInfo.Oid, data.RpId.ToString(), data.LoadPage,
                    m_loadCommentInfo.CommentMode).Request();
                if (!result.status)
                {
                    throw new CustomizedErrorException($"{result.message}");
                }

                var dataCommentModel = JsonConvert.DeserializeObject<DataCommentModel>(result.results);
                var dataCommentViewModel = m_mapper.Map<DataCommentViewModel>(dataCommentModel);

                if (dataCommentViewModel.Code != 0)
                {
                    throw new CustomizedErrorException($"{dataCommentModel.Message}");
                }
                if (dataCommentViewModel.Data.Replies != null && dataCommentViewModel.Data.Replies.Count != 0)
                {
                    if (dataCommentViewModel.Data.Replies.Count >= 10)
                    {
                        data.ShowReplyMore = true;
                    }

                    foreach (var item in dataCommentViewModel.Data.Replies)
                    {
                        data.Replies.Add(item);
                    }

                    data.LoadPage++;
                }
                else
                {
                    data.ShowReplyMore = false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                Notify.ShowMessageToast(ex.Message);
            }
            finally
            {
                data.ShowLoading = false;
            }
        }

        private async void DoLike(CommentViewModel data)
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请登录后再执行操作");
                return;
            }
            try
            {
                var action = 0;
                if (data.Action == 0)
                {
                    action = 1;
                }
                var result = await m_commentApi.Like(m_loadCommentInfo.Oid, data.RpId.ToString(), action, m_loadCommentInfo.CommentMode).Request();
                if (!result.status)
                {
                    throw new CustomizedErrorException($"{result.message}");
                }

                var obj = JObject.Parse(result.results);

                if (obj["code"].ToInt32() != 0)
                {
                    throw new CustomizedErrorException($"{obj["message"]?.ToString()}");
                }
                if (data.Action == 0)
                {
                    data.Action = 1;
                    data.Like += 1;
                }
                else
                {
                    data.Action = 0;
                    data.Like -= 1;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                Notify.ShowMessageToast($"操作失败:{ex.Message}");
            }
        }

        private void BtnLike_Click(object sender, RoutedEventArgs e)
        {
            var m = (sender as HyperlinkButton)?.DataContext as CommentViewModel;
            DoLike(m);
        }

        private async void BtnShowComment_Click(object sender, RoutedEventArgs e)
        {
            var m = (sender as HyperlinkButton)?.DataContext as CommentViewModel;
            if (m.ShowReplies == false)
            {
                ListViewComments.ScrollIntoView(m);
                m.ShowReplies = true;
                m.ShowReplyBtn = false;
                m.ShowReplyBox = true;
                m.Replies = null;
                m.LoadPage = 1;
                if (m.Replies == null || m.Replies.Count == 0)
                {
                    await GetReply(m);
                }
            }
            else
            {
                m.ShowReplyBtn = false;
                m.ShowReplies = false;
                ListViewComments.ScrollIntoView(m);
            }
        }

        private void BtnShowReplyBtn_Click(object sender, RoutedEventArgs e)
        {
            var m = (sender as HyperlinkButton)?.DataContext as CommentViewModel;
            m.ShowReplyBox = !m.ShowReplyBox;
        }

        private void BtnDoNotLike_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void BtnLoadMoreReply_Click(object sender, RoutedEventArgs e)
        {
            var m = (sender as HyperlinkButton).DataContext as CommentViewModel;
            await GetReply(m);
        }

        private async void BtnLoadMore_Click(object sender, RoutedEventArgs e)
        {
            if (!m_viewModel.Loading)
            {
                await GetComment();
            }

        }

        private void BtnHotSort_Click(object sender, RoutedEventArgs e)
        {
            m_nextCursor = null;
            m_loadCommentInfo.CommentSort = CommentSort.Hot;
            LoadComment(m_loadCommentInfo);
        }

        private void BtnNewSort_Click(object sender, RoutedEventArgs e)
        {
            m_nextCursor = null;
            m_loadCommentInfo.CommentSort = CommentSort.New;
            LoadComment(m_loadCommentInfo);
        }

        private void BtnSendReply_Click(object sender, RoutedEventArgs e)
        {
            var m = (sender as Button).DataContext as CommentViewModel;
            ReplyComment(m);
        }

        private async void ReplyComment(CommentViewModel m)
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请登录后再执行操作");
                return;
            }
            if (m.ReplyText.Trim().Length == 0)
            {
                Notify.ShowMessageToast("不能发送空白信息");
                return;
            }
            try
            {
                var re = await m_commentApi.ReplyComment(m_loadCommentInfo.Oid, m.RpId.ToString(), m.RpId.ToString(), Uri.EscapeDataString(m.ReplyText), m_loadCommentInfo.CommentMode).Request();
                if (re.status)
                {
                    JObject obj = JObject.Parse(re.results);
                    if (obj["code"].ToInt32() == 0)
                    {
                        Notify.ShowMessageToast("回复评论成功");
                        m.LoadPage = 1;
                        m.Replies.Clear();
                        m.ReplyText = "";
                        GetReply(m).RunWithoutAwait();
                    }
                    else
                    {
                        Notify.ShowMessageToast(obj["message"].ToString());
                    }
                }
                else
                {
                    Notify.ShowMessageToast(re.message);
                }

            }
            catch (Exception)
            {
                Notify.ShowMessageToast("发送评论失败");
                // throw;
            }

        }

        private void BtnReplyAt_Click(object sender, RoutedEventArgs e)
        {
            var m = (sender as Button).DataContext as CommentViewModel;
            ReplyAt(m);
        }

        private async void ReplyAt(CommentViewModel m)
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请登录后再执行操作");
                return;
            }
            if (m.ReplyText.Trim().Length == 0)
            {
                Notify.ShowMessageToast("不能发送空白信息");
                return;
            }
            try
            {
                //string url = $"{ApiHelper.API_BASE_URL}/x/v2/reply/add";

                var txt = "回复 @" + m.Member.Uname + ":" + m.ReplyText;
                //string content =
                //    string.Format("access_key={0}&appkey={1}&platform=android&type={2}&oid={3}&ts={4}&message={5}&root={6}&parent={7}",
                //    ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, _type, _loadCommentInfo.oid, ApiHelper.GetTimeSpan_2, Uri.EscapeDataString(txt), m.root, m.rpid);
                //content += "&sign=" + ApiHelper.GetSign(content);

                var re = await m_commentApi.ReplyComment(m_loadCommentInfo.Oid, m.Root.ToString(), m.RpId.ToString(), Uri.EscapeDataString(txt), m_loadCommentInfo.CommentMode).Request();
                if (re.status)
                {
                    JObject obj = JObject.Parse(re.results);
                    if (obj["code"].ToInt32() == 0)
                    {
                        Notify.ShowMessageToast("回复评论成功");
                        m.LoadPage = 1;
                        m.Replies.Clear();
                        m.ReplyText = "";
                        await GetReply(m);
                    }
                    else
                    {
                        Notify.ShowMessageToast(obj["message"].ToString());
                    }
                }
                else
                {
                    Notify.ShowMessageToast(re.message);
                }
            }
            catch (Exception)
            {
                Notify.ShowMessageToast("发送评论失败");
                // throw;
            }
        }

        private void BtnDeleteComment_Click(object sender, RoutedEventArgs e)
        {
            CommentViewModel m = null;
            if (sender is HyperlinkButton)
            {
                m = (sender as HyperlinkButton).DataContext as CommentViewModel;
            }
            if (sender is MenuFlyoutItem)
            {
                m = (sender as MenuFlyoutItem).DataContext as CommentViewModel;
            }
            DeleteComment(m);
        }

        private async void DeleteComment(CommentViewModel m)
        {
            if (m == null)
            {
                return;
            }
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请登录后再执行操作");
                return;
            }

            try
            {
                //string url = $"{ApiHelper.API_BASE_URL}/x/v2/reply/del";

                //string content =
                //    string.Format("access_key={0}&appkey={1}&platform=android&type={2}&oid={3}&ts={4}&rpid={5}",
                //    ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, _type, _loadCommentInfo.oid, ApiHelper.GetTimeSpan_2, m.rpid);
                //content += "&sign=" + ApiHelper.GetSign(content);

                //var re = await WebClientClass.PostResults(new Uri(url), content);
                var re = await m_commentApi.DeleteComment(m_loadCommentInfo.Oid, m.RpId.ToString(), m_loadCommentInfo.CommentMode).Request();
                if (re.status)
                {
                    JObject obj = JObject.Parse(re.results);
                    if (obj["code"].ToInt32() == 0)
                    {
                        Notify.ShowMessageToast("评论删除成功");
                        RefreshComment();
                    }
                    else
                    {
                        Notify.ShowMessageToast(obj["message"].ToString());
                    }
                }
                else
                {
                    Notify.ShowMessageToast(re.message);
                }
            }
            catch (Exception)
            {
                Notify.ShowMessageToast("删除评论失败");
                // throw;
            }


        }

        private void BtnFace_Click(object sender, RoutedEventArgs e)
        {
            m_selectComment = (sender as Button).DataContext as CommentViewModel;
            FaceFlyout.ShowAt(sender as Button);
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            m_selectComment.ReplyText += (e.ClickedItem as EmotePackageItemModel).text.ToString();
        }

        private async void BtnOpenSendComment_Click(object sender, RoutedEventArgs e)
        {
            SendCommentDialog sendCommentDialog = new SendCommentDialog(m_loadCommentInfo.Oid, (CommentType)m_loadCommentInfo.CommentMode);
            await sendCommentDialog.ShowAsync();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadComment(m_loadCommentInfo);
        }

        private void NotePicturesView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (m_disableShowPicture)
            {
                Notify.ShowMessageToast("暂不支持查看图片");
                return;
            }
            var notePicture = e.ClickedItem as NotePicture;
            var notePicturesView = sender as AdaptiveGridView;
            if (notePicture == null || notePicturesView == null) return;
            var comment = notePicturesView.DataContext as CommentViewModel;
            if (comment == null) return;
            var notePictures = comment.Content.Pictures;
            var index = notePictures.IndexOf(notePicture);
            var pictures = notePictures.Select(x => x.ImgSrc).ToList();
            MessageCenter.OpenImageViewer(pictures, index);
        }

        private void CommentControl_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width != e.PreviousSize.Width)
            {
                m_viewModel.Width = e.NewSize.Width;
            }
        }

        #endregion

        #region Public Methods

        public void ClearComment()
        {
            m_viewModel.Comments.Clear();
            m_page = 1;
            m_viewModel.HotCommentsVisibility = false;
        }

        public async void LoadMore()
        {
            if (!m_viewModel.Loading)
            {
                await GetComment();
            }
        }

        public void RefreshComment()
        {
            LoadComment(m_loadCommentInfo);
        }

        /// <summary>
        /// 初始化并加载评论
        /// </summary>
        /// <param name="loadCommentInfo"></param>
        public async void LoadComment(LoadCommentInfo loadCommentInfo, bool disableShowPicture = false)
        {
            m_disableShowPicture = disableShowPicture;
            if (loadCommentInfo.CommentSort == CommentSort.Hot)
            {
                m_viewModel.HotCommentsVisibility = true;
                m_viewModel.NewCommentVisibility = false;
            }
            else
            {
                m_viewModel.HotCommentsVisibility = false;
                m_viewModel.NewCommentVisibility = true;
            }

            m_viewModel.IsCommentDialog = loadCommentInfo.IsDialog;
            m_loadCommentInfo = loadCommentInfo;
            m_page = 1;
            await GetComment();
            await emoteVM.GetEmote(EmoteBusiness.reply);
        }

        #endregion
    }
}
