using BiliLite.Models.Requests.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;
using BiliLite.Extensions;

namespace BiliLite.Modules.Player
{
    public class InteractionVideoVM : IModules
    {
        readonly PlayerAPI playerAPI;
        private string Aid { get; }
        private int GraphVersion { get; }
        public InteractionVideoVM(string avid, int graph_version)
        {
            this.Aid = avid;
            this.GraphVersion = graph_version;
            playerAPI = new PlayerAPI();
        }

        private bool _loading;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }


        private InteractionEdgeInfoModel _info;
        public InteractionEdgeInfoModel Info
        {
            get { return _info; }
            set { _info = value; DoPropertyChanged("Info"); }
        }


        private List<InteractionEdgeInfoStoryListModel> _list;

        public List<InteractionEdgeInfoStoryListModel> List
        {
            get { return _list; }
            set { _list = value; DoPropertyChanged("List"); }
        }


        private InteractionEdgeInfoStoryListModel _select;

        public InteractionEdgeInfoStoryListModel Select
        {
            get { return _select; }
            set { _select = value; DoPropertyChanged("Select"); }
        }

        public async Task GetNodes(int edge_id = 0)
        {
            try
            {
                Loading = true;
                var api = playerAPI.InteractionEdgeInfo(aid: Aid, GraphVersion, edge_id);
                var result = await api.Request();
                if (result.status)
                {
                    var data = await result.GetData<InteractionEdgeInfoModel>();
                    if (data.code == 0)
                    {
                        if (data.data.edges != null && data.data.edges.questions != null)
                        {
                            foreach (var item in data.data.edges.questions)
                            {
                                foreach (var item1 in item.choices)
                                {
                                    item1.color = data.data.edges.skin.text_color;
                                    item1.cover = data.data.edges.skin.choice_image;
                                }
                            }
                        }

                        Info = data.data;
                        List = data.data.story_list;
                        Select = Info.story_list.FirstOrDefault(x => x.edge_id == Info.edge_id);
                    }
                }

            }
            catch (Exception ex)
            {
                var data = HandelError<object>(ex);
            }
            finally
            {
                Loading = false;
            }
        }

    }
    public class InteractionEdgeInfoModel
    {
        public string title { get; set; }
        public int edge_id { get; set; }
        /// <summary>
        /// 已解锁的节点
        /// </summary>
        public List<InteractionEdgeInfoStoryListModel> story_list { get; set; }
        /// <summary>
        /// 选项信息
        /// </summary>
        public InteractionEdgeInfoEdgesModel edges { get; set; }
        public int is_leaf { get; set; }
    }
    public class InteractionEdgeInfoStoryListModel
    {
        public int node_id { get; set; }
        public int edge_id { get; set; }
        public string title { get; set; }
        public int cid { get; set; }
        public int start_pos { get; set; }
        public string cover { get; set; }
        public int cursor { get; set; }
        public int? is_current { get; set; }
    }


    public class InteractionEdgeInfoChoiceModel
    {
        public int id { get; set; }
        public string platform_action { get; set; }
        public string native_action { get; set; }
        public string condition { get; set; }
        public int cid { get; set; }
        public string option { get; set; }
        public int is_default { get; set; }
        public string cover { get; set; }
        public SolidColorBrush color { get; set; }
    }

    public class InteractionEdgeInfoQuestionModel
    {
        public int id { get; set; }
        public int type { get; set; }
        public int start_time_r { get; set; }
        public int duration { get; set; }
        public int pause_video { get; set; }
        public string title { get; set; }
        public List<InteractionEdgeInfoChoiceModel> choices { get; set; }
    }

    public class InteractionEdgeInfoSkinModel
    {
        public string choice_image { get; set; }
        public string title_text_color { get; set; }
        public SolidColorBrush text_color
        {
            get
            {
                Color color = new Color();
                color.R = byte.Parse(title_text_color.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                color.G = byte.Parse(title_text_color.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                color.B = byte.Parse(title_text_color.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                color.A = byte.Parse(title_text_color.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
                return new SolidColorBrush(color);
            }
        }
        public string title_shadow_color { get; set; }
        public double title_shadow_offset_y { get; set; }
        public string progressbar_color { get; set; }
        public string progressbar_shadow_color { get; set; }
    }

    public class InteractionEdgeInfoEdgesModel
    {
        public List<InteractionEdgeInfoQuestionModel> questions { get; set; }
        public InteractionEdgeInfoSkinModel skin { get; set; }
    }


}
