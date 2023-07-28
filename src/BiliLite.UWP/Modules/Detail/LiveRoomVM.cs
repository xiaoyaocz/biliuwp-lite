using BiliLite.Models;
using BiliLite.Models.Requests.Api;
using BiliLite.Models.Requests.Api.Live;
using BiliLite.Modules.Live;
using BiliLite.Modules.LiveRoomDetailModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Services;

namespace BiliLite.Modules
{
    public class LiveRoomVM : IModules, IDisposable
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        public LiveRoomAnchorLotteryVM anchorLotteryVM;
        readonly LiveRoomAPI liveRoomAPI;
        readonly PlayerAPI PlayerAPI;
        System.Threading.CancellationTokenSource cancelSource;
        Live.LiveMessage liveMessage;
        public event EventHandler<LiveRoomPlayUrlModel> ChangedPlayUrl;
        public event EventHandler<LiveRoomEndAnchorLotteryInfoModel> LotteryEnd;
        public event EventHandler<string> AddNewDanmu;

        readonly Timer timer;
        readonly Timer timer_box;
        readonly Timer timer_auto_hide_gift;
        public LiveRoomVM()
        {
            liveRoomAPI = new LiveRoomAPI();
            PlayerAPI = new PlayerAPI();
            //liveMessage = new Live.LiveMessage();
            anchorLotteryVM = new LiveRoomAnchorLotteryVM();
            MessageCenter.LoginedEvent += MessageCenter_LoginedEvent;
            MessageCenter.LogoutedEvent += MessageCenter_LogoutedEvent;
            Logined = SettingService.Account.Logined;
            Messages = new ObservableCollection<DanmuMsgModel>();
            GiftMessage = new ObservableCollection<GiftMsgModel>();
            Guards = new ObservableCollection<LiveGuardRankItem>();
            BagGifts = new ObservableCollection<LiveGiftItem>();
            SuperChats = new ObservableCollection<SuperChatMsgModel>();
            timer = new Timer(1000);
            timer_box = new Timer(1000);
            timer_auto_hide_gift = new Timer(1000);
            timer.Elapsed += Timer_Elapsed;
            timer_box.Elapsed += Timer_box_Elapsed;
            timer_auto_hide_gift.Elapsed += Timer_auto_hide_gift_Elapsed;

            LoadMoreGuardCommand = new RelayCommand(LoadMoreGuardList);
            ShowBagCommand = new RelayCommand(SetShowBag);
            RefreshBagCommand = new RelayCommand(RefreshBag);
        }

        private void LiveMessage_NewMessage(MessageType type, object message)
        {
            if (Messages == null) return;
            switch (type)
            {
                case MessageType.ConnectSuccess:
                    Messages.Add(new DanmuMsgModel()
                    {
                        username = message.ToString(),
                        uname_color = new SolidColorBrush(Colors.Gray)
                    });
                    break;
                case MessageType.Online:
                    Online = (int)message;
                    break;
                case MessageType.Danmu:
                    {
                        var m = message as DanmuMsgModel;
                        m.uname_color = new SolidColorBrush(Colors.Gray);
                        if (m.medalColor != null && m.medalColor != "")
                        {
                            m.ul_color = new SolidColorBrush(m.ulColor.StrToColor());
                        }
                        else
                        {
                            m.ul_color = new SolidColorBrush(Colors.Gray);
                        }
                        if (m.medalColor != null && m.medalColor != "")
                        {
                            m.medal_color = new SolidColorBrush(m.medalColor.StrToColor());
                        }

                        if (Messages.Count >= CleanCount)
                        {
                            Messages.Clear();
                        }
                        Messages.Add(m);
                        AddNewDanmu?.Invoke(this, m.text);
                    }
                    break;
                case MessageType.Gift:
                    {
                        if (!ReceiveGiftMsg)
                        {
                            return;
                        }
                        if (GiftMessage.Count >= 2)
                        {
                            GiftMessage.RemoveAt(0);
                        }
                        ShowGiftMessage = true;
                        hide_gift_flag = 1;
                        var info = message as GiftMsgModel;
                        info.gif = _allGifts.FirstOrDefault(x => x.id == info.giftId)?.gif ?? Constants.App.TRANSPARENT_IMAGE;
                        GiftMessage.Add(info);
                        if (!timer_auto_hide_gift.Enabled)
                        {
                            timer_auto_hide_gift.Start();
                        }
                    }

                    break;
                case MessageType.Welcome:
                    {
                        var info = message as WelcomeMsgModel;
                        if (ReceiveWelcomeMsg)
                        {
                            Messages.Add(new DanmuMsgModel()
                            {
                                isVip = ((info.svip) ? Visibility.Collapsed : Visibility.Visible),
                                isBigVip = ((info.svip) ? Visibility.Visible : Visibility.Collapsed),
                                hasUL = Visibility.Collapsed,
                                username = info.uname,
                                uname_color = new SolidColorBrush(Colors.HotPink),
                                text = " 进入直播间"
                            });
                        }
                    }
                    break;
                case MessageType.WelcomeGuard:
                    {
                        var info = message as WelcomeMsgModel;
                        if (ReceiveWelcomeMsg)
                        {
                            Messages.Add(new DanmuMsgModel()
                            {
                                isVip = ((info.svip) ? Visibility.Collapsed : Visibility.Visible),
                                isBigVip = ((info.svip) ? Visibility.Visible : Visibility.Collapsed),
                                hasUL = Visibility.Collapsed,
                                username = info.uname,
                                uname_color = new SolidColorBrush(Colors.HotPink),
                                text = " (舰长)进入直播间"
                            });
                        }
                    }
                    break;
                case MessageType.SystemMsg:
                    break;
                case MessageType.SuperChat:
                case MessageType.SuperChatJpn:
                    SuperChats.Add(message as SuperChatMsgModel);
                    break;
                case MessageType.AnchorLotteryStart:
                    if (ReceiveLotteryMsg)
                    {
                        var info = message.ToString();
                        anchorLotteryVM.SetLotteryInfo(JsonConvert.DeserializeObject<LiveRoomAnchorLotteryInfoModel>(info));
                    }
                    break;
                case MessageType.AnchorLotteryEnd:
                    break;
                case MessageType.AnchorLotteryAward:
                    if (ReceiveLotteryMsg)
                    {
                        var info = JsonConvert.DeserializeObject<LiveRoomEndAnchorLotteryInfoModel>(message.ToString());
                        LotteryEnd?.Invoke(this, info);
                    }
                    break;

                case MessageType.GuardBuy:
                    break;
                default:
                    break;
            }
        }

