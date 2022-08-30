using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiliLite.Models;
using BiliLite.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;
using Newtonsoft.Json.Linq;


namespace BiliLite.Modules
{
    public class AnimeTimelineVM : IModules
    {
        readonly Api.Home.AnimeAPI animeApi;
        public AnimeType animeType { get; set; }
        public AnimeTimelineVM(AnimeType type)
        {
            animeApi = new Api.Home.AnimeAPI();
            AnimeTypeItems = new List<AnimeTypeItem>()
                {
                    new AnimeTypeItem()
                    {
                        Name="番剧",
                        AnimeType= AnimeType.bangumi
                    },
                    new AnimeTypeItem()
                    {
                        Name="国创",
                        AnimeType= AnimeType.guochuang
                    }
                };
            SelectAnimeType = AnimeTypeItems.FirstOrDefault(x => x.AnimeType == type);
            animeType = type;
            
        }
        private bool _loading = true;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }

        private List<AnimeTypeItem> _animeTypeItems;

        public List<AnimeTypeItem> AnimeTypeItems
        {
            get { return _animeTypeItems; }
            set { _animeTypeItems = value; DoPropertyChanged("AnimeTypeItems"); }
        }


        private AnimeTypeItem _selectAnimeType;

        public AnimeTypeItem SelectAnimeType
        {
            get { return _selectAnimeType; }
            set { _selectAnimeType = value; DoPropertyChanged("SelectAnimeType"); }
        }


        private AnimeTimelineModel _today;

        public AnimeTimelineModel Today
        {
            get { return _today; }
            set { _today = value; DoPropertyChanged("Today"); }
        }

        private List<AnimeTimelineModel> _timelines;

        public List<AnimeTimelineModel> Timelines
        {
            get { return _timelines; }
            set { _timelines = value; DoPropertyChanged("Timelines"); }
        }

        public async Task GetTimeline()
        {
            try
            {
                Loading = true;
                var api = animeApi.Timeline((int)animeType);

                var results = await api.Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<List<AnimeTimelineModel>>>();
                    if (data.success)
                    {
                        Timelines = data.data;
                        Today = data.data.FirstOrDefault(x => x.is_today);
                    }
                    else
                    {
                        Utils.ShowMessageToast(data.message);
                    }
                }
                else
                {
                    Utils.ShowMessageToast(results.message);

                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<AnimeHomeModel>(ex);
                Utils.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }

    }
    public class AnimeTypeItem
    {
        public string Name { get; set; }
        public AnimeType AnimeType { get; set; }
    }
    public class AnimeTimelineModel
    {
        public string week { get; set; }
        public int day_week { get; set; }
        public string date { get; set; }
        public bool is_today { get; set; }
        public List<AnimeTimelineItemModel> seasons { get; set; }
    }
    public class AnimeTimelineItemModel
    {
        public int season_id { get; set; }
        public string cover { get; set; }
        public string square_cover { get; set; }
        public string pub_index { get; set; }
        public string pub_time { get; set; }
        public string title { get; set; }
    }
}
