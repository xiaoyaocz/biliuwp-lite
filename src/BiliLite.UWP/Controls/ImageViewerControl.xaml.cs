using BiliLite.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls
{
    public class ImageViewerParameter
    {
        public int Index { get; set; }
        public List<string> Images { get; set; }
    }
    public sealed partial class ImageViewerControl : UserControl
    {
        public event EventHandler CloseEvent;
        public ImageViewerControl()
        {
            this.InitializeComponent();
        }

        public List<ImageInfo> imgs;

        int index = 0;
        public void InitImage(ImageViewerParameter e)
        {
            imgs = new List<ImageInfo>();
            foreach (var item in e.Images)
            {
                imgs.Add(new ImageInfo()
                {
                    ImageUrl = item
                });
            }
            ChangedImage(e.Index);
            btnOrigin.Focus(FocusState.Programmatic);
        }
        public void ClearImage()
        {
            image.Source = null;
            imgs = null;

        }
        private async void ChangedImage(int i)
        {

            try
            {
                loadFaild.Visibility = Visibility.Collapsed;
                loading.Visibility = Visibility.Visible;
                image.Source = null;
                scrollViewer.ChangeView(null, null, 1);
                index = i;
                txtIndex.Text = $"{index + 1} / {imgs.Count}";
                if (imgs[i].ImageBytes == null)
                {
                    var imgBytes = await LoadImage(imgs[i].ImageUrl);

                    imgs[i].ImageBytes = imgBytes;

                }
                MemoryStream memoryStream = new MemoryStream(imgs[i].ImageBytes);
                var img = new BitmapImage();
                await img.SetSourceAsync(memoryStream.AsRandomAccessStream());
                imgs[i].Height = img.PixelHeight;
                imgs[i].Width = img.PixelWidth;
                image.Source = img;
                UpdateLayout();

                double factor;
                if (imgs[i].Height > imgs[i].Width)
                {
                    factor = scrollViewer.ViewportHeight / imgs[i].Height;
                }
                else
                {
                    factor = scrollViewer.ViewportWidth / imgs[i].Width;
                }

                scrollViewer.ChangeView(null, null, factor > 1 ? 1 : (float)factor);

            }
            catch (Exception ex)
            {
                loadFaild.Visibility = Visibility.Visible;
            }
            finally
            {
                loading.Visibility = Visibility.Collapsed;
            }
        }
        private async Task<byte[]> LoadImage(string url)
        {

            using (HttpClient clinet = new HttpClient())
            {
                var data = await clinet.GetByteArrayAsync(url);



                return data;
            }

        }

        private void btnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (imgs == null) return;
            scrollViewer.ChangeView(null, null, scrollViewer.ZoomFactor + (float)0.1);
        }

        private void btnZoomOut_Click(object sender, RoutedEventArgs e)
        {
            if (imgs == null) return;
            scrollViewer.ChangeView(null, null, scrollViewer.ZoomFactor - (float)0.1);
        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (imgs == null) return;
            if (index - 1 < 0) return;
            ChangedImage(index - 1);
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (imgs == null) return;
            if (index + 1 >= imgs.Count) return;
            ChangedImage(index + 1);
        }

        private void btnOrigin_Click(object sender, RoutedEventArgs e)
        {
            scrollViewer.ChangeView(null, null, 1);
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (imgs == null) return;
            if (imgs[index].ImageBytes == null) return;
            var bytes = imgs[index].ImageBytes;

            FileSavePicker save = new FileSavePicker();
            save.SuggestedStartLocation = PickerLocationId.PicturesLibrary;



            save.FileTypeChoices.Add("图片", new List<string>() { Path.GetExtension(imgs[index].ImageUrl) });
            save.SuggestedFileName = "bili_img_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            StorageFile file = await save.PickSaveFileAsync();
            if (file != null)
            {


                CachedFileManager.DeferUpdates(file);
                await FileIO.WriteBytesAsync(file, bytes);
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                Notify.ShowMessageToast("保存成功");
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (imgs == null) return;
            ChangedImage(index);
        }

        private async void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            if (imgs == null) return;
            var msg = "上一张：← 或 PageUp\r\n下一张：→ 或 PageDown\r\n放大：↑ 或 按住Ctrl+鼠标滚轮向上\r\n缩小：↓ 或 按住Ctrl+鼠标滚轮向下";
            await new MessageDialog(msg, "快捷键").ShowAsync();
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CloseEvent?.Invoke(sender, new EventArgs());
        }

        private async void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (imgs == null) return;
            if (imgs[index].ImageBytes == null) return;
            var bytes = imgs[index].ImageBytes;
            var data = new DataPackage();
            InMemoryRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream();
            await randomAccessStream.WriteAsync(bytes.AsBuffer());
            randomAccessStream.Seek(0);



            data.SetBitmap(RandomAccessStreamReference.CreateFromStream(randomAccessStream));

            data.RequestedOperation = DataPackageOperation.Copy;
            var rsult = Clipboard.SetContentWithOptions(data, null);
            Notify.ShowMessageToast("已复制到剪切板");
        }
    }

    public class ImageInfo
    {
        public string ImageUrl { get; set; }
        public byte[] ImageBytes { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }
}