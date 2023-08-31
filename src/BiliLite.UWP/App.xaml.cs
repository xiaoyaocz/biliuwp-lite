using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Models.Events;
using BiliLite.Modules;
using BiliLite.Services;
using FFmpegInteropX;
using Microsoft.Extensions.Hosting;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Runtime.InteropServices;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace BiliLite
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : Application, ILogProvider
    {
        private static readonly ILogger logger = GlobalLogger.FromCurrentType();
        private static IHost _host;

        public static IServiceProvider ServiceProvider { get => _host.Services; }

        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            App.Current.UnhandledException += App_UnhandledException;
            FFmpegInteropLogging.SetLogLevel(LogLevel.Info);
            FFmpegInteropLogging.SetLogProvider(this);
            SqlHelper.InitDB();
            LogService.Init();
            RegisterService();
            OpenCCNET.ZhConverter.Initialize();
            this.Suspending += OnSuspending;
            this.InitializeComponent();
        }
        private void RegisterExceptionHandlingSynchronizationContext()
        {
            ExceptionHandlingSynchronizationContext
                .Register()
                .UnhandledException += SynchronizationContext_UnhandledException;
        }

        private void SynchronizationContext_UnhandledException(object sender, AysncUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            try
            {
                logger.Log("程序运行出现错误", LogType.Error, e.Exception);
                Notify.ShowMessageToast("程序出现一个错误，已记录");
            }
            catch (Exception)
            {
            }
        }
        private void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            try
            {
                logger.Log("程序运行出现错误", LogType.Error, e.Exception);
                Notify.ShowMessageToast("程序出现一个错误，已记录");
            }
            catch (Exception)
            {
            }

        }

        public void Log(LogLevel level, string message)
        {
            System.Diagnostics.Debug.WriteLine("FFmpeg ({0}): {1}", level, message);
        }
        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {

            Navigation(e.Arguments, e.PrelaunchActivated);
            await LogService.DeleteExpiredLogFile();
        }


        private async void Navigation(object arguments, bool prelaunch = false)
        {
            // We don't have ARM64 support of SYEngine.
            if (RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
            {
                SYEngine.Core.Initialize();
            }
            try
            {
                var systemId = Windows.System.Profile.SystemIdentification.GetSystemIdForPublisher();
                var deviceId = Windows.Security.Cryptography.CryptographicBuffer.EncodeToHexString(systemId.Id).ToUpper();
                ApiHelper.deviceId = deviceId ?? "";
            }
            catch (Exception)
            {
            }

            InitBili();
            Frame rootFrame = Window.Current.Content as Frame;

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (rootFrame == null)
            {
                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;


                //主题颜色
                rootFrame.RequestedTheme = (ElementTheme)SettingService.GetValue<int>(SettingConstants.UI.THEME, 0);

                // 将框架放在当前窗口中
                Window.Current.Content = rootFrame;
            }

            if (prelaunch == false)
            {
                if (rootFrame.Content == null)
                {
                    // 当导航堆栈尚未还原时，导航到第一页，
                    // 并通过将所需信息作为导航参数传入来配置
                    // 参数

                    var mode = SettingService.GetValue<int>(SettingConstants.UI.DISPLAY_MODE, 0);
                    if (mode == 0)
                    {
                        rootFrame.Navigate(typeof(MainPage), arguments);
                    }
                    else
                    {
                        rootFrame.Navigate(typeof(NoTabMainPage), arguments);
                    }
                }
                else
                {
                    if (arguments != null && !string.IsNullOrEmpty(arguments.ToString()))
                    {
                        await MessageCenter.HandelUrl(arguments.ToString());
                    }

                }
                // 确保当前窗口处于活动状态
                Window.Current.Activate();
                ExtendAcrylicIntoTitleBar();
            }
        }

        private async void InitBili()
        {
            //首次运行设置首页的显示样式
            if (SystemInformation.IsFirstRun)
            {
                var display = DisplayInformation.GetForCurrentView();
                if (display.ScreenWidthInRawPixels >= 1920 && (display.ScreenWidthInRawPixels / display.ScreenHeightInRawPixels > 16 / 9))
                {
                    //如果屏幕分辨率大于16：9,设置为List
                    SettingService.SetValue<int>(SettingConstants.UI.RECMEND_DISPLAY_MODE, 1);
                }
            }
            //圆角
            App.Current.Resources["ImageCornerRadius"] = new CornerRadius(SettingService.GetValue<double>(SettingConstants.UI.IMAGE_CORNER_RADIUS, 0));
            await AppHelper.SetRegions();
            DownloadVM.Instance.LoadDownloading();
            DownloadVM.Instance.LoadDownloaded();
            VideoPlayHistoryHelper.LoadABPlayHistories(true);
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }

        public static void ExtendAcrylicIntoTitleBar()
        {
            UISettings uISettings = new UISettings();
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonForegroundColor = TitltBarButtonColor(uISettings);
            uISettings.ColorValuesChanged += new TypedEventHandler<UISettings, object>((setting, args) =>
            {
                titleBar.ButtonForegroundColor = TitltBarButtonColor(setting);
            });
        }
        private static Color TitltBarButtonColor(UISettings uISettings)
        {
            var settingTheme = SettingService.GetValue<int>(SettingConstants.UI.THEME, 0);
            var uiSettings = new Windows.UI.ViewManagement.UISettings();
            var color = uiSettings.GetColorValue(UIColorType.Foreground);
            if (settingTheme != 0)
            {
                color = settingTheme == 1 ? Colors.Black : Colors.White;
            }
            return color;
        }
        protected override void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);

            if (args.Kind == ActivationKind.Protocol)
            {
                ProtocolActivatedEventArgs eventArgs = args as ProtocolActivatedEventArgs;
                Navigation(eventArgs.Uri.AbsoluteUri, false);
            }

        }

        private void RegisterService()
        {
            try
            {
                var startup = new Startup();

                var hostBuilder = new HostBuilder()
                    .ConfigureServices(startup.ConfigureServices);
                _host = hostBuilder.Build();
            }
            catch (Exception ex)
            {
                logger.Error("Start Host Error",ex);
            }
        }
    }
}
