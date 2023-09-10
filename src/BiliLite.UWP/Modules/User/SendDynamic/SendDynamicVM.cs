using BiliLite.Controls.Dynamic;
using BiliLite.Models;
using BiliLite.Models.Requests.Api.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using BiliLite.Extensions;
using BiliLite.Models.Responses;
using BiliLite.Models.Common;
using BiliLite.Services;

namespace BiliLite.Modules.User
{
    public class SendDynamicVM : IModules
    {
        private static readonly ILogger logger = GlobalLogger.FromCurrentType();

        readonly DynamicAPI dynamicAPI;
        public SendDynamicVM()
        {
            dynamicAPI = new DynamicAPI();
            Images = new ObservableCollection<UploadImagesModel>();
        }
        public SendDynamicVM(UserDynamicItemDisplayViewModel repostInfo)
        {
            dynamicAPI = new DynamicAPI();
            RepostInfo = repostInfo;
            IsRepost = true;
        }
        private bool _IsRepost = false;
        public bool IsRepost
        {
            get { return _IsRepost; }
            set { _IsRepost = value; DoPropertyChanged("IsRepost"); }
        }

        public UserDynamicItemDisplayViewModel RepostInfo { get; set; }

        private string _Content = "";
        public string Content
        {
            get { return _Content; }
            set { _Content = value; DoPropertyChanged("Content"); DoPropertyChanged("Count"); }
        }

        private ObservableCollection<UploadImagesModel> _images;

        public ObservableCollection<UploadImagesModel> Images
        {
            get { return _images; }
            set { _images = value; DoPropertyChanged("Images"); }
        }
        private bool _uploading;

        public bool Uploading
        {
            get { return _uploading; }
            set { _uploading = value; DoPropertyChanged("Uploading"); }
        }

        private bool _showImage = false;

        public bool ShowImage
        {
            get { return _showImage; }
            set { _showImage = value; DoPropertyChanged("ShowImage"); }
        }
        public List<AtDisplayModel> AtDisplaylist = new List<AtDisplayModel>();
        public List<AtModel> Atlist = new List<AtModel>();
        public async void UploadImage(StorageFile file)
        {
            try
            {

                Uploading = true;
                var api = dynamicAPI.UploadImage();


                IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);
                var bytes = new byte[fileStream.Size];
                await fileStream.ReadAsync(bytes.AsBuffer(), (uint)fileStream.Size, Windows.Storage.Streams.InputStreamOptions.None);
                var client = new RestClient(api.url);
                var request = new RestRequest();
                request.Method = Method.Post;
                request.AddParameter("biz", "draw");
                request.AddParameter("category", "daily");
                request.AddFile("file_up", bytes, file.Name);
                RestResponse response = await client.ExecuteAsync(request);
                var content = response.Content;

                ApiDataModel<UploadImagesModel> result = JsonConvert.DeserializeObject<ApiDataModel<UploadImagesModel>>(content);
                if (result.code == 0)
                {
                    result.data.image_size = (await file.GetBasicPropertiesAsync()).Size / 1024;
                    Images.Add(result.data);
                }
                else
                {
                    Notify.ShowMessageToast(result.message);
                }
            }
            catch (Exception ex)
            {
                logger.Log("图片上传失败", LogType.Fatal, ex);
                Notify.ShowMessageToast("图片上传失败");
            }
            finally
            {
                Uploading = false;
                ShowImage = Images.Count > 0;

            }
        }

