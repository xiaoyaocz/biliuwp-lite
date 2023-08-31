using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using BiliLite.Services.Interfaces;
using BiliLite.ViewModels.Video;
using NSDanmaku.Controls;
using NSDanmaku.Model;

namespace BiliLite.Services
{
    public class NsDanmakuController : IDanmakuController
    {
        private Danmaku m_danmakuControl;

        public NsDanmakuController()
        {
            DanmakuViewModel = new DanmakuViewModel()
            {
                ShowAreaControl = true,
                ShowBoldControl = true,
                ShowBoldStyleControl = true,
                IsHide = false,
            };
        }

        public override void Init(UserControl danmakuElement)
        {
            m_danmakuControl = danmakuElement as Danmaku;
        }

        public override void Clear()
        {
            m_danmakuControl.ClearAll();
        }

        public override void Hide()
        {
            base.Hide();
            m_danmakuControl.Visibility = Visibility.Collapsed;
        }

        public override void Show()
        {
            base.Show();
            m_danmakuControl.Visibility = Visibility.Visible;
        }

        public override void HideTop()
        {
            m_danmakuControl.HideDanmaku(DanmakuLocation.Top);
        }

        public override void HideBottom()
        {
            m_danmakuControl.HideDanmaku(DanmakuLocation.Bottom);
        }

        public override void HideScroll()
        {
            m_danmakuControl.HideDanmaku(DanmakuLocation.Scroll);
        }

        public override void ShowTop()
        {
            m_danmakuControl.ShowDanmaku(DanmakuLocation.Top);
        }

        public override void ShowBottom()
        {
            m_danmakuControl.ShowDanmaku(DanmakuLocation.Bottom);
        }

        public override void ShowScroll()
        {
            m_danmakuControl.ShowDanmaku(DanmakuLocation.Scroll);
        }

        public override void SetFontZoom(double fontZoom)
        {
            base.SetFontZoom(fontZoom);
            m_danmakuControl.DanmakuSizeZoom = fontZoom;
        }

        public override void SetArea(double area)
        {
            base.SetArea(area);
            m_danmakuControl.DanmakuArea = area;
        }

        public override void SetSpeed(int speed)
        {
            base.SetSpeed(speed);
            m_danmakuControl.DanmakuDuration = speed;
        }

        public override void SetTopMargin(double topMargin)
        {
            base.SetTopMargin(topMargin);
            m_danmakuControl.Margin = new Thickness(0, topMargin, 0, 0);
        }

        public override void SetOpacity(double opacity)
        {
            base.SetOpacity(opacity);
            m_danmakuControl.Opacity = opacity;
        }

        public override void SetBold(bool bold)
        {
            base.SetBold(bold);
            m_danmakuControl.DanmakuBold = bold;
        }

        public override void SetBolderStyle(int bolderStyle)
        {
            base.SetBolderStyle(bolderStyle);
            m_danmakuControl.DanmakuStyle = (DanmakuBorderStyle)bolderStyle;
        }

        public override void Load(IEnumerable danmakuList)
        {
            var realDanmakuList = (danmakuList as IEnumerable<DanmakuModel>).ToList();
            foreach (var danmakuItem in realDanmakuList)
            {
                m_danmakuControl.AddDanmu(danmakuItem, false);
            }
        }

        public override void Add(object danmakuItem,bool owner)
        {
            var realDanmakuItem = danmakuItem as DanmakuModel;
            m_danmakuControl.AddDanmu(realDanmakuItem, owner);
        }

        public override void Pause()
        {
            m_danmakuControl.PauseDanmaku();
        }

        public override void Resume()
        {
            m_danmakuControl.ResumeDanmaku();
        }

        public override void UpdateSize(double width, double height)
        {
            var rectangle = new RectangleGeometry();
            rectangle.Rect = new Rect(0, 0, width, height);
            m_danmakuControl.Clip = rectangle;
        }
    }
}
