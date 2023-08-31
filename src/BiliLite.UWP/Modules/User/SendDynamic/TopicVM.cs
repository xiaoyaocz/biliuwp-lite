using BiliLite.Models;
using BiliLite.Models.Requests.Api.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using BiliLite.Extensions;

namespace BiliLite.Modules.User.SendDynamic
{
    public class TopicVM : IModules
    {
        readonly DynamicAPI dynamicAPI;
        public TopicVM()
        {
            dynamicAPI = new DynamicAPI();
            Items = new ObservableCollection<RcmdTopicModel>();
        }

        private ObservableCollection<RcmdTopicModel> _items;

        public ObservableCollection<RcmdTopicModel> Items
        {
            get { return _items; }
            set { _items = value; DoPropertyChanged("Items"); }
        }

        private bool _loading = true;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }

        public async Task GetTopic()
        {
            try
            {
                Loading = true;
                var api = dynamicAPI.RecommendTopic();

                var results = await api.Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        Items = JsonConvert.DeserializeObject<ObservableCollection<RcmdTopicModel>>(data.data["rcmds"].ToString());

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
                var handel = HandelError<TopicVM>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }


    }

    public class RcmdTopicModel
    {
        public int topic_id { get; set; }
        public string topic_name { get; set; }
        public int is_activity { get; set; }
        public string display
        {
            get { return "#" + topic_name + "#"; }
        }

    }
}
