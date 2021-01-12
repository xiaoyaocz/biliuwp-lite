using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Markup;
using System.Text.RegularExpressions;
using Windows.UI;
using System.ComponentModel;
using Windows.UI.Xaml.Documents;
using System.Threading.Tasks;
using BiliLite.Helpers;
using BiliLite.Pages;
using BiliLite.Api;
using static BiliLite.Api.CommentApi;
using BiliLite.Modules;
using BiliLite.Dialogs;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls
{
    public sealed partial class CommentControl : UserControl
    {
        readonly CommentApi commentApi;
        EmoteVM emoteVM;
        public CommentControl()
        {
            this.InitializeComponent();
            commentApi = new CommentApi();
            emoteVM = new EmoteVM();
        }

        private void btn_User_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.OpenNewWindow(this, new NavigationInfo()
            {
                icon = Symbol.Account,
                title = "用户信息",
                page = typeof(UserInfoPage),
                parameters = (sender as HyperlinkButton).Tag.ToString()
            });
        }

        public void ClearComment()
        {
            //ls_hot.ItemsSource = null;
            ls_new.ItemsSource = null;
            _page = 1;
            hot.Visibility = Visibility.Collapsed;
        }
        public int CommentCount
        {
            get
            {
                return ls_new.Items.Count;
            }
        }

        public bool HasMore
        {
            get
            {
                if (btn_LoadMore.Visibility == Visibility.Visible)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public async void LoadMore()
        {
            if (!_loading)
            {
                await GetComment();
            }
        }

        public void RefreshComment()
        {
            LoadComment(_loadCommentInfo);
        }


        int _page = 1;
        public bool _loading = false;
        LoadCommentInfo _loadCommentInfo;

        /// <summary>
        /// 初始化并加载评论
        /// </summary>
        /// <param name="loadCommentInfo"></param>
        public async void LoadComment(LoadCommentInfo loadCommentInfo)
        {

            if (loadCommentInfo.commentSort == commentSort.Hot)
            {
                hot.Visibility = Visibility.Visible;
                _new.Visibility = Visibility.Collapsed;
            }
            else
            {
                hot.Visibility = Visibility.Collapsed;
                _new.Visibility = Visibility.Visible;
            }

            _loadCommentInfo = loadCommentInfo;
            _page = 1;
            await GetComment();
            await emoteVM.GetEmote(EmoteBusiness.reply);
        }

        private async Task GetComment()
        {
            if (_page == 1)
            {
                noRepost.Visibility = Visibility.Collapsed;
                closeRepost.Visibility = Visibility.Collapsed;
                //ls_hot.ItemsSource = null;
                ls_new.ItemsSource = null;
            }
            try
            {

                btn_LoadMore.Visibility = Visibility.Collapsed;
                _loading = true;
                pr_load.Visibility = Visibility.Visible;
                ObservableCollection<CommentModel> ls = new ObservableCollection<CommentModel>();


                var re = await commentApi.Comment(_loadCommentInfo.oid, _loadCommentInfo.commentSort, _page, _loadCommentInfo.commentMode).Request();
                if (re.status)
                {
                    dataCommentModel m = JsonConvert.DeserializeObject<dataCommentModel>(re.results);
                    if (m.code == 0)
                    {

                        if (m.data.replies != null && m.data.replies.Count != 0)
                        {

                            if (_page == 1)
                            {
                                if (m.data.upper.top != null)
                                {
                                    m.data.upper.top.showTop = Visibility.Visible;
                                    m.data.replies.Insert(0, m.data.upper.top);
                                }
                                //ls_hot.ItemsSource = m.data.hots;
                                ls_new.ItemsSource = m.data.replies;
                            }
                            else
                            {
                                foreach (var item in m.data.replies)
                                {
                                    (ls_new.ItemsSource as ObservableCollection<CommentModel>).Add(item);
                                }
                            }
                            _page++;

                            if (m.data.replies.Count >= 20)
                            {
                                btn_LoadMore.Visibility = Visibility.Visible;
                            }
                        }
                        else
                        {
                            if (_page == 1)
                            {
                                noRepost.Visibility = Visibility.Visible;
                                btn_LoadMore.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                Utils.ShowMessageToast("全部加载完了...");
                            }
                        }
                    }
                    else
                    {
                        if (m.code == 12002)
                        {
                            closeRepost.Visibility = Visibility.Visible;
                            btn_LoadMore.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            Utils.ShowMessageToast(m.message);
                        }
                    }
                }
                else
                {
                    Utils.ShowMessageToast("加载评论失败");
                }

            }
            catch (Exception)
            {
                Utils.ShowMessageToast("加载评论失败");
            }
            finally
            {
                _loading = false;
                pr_load.Visibility = Visibility.Collapsed;

            }
        }

        private async Task GetReply(CommentModel data)
        {
            try
            {
                if (data.replies == null)
                {
                    data.replies = new ObservableCollection<CommentModel>();
                }
                data.showReplyMore = Visibility.Collapsed;
                data.showLoading = Visibility.Visible;
                ObservableCollection<CommentModel> ls = new ObservableCollection<CommentModel>();
                var re = await commentApi.Reply(_loadCommentInfo.oid, data.rpid.ToString(), data.loadpage, _loadCommentInfo.commentMode).Request();
                if (re.status)
                {
                    dataCommentModel m = JsonConvert.DeserializeObject<dataCommentModel>(re.results);
                    if (m.code == 0)
                    {
                        if (m.data.replies != null && m.data.replies.Count != 0)
                        {
                            if (m.data.replies.Count >= 10)
                            {
                                data.showReplyMore = Visibility.Visible;
                            }
                            foreach (var item in m.data.replies)
                            {
                                data.replies.Add(item);
                            }
                            data.loadpage++;
                        }
                        else
                        {
                            data.showReplyMore = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        Utils.ShowMessageToast(m.message);
                    }
                }
                else
                {
                    Utils.ShowMessageToast(re.message);
                }
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("加载评论失败");
                //throw;
            }
            finally
            {
                data.showLoading = Visibility.Collapsed;
            }
        }

        private async void doLike(CommentModel data)
        {
            if (!SettingHelper.Account.Logined && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请登录后再执行操作");
                return;
            }
            try
            {
                var action = 0;
                if (data.action == 0)
                {
                    action = 1;
                }
                var re = await commentApi.Like(_loadCommentInfo.oid, data.rpid.ToString(), action, _loadCommentInfo.commentMode).Request();
                if (re.status)
                {
                    JObject obj = JObject.Parse(re.results);
                    if (obj["code"].ToInt32() == 0)
                    {
                        if (data.action == 0)
                        {
                            data.action = 1;
                            data.like += 1;
                        }
                        else
                        {
                            data.action = 0;
                            data.like -= 1;
                        }
                    }
                    else
                    {
                        Utils.ShowMessageToast(obj["message"].ToString());
                    }
                }
                else
                {
                    Utils.ShowMessageToast(re.message);
                }

            }
            catch (Exception)
            {
                Utils.ShowMessageToast("操作失败");
                // throw;
            }



        }

        private void btn_Like_Click(object sender, RoutedEventArgs e)
        {
            var m = (sender as HyperlinkButton).DataContext as CommentModel;
            doLike(m);
        }

        private async void btn_ShowComment_Click(object sender, RoutedEventArgs e)
        {
            var m = (sender as HyperlinkButton).DataContext as CommentModel;
            if (m.showReplies == Visibility.Collapsed)
            {
                ls_new.ScrollIntoView(m);
                m.showReplies = Visibility.Visible;
                m.showReplyBtn = Visibility.Collapsed;
                m.showReplyBox = Visibility.Visible;
                m.replies = null;
                m.loadpage = 1;
                if (m.replies == null || m.replies.Count == 0)
                {
                    await GetReply(m);
                }
            }
            else
            {
                m.showReplyBtn = Visibility.Collapsed;
                m.showReplies = Visibility.Collapsed;
                ls_new.ScrollIntoView(m);
            }
        }

        private void btn_ShowReplyBtn_Click(object sender, RoutedEventArgs e)
        {
            var m = (sender as HyperlinkButton).DataContext as CommentModel;
            if (m.showReplyBox == Visibility.Visible)
            {
                m.showReplyBox = Visibility.Collapsed;
            }
            else
            {
                m.showReplyBox = Visibility.Visible;
            }

        }

        private void btn_DonotLike_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void btn_LoadMoreReply_Click(object sender, RoutedEventArgs e)
        {
            var m = (sender as HyperlinkButton).DataContext as CommentModel;
            await GetReply(m);
        }

        private async void btn_LoadMore_Click(object sender, RoutedEventArgs e)
        {
            if (!_loading)
            {
                await GetComment();
            }

        }

        private void btn_HotSort_Click(object sender, RoutedEventArgs e)
        {
            _loadCommentInfo.commentSort = commentSort.Hot;
            LoadComment(_loadCommentInfo);
        }

        private void btn_NewSort_Click(object sender, RoutedEventArgs e)
        {
            _loadCommentInfo.commentSort = commentSort.New;
            LoadComment(_loadCommentInfo);
        }





        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var m = (sender as Button).DataContext as CommentModel;

            if (m != null)
            {
                m.replyText += (sender as Button).Content.ToString();
            }
        }




        private void btn_SendReply_Click(object sender, RoutedEventArgs e)
        {
            var m = (sender as Button).DataContext as CommentModel;
            ReplyComment(m);
        }

        private async void ReplyComment(CommentModel m)
        {
            if (!SettingHelper.Account.Logined && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请登录后再执行操作");
                return;
            }
            if (m.replyText.Trim().Length == 0)
            {
                Utils.ShowMessageToast("不能发送空白信息");
                return;
            }
            try
            {
                var re = await commentApi.ReplyComment(_loadCommentInfo.oid, m.rpid.ToString(), m.rpid.ToString(), Uri.EscapeDataString(m.replyText), _loadCommentInfo.commentMode).Request();
                if (re.status)
                {
                    JObject obj = JObject.Parse(re.results);
                    if (obj["code"].ToInt32() == 0)
                    {
                        Utils.ShowMessageToast("回复评论成功");
                        m.loadpage = 1;
                        m.replies.Clear();
                        m.replyText = "";
                        GetReply(m);
                    }
                    else
                    {
                        Utils.ShowMessageToast(obj["message"].ToString());
                    }
                }
                else
                {
                    Utils.ShowMessageToast(re.message);
                }

            }
            catch (Exception)
            {
                Utils.ShowMessageToast("发送评论失败");
                // throw;
            }

        }
        private void btn_ReplyAt_Click(object sender, RoutedEventArgs e)
        {
            var m = (sender as Button).DataContext as CommentModel;
            ReplyAt(m);
        }
        private async void ReplyAt(CommentModel m)
        {
            if (!SettingHelper.Account.Logined && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请登录后再执行操作");
                return;
            }
            if (m.replyText.Trim().Length == 0)
            {
                Utils.ShowMessageToast("不能发送空白信息");
                return;
            }
            try
            {
                //string url = "https://api.bilibili.com/x/v2/reply/add";

                var txt = "回复 @" + m.member.uname + ":" + m.replyText;
                //string content =
                //    string.Format("access_key={0}&appkey={1}&platform=android&type={2}&oid={3}&ts={4}&message={5}&root={6}&parent={7}",
                //    ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, _type, _loadCommentInfo.oid, ApiHelper.GetTimeSpan_2, Uri.EscapeDataString(txt), m.root, m.rpid);
                //content += "&sign=" + ApiHelper.GetSign(content);

                var re = await commentApi.ReplyComment(_loadCommentInfo.oid, m.root.ToString(), m.rpid.ToString(), Uri.EscapeDataString(txt), _loadCommentInfo.commentMode).Request();
                if (re.status)
                {
                    JObject obj = JObject.Parse(re.results);
                    if (obj["code"].ToInt32() == 0)
                    {
                        Utils.ShowMessageToast("回复评论成功");
                        m.loadpage = 1;
                        m.replies.Clear();
                        m.replyText = "";
                        await GetReply(m);
                    }
                    else
                    {
                        Utils.ShowMessageToast(obj["message"].ToString());
                    }
                }
                else
                {
                    Utils.ShowMessageToast(re.message);
                }

            }
            catch (Exception)
            {
                Utils.ShowMessageToast("发送评论失败");
                // throw;
            }






        }

        private void btn_DeleteComment_Click(object sender, RoutedEventArgs e)
        {
            CommentModel m = null;
            if (sender is HyperlinkButton)
            {
                m = (sender as HyperlinkButton).DataContext as CommentModel;
            }
            if (sender is MenuFlyoutItem)
            {
                m = (sender as MenuFlyoutItem).DataContext as CommentModel;
            }
            DeletComment(m);
        }

        private async void DeletComment(CommentModel m)
        {
            if (m == null)
            {
                return;
            }
            if (!SettingHelper.Account.Logined && !await Utils.ShowLoginDialog())
            {
                Utils.ShowMessageToast("请登录后再执行操作");
                return;
            }

            try
            {
                //string url = "https://api.bilibili.com/x/v2/reply/del";

                //string content =
                //    string.Format("access_key={0}&appkey={1}&platform=android&type={2}&oid={3}&ts={4}&rpid={5}",
                //    ApiHelper.access_key, ApiHelper.AndroidKey.Appkey, _type, _loadCommentInfo.oid, ApiHelper.GetTimeSpan_2, m.rpid);
                //content += "&sign=" + ApiHelper.GetSign(content);

                //var re = await WebClientClass.PostResults(new Uri(url), content);
                var re = await commentApi.DeleteComment(_loadCommentInfo.oid, m.rpid.ToString(), _loadCommentInfo.commentMode).Request();
                if (re.status)
                {
                    JObject obj = JObject.Parse(re.results);
                    if (obj["code"].ToInt32() == 0)
                    {
                        Utils.ShowMessageToast("评论删除成功");
                        RefreshComment();
                    }
                    else
                    {
                        Utils.ShowMessageToast(obj["message"].ToString());
                    }
                }
                else
                {
                    Utils.ShowMessageToast(re.message);
                }
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("删除评论失败");
                // throw;
            }


        }
        CommentModel selectComment;
        private void btnFace_Click(object sender, RoutedEventArgs e)
        {
            selectComment = (sender as Button).DataContext as CommentModel;
            FaceFlyout.ShowAt(sender as Button);
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            selectComment.replyText += (e.ClickedItem as EmotePackageItemModel).text.ToString();
        }

        private async void btnOpenSendComment_Click(object sender, RoutedEventArgs e)
        {
            SendCommentDialog sendCommentDialog = new SendCommentDialog(_loadCommentInfo.oid, (CommentType)_loadCommentInfo.commentMode);
            await sendCommentDialog.ShowAsync();
        }
    }

    public class LoadCommentInfo
    {
        public int commentMode { get; set; }
        public commentSort commentSort { get; set; }
        public string oid { get; set; }
    }

    public class dataCommentModel
    {

        public int code { get; set; }
        public string message { get; set; }

        public dataCommentModel data { get; set; }

        public dataCommentModel page { get; set; }
        public int acount { get; set; }
        public int count { get; set; }
        public int num { get; set; }
        public int size { get; set; }

        public ObservableCollection<CommentModel> hots { get; set; }
        public ObservableCollection<CommentModel> replies { get; set; }

        public dataCommentModel upper { get; set; }
        public CommentModel top { get; set; }

    }
    public class CommentModel : INotifyPropertyChanged
    {

        public CommentModel()
        {
            LaunchUrlCommand = new RelayCommand<object>(ButtonClick);
        }

        private int _action;//0未点赞,1已经点赞
        public int action
        {
            get { return _action; }
            set { _action = value; thisPropertyChanged("action"); thisPropertyChanged("LikeColor"); }
        }

        public SolidColorBrush LikeColor
        {
            get
            {
                if (action == 0)
                {
                    return new SolidColorBrush(Colors.Gray);
                }
                else
                {
                    return new SolidColorBrush((Color)Application.Current.Resources["HighLightColor"]);
                }
            }
        }


        public long rpid { get; set; }
        public long oid { get; set; }
        public int type { get; set; }
        public long mid { get; set; }
        public long root { get; set; }
        public long parent { get; set; }

        public int count { get; set; }
        private int _rcount;
        public int rcount
        {
            get { return _rcount; }
            set { _rcount = value; thisPropertyChanged("rcount"); }
        }
        public int _like { get; set; }
        public int like
        {
            get { return _like; }
            set { _like = value; thisPropertyChanged("like"); thisPropertyChanged("like_str"); }
        }


        public string rcount_str
        {
            get
            {
                if (rcount > 10000)
                {
                    return ((double)rcount / 10000).ToString("0.0") + "万";
                }
                else
                {
                    return rcount.ToString();
                }
            }

        }
        public string like_str
        {
            get
            {
                if (like > 10000)
                {
                    return ((double)like / 10000).ToString("0.0") + "万";
                }
                else
                {
                    return like.ToString();
                }
            }

        }


        public int floor { get; set; }
        public int state { get; set; }
        public long ctime { get; set; }
        public string time
        {
            get
            {
                //DateTime dtStart = new DateTime(1970, 1, 1);
                //long lTime = long.Parse(ctime + "0000000");
                ////long lTime = long.Parse(textBox1.Text);
                //TimeSpan toNow = new TimeSpan(lTime);
                //return dtStart.Add(toNow).ToLocalTime().ToString();

                DateTime dtStart = new DateTime(1970, 1, 1);
                long lTime = long.Parse(ctime + "0000000");
                //long lTime = long.Parse(textBox1.Text);
                TimeSpan toNow = TimeSpan.FromSeconds(ctime);
                DateTime dt = dtStart.Add(toNow).ToLocalTime();
                TimeSpan span = DateTime.Now - dt;
                if (span.TotalDays > 7)
                {
                    return dt.ToString("yyyy-MM-dd");
                }
                else
                if (span.TotalDays > 1)
                {
                    return string.Format("{0}天前", (int)Math.Floor(span.TotalDays));
                }
                else
                if (span.TotalHours > 1)
                {
                    return string.Format("{0}小时前", (int)Math.Floor(span.TotalHours));
                }
                else
                if (span.TotalMinutes > 1)
                {
                    return string.Format("{0}分钟前", (int)Math.Floor(span.TotalMinutes));
                }
                else
                if (span.TotalSeconds >= 1)
                {
                    return string.Format("{0}秒前", (int)Math.Floor(span.TotalSeconds));
                }
                else
                {
                    return "1秒前";
                }


            }
        }

        public string rpid_str { get; set; }

        public CommentMemberModel member { get; set; }
        public CommentContentModel content { get; set; }


        private ObservableCollection<CommentModel> _replies = new ObservableCollection<CommentModel>();
        public ObservableCollection<CommentModel> replies
        {
            get { return _replies; }
            set { _replies = value; thisPropertyChanged("replies"); }
        }
        //public ObservableCollection<CommentModel> replies { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void thisPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }


        private Visibility _showReplies = Visibility.Collapsed;
        public Visibility showReplies
        {
            get { return _showReplies; }
            set { _showReplies = value; thisPropertyChanged("showReplies"); }
        }

        private Visibility _showReplyBtn = Visibility.Collapsed;
        public Visibility showReplyBtn
        {
            get { return _showReplyBtn; }
            set { _showReplyBtn = value; thisPropertyChanged("showReplyBtn"); }
        }

        private Visibility _showReplyBox = Visibility.Collapsed;
        public Visibility showReplyBox
        {
            get { return _showReplyBox; }
            set { _showReplyBox = value; thisPropertyChanged("showReplyBox"); }
        }


        private Visibility _showReplyMore = Visibility.Collapsed;
        public Visibility showReplyMore
        {
            get { return _showReplyMore; }
            set { _showReplyMore = value; thisPropertyChanged("showReplyMore"); }
        }

        private Visibility _showLoading = Visibility.Collapsed;
        public Visibility showLoading
        {
            get { return _showLoading; }
            set { _showLoading = value; thisPropertyChanged("showLoading"); }
        }


        public Visibility showDelete
        {
            get
            {
                if (SettingHelper.Account.Logined && mid.ToString() == SettingHelper.Account.UserID.ToString())
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }



        private int _loadpage = 1;
        public int loadpage
        {
            get { return _loadpage; }
            set { _loadpage = value; thisPropertyChanged("loadpage"); }
        }



        public string replyAt
        {
            get
            {
                return "回复 @" + member.uname;
            }
        }


        private string _replyText;
        public string replyText
        {
            get { return _replyText; }
            set { _replyText = value; thisPropertyChanged("replyText"); }
        }


        private Visibility _showTop = Visibility.Collapsed;
        public Visibility showTop
        {
            get { return _showTop; }
            set { _showTop = value; thisPropertyChanged("showTop"); }
        }



        public RelayCommand<object> LaunchUrlCommand { get; private set; }

        private async void ButtonClick(object paramenter)
        {

            await MessageCenter.HandelUrl(paramenter.ToString());
            return;
           

        }


    }

    public class CommentContentModel
    {

        public string message { get; set; }
        public int plat { get; set; }
        public string plat_str
        {
            get
            {
                switch (plat)
                {
                    case 2:
                        return "来自 Android";
                    case 3:
                        return "来自 IOS";
                    case 4:
                        return "来自 WindowsPhone";
                    case 6:
                        return "来自 Windows";
                    default:
                        return "";
                }
            }
        }
        public string device { get; set; }
        public RichTextBlock text 
        {
            get
            {
                //var tx = new RichTextBlock();
                //Paragraph paragraph = new Paragraph();
                //Run run = new Run() { Text = message };
                //paragraph.Inlines.Add(run);
                //tx.Blocks.Add(paragraph);
                //return tx;

                return  ControlHelper.StringToRichText(message, emote);
            }

        }

        public JObject emote { get; set; }
    }


    public class CommentMemberModel
    {
        public string mid { get; set; }
        public string uname { get; set; }
        public string sex { get; set; }
        public string sign { get; set; }


        //public string avatar { get; set; }
        private string _avatar;
        public string avatar { get { return _avatar; } set { _avatar = value + "@64w_64h.jpg"; } }


        public CommentMemberModel level_info { get; set; }
        public int current_level { get; set; }
        public string LV
        {
            get
            {
                switch (level_info.current_level)
                {
                    case 0:
                        return "ms-appx:///Assets/Icon/lv0.png";
                    case 1:
                        return "ms-appx:///Assets/Icon/lv1.png";
                    case 2:
                        return "ms-appx:///Assets/Icon/lv2.png";
                    case 3:
                        return "ms-appx:///Assets/Icon/lv3.png";
                    case 4:
                        return "ms-appx:///Assets/Icon/lv4.png";
                    case 5:
                        return "ms-appx:///Assets/Icon/lv5.png";
                    case 6:
                        return "ms-appx:///Assets/Icon/lv6.png";
                    default:
                        return AppHelper.TRANSPARENT_IMAGE;
                }
            }
        }


        public string pendant_str
        {
            get
            {
                if (pendant != null)
                {
                    if (pendant.image == "")
                    {
                        return AppHelper.TRANSPARENT_IMAGE;
                    }
                    return pendant.image;
                }
                else
                {
                    return AppHelper.TRANSPARENT_IMAGE;
                }
            }
        }
        public CommentMemberModel pendant { get; set; }
        public int pid { get; set; }
        public string name { get; set; }
        public string image { get; set; }

        public CommentMemberModel official_verify { get; set; }
        public int type { get; set; }
        public string desc { get; set; }

        public string Verify
        {
            get
            {
                if (official_verify == null)
                {
                    return "";
                }
                switch (official_verify.type)
                {
                    case 0:
                        return AppHelper.VERIFY_PERSONAL_IMAGE;
                    case 1:
                        return AppHelper.VERIFY_OGANIZATION_IMAGE;
                    default:
                        return AppHelper.TRANSPARENT_IMAGE;
                }
            }
        }

        public CommentMemberModel vip { get; set; }
        public int vipType { get; set; }
        public SolidColorBrush vip_co
        {
            get
            {
                if (vip.vipType == 2)
                {
                    return new SolidColorBrush(Colors.DeepPink);
                }
                else
                {
                    return new SolidColorBrush((Color)Application.Current.Resources["TextColor"]);
                }
            }
        }



    }

}
