using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Atelier39;
using BiliLite.Services.Interfaces;
using BiliLite.ViewModels.Video;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace BiliLite.Services
{
    public class FrostMasterDanmakuController : IDanmakuController
    {
        private CanvasAnimatedControl m_danmakuCanvas;
        private DanmakuFrostMaster m_danmakuMaster;

        public FrostMasterDanmakuController()
        {
            DanmakuViewModel = new DanmakuViewModel()
            {
                ShowAreaControl = false,
                ShowBoldControl = false,
                ShowBoldStyleControl = false,
                IsHide = false,
            };
        }

        public override void Init(UserControl danmakuElement)
        {
            m_danmakuCanvas = danmakuElement as CanvasAnimatedControl;
            m_danmakuMaster = new DanmakuFrostMaster(m_danmakuCanvas);
            m_danmakuMaster.SetAutoControlDensity(false);
            m_danmakuMaster.SetRollingDensity(-1);
            m_danmakuMaster.SetIsTextBold(true);
            m_danmakuMaster.SetBorderColor(Colors.Blue);
        }

        public override void Clear()
        {
            m_danmakuMaster.Clear();
        }

        public override void Hide()
        {
            base.Hide();
            m_danmakuCanvas.Visibility = Visibility.Collapsed;
            DanmakuViewModel.IsHide = true;
        }

        public override void Show()
        {
            base.Show();
            m_danmakuCanvas.Visibility = Visibility.Visible;
            DanmakuViewModel.IsHide = false;
        }

        public override void HideTop()
        {
            m_danmakuMaster.SetLayerRenderState(DanmakuDefaultLayerDef.TopLayerId, false);
        }

        public override void HideBottom()
        {
            m_danmakuMaster.SetLayerRenderState(DanmakuDefaultLayerDef.BottomLayerId, false);
        }

        public override void HideScroll()
        {
            m_danmakuMaster.SetLayerRenderState(DanmakuDefaultLayerDef.RollingLayerId, false);
        }

        public override void ShowTop()
        {
            m_danmakuMaster.SetLayerRenderState(DanmakuDefaultLayerDef.TopLayerId, true);
        }

        public override void ShowBottom()
        {
            m_danmakuMaster.SetLayerRenderState(DanmakuDefaultLayerDef.BottomLayerId, true);
        }

        public override void ShowScroll()
        {
            m_danmakuMaster.SetLayerRenderState(DanmakuDefaultLayerDef.RollingLayerId, true);
        }

        public override void SetFontZoom(double fontZoom)
        {
            base.SetFontZoom(fontZoom);
            var fontLevel = fontZoom * 3;
            m_danmakuMaster.SetDanmakuFontSizeOffset((int)fontLevel);
        }

        public override void SetSpeed(int speed)
        {
            base.SetSpeed(speed);
            speed /= 2;
            m_danmakuMaster.SetRollingSpeed(speed);
        }

        public override void SetTopMargin(double topMargin)
        {
            base.SetTopMargin(topMargin);
            m_danmakuCanvas.Margin = new Thickness(0, topMargin, 0, 0);
        }

        public override void SetOpacity(double opacity)
        {
            base.SetOpacity(opacity);
            m_danmakuMaster.SetOpacity(opacity);
        }

        public override void SetBold(bool bold)
        {
            base.SetBold(bold);
            m_danmakuMaster.SetIsTextBold(bold);
        }

        public override void SetDensity(int density)
        {
            base.SetDensity(density);
            m_danmakuMaster.SetRollingDensity(density-1);
        }

        public override void Load(IEnumerable danmakuList)
        {
            var realDanmakuList = (danmakuList as List<DanmakuItem>).ToList();
            m_danmakuMaster.SetDanmakuList(realDanmakuList);
        }

        public override void Add(object danmakuItem, bool owner)
        {
            var realDanmakuItem = danmakuItem as DanmakuItem;
            m_danmakuMaster.AddRealtimeDanmaku(realDanmakuItem, false);
        }

        public override void Pause()
        {
            m_danmakuMaster.Pause();
        }

        public override void Resume()
        {
            m_danmakuMaster.Resume();
        }

        public override void UpdateTime(long position)
        {
            m_danmakuMaster.UpdateTime((uint)position * 1000);
        }
    }
}