        /// <summary>
        /// 转发
        /// </summary>
        public async Task<bool> SendRepost()
        {
            var ctrl = "[]";
            var at_uids = "";
            Atlist.Clear();

            if (AtDisplaylist.Count != 0)
            {

                foreach (var item in AtDisplaylist)
                {
                    if (Content.Contains(item.text))
                    {
                        Atlist.Add(new AtModel()
                        {
                            data = item.data.ToString(),
                            length = item.length - 2,
                            location = Content.IndexOf(item.text),
                            type = 1
                        });
                        var d = item.text.Replace("[", "").Replace("]", "");
                        Content = Content.Replace(item.text, d);
                        at_uids += item.data.ToString() + ",";
                    }
                }
                ctrl = JsonConvert.SerializeObject(Atlist);
                at_uids = at_uids.Remove(at_uids.Length - 1, 1);
                AtDisplaylist.Clear();
            }
            if (Content == "")
            {
                Content = "转发动态";
            }
            try
            {

                HttpResults httpResults = await dynamicAPI.RepostDynamic(RepostInfo.DynamicID, Content, at_uids, ctrl).Request();
                if (httpResults.status)
                {
                    var data = await httpResults.GetData<JObject>();
                    if (data.code == 0)
                    {

                        Notify.ShowMessageToast("转发成功");
                        AtDisplaylist.Clear();
                        return true;
                    }
                    else
                    {

                        Notify.ShowMessageToast("发表动态失败:" + data.message);
                        return false;
                    }
                }
                else
                {

                    Notify.ShowMessageToast(httpResults.message);
                    return false;
                }

            }
            catch (Exception ex)
            {
                Notify.ShowMessageToast("转发动态失败" + ex.Message);
                logger.Log("转发动态失败", LogType.Error, ex);
                return false;
            }

        }
        public async Task<bool> SendDynamic()
        {
            if (Content.Trim().Length == 0)
            {
                Notify.ShowMessageToast("不能发送空白动态");
                return false;
            }

            var ctrl = "[]";
            var at_uids = "";
            Atlist.Clear();
            var txt = Content;
            if (AtDisplaylist.Count != 0)
            {
                foreach (var item in AtDisplaylist)
                {
                    if (txt.Contains(item.text))
                    {
                        Atlist.Add(new AtModel()
                        {
                            data = item.data.ToString(),
                            length = item.length - 2,
                            location = txt.IndexOf(item.text),
                            type = 1
                        });
                        var d = item.text.Replace("[", "").Replace("]", "");
                        txt = txt.Replace(item.text, d);
                        at_uids += item.data.ToString() + ",";
                    }
                }
                ctrl = JsonConvert.SerializeObject(Atlist);
                at_uids = at_uids.Remove(at_uids.Length - 1, 1);

            }

            List<SendImagesModel> send_pics = new List<SendImagesModel>();
            foreach (var item in Images)
            {
                send_pics.Add(new SendImagesModel()
                {
                    img_height = item.image_height,
                    img_size = item.image_size,
                    img_src = item.image_url,
                    img_width = item.image_width
                });
            }
            var imgStr = JsonConvert.SerializeObject(send_pics);
            try
            {
                HttpResults httpResults;
                if (send_pics.Count == 0)
                {
                    httpResults = await dynamicAPI.CreateDynamicText(txt, at_uids, ctrl).Request();
                }
                else
                {
                    httpResults = await dynamicAPI.CreateDynamicPhoto(imgStr, txt, at_uids, ctrl).Request();
                }
                if (httpResults.status)
                {
                    var data = await httpResults.GetData<JObject>();
                    if (data.code == 0)
                    {

                        Notify.ShowMessageToast("发表动态成功");
                        AtDisplaylist.Clear();
                        return true;
                    }
                    else
                    {

                        Notify.ShowMessageToast("发表动态失败:" + data.message);
                        return false;
                    }
                }
                else
                {
                    Notify.ShowMessageToast(httpResults.message);
                    return false;
                }

            }
            catch (Exception ex)
            {

                Notify.ShowMessageToast("发表动态发生错误");
                logger.Log("发表动态失败", LogType.Error, ex);
                return false;
            }

        }
    }

    public class UploadImagesModel
    {
        public int image_height { get; set; }
        public string image_url { get; set; }

        public double image_size { get; set; }

        public string image
        {
            get
            {
                return image_url + "@120w_120h_1e_1c.jpg";
            }
        }
        public int image_width { get; set; }

    }
    public class SendImagesModel
    {


        public int img_height { get; set; }
        public string img_src { get; set; }

        public double img_size { get; set; }
        public int img_width { get; set; }

    }
    public class AtModel
    {
        public string data { get; set; }
        public int location { get; set; }
        public int length { get; set; }
        public int type { get; set; } = 1;
    }
    public class AtDisplayModel
    {
        public long data { get; set; }
        public string text { get; set; }
        public int location { get; set; }
        public int length { get; set; }
    }

}