        public ICommand LoadMoreGuardCommand { get; private set; }
        public ICommand ShowBagCommand { get; private set; }
        public ICommand RefreshBagCommand { get; private set; }
        private async void Timer_auto_hide_gift_Elapsed(object sender, ElapsedEventArgs e)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (GiftMessage != null && GiftMessage.Count != 0)
                {
                    if (hide_gift_flag >= 5)
                    {
                        ShowGiftMessage = false;
                        GiftMessage.Clear();
                    }
                    else
                    {
                        hide_gift_flag++;
                    }
                }
            });
        }

        private void MessageCenter_LogoutedEvent(object sender, EventArgs e)
        {
            Logined = false;
            timer_box.Stop();
            ShowBox = false;
            OpenBox = false;
        }

        private async void MessageCenter_LoginedEvent(object sender, object e)
        {
            Logined = true;
            await LoadWalletInfo();
            await LoadBag();
            //await GetFreeSilverTime();
        }

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (LiveInfo == null && !Liveing)
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        LiveTime = "";
                    });
                    return;
                }
                var start_time = TimeExtensions.TimestampToDatetime(LiveInfo.room_info.live_start_time);
                var ts = DateTime.Now - start_time;

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                {
                                    for (int i = 0; i < SuperChats.Count; i++)
                                    {
                                        var item = SuperChats[i];
                                        if (item.time <= 0)
                                        {
                                            SuperChats.Remove(item);
                                        }
                                        else
                                        {
                                            item.time -= 1;
                                        }
                                    }

                                    LiveTime = ts.ToString(@"hh\:mm\:ss");
                                });
            }
            catch (Exception)
            {
            }

        }
        public static List<LiveTitleModel> Titles { get; set; }

        private bool _logined = false;
        public bool Logined
        {
            get { return _logined; }
            set { _logined = value; DoPropertyChanged("Logined"); }
        }
        /// <summary>
        /// 直播ID
        /// </summary>
        public int RoomID { get; set; }

        public ObservableCollection<DanmuMsgModel> Messages { get; set; }
        public ObservableCollection<GiftMsgModel> GiftMessage { get; set; }
        public ObservableCollection<LiveGiftItem> BagGifts { get; set; }
        public bool ReceiveWelcomeMsg { get; set; } = true;
        public bool ReceiveLotteryMsg { get; set; } = true;
        public bool ReceiveGiftMsg { get; set; } = true;
        private int hide_gift_flag = 1;
        private bool _show_gift_message = false;
        public bool ShowGiftMessage
        {
            get { return _show_gift_message; }
            set { _show_gift_message = value; DoPropertyChanged("ShowGiftMessage"); }
        }



        private int _online = 0;
        /// <summary>
        /// 人气值
        /// </summary>
        public int Online
        {
            get { return _online; }
            set { _online = value; DoPropertyChanged("Online"); }
        }

        private bool _loading = true;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }

        private bool _attention = false;
        public bool Attention
        {
            get { return _attention; }
            set { _attention = value; DoPropertyChanged("Attention"); }
        }

        private bool _ShowBag = false;
        public bool ShowBag
        {
            get { return _ShowBag; }
            set { _ShowBag = value; DoPropertyChanged("ShowBag"); }
        }


        private List<LiveRoomRankVM> ranks;
        public List<LiveRoomRankVM> Ranks
        {
            get { return ranks; }
            set { ranks = value; DoPropertyChanged("Ranks"); }
        }

        private LiveRoomRankVM _selectRank;
        public LiveRoomRankVM SelectRank
        {
            get { return _selectRank; }
            set { _selectRank = value; }
        }

        public ObservableCollection<LiveGuardRankItem> Guards { get; set; }
        public ObservableCollection<SuperChatMsgModel> SuperChats { get; set; }

        private LiveRoomWebUrlQualityDescriptionItemModel _current_qn;
        public LiveRoomWebUrlQualityDescriptionItemModel current_qn
        {
            get { return _current_qn; }
            set { _current_qn = value; }
        }

        private List<LiveRoomWebUrlQualityDescriptionItemModel> _qualites;
        public List<LiveRoomWebUrlQualityDescriptionItemModel> qualites
        {
            get { return _qualites; }
            set { _qualites = value; DoPropertyChanged("qualites"); }
        }

        private List<LiveGiftItem> _allGifts = new List<LiveGiftItem>();

        private List<LiveGiftItem> _gifts;
        public List<LiveGiftItem> Gifts
        {
            get { return _gifts; }
            set { _gifts = value; DoPropertyChanged("Gifts"); }
        }

        private List<LiveBagGiftItem> _bag;
        public List<LiveBagGiftItem> Bag
        {
            get { return _bag; }
            set { _bag = value; DoPropertyChanged("Bag"); }
        }

        public List<LiveRoomRealPlayUrlsModel> urls { get; set; }
        private LiveWalletInfo _wallet;

        public LiveWalletInfo WalletInfo
        {
            get { return _wallet; }
            set { _wallet = value; DoPropertyChanged("WalletInfo"); }
        }


        private LiveInfoModel _LiveInfo;
        public LiveInfoModel LiveInfo
        {
            get { return _LiveInfo; }
            set { _LiveInfo = value; DoPropertyChanged("LiveInfo"); }
        }
        private LiveAnchorProfile _profile;
        public LiveAnchorProfile Profile
        {
            get { return _profile; }
            set { _profile = value; DoPropertyChanged("Profile"); }
        }

        private bool _liveing = true;
        public bool Liveing
        {
            get { return _liveing; }
            set { _liveing = value; DoPropertyChanged("Liveing"); }
        }

        private string _live_time = "";
        public string LiveTime
        {
            get { return _live_time; }
            set { _live_time = value; DoPropertyChanged("LiveTime"); }
        }
        public int CleanCount { get; set; } = 200;
        /// <summary>
        /// 读取直播头衔
        /// </summary>
        /// <returns></returns>
        public async Task GetTitles()
        {
            try
            {
                if (Titles != null)
                {
                    return;
                }
                var results = await liveRoomAPI.LiveTitles().Request();
                if (results.status)
                {
                    var data = await results.GetData<List<LiveTitleModel>>();
                    if (data.success)
                    {
                        Titles = data.data;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log("读取直播头衔失败", LogType.Fatal, ex);
            }
        }
        /// <summary>
        /// 读取直播播放地址
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="qn"></param>
        /// <returns></returns>
        public async Task GetPlayUrl(int roomId, int qn = 0)
        {
            try
            {
                Loading = true;
                var results = await PlayerAPI.LivePlayUrl(roomId.ToString(), qn).Request();
                if (!results.status)
                {
                    throw new CustomizedErrorException(results.message);
                }
                var data = await results.GetJson<ApiDataModel<LiveRoomPlayUrlModel>>();

                if (!data.success)
                {
                    throw new CustomizedErrorException(data.message);
                }
                // 暂时不优先使用flv流
                LiveRoomWebUrlStreamItemModel stream = null;
                if (data.data.playurl_info.playurl.stream.Any(item => item.protocol_name == "http_hls"))
                {
                    stream = data.data.playurl_info.playurl.stream.FirstOrDefault(item => item.protocol_name == "http_hls");
                }
                else if (data.data.playurl_info.playurl.stream.Any(item => item.protocol_name == "http_stream"))
                {
                    stream = data.data.playurl_info.playurl.stream.FirstOrDefault(item => item.protocol_name == "http_stream");
                }
                else
                {
                    throw new CustomizedErrorException("找不到直播流地址");
                }
                var codecList = stream.format[0].codec;

                var routeIndex = 1;
                foreach (var item in codecList.SelectMany(codecItem => codecItem.url_info))
                {
                    item.name = "线路" + routeIndex;
                    routeIndex++;
                }

                // 暂时不使用hevc流
                var codec = codecList.FirstOrDefault(item => item.codec_name == "avc");

                var acceptQnList = codec.accept_qn;
                qualites ??= data.data.playurl_info.playurl.g_qn_desc.Where(item => acceptQnList.Contains(item.qn)).ToList();
                current_qn = data.data.playurl_info.playurl.g_qn_desc.FirstOrDefault(x => x.qn == codec.current_qn);
                
                var urlList = codec.url_info.Select(urlInfo => new LiveRoomRealPlayUrlsModel { url = urlInfo.host + codec.base_url + urlInfo.extra, name = urlInfo.name }).ToList();

                urls = urlList;

                ChangedPlayUrl?.Invoke(this, data.data);
            }
            catch (Exception ex)
            {
                Notify.ShowMessageToast($"读取播放地址失败:{ex.Message}");
                _logger.Error($"读取播放地址失败:{ex.Message}", ex);
            }
            finally
            {
                Loading = false;
            }
        }
        /// <summary>
        /// 读取直播间详细信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task LoadLiveRoomDetail(string id)
        {
            try
            {
                if (cancelSource != null)
                {
                    cancelSource.Cancel();
                    cancelSource.Dispose();
                }
                if (liveMessage != null)
                {
                    liveMessage.NewMessage -= LiveMessage_NewMessage;
                    liveMessage = null;

                }
                liveMessage = new LiveMessage();
                liveMessage.NewMessage += LiveMessage_NewMessage;
                cancelSource = new System.Threading.CancellationTokenSource();

                Loading = true;
                var result = await liveRoomAPI.LiveRoomInfo(id).Request();
                if (result.status)
                {
                    var data = await result.GetData<LiveInfoModel>();
                    if (data.success)
                    {
                        RoomID = data.data.room_info.room_id;
                        Online = data.data.room_info.online;
                        Liveing = data.data.room_info.live_status == 1;
                        LiveInfo = data.data;
                        if (Ranks == null)
                        {
                            Ranks = new List<LiveRoomRankVM>() {
                                new LiveRoomRankVM(RoomID,data.data.room_info.uid,"金瓜子榜","gold-rank"),
                                new LiveRoomRankVM(RoomID,data.data.room_info.uid,"今日礼物榜","today-rank"),
                                new LiveRoomRankVM(RoomID,data.data.room_info.uid,"七日礼物榜","seven-rank"),
                                new LiveRoomRankVM(RoomID,data.data.room_info.uid,"粉丝榜","fans"),
                            };
                            SelectRank = Ranks[0];
                            DoPropertyChanged("SelectRank");
                        }


                        await LoadAnchorProfile();
                        if (Liveing)
                        {
                            timer.Start();
                            await GetPlayUrl(RoomID, SettingService.GetValue(SettingConstants.Live.DEFAULT_QUALITY, 10000));
                            //GetFreeSilverTime();  
                            await LoadSuperChat();
                            if (ReceiveLotteryMsg)
                            {
                                anchorLotteryVM.LoadLotteryInfo(RoomID).RunWithoutAwait();
                            }
                        }
                        await GetRoomGiftList();
                        await LoadBag();
                        await LoadWalletInfo();
                        if (Titles == null)
                        {
                            await GetTitles();
                        }
                        EntryRoom();
                        ReceiveMessage(data.data.room_info.room_id);
                    }
                    else
                    {
                        Notify.ShowMessageToast("加载直播间失败:" + data.message);
                    }
                }
                else
                {
                    Notify.ShowMessageToast(result.message);
                }
            }
            catch (Exception ex)
            {
                var result = HandelError<object>(ex);
                Notify.ShowMessageToast(ex.Message);
            }
            finally
            {
                Loading = false;
            }
        }

        private async void ReceiveMessage(int roomId)
        {
            try
            {
                var uid = 0;
                if (SettingService.Account.Logined)
                {
                    uid = SettingService.Account.UserID;
                }
                if (liveMessage == null)
                {
                    liveMessage = new LiveMessage();
                }
                await liveMessage.Connect(roomId, uid, cancelSource.Token);
            }
            catch (TaskCanceledException)
            {
                Messages.Add(new DanmuMsgModel()
                {
                    username = "取消连接"
                });
            }
            catch (Exception ex)
            {
                Messages?.Add(new DanmuMsgModel()
                {
                    username = "连接失败:" + ex.Message
                });
            }

        }

        /// <summary>
        /// 读取醒目留言
        /// </summary>
        /// <returns></returns>
        public async Task LoadSuperChat()
        {
            try
            {

                var result = await liveRoomAPI.RoomSuperChat(RoomID).Request();
                if (result.status)
                {
                    var data = await result.GetData<JObject>();
                    if (data.success)
                    {
                        SuperChats.Clear();
                        var ls = JsonConvert.DeserializeObject<List<LiveRoomSuperChatModel>>(data.data["list"]?.ToString() ?? "[]");
                        foreach (var item in ls)
                        {
                            SuperChats.Add(new SuperChatMsgModel()
                            {
                                background_bottom_color = item.background_bottom_color,
                                background_color = item.background_color,
                                background_image = item.background_image,
                                end_time = item.end_time,
                                face = item.user_info.face,
                                face_frame = item.user_info.face_frame,
                                font_color = string.IsNullOrEmpty(item.font_color) ? "#FFFFFF" : item.font_color,
                                max_time = item.end_time - item.start_time,
                                message = item.message,
                                price = item.price,
                                start_time = item.start_time,
                                time = item.time,
                                username = item.user_info.uname
                            });
                        }
                    }
                    else
                    {
                        Notify.ShowMessageToast("读取醒目留言失败:" + data.message);
                    }
                }
                else
                {
                    Notify.ShowMessageToast(result.message);
                }
            }
            catch (Exception ex)
            {
                var result = HandelError<object>(ex);
                Notify.ShowMessageToast(ex.Message);
            }
        }
        /// <summary>
        /// 进入房间
        /// </summary>
        public async void EntryRoom()
        {
            try
            {
                if (SettingService.Account.Logined)
                {
                    await liveRoomAPI.RoomEntryAction(RoomID).Request();
                }
            }
            catch (Exception)
            {
            }

        }
        /// <summary>
        /// 读取钱包信息
        /// </summary>
        /// <returns></returns>
        public async Task LoadWalletInfo()
        {
            try
            {
                if (!Logined)
                {
                    return;
                }
                var result = await liveRoomAPI.MyWallet().Request();
                if (result.status)
                {
                    var data = await result.GetData<LiveWalletInfo>();
                    if (data.success)
                    {
                        WalletInfo = data.data;
                    }
                    else
                    {
                        Notify.ShowMessageToast("读取钱包失败:" + data.message);
                    }
                }
                else
                {
                    Notify.ShowMessageToast(result.message);
                }
            }
            catch (Exception ex)
            {
                var result = HandelError<object>(ex);
                Notify.ShowMessageToast(ex.Message);
            }
        }
        /// <summary>
        /// 读取背包礼物
        /// </summary>
        /// <returns></returns>
        public async Task LoadBag()
        {
            try
            {
                if (!Logined)
                {
                    return;
                }
                var result = await liveRoomAPI.GiftList(LiveInfo.room_info.area_id, LiveInfo.room_info.parent_area_id, RoomID).Request();
                if (result.status)
                {
                    var data = await result.GetData<JObject>();
                    if (data.success)
                    {
                        var list = JsonConvert.DeserializeObject<List<LiveGiftItem>>(data.data["list"].ToString());

                        var bag_result = await liveRoomAPI.BagList(RoomID).Request();
                        if (bag_result.status)
                        {
                            var bag_data = await bag_result.GetData<JObject>();
                            if (bag_data.success)
                            {
                                BagGifts.Clear();
                                var ls = JsonConvert.DeserializeObject<List<LiveBagGiftItem>>(bag_data.data["list"]?.ToString() ?? "[]");
                                if (ls != null)
                                    foreach (var item in ls)
                                    {
                                        var _gift = list.FirstOrDefault(x => x.id == item.gift_id);
                                        var gift = _gift.ObjectClone();
                                        gift.gift_num = item.gift_num;
                                        gift.corner_mark = item.corner_mark;
                                        gift.bag_id = item.bag_id;
                                        BagGifts.Add(gift);
                                    }
                                //WalletInfo = data.data;
                            }
                            else
                            {
                                Notify.ShowMessageToast("读取背包失败:" + bag_data.message);
                            }
                        }
                        else
                        {
                            Notify.ShowMessageToast(bag_result.message);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                var result = HandelError<object>(ex);
                Notify.ShowMessageToast(ex.Message);
            }
        }
        /// <summary>
        /// 读取主播资料
        /// </summary>
        /// <returns></returns>
        public async Task LoadAnchorProfile()
        {
            try
            {
                var result = await liveRoomAPI.AnchorProfile(LiveInfo.room_info.uid).Request();
                if (result.status)
                {
                    var data = await result.GetData<LiveAnchorProfile>();
                    if (data.success)
                    {
                        Profile = data.data;
                        Attention = Profile.relation_status > 1;
                    }
                    else
                    {
                        Notify.ShowMessageToast("读取主播信息失败:" + data.message);
                    }
                }
                else
                {
                    Notify.ShowMessageToast(result.message);
                }
            }
            catch (Exception ex)
            {
                var result = HandelError<object>(ex);
                Notify.ShowMessageToast(ex.Message);
            }
        }
        /// <summary>
        /// 读取直播间可用礼物列表
        /// </summary>
        /// <returns></returns>
        public async Task GetRoomGiftList()
        {
            try
            {
                var result = await liveRoomAPI.GiftList(LiveInfo.room_info.area_id, LiveInfo.room_info.parent_area_id, RoomID).Request();
                if (result.status)
                {
                    var data = await result.GetData<JObject>();
                    if (data.success)
                    {
                        var list = JsonConvert.DeserializeObject<List<LiveGiftItem>>(data.data["list"].ToString());
                        if (_allGifts == null || _allGifts.Count == 0) { _allGifts = list; }

                        var result_room = await liveRoomAPI.RoomGifts(LiveInfo.room_info.area_id, LiveInfo.room_info.parent_area_id, RoomID).Request();
                        if (result_room.status)
                        {
                            var data_room = await result_room.GetData<JObject>();
                            var list_room = JsonConvert.DeserializeObject<List<LiveRoomGiftItem>>(data_room.data["list"].ToString());
                            List<LiveGiftItem> ls = new List<LiveGiftItem>() {
                               list.FirstOrDefault(x=>x.id==1)
                            };
                            foreach (var item in list_room)
                            {
                                ls.Add(list.FirstOrDefault(x => x.id == item.gift_id));
                            }
                            Gifts = ls;
                        }
                    }

                }


            }
            catch (Exception ex)
            {
                Notify.ShowMessageToast("读取礼物信息失败");
                _logger.Log("读取礼物信息失败", LogType.Error, ex);
            }
        }

        public int GuardPage { get; set; } = 1;
        private bool _loadingGuard = true;
        public bool LoadingGuard
        {
            get { return _loadingGuard; }
            set { _loadingGuard = value; DoPropertyChanged("LoadingGuard"); }
        }
        private bool _moreGuard = false;
        public bool LoadMoreGuard
        {
            get { return _moreGuard; }
            set { _moreGuard = value; DoPropertyChanged("LoadMoreGuard"); }
        }
        /// <summary>
        /// 读取舰队信息
        /// </summary>
        /// <returns></returns>
        public async Task GetGuardList()
        {
            try
            {
                LoadingGuard = true;
                LoadMoreGuard = false;
                var result = await liveRoomAPI.GuardList(LiveInfo.room_info.uid, RoomID, GuardPage).Request();
                if (result.status)
                {
                    var data = await result.GetData<JObject>();
                    if (data.success)
                    {
                        var top3 = JsonConvert.DeserializeObject<List<LiveGuardRankItem>>(data.data["top3"].ToString());
                        if (Guards.Count == 0 && top3 != null && top3.Count != 0)
                        {
                            foreach (var item in top3)
                            {
                                Guards.Add(item);
                            }
                        }
                        var list = JsonConvert.DeserializeObject<List<LiveGuardRankItem>>(data.data["list"].ToString());
                        if (list != null && list.Count != 0)
                        {
                            foreach (var item in list)
                            {
                                Guards.Add(item);
                            }
                        }
                        if (GuardPage >= data.data["info"]["page"].ToInt32())
                        {
                            LoadMoreGuard = false;
                        }
                        else
                        {
                            LoadMoreGuard = true;
                            GuardPage++;
                        }
                    }

                }


            }
            catch (Exception ex)
            {
                Notify.ShowMessageToast("读取舰队失败");
                _logger.Log("读取舰队失败", LogType.Error, ex);
            }
            finally
            {
                LoadingGuard = false;
            }
        }
        /// <summary>
        /// 加载更多舰队信息
        /// </summary>
        public async void LoadMoreGuardList()
        {
            if (LoadingGuard)
            {
                return;
            }
            await GetGuardList();
        }


        private bool _showbox = false;
        public bool ShowBox
        {
            get { return _showbox; }
            set { _showbox = value; DoPropertyChanged("ShowBox"); }
        }
        private bool _box_is_open = false;
        public bool OpenBox
        {
            get { return _box_is_open; }
            set { _box_is_open = value; DoPropertyChanged("OpenBox"); }
        }

        private string _box_time = "--:--";
        public string BoxTime
        {
            get { return _box_time; }
            set { _box_time = value; DoPropertyChanged("BoxTime"); }
        }

        public DateTime freeSilverTime { get; set; }
        public bool AutoReceiveFreeSilver { get; set; }


        public async Task GetFreeSilverTime()
        {
            try
            {
                if (!SettingService.Account.Logined)
                {
                    ShowBox = false;
                    OpenBox = false;
                    return;
                }
                OpenBox = false;
                var result = await liveRoomAPI.FreeSilverTime().Request();
                if (result.status)
                {
                    var data = await result.GetData<JObject>();
                    if (data.success)
                    {
                        ShowBox = true;
                        freeSilverTime = TimeExtensions.TimestampToDatetime(Convert.ToInt64(data.data["time_end"].ToString()));
                        timer_box.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                Notify.ShowMessageToast("读取直播免费瓜子时间失败");
                _logger.Log("读取直播免费瓜子时间失败", LogType.Error, ex);
            }
        }

        public async Task GetFreeSilver()
        {
            try
            {
                if (!SettingService.Account.Logined)
                {
                    return;
                }
                var result = await liveRoomAPI.GetFreeSilver().Request();
                if (result.status)
                {
                    var data = await result.GetData<JObject>();
                    if (data.success)
                    {
                        Notify.ShowMessageToast("宝箱领取成功,瓜子+" + data.data["awardSilver"]);
                        //GetFreeSilverTime();
                        LoadWalletInfo();
                    }
                    else
                    {
                        GetFreeSilverTime();
                    }
                }
            }
            catch (Exception ex)
            {
                Notify.ShowMessageToast("读取直播免费瓜子时间失败");
                _logger.Log("读取直播免费瓜子时间失败", LogType.Error, ex);
            }
        }

        private async void Timer_box_Elapsed(object sender, ElapsedEventArgs e)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
             {
                 if (DateTime.Now >= freeSilverTime)
                 {
                     ShowBox = false;
                     OpenBox = true;
                     timer_box.Stop();
                     if (AutoReceiveFreeSilver)
                     {
                         await GetFreeSilver();
                     }
                 }
                 else
                 {
                     OpenBox = false;
                     BoxTime = (freeSilverTime - DateTime.Now).ToString(@"mm\:ss");
                 }
             });
        }
        public async Task SendGift(LiveGiftItem liveGiftItem)
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录");
                return;
            }
            try
            {
                var result = await liveRoomAPI.SendGift(LiveInfo.room_info.uid, liveGiftItem.id, liveGiftItem.num, RoomID, liveGiftItem.coin_type, liveGiftItem.price).Request();
                if (result.status)
                {
                    var data = await result.GetData<object>();
                    if (data.success)
                    {
                        LoadWalletInfo();
                    }
                    else
                    {
                        Notify.ShowMessageToast(data.message);
                    }
                }
                else
                {
                    Notify.ShowMessageToast(result.message);
                }
            }
            catch (Exception ex)
            {
                _logger.Log("赠送礼物出现错误", LogType.Error, ex);
                Notify.ShowMessageToast("赠送礼物出现错误");
            }

        }
        public async Task SendBagGift(LiveGiftItem liveGiftItem)
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录");
                return;
            }
            try
            {
                var result = await liveRoomAPI.SendBagGift(LiveInfo.room_info.uid, liveGiftItem.id, liveGiftItem.num, liveGiftItem.bag_id, RoomID).Request();
                if (result.status)
                {
                    var data = await result.GetData<object>();
                    if (data.success)
                    {
                        await LoadBag();
                    }
                    else
                    {
                        Notify.ShowMessageToast(data.message);
                    }
                }
                else
                {
                    Notify.ShowMessageToast(result.message);
                }
            }
            catch (Exception ex)
            {
                _logger.Log("赠送礼物出现错误", LogType.Error, ex);
                Notify.ShowMessageToast("赠送礼物出现错误");
            }

        }

        private void SetShowBag()
        {
            if (!ShowBag && !SettingService.Account.Logined)
            {
                Notify.ShowMessageToast("请先登录");
                return;
            }
            ShowBag = !ShowBag;
        }
        private async void RefreshBag()
        {
            if (!SettingService.Account.Logined)
            {
                Notify.ShowMessageToast("请先登录");
                return;
            }
            await LoadBag();
        }
        public async Task<bool> SendDanmu(string text)
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录");
                return false;
            }
            try
            {
                var result = await liveRoomAPI.SendDanmu(text, RoomID).Request();
                if (result.status)
                {
                    var data = await result.GetData<object>();
                    if (data.success)
                    {
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
                    Notify.ShowMessageToast(result.message);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Log("发送弹幕出现错误", LogType.Error, ex);
                Notify.ShowMessageToast("发送弹幕出现错误");
                return false;
            }

        }

        public void Dispose()
        {
            if (cancelSource != null)
            {
                cancelSource.Cancel();
            }
            if (liveMessage != null)
            {
                liveMessage.Dispose();
            }

            if (timer != null)
            {
                timer.Stop();
            }
            if (timer_box != null)
            {
                timer_box.Stop();
            }
            if (timer_auto_hide_gift != null)
            {
                timer_auto_hide_gift.Stop();
            }
            if (anchorLotteryVM != null)
            {
                anchorLotteryVM.timer.Stop();
                anchorLotteryVM = null;
            }

            Messages?.Clear();
            Messages = null;
            GiftMessage?.Clear();
            GiftMessage = null;
            Guards?.Clear();
            Guards = null;
        }

        //public void SetDelay(int ms)
        //{
        //    if (liveDanmaku != null)
        //    {
        //        liveDanmaku.delay = ms;
        //    }
        //}
    }

    public class LiveRoomRankVM : IModules
    {
        private static readonly ILogger logger = GlobalLogger.FromCurrentType();

        readonly LiveRoomAPI liveRoomAPI;
        public LiveRoomRankVM(int roomid, long uid, string title, string type)
        {
            liveRoomAPI = new LiveRoomAPI();
            RankType = type;
            Title = title;
            RoomID = roomid;
            Uid = uid;
            LoadMoreCommand = new RelayCommand(LaodMore);
            Items = new ObservableCollection<LiveRoomRankItemModel>();
        }
        public ICommand LoadMoreCommand { get; private set; }
        public int RoomID { get; set; }
        public long Uid { get; set; }
        public string Title { get; set; }
        public string RankType { get; set; }
        private bool _loading = false;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }

        public ObservableCollection<LiveRoomRankItemModel> Items { get; set; }
        public int Page { get; set; } = 1;

        private bool _canLoadMore = false;

        public bool CanLoadMore
        {
            get { return _canLoadMore; }
            set { _canLoadMore = value; DoPropertyChanged("CanLoadMore"); }
        }
        public int Next { get; set; } = 0;

        public async Task LoadData()
        {
            try
            {
                Loading = true;
                CanLoadMore = false;
                var api = liveRoomAPI.FansList(Uid, RoomID, Page);
                if (RankType != "fans")
                {
                    api = liveRoomAPI.RoomRankList(Uid, RoomID, RankType, Next);
                }
                var result = await api.Request();
                if (result.status)
                {
                    var data = result.GetJObject();
                    if (data["code"].ToInt32() == 0)
                    {
                        var list = JsonConvert.DeserializeObject<List<LiveRoomRankItemModel>>(data["data"]["list"].ToString());
                        if (list != null)
                        {
                            foreach (var item in list)
                            {
                                Items.Add(item);
                            }
                        }

                        if (RankType != "fans")
                        {
                            Next = data["data"]["next_offset"].ToInt32();
                            CanLoadMore = Next != 0;
                        }
                        else
                        {
                            var total = data["data"]["total_page"].ToInt32();
                            if (Page < total)
                            {
                                CanLoadMore = true;
                                Page++;
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
                    Notify.ShowMessageToast(result.message);
                }
            }
            catch (Exception ex)
            {
                Notify.ShowMessageToast("读取直播排行榜失败：" + RankType);
                logger.Log("读取直播排行榜失败" + RankType, LogType.Error, ex);
            }
            finally
            {
                Loading = false;
            }
        }


        public async void LaodMore()
        {
            if (Loading)
            {
                return;
            }
            await LoadData();
        }
    }

    public class LiveRoomAnchorLotteryVM : IModules
    {
        private static readonly ILogger logger = GlobalLogger.FromCurrentType();

        readonly LiveRoomAPI liveRoomAPI;
        public LiveRoomAnchorLotteryVM()
        {
            liveRoomAPI = new LiveRoomAPI();
            timer = new Timer(1000);
            timer.Elapsed += Timer_Elapsed;
        }

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (LotteryInfo != null)
                {
                    if (LotteryInfo.time <= 0)
                    {
                        End = false;
                        timer.Stop();
                        DownTime = "已开奖";
                        Show = false;
                        LotteryInfo = null;
                        return;
                    }
                    var time = TimeSpan.FromSeconds(LotteryInfo.time);
                    DownTime = time.ToString(@"mm\:ss");
                    LotteryInfo.time--;
                }
            });
        }



        public Timer timer;
        private LiveRoomAnchorLotteryInfoModel _LotteryInfo;
        public LiveRoomAnchorLotteryInfoModel LotteryInfo
        {
            get { return _LotteryInfo; }
            set { _LotteryInfo = value; DoPropertyChanged("LotteryInfo"); }
        }



        private string _downTime = "--:--";
        public string DownTime
        {
            get { return _downTime; }
            set { _downTime = value; DoPropertyChanged("DownTime"); }
        }

        private bool _end = false;
        public bool End
        {
            get { return _end; }
            set { _end = value; DoPropertyChanged("End"); }
        }

        private bool _show = false;
        public bool Show
        {
            get { return _show; }
            set { _show = value; DoPropertyChanged("Show"); }
        }

        public async Task LoadLotteryInfo(int roomId)
        {
            try
            {
                var result = await liveRoomAPI.RoomLotteryInfo(roomId).Request();
                if (result.status)
                {
                    var obj = await result.GetData<JObject>();
                    if (obj.success)
                    {
                        var data = JsonConvert.DeserializeObject<LiveRoomAnchorLotteryInfoModel>(obj.data["anchor"].ToString());
                        if (data != null)
                        {
                            LotteryInfo = data;
                            Show = true;
                            timer.Start();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log("加载主播抽奖信息失败", LogType.Error, ex);
            }
        }

        public void SetLotteryInfo(LiveRoomAnchorLotteryInfoModel info)
        {
            LotteryInfo = info;
            Show = true;
            timer.Start();
        }


    }


    public class LiveRoomPlayUrlModel
    {
        public LiveRoomWebUrlPlayurlInfoItemModel playurl_info { get; set; }
    }
    public class LiveRoomWebUrlPlayurlInfoItemModel
    {
        public LiveRoomWebUrlPlayurlItemModel playurl { get; set; }
    }
    public class LiveRoomWebUrlPlayurlItemModel
    {
        public List<LiveRoomWebUrlQualityDescriptionItemModel> g_qn_desc { get; set; }
        public List<LiveRoomWebUrlStreamItemModel> stream { get; set; }
    }
    public class LiveRoomWebUrlQualityDescriptionItemModel
    {
        public int qn { get; set; }
        public string desc { get; set; }
    }
    public class LiveRoomWebUrlStreamItemModel
    {
        public List<LiveRoomWebUrlFormatItemModel> format { get; set; }
        public string protocol_name { get; set; }
    }
    public class LiveRoomWebUrlFormatItemModel
    {
        public List<LiveRoomWebUrlCodecItemModel> codec { get; set; }
    }
    public class LiveRoomWebUrlCodecItemModel
    {
        public int current_qn { get; set; }
        public string base_url { get; set; }
        public string codec_name { get; set; }
        public List<int> accept_qn { get; set; }
        public List<LiveRoomWebUrlUrlinfoItemModel> url_info { get; set; }
    }
    public class LiveRoomWebUrlUrlinfoItemModel
    {
        public string name { get; set; }
        public string host { get; set; }
        public string extra { get; set; }
    }
    public class LiveRoomRealPlayUrlsModel
    {
        public string name { get; set; }
        public string url { get; set; }
    }

    namespace LiveRoomDetailModels
    {
        public class LiveTitleModel
        {
            public string id { get; set; }
            public string title { get; set; }
            public string img { get; set; }
        }

        public class LiveInfoModel
        {
            public LiveRoomInfoModel room_info { get; set; }
            public LiveRoomGuardInfoModel guard_info { get; set; }
            public LiveAnchorInfoModel anchor_info { get; set; }
        }
        public class LiveRoomGuardInfoModel
        {
            public int count { get; set; }
            public int achievement_level { get; set; }
        }
        public class LiveRoomInfoPendantsFrameModel
        {
            public string name { get; set; }
            public int position { get; set; }
            public string value { get; set; }
            public string desc { get; set; }
        }

        public class LiveRoomInfoPendantsModel
        {
            public LiveRoomInfoPendantsFrameModel frame { get; set; }
            public object badge { get; set; }
        }

        public class LiveRoomInfoModel
        {
            public long uid { get; set; }
            public int room_id { get; set; }
            public int short_id { get; set; }
            public string title { get; set; }
            public string cover { get; set; }
            public string tags { get; set; }
            public string background { get; set; }
            public string description { get; set; }
            public int online { get; set; }
            public int live_status { get; set; }
            public long live_start_time { get; set; }
            public int live_screen_type { get; set; }
            public int lock_status { get; set; }
            public int lock_time { get; set; }
            public int hidden_status { get; set; }
            public int hidden_time { get; set; }
            public int area_id { get; set; }
            public string area_name { get; set; }
            public int parent_area_id { get; set; }
            public string parent_area_name { get; set; }
            public string keyframe { get; set; }
            public int special_type { get; set; }
            public string up_session { get; set; }
            public int pk_status { get; set; }
            public LiveRoomInfoPendantsModel pendants { get; set; }
            public int on_voice_join { get; set; }
            public int tv_screen_on { get; set; }
        }



        public class LiveAnchorInfoOfficialInfoModel
        {
            public int role { get; set; }
            public string title { get; set; }
            public string desc { get; set; }
        }

        public class LiveAnchorInfoBaseInfoModel
        {
            public string uname { get; set; }
            public string face { get; set; }
            public string gender { get; set; }
            public LiveAnchorInfoOfficialInfoModel official_info { get; set; }
        }

        public class LiveAnchorInfoLiveInfoModel
        {
            public int level { get; set; }
            public int level_color { get; set; }
        }

        public class LiveAnchorInfoRelationInfoModel
        {
            public int attention { get; set; }
        }

        public class LiveAnchorInfoModel
        {
            public LiveAnchorInfoBaseInfoModel base_info { get; set; }
            public LiveAnchorInfoLiveInfoModel live_info { get; set; }
            public LiveAnchorInfoRelationInfoModel relation_info { get; set; }
        }


        [Serializable]
        public class LiveGiftItemCountMap
        {
            public int num { get; set; }
            public string text { get; set; }
        }
        [Serializable]
        public class LiveGiftItem
        {
            public int id { get; set; }
            public int bag_id { get; set; }
            public string name { get; set; }
            public int price { get; set; }
            public int type { get; set; }
            public string coin_type { get; set; }
            public bool is_gold
            {
                get
                {
                    return coin_type == "gold";
                }
            }
            public int bag_gift { get; set; }
            public int effect { get; set; }
            public string corner_mark { get; set; }
            public bool show_corner_mark { get { return !string.IsNullOrEmpty(corner_mark); } }
            public string corner_background { get; set; }
            public int broadcast { get; set; }
            public int draw { get; set; }
            public int stay_time { get; set; }
            public int animation_frame_num { get; set; }
            public string desc { get; set; }
            public string rule { get; set; }
            public string rights { get; set; }
            public int privilege_required { get; set; }
            public List<LiveGiftItemCountMap> count_map { get; set; }
            public string img_basic { get; set; }
            public string img_dynamic { get; set; }
            public string frame_animation { get; set; }
            public string gif { get; set; }
            public string webp { get; set; }
            public string full_sc_web { get; set; }
            public string full_sc_horizontal { get; set; }
            public string full_sc_vertical { get; set; }
            public string full_sc_horizontal_svga { get; set; }
            public string full_sc_vertical_svga { get; set; }
            public string bullet_head { get; set; }
            public string bullet_tail { get; set; }
            public int limit_interval { get; set; }
            public long bind_ruid { get; set; }
            public int bind_roomid { get; set; }
            public int bag_coin_type { get; set; }
            public int broadcast_id { get; set; }
            public int draw_id { get; set; }
            public int gift_type { get; set; }
            public int weight { get; set; }
            public int max_send_limit { get; set; }
            public int gift_num { get; set; } = 0;
            public int combo_resources_id { get; set; }
            public int goods_id { get; set; }

            public int num { get; set; } = 1;
        }
        public class LiveBagGiftItem
        {
            public int bag_id { get; set; }
            public string gift_name { get; set; }
            public int gift_id { get; set; }
            public int gift_type { get; set; }
            public int gift_num { get; set; }
            public int type { get; set; }
            public string card_gif { get; set; }
            public string corner_mark { get; set; }
            public long expire_at { get; set; }
            public string img { get; set; }
        }
        public class LiveRoomGiftItem
        {
            public int position { get; set; }
            public int gift_id { get; set; }
            public int id { get; set; }
            public int plan_id { get; set; }
        }
        public class LiveWalletInfo
        {
            public int gold { get; set; }
            public int silver { get; set; }
        }

        public class LiveAnchorProfileGloryInfo
        {
            public string gid { get; set; }
            public string name { get; set; }
            public string activity_name { get; set; }
            public string activity_date { get; set; }
            public string pic_url { get; set; }
            public string jump_url { get; set; }
        }

        public class LiveAnchorProfilePkInfo
        {
            public int season_id { get; set; }
            public string season_name { get; set; }
            public string season_date_start { get; set; }
            public string season_date_end { get; set; }
            public string pk_rank_name { get; set; }
            public string first_pic_url { get; set; }
            public int pk_rank_star { get; set; }
            public string second_rank_icon { get; set; }
            public string rank_info_url_app { get; set; }
        }

        public class LiveAnchorProfile
        {
            public long uid { get; set; }
            public string uname { get; set; }
            public string face { get; set; }
            public int verify_type { get; set; }
            public string verify
            {
                get
                {
                    switch (verify_type)
                    {
                        case 0:
                            return Constants.App.VERIFY_PERSONAL_IMAGE;
                        case 1:
                            return Constants.App.VERIFY_OGANIZATION_IMAGE;
                        default:
                            return Constants.App.TRANSPARENT_IMAGE;
                    }
                }
            }
            public string desc { get; set; }
            public int level { get; set; }
            public int level_color { get; set; }
            public int main_vip { get; set; }
            public int uname_color { get; set; }
            public int room_id { get; set; }
            public string area_name { get; set; }
            public string pendant { get; set; }
            public int pendant_from { get; set; }
            public List<LiveAnchorProfileGloryInfo> glory_info { get; set; }
            public List<LiveAnchorProfilePkInfo> pk_info { get; set; }
            public string season_info_url { get; set; }
            public int follow_num { get; set; }
            public bool is_fans { get; set; }
            public int relation_status { get; set; }
        }

        public class LiveGuardRankItem
        {
            public string username { get; set; }
            public long uid { get; set; }
            public long ruid { get; set; }
            public string face { get; set; }
            public int guard_level { get; set; }
            public string rank_img
            {
                get
                {
                    return "ms-appx:///Assets/Live/ic_live_guard_" + guard_level + ".png";
                }
            }
        }


        public class LiveRoomRankItemModel
        {
            public int rank { get; set; }
            public long uid { get; set; }
            public string uname { get; set; }
            public string face { get; set; }
            public int score { get; set; }
            public string icon { get; set; } = Constants.App.TRANSPARENT_IMAGE;
            public bool show_right
            {
                get
                {
                    return score != 0;
                }
            }


            public string medal_name { get; set; }
            public string level { get; set; }
            public string color { get; set; }

            public bool show_medal
            {
                get
                {
                    return !string.IsNullOrEmpty(medal_name);
                }
            }
        }


        public class LiveRoomAnchorLotteryInfoModel
        {
            public string asset_icon { get; set; }
            public string award_image { get; set; }
            public string award_name { get; set; }
            public int award_num { get; set; }
            public int cur_gift_num { get; set; }
            public long current_time { get; set; }
            public string danmu { get; set; }
            public int gift_id { get; set; }

            public bool show_gift
            {
                get
                {
                    return gift_id != 0;
                }
            }

            public string gift_name { get; set; }
            public int gift_num { get; set; }
            public int gift_price { get; set; }
            public int goaway_time { get; set; }
            public int id { get; set; }
            public int join_type { get; set; }
            public int lot_status { get; set; }
            public int max_time { get; set; }
            public string require_text { get; set; }
            public int require_type { get; set; }
            public int require_value { get; set; }
            public int room_id { get; set; }
            public int send_gift_ensure { get; set; }
            public int show_panel { get; set; }
            public int status { get; set; }
            public int time { get; set; }
            public string url { get; set; }
            public string web_url { get; set; }
        }

        public class LiveRoomEndAnchorLotteryInfoModel
        {
            public int id { get; set; }
            public int lot_status { get; set; }
            public string award_image { get; set; }
            public string award_name { get; set; }
            public int award_num { get; set; }
            public string url { get; set; }
            public string web_url { get; set; }
            public List<LiveRoomEndAnchorLotteryInfoUserModel> award_users { get; set; }
        }
        public class LiveRoomEndAnchorLotteryInfoUserModel
        {
            public long uid { get; set; }
            public int level { get; set; }
            public string uname { get; set; }
            public string face { get; set; }

        }


        public class LiveRoomSuperChatUserInfoModel
        {
            public string uname { get; set; }
            public string face { get; set; }
            public string face_frame { get; set; }
            public int guard_level { get; set; }
            public int user_level { get; set; }
            public int is_vip { get; set; }
            public int is_svip { get; set; }
            public int is_main_vip { get; set; }
        }

        public class LiveRoomSuperChatModel
        {
            public int id { get; set; }
            public long uid { get; set; }
            public string background_image { get; set; }
            public string background_color { get; set; }
            public string background_icon { get; set; }
            public string background_bottom_color { get; set; }
            public string background_price_color { get; set; }
            public string font_color { get; set; }
            public int price { get; set; }
            public int rate { get; set; }
            public int time { get; set; }
            public int start_time { get; set; }
            public int end_time { get; set; }
            public string message { get; set; }
            public int trans_mark { get; set; }
            public string message_trans { get; set; }
            public int ts { get; set; }
            public string token { get; set; }
            public LiveRoomSuperChatUserInfoModel user_info { get; set; }
        }
    }

}
