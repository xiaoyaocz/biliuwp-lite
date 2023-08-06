using BiliLite.Controls.Dynamic;
using BiliLite.Models;
using BiliLite.Models.Dynamic;
using BiliLite.Pages;
using BiliLite.Pages.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using static BiliLite.Models.Requests.Api.User.DynamicAPI;
using BiliLite.Dialogs;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Models.Requests.Api.User;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using DynamicItemDataTemplateSelector = BiliLite.Controls.DataTemplateSelectors.DynamicItemDataTemplateSelector;

namespace BiliLite.Modules.User
{
    public class DynamicVM : BaseViewModel
    {
        #region Fields

        private readonly WatchLaterVM m_watchLaterVm;
        private readonly DynamicAPI m_dynamicApi;
        private DynamicItemDataTemplateSelector m_dynamicItemDataTemplateSelector;

        #endregion

        #region Constructors

        public DynamicVM()
        {
            m_dynamicApi = new DynamicAPI();
            m_watchLaterVm = new WatchLaterVM();
            m_dynamicItemDataTemplateSelector = new DynamicItemDataTemplateSelector();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);

            UserCommand = new RelayCommand<object>(OpenUser);
            LaunchUrlCommand = new RelayCommand<object>(LaunchUrl);
            WebCommand = new RelayCommand<object>(OpenWeb);
            TagCommand = new RelayCommand<object>(OpenTag);
            LotteryCommand = new RelayCommand<object>(OpenLottery);
            VoteCommand = new RelayCommand<object>(OpenVote);
            ImageCommand = new RelayCommand<object>(OpenImage);
            CommentCommand = new RelayCommand<DynamicItemDisplayModel>(OpenComment);
            DeleteCommand = new RelayCommand<DynamicItemDisplayModel>(Delete);
            LikeCommand = new RelayCommand<DynamicItemDisplayModel>(DoLike);
            RepostCommand = new RelayCommand<DynamicItemDisplayModel>(OpenSendDynamicDialog);
            DetailCommand = new RelayCommand<DynamicItemDisplayModel>(OpenDetail);
        }

        #endregion

        #region Properties

        public ICommand VoteCommand { get; set; }
        public ICommand UserCommand { get; set; }
        public ICommand LotteryCommand { get; set; }
        public ICommand LaunchUrlCommand { get; set; }
        public ICommand WebCommand { get; set; }
        public ICommand TagCommand { get; set; }
        public ICommand ImageCommand { get; set; }
        public ICommand CommentCommand { get; set; }
        public ICommand RepostCommand { get; set; }
        public ICommand LikeCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand DetailCommand { get; set; }
        public ICommand RefreshCommand { get; private set; }
        public ICommand LoadMoreCommand { get; private set; }
        public bool Loading { get; set; } = true;
        public bool CanLoadMore { get; set; }
        public DynamicType DynamicType { get; set; } = DynamicType.UserDynamic;
        public string Uid { get; set; }
        public UserDynamicType UserDynamicType { get; set; } = UserDynamicType.All;

        public ObservableCollection<DynamicItemDisplayModel> Items { get; set; }

        #endregion

        #region Events

        public event EventHandler<DynamicItemDisplayModel> OpenCommentEvent;

        #endregion

        #region Private Methods

