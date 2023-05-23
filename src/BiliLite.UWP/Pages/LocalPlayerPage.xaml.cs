using BiliLite.Controls;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Video;
using BiliLite.Services;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
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
                this.Margin = new Thickness(0, SettingService.GetValue<int>(SettingConstants.UI.DISPLAY_MODE, 0) == 0 ? -48 : -48, 0, 0);
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
                if (SettingService.GetValue<bool>(SettingConstants.Player.AUTO_FULL_SCREEN, false))
                {
                    player.IsFullScreen = true;
                }
                else
                {
                    player.IsFullWindow = SettingService.GetValue<bool>(SettingConstants.Player.AUTO_FULL_WINDOW, false);
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
