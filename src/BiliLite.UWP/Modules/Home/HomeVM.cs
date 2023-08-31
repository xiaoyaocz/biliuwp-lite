using BiliLite.Services;
using FontAwesome5;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using BiliLite.Models.Common;

namespace BiliLite.Modules
{
    public class HomeVM : IModules
    {
        Account account;
        public HomeVM()
        {
            account = new Account();
            HomeNavItems = SettingService.GetValue<ObservableCollection<HomeNavItem>>(SettingConstants.UI.HOEM_ORDER, GetAllNavItems());
            //var chanel=HomeNavItems.FirstOrDefault(x => x.Icon == EFontAwesomeIcon.Solid_Shapes);
            //var chanel=HomeNavItems.FirstOrDefault(x => x.Icon == EFontAwesomeIcon.Solid_Shapes);
            //if (chanel.Title=="频道")
            //{
            //    chanel = GetAllNavItems().FirstOrDefault(x => x.Icon == EFontAwesomeIcon.Solid_Shapes);
            //    SettingService.SetValue(SettingConstants.UI.HOEM_ORDER, HomeNavItems);
            //}
            SelectItem = HomeNavItems.FirstOrDefault();
            if (SettingService.Account.Logined)
            {
                IsLogin = true;
                foreach (var item in HomeNavItems)
                {
                    if (!item.Show && item.NeedLogin) item.Show = true;
                }
            }
        }
        public static ObservableCollection<HomeNavItem> GetAllNavItems()
        {
            return new ObservableCollection<HomeNavItem>() {
            new HomeNavItem(){
                Icon=FontAwesome5.EFontAwesomeIcon.Solid_Home,
                Page=typeof(Pages.Home.RecommendPage),
                Title="推荐",
                NeedLogin=false,
                Show=true
            },
            new HomeNavItem(){
                Icon=FontAwesome5.EFontAwesomeIcon.Solid_Fire,
                Page=typeof(Pages.Home.HotPage),
                Title="热门",
                NeedLogin=false,
                Show=true
            },
             new HomeNavItem(){
                Icon=FontAwesome5.EFontAwesomeIcon.Solid_Heart,
                Page=typeof(Pages.Home.UserDynamicPage),
                Title="动态",
                NeedLogin=true,
                Show=false
            },
            new HomeNavItem(){
                Icon=FontAwesome5.EFontAwesomeIcon.Solid_Heart,
                Page=typeof(Pages.Home.DynamicPage),
                Title="视频动态",
                NeedLogin=true,
                Show=false
            },
            new HomeNavItem(){
                Icon=FontAwesome5.EFontAwesomeIcon.Solid_Paw,
                Page=typeof(Pages.Home.AnimePage),
                Title="番剧",
                Parameters=AnimeType.Bangumi,
                NeedLogin=false,
                Show=true
            },
            new HomeNavItem(){
                Icon= FontAwesome5.EFontAwesomeIcon.Solid_Feather,
                Page=typeof(Pages.Home.AnimePage),
                Title="国创",
                Parameters=AnimeType.GuoChuang,
                NeedLogin=false,
                Show=true
            },
            new HomeNavItem(){
                Icon=FontAwesome5.EFontAwesomeIcon.Solid_Video,
                Page=typeof(Pages.Home.LivePage),
                Title="直播",
                NeedLogin=false,
                Show=true
            },
            new HomeNavItem(){
                Icon= FontAwesome5.EFontAwesomeIcon.Solid_Film,
                Page=typeof(Pages.Home.MoviePage),
                Title="放映厅",
                NeedLogin=false,
                Show=true
            },
            new HomeNavItem(){
                Icon=FontAwesome5.EFontAwesomeIcon.Solid_Shapes,
                Page=typeof(Pages.Home.RegionsPage),
                Title="分区",
                NeedLogin=false,
                Show=true
            },
            //new HomeNavItem(){
            //    Icon=FontAwesome5.EFontAwesomeIcon.Solid_Bars,
            //    Page=typeof(Pages.Home.ChannelPage),
            //    Title="频道",
            //    NeedLogin=false,
            //    Show=true
            //},
            new HomeNavItem(){
                Icon=FontAwesome5.EFontAwesomeIcon.Solid_Trophy,
                Page=typeof(Pages.RankPage),
                Title="排行榜",
                NeedLogin=false,
                Show=true
            },
            new HomeNavItem(){
                Icon=FontAwesome5.EFontAwesomeIcon.Solid_Compass,
                Page=typeof(Pages.Other.FindMorePage),
                Title="发现",
                NeedLogin=false,
                Show=true
            },

        };
        }

        private ObservableCollection<HomeNavItem> _HomeNavItems;
        public ObservableCollection<HomeNavItem> HomeNavItems
        {
            get { return _HomeNavItems; }
            set { _HomeNavItems = value; DoPropertyChanged("HomeNavItems"); }
        }

        private HomeNavItem _SelectItem;
        public HomeNavItem SelectItem
        {
            get { return _SelectItem; }
            set { _SelectItem = value; DoPropertyChanged("SelectItem"); }
        }

        private bool _IsLogin = false;

        public bool IsLogin
        {
            get { return _IsLogin; }
            set { _IsLogin = value; DoPropertyChanged("IsLogin"); }
        }

        private HomeUserCardModel _profile;
        public HomeUserCardModel Profile
        {
            get { return _profile; }
            set { _profile = value; DoPropertyChanged("Profile"); }
        }

        public async Task LoginUserCard()
        {
            var data = await account.GetHomeUserCard();
            if (data != null)
            {
                Profile = data;
                return;
            }
            //检查Token


        }

        private ObservableCollection<string> m_suggestSearchContents;

        public ObservableCollection<string> SuggestSearchContents
        {
            get => m_suggestSearchContents;
            set
            {
                m_suggestSearchContents = value;
                DoPropertyChanged("SuggestSearchContents");
            }
        }
    }
    public class HomeNavItem : IModules
    {
        public string Title { get; set; }
        public EFontAwesomeIcon Icon { get; set; }
        public Type Page { get; set; }
        public object Parameters { get; set; }
        public bool NeedLogin { get; set; }
        private bool _show;

        public bool Show
        {
            get { return _show; }
            set { _show = value; DoPropertyChanged("Show"); }
        }

    }
}
