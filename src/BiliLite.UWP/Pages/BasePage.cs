using BiliLite.Controls;
using BiliLite.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace BiliLite.Pages
{
    public class BasePage : Page
    {
        public string Title { get; set; }
        public BasePage()
        {
            this.NavigationCacheMode = (SettingHelper.GetValue<int>(SettingHelper.UI.DISPLAY_MODE, 0) == 1) ? Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled : Windows.UI.Xaml.Navigation.NavigationCacheMode.Disabled;
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back || e.SourcePageType == typeof(BlankPage))
            {
                this.NavigationCacheMode = NavigationCacheMode.Disabled;
            }

            base.OnNavigatingFrom(e);
        }
    }

    public class PlayPage : BasePage
    {
        public PlayerControl Player { get; set; }
        public void Pause()
        {
            Player.PlayerInstance.Pause();
        }
        public void Play()
        {
            Player.PlayerInstance.Play();
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back || e.SourcePageType == typeof(BlankPage))
            {
               // (this.Content as Grid).Children.Clear();
              //  GC.Collect();
            }
            base.OnNavigatingFrom(e);
        }
    }

}