        private async void LoadMore()
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
            await GetDynamicItems(last.DynamicID);
        }
        private void OpenUser(object id)
        {
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Contact,
                page = typeof(UserInfoPage),
                title = "用户中心",
                parameters = id
            });
        }
        private async void LaunchUrl(object url)
        {
            var result = await MessageCenter.HandelUrl(url.ToString());
            if (!result)
            {
                Notify.ShowMessageToast("无法打开Url");
            }
        }
        private void OpenWeb(object url)
        {
            MessageCenter.NavigateToPage(null, new NavigationInfo()
            {
                icon = Symbol.World,
                page = typeof(WebPage),
                title = "加载中...",
                parameters = url
            });
        }

        private void OpenImage(object data)
        {
            DyanmicItemDisplayImageInfo info = data as DyanmicItemDisplayImageInfo;
            MessageCenter.OpenImageViewer(info.AllImages, info.Index);
        }
        private void OpenTag(object name)
        {
            //TODO 打开话题
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.World,
                page = typeof(WebPage),
                title = name.ToString(),
                parameters = "https://t.bilibili.com/topic/name/" + Uri.EscapeDataString(name.ToString())
            });
        }
        private async void OpenLottery(object id)
        {
            ContentDialog contentDialog = new ContentDialog()
            {
                IsPrimaryButtonEnabled = true,
                Title = "抽奖",
                PrimaryButtonText = "关闭"
            };
            contentDialog.PrimaryButtonClick += new TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs>((sender, e) =>
            {
                contentDialog.Hide();
            });
            contentDialog.Content = new WebView()
            {
                Width = 500,
                Height = 500,
                Source = new Uri($"https://t.bilibili.com/lottery/h5/index/#/result?business_id={id.ToString()}&business_type=1&isWeb=1"),
            };
            await contentDialog.ShowAsync();
        }
        private void OpenVote(object id)
        {
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Document,
                page = typeof(WebPage),
                title = "投票",
                parameters = $"https://t.bilibili.com/vote/h5/index/#/result?vote_id={id.ToString()}"
            });
            //ContentDialog contentDialog = new ContentDialog()
            //{
            //    IsPrimaryButtonEnabled = true,
            //    Title = "投票",
            //    PrimaryButtonText = "关闭"
            //};
            //contentDialog.PrimaryButtonClick += new TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs>((sender, e) =>
            //{
            //    contentDialog.Hide();
            //});
            //contentDialog.Content = new WebView()
            //{
            //    Width = 500,
            //    Height = 500,
            //    Source = new Uri($"https://t.bilibili.com/vote/h5/index/#/result?vote_id={ id.ToString()}"),
            //};
            //await contentDialog.ShowAsync();
        }
        private void OpenComment(DynamicItemDisplayModel data)
        {
            OpenCommentEvent?.Invoke(this, data);
        }
        private void OpenDetail(DynamicItemDisplayModel data)
        {
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Document,
                page = typeof(DynamicDetailPage),
                title = "动态详情",
                parameters = data.DynamicID
            });
        }
        private async void Delete(DynamicItemDisplayModel item)
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录后再操作");
                return;
            }
            MessageDialog messageDialog = new MessageDialog("确定要删除动态吗?", "删除动态");
            messageDialog.Commands.Add(new UICommand("确定", cmd => { }, commandId: 0));
            messageDialog.Commands.Add(new UICommand("取消", cmd => { }, commandId: 1));
            var result = await messageDialog.ShowAsync();
            if (result.Id.ToInt32() == 1) return;
            try
            {
                var results = await m_dynamicApi.Delete(item.DynamicID).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<object>>();
                    if (data.success)
                    {
                        Items.Remove(item);
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
        private async void DoLike(DynamicItemDisplayModel item)
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录后再操作");
                return;
            }

            try
            {
                var results = await m_dynamicApi.Like(item.DynamicID, item.Liked ? 2 : 1).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<object>>();
                    if (data.success)
                    {
                        if (item.Liked)
                        {
                            item.Liked = false;
                            item.LikeCount -= 1;
                        }
                        else
                        {
                            item.Liked = true;
                            item.LikeCount += 1;
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
        private async void OpenSendDynamicDialog(DynamicItemDisplayModel data)
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录后再操作");
                return;
            }

            SendDynamicDialog sendDynamicDialog = new SendDynamicDialog();
            if (data != null)
            {
                sendDynamicDialog = new SendDynamicDialog(data);
            }
            await sendDynamicDialog.ShowAsync();
        }

        private DynamicItemDisplayModel ConvertToDisplay(DynamicCardModel item)
        {
            try
            {
                var card = JObject.Parse(item.card);
                var extend_json = JObject.Parse(item.extend_json);
                var data = new DynamicItemDisplayModel()
                {
                    CommentCount = item.desc.comment,
                    Datetime = TimeExtensions.TimestampToDatetime(item.desc.timestamp).ToString(),
                    DynamicID = item.desc.dynamic_id,
                    LikeCount = item.desc.like,
                    Mid = item.desc.uid,
                    ShareCount = item.desc.repost,
                    Time = item.desc.timestamp.HandelTimestamp(),
                    IntType = item.desc.type,
                    ReplyID = item.desc.rid_str,
                    ReplyType = item.desc.r_type,
                    Type = DynamicParse.ParseType(item.desc.type),
                    UserCommand = UserCommand,
                    LaunchUrlCommand = LaunchUrlCommand,
                    WebCommand = WebCommand,
                    TagCommand = TagCommand,
                    LotteryCommand = LotteryCommand,
                    VoteCommand = VoteCommand,
                    IsSelf = item.desc.uid == SettingService.Account.UserID,
                    ImageCommand = ImageCommand,
                    CommentCommand = CommentCommand,
                    LikeCommand = LikeCommand,
                    DeleteCommand = DeleteCommand,
                    RepostCommand = RepostCommand,
                    DetailCommand = DetailCommand,
                    WatchLaterCommand = m_watchLaterVm.AddCommand,
                    Liked = item.desc.is_liked == 1
                };
                if (data.Type == DynamicDisplayType.Other)
                {
                    return new DynamicItemDisplayModel()
                    {
                        Type = DynamicDisplayType.Other,
                        IntType = 999,
                        DynamicID = item.desc.dynamic_id,
                        ContentStr = $"未适配的类型{data.IntType}:\r\n" + JsonConvert.SerializeObject(item)
                    };
                }
                data.OneRowInfo = DynamicParse.ParseOneRowInfo(data.Type, card);
                if (data.Type == DynamicDisplayType.ShortVideo)
                {
                    data.ShortVideoInfo = DynamicParse.ParseShortVideoInfo(card);
                }
                if (data.Type == DynamicDisplayType.Photo)
                {
                    List<DyanmicItemDisplayImageInfo> imgs = new List<DyanmicItemDisplayImageInfo>();
                    List<string> allImageUrl = new List<string>();
                    int i = 0;
                    foreach (var img in card["item"]["pictures"])
                    {
                        allImageUrl.Add(img["img_src"].ToString());
                        imgs.Add(new DyanmicItemDisplayImageInfo()
                        {
                            ImageUrl = img["img_src"].ToString(),
                            Height = img["img_height"].ToInt32(),
                            Width = img["img_width"].ToInt32(),
                            Index = i,
                            ImageCommand = ImageCommand
                        });
                        i++;
                    }

                    //偷懒方法，点击图片时可以获取全部图片信息，好孩子不要学
                    imgs.ForEach((x) => x.AllImages = allImageUrl);

                    data.ImagesInfo = imgs;
                }

                if (data.Type == DynamicDisplayType.Repost)
                {
                    if (card.ContainsKey("origin_user"))
                    {
                        var originUser = JsonConvert.DeserializeObject<DynamicCardDescUserProfileModel>(card["origin_user"].ToString());
                        var originDisplay = ConvertToDisplay(new DynamicCardModel()
                        {
                            extend_json = card["origin_extend_json"].ToString(),
                            card = card["origin"].ToString(),
                            display = item.display?.origin,
                            desc = new DynamicCardDescModel()
                            {
                                user_profile = originUser,
                                uid = originUser.info.uid,
                                dynamic_id = item.desc.orig_dy_id,
                                type = item.desc.orig_type
                            }
                        });
                        originDisplay.IsRepost = true;
                        data.OriginInfo = new List<DynamicItemDisplayModel>() { originDisplay };
                    }
                    else
                    {

                        data.OriginInfo = new List<DynamicItemDisplayModel>() {
                        new DynamicItemDisplayModel()
                        {
                            IsRepost=true,
                            IntType=1024,
                            Type= DynamicDisplayType.Miss
                        }
                    };
                    }

                }
                if (item.desc.comment == 0 && card.ContainsKey("stat"))
                {
                    data.CommentCount = card["stat"]["reply"].ToInt32();
                }
                //Season数据会出现desc.comment为0的情况
                if (item.desc.comment == 0 && card.ContainsKey("reply_count"))
                {
                    data.CommentCount = card["reply_count"].ToInt32();
                }
                //专栏数据会出现desc.comment为0的情况
                if (item.desc.comment == 0 && card.ContainsKey("stats"))
                {
                    data.CommentCount = card["stats"]["reply"].ToInt32();
                }
                var content = "";
                //内容
                if (card.ContainsKey("item") && card["item"]["content"] != null)
                {
                    content = card["item"]["content"]?.ToString();
                    extend_json["at_control"] = card["item"]["ctrl"];
                }
                else if (card.ContainsKey("item") && card["item"]["description"] != null)
                {
                    content = card["item"]["description"]?.ToString();
                    extend_json["at_control"] = card["item"]["at_control"];
                }
                else if (card.ContainsKey("dynamic"))
                {
                    content = card["dynamic"]?.ToString();
                }
                else if (card.ContainsKey("vest") && card["vest"]["content"] != null)
                {
                    content = card["vest"]["content"]?.ToString();
                }

                if (!string.IsNullOrEmpty(content))
                {
                    data.ContentStr = content;
                    data.Content = DynamicParse.StringToRichText(item.desc.dynamic_id, content, item.display?.emoji_info?.emoji_details, extend_json);
                }
                else
                {
                    data.ShowContent = false;
                }


                if (item.desc.user_profile != null)
                {
                    data.UserName = item.desc.user_profile.info.uname;
                    data.Photo = item.desc.user_profile.info.face;
                    if (item.desc.user_profile.vip != null)
                    {
                        data.IsYearVip = item.desc.user_profile.vip.vipStatus == 1 && item.desc.user_profile.vip.vipType == 2;
                    }
                    switch (item.desc.user_profile.card?.official_verify?.type ?? 3)
                    {
                        case 0:
                            data.Verify = Constants.App.VERIFY_PERSONAL_IMAGE;
                            break;
                        case 1:
                            data.Verify = Constants.App.VERIFY_OGANIZATION_IMAGE;
                            break;
                        default:
                            data.Verify = Constants.App.TRANSPARENT_IMAGE;
                            break;
                    }
                    if (!string.IsNullOrEmpty(item.desc.user_profile.pendant?.image))
                    {
                        data.Pendant = item.desc.user_profile.pendant.image;
                    }
                    //装扮
                    data.DecorateName = item.desc.user_profile.decorate_card?.name;
                    data.DecorateText = item.desc.user_profile.decorate_card?.fan?.num_desc;
                    data.DecorateColor = item.desc.user_profile.decorate_card?.fan?.color;
                    data.DecorateImage = item.desc.user_profile.decorate_card?.big_card_url;
                }


                if (card.ContainsKey("apiSeasonInfo"))
                {
                    data.UserName = card["apiSeasonInfo"]["title"].ToString();
                    data.Photo = card["apiSeasonInfo"]["cover"].ToString();
                    data.TagName = card["apiSeasonInfo"]["type_name"].ToString();
                    data.ShowTag = true;
                    data.Time = data.Time + "更新了";
                }
                if (card.ContainsKey("season"))
                {
                    data.UserName = card["season"]["title"].ToString();
                    data.Photo = card["season"]["cover"].ToString();
                    data.TagName = card["season"]["type_name"].ToString();
                    data.ShowTag = true;
                    data.Time = data.Time + "更新了";
                }
                return data;
            }
            catch (Exception)
            {
                return new DynamicItemDisplayModel()
                {
                    Type = DynamicDisplayType.Other,
                    IntType = 999,
                    DynamicID = item.desc.dynamic_id,
                    ContentStr = "动态加载失败:\r\n" + JsonConvert.SerializeObject(item)
                };
            }

        }
        #endregion

        #region Public Methods

        public async void Refresh()
        {
            if (Loading)
            {
                return;
            }
            Items = null;
            await GetDynamicItems();
        }

        public async Task GetDynamicItems(string idx = "")
        {
            try
            {
                CanLoadMore = false;
                Loading = true;

                var api = m_dynamicApi.DyanmicNew(UserDynamicType);
                switch (DynamicType)
                {
                    case DynamicType.UserDynamic:
                        if (idx != "")
                        {
                            api = m_dynamicApi.HistoryDynamic(idx, UserDynamicType);
                        }
                        break;
                    case DynamicType.Topic:
                        break;
                    case DynamicType.Space:
                        api = m_dynamicApi.SpaceHistory(Uid, idx);
                        break;
                    default:
                        break;
                }

                var results = await api.Request();
                if (results.status)
                {
                    var data = results.GetJObject();
                    if (data["code"].ToInt32() == 0)
                    {
                        var items = JsonConvert.DeserializeObject<List<DynamicCardModel>>(data["data"]?["cards"]?.ToString() ?? "[]");
                        if (items.Count > 0)
                        {
                            CanLoadMore = true;
                        }
                        ObservableCollection<DynamicItemDisplayModel> _ls = new ObservableCollection<DynamicItemDisplayModel>();
                        foreach (var item in items)
                        {
                            _ls.Add(ConvertToDisplay(item));
                        }
                        if (Items == null)
                        {

                            Items = _ls;
                        }
                        else
                        {
                            foreach (var item in _ls)
                            {
                                Items.Add(item);
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
                var handel = HandelError<DynamicVM>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;

            }
        }
        public async Task GetDynamicDetail(string id)
        {
            try
            {
                CanLoadMore = false;
                Loading = true;
                var api = m_dynamicApi.DynamicDetail(id);

                var results = await api.Request();
                if (results.status)
                {
                    var data = results.GetJObject();
                    if (data["code"].ToInt32() == 0)
                    {
                        var items = JsonConvert.DeserializeObject<DynamicCardModel>(data["data"]["card"].ToString());
                        ObservableCollection<DynamicItemDisplayModel> _ls = new ObservableCollection<DynamicItemDisplayModel>();
                        _ls.Add(ConvertToDisplay(items));
                        Items = _ls;
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
                var handel = HandelError<DynamicVM>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;

            }
        }

        #endregion



    }


}
