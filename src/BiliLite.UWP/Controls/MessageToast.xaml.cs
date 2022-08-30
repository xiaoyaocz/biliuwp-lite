using Microsoft.Toolkit.Uwp.UI.Animations;
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

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls
{

    public sealed partial class MessageToast : UserControl
    {
        private Popup m_Popup;

        private string m_TextBlockContent = "";
        private TimeSpan m_ShowTime;
        public MessageToast()
        {
            this.InitializeComponent();
            m_Popup = new Popup();
            this.Width = Window.Current.Bounds.Width;
            this.Height = Window.Current.Bounds.Height;
            m_Popup.Child = this;
            this.Loaded += NotifyPopup_Loaded;
            this.Unloaded += NotifyPopup_Unloaded;
        }

        public MessageToast(string content, TimeSpan showTime) : this()
        {
            if (m_TextBlockContent == null)
            {
                m_TextBlockContent = "";
            }
            this.m_TextBlockContent = content;
            this.m_ShowTime = showTime;
        }
        public MessageToast(string content, TimeSpan showTime, List<MyUICommand> commands) : this()
        {
            if (m_TextBlockContent == null)
            {
                m_TextBlockContent = "";
            }
            this.m_TextBlockContent = content;
            this.m_ShowTime = showTime;
            foreach (var item in commands)
            {
                HyperlinkButton button = new HyperlinkButton()
                {
                    Margin = new Thickness(8, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Content = new TextBlock() { Text = item.Label }
                };
                button.Click += new RoutedEventHandler((sender, e) => {
                    item.Invoked?.Invoke(this, item);
                });
                btns.Children.Add(button);
            }
        }


        public void Show()
        {
            this.m_Popup.IsOpen = true;

        }
        public async void Close()
        {
            await this.Offset(offsetX: 0, offsetY: (float)border.ActualHeight, duration: 200, delay: 0, easingType: EasingType.Default).StartAsync();
            this.m_Popup.IsOpen = false;
        }
        private async void NotifyPopup_Loaded(object sender, RoutedEventArgs e)
        {
            if (m_TextBlockContent == null)
            {
                m_TextBlockContent = "";
            }
            this.tbNotify.Text = m_TextBlockContent;
            Window.Current.SizeChanged += Current_SizeChanged;

            await this.Offset(offsetX: 0, offsetY: -72, duration: 200, delay: 0, easingType: EasingType.Default).StartAsync();
            await this.Offset(offsetX: 0, offsetY: (float)border.ActualHeight, duration: 200, delay: m_ShowTime.TotalMilliseconds, easingType: EasingType.Default).StartAsync();
            this.m_Popup.IsOpen = false;
        }


        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            this.Width = e.Size.Width;
            this.Height = e.Size.Height;
        }

        private void NotifyPopup_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Current_SizeChanged;
        }


    }
    public class MyUICommand
    {
        public MyUICommand(string lable)
        {
            Label = lable;
        }
        public MyUICommand(string lable, EventHandler<MyUICommand> invoked)
        {
            Label = lable;
            Invoked = invoked;
        }
        public object Id { get; set; }
        public EventHandler<MyUICommand> Invoked { get; set; }
        public string Label { get; set; }


    }
}
