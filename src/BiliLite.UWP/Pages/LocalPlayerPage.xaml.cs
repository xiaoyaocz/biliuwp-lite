using BiliLite.Controls;
using BiliLite.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LocalPlayerPage : PlayPage
    {
        public LocalPlayerPage()
        {
            this.InitializeComponent();
            Title = "本地播放";
            this.Loaded += LocalPlayerPage_Loaded;
            this.Player = this.player;
            player.FullScreenEvent += Player_FullScreenEvent;
        }

        private void Player_FullScreenEvent(object sender, bool e)
        {
            if (e)
            {
                this.Margin = new Thickness(0, SettingHelper.GetValue<int>(SettingHelper.UI.DISPLAY_MODE, 0) == 0 ? -40 : -32, 0, 0);

            }
            else
            {
                this.Margin = new Thickness(0);

            }
        }

        private void LocalPlayerPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Parent is MyFrame)
            {
                (this.Parent as MyFrame).ClosedPage -= LocalPlayerPage_ClosedPage;
                (this.Parent as MyFrame).ClosedPage += LocalPlayerPage_ClosedPage;
            }
        }

        private void LocalPlayerPage_ClosedPage(object sender, EventArgs e)
        {
            Player_FullScreenEvent(this, false);
            player.MiniWidnows(false);
            player?.Dispose();

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                if (SettingHelper.GetValue<bool>(SettingHelper.Player.AUTO_FULL_SCREEN, false))
                {
                    player.IsFullScreen = true;
                }
                else
                {
                    player.IsFullWindow = SettingHelper.GetValue<bool>(SettingHelper.Player.AUTO_FULL_WINDOW, false);
                }
                var data = e.Parameter as LocalPlayInfo;
                player.InitializePlayInfo(data.PlayInfos, data.Index);
            }
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back || e.SourcePageType == typeof(BlankPage))
            {
                Player_FullScreenEvent(this, false);
                player.MiniWidnows(false);
                player?.Dispose();
            }
            base.OnNavigatingFrom(e);
        }
    }
    public class LocalPlayInfo
    {
        public List<PlayInfo> PlayInfos { get; set; }
        public int Index { get; set; } = 0;
    }
}
