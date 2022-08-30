using FFMpegCore;
using FFMpegCore.Enums;
using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BiliLite.Win32Tools
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        ConvertFileInfo convertFileInfo;
        string currentDir = "";
        string ffmpegFile = "";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadInfo()
        {
            var str = Windows.Storage.ApplicationData.Current.LocalSettings.Values["VideoConverterInfo"] as string;
            convertFileInfo = System.Text.Json.JsonSerializer.Deserialize<ConvertFileInfo>(str);
            txtName.Text = convertFileInfo.title;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadInfo();
                txtStatus.Text = "正在解压FFmpeg,请稍等";

                var result = await DecompressFFmpeg();
                if (!result)
                {
                    progressBar.Visibility = Visibility.Collapsed;
                    txtStatus.Text = "解压FFmpeg失败，请关闭程序后再试";
                    return;
                }
                txtStatus.Text = "正在导出视频";
                StartTask();
            }
            catch (Exception ex)
            {
                progressBar.Visibility = Visibility.Collapsed;
                txtStatus.Text = $"执行任务失败：\r\n{ex.Message}";
            }

        }

        private async Task<bool> DecompressFFmpeg()
        {

            var zipDir = Assembly.GetExecutingAssembly().Location;
            zipDir = System.IO.Path.GetDirectoryName(zipDir);
            currentDir = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            return await Task.Run<bool>(() =>
            {
                try
                {

                    var ffmpeg7ZipPath = System.IO.Path.Combine(zipDir, "ffmpeg.7z");
                    ffmpegFile = System.IO.Path.Combine(currentDir, "ffmpeg.exe");
                    //检查文件是否存在
                    if (File.Exists(ffmpegFile))
                    {
                        return true;
                    }
                    //解压文件
                    using (var archive = SevenZipArchive.Open(ffmpeg7ZipPath))
                    {
                        foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                        {
                            entry.WriteToDirectory(currentDir, new ExtractionOptions()
                            {
                                ExtractFullPath = true,
                                Overwrite = true
                            });
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "FFmpeg解压失败");
                }
                return false;
            });
        }

        private async void StartTask()
        {
            if (convertFileInfo.inputFiles.Count == 0)
            {
                progressBar.Visibility = Visibility.Collapsed;
                txtStatus.Text = "视频为空";
                return;
            }
            GlobalFFOptions.Configure(new FFOptions { BinaryFolder = currentDir, TemporaryFilesFolder = currentDir });

            if (convertFileInfo.isDash)
            {
                if (convertFileInfo.subtitle.Count <= 0)
                {
                    await ConvertDash();
                }
                else
                {
                    await ConvertDashWithSubtitle();
                }

            }
            else
            {
                await ConvertToMp4();
            }

        }
        private async Task ConvertDash()
        {
            try
            {
                var info = await FFMpegArguments.FromFileInput(convertFileInfo.inputFiles.FirstOrDefault(x => x.Contains("video.m4s")))
                    .AddFileInput(convertFileInfo.inputFiles.FirstOrDefault(x => x.Contains("audio.m4s")))
                    .OutputToFile(convertFileInfo.outFile, true, options =>
                        options.WithVideoCodec("copy").WithAudioCodec("copy").WithFastStart()
                    ).ProcessAsynchronously();
                progressBar.Visibility = Visibility.Collapsed;
                txtStatus.Text = "视频导出成功!";
            }
            catch (Exception ex)
            {
                progressBar.Visibility = Visibility.Collapsed;
                txtStatus.Text = $"视频导出失败：\r\n{ex.Message}";
            }

        }
        private async Task ConvertToMp4()
        {
            try
            {
                var info = FFMpegArguments.FromFileInput(convertFileInfo.inputFiles.FirstOrDefault());
                if (convertFileInfo.subtitle.Count > 0)
                {
                    info = info.AddFileInput(convertFileInfo.subtitle.FirstOrDefault());
                }
                var processor = info.OutputToFile(convertFileInfo.outFile, true, options =>
                        options.WithArgument(new FFMpegCore.Arguments.CustomArgument(convertFileInfo.subtitle.Count > 0 ? "-c copy -c:s mov_text" : "-c copy"))
                        .WithFastStart()
                );
                await processor.ProcessAsynchronously();
                progressBar.Visibility = Visibility.Collapsed;
                txtStatus.Text = "视频导出成功!";
            }
            catch (Exception ex)
            {
                progressBar.Visibility = Visibility.Collapsed;
                txtStatus.Text = $"视频导出失败：\r\n{ex.Message}";
            }

        }
        private async Task ConvertDashWithSubtitle()
        {
            try
            {
                var info = await FFMpegArguments.FromFileInput(convertFileInfo.inputFiles.FirstOrDefault(x => x.Contains("video.m4s")))
                    .AddFileInput(convertFileInfo.inputFiles.FirstOrDefault(x => x.Contains("audio.m4s")))
                    .AddFileInput(convertFileInfo.subtitle.FirstOrDefault())
                    .OutputToFile(convertFileInfo.outFile, true, options =>
                      options.WithArgument(new FFMpegCore.Arguments.CustomArgument("-c copy -c:s mov_text")).WithFastStart()
                    ).ProcessAsynchronously();
                progressBar.Visibility = Visibility.Collapsed;
                txtStatus.Text = "视频导出成功!";
            }
            catch (Exception ex)
            {
                progressBar.Visibility = Visibility.Collapsed;
                txtStatus.Text = $"视频导出失败：\r\n{ex.Message}";
            }

        }


    }


    public class ConvertFileInfo
    {
        public string title { get; set; }
        public List<string> inputFiles { get; set; }
        public List<string> subtitle { get; set; }
        public string outFile { get; set; }
        public bool isDash { get; set; }
    }
}
