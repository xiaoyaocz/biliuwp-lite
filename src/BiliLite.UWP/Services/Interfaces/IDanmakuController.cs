using System.Collections;
using Windows.UI.Xaml.Controls;
using BiliLite.ViewModels.Video;

namespace BiliLite.Services.Interfaces
{
    public abstract class IDanmakuController
    {
        public DanmakuViewModel DanmakuViewModel { get; protected set; }

        public IDanmakuController()
        {
            DanmakuViewModel = new DanmakuViewModel();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public abstract void Init(UserControl danmakuElement);

        /// <summary>
        /// 清空
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// 隐藏
        /// </summary>
        public virtual void Hide()
        {
            DanmakuViewModel.IsHide = true;
        }

        /// <summary>
        /// 显示
        /// </summary>
        public virtual void Show()
        {
            DanmakuViewModel.IsHide = false;
        }

        /// <summary>
        /// 隐藏顶部
        /// </summary>
        public abstract void HideTop();

        /// <summary>
        /// 隐藏底部
        /// </summary>
        public abstract void HideBottom();

        /// <summary>
        /// 隐藏滚动
        /// </summary>
        public abstract void HideScroll();

        /// <summary>
        /// 显示顶部
        /// </summary>
        public abstract void ShowTop();

        /// <summary>
        /// 显示底部
        /// </summary>
        public abstract void ShowBottom();

        /// <summary>
        /// 显示滚动
        /// </summary>
        public abstract void ShowScroll();

        /// <summary>
        /// 设置弹幕字体缩放
        /// </summary>
        public virtual void SetFontZoom(double fontZoom)
        {
            DanmakuViewModel.SizeZoom = fontZoom;
        }

        /// <summary>
        /// 设置弹幕显示区域
        /// </summary>
        public virtual void SetArea(double area)
        {
            DanmakuViewModel.Area = area;
        }

        /// <summary>
        /// 设置弹幕速度
        /// </summary>
        public virtual void SetSpeed(int speed)
        {
            DanmakuViewModel.Speed = speed;
        }

        /// <summary>
        /// 设置弹幕顶部距离
        /// </summary>
        public virtual void SetTopMargin(double topMargin)
        {
            DanmakuViewModel.MarginTop = topMargin;
        }

        /// <summary>
        /// 设置透明度
        /// </summary>
        public virtual void SetOpacity(double opacity)
        {
            DanmakuViewModel.Opacity = opacity;
        }

        /// <summary>
        /// 设置弹幕加粗
        /// </summary>
        public virtual void SetBold(bool bold)
        {
            DanmakuViewModel.Bold = bold;
        }

        /// <summary>
        /// 设置弹幕密度
        /// </summary>
        /// <param name="density"></param>
        public virtual void SetDensity(int density)
        {
            DanmakuViewModel.Density = density;
        }

        /// <summary>
        /// 设置弹幕边框样式
        /// </summary>
        public virtual void SetBolderStyle(int bolderStyle)
        {
            DanmakuViewModel.BolderStyle = bolderStyle;
        }

        /// <summary>
        /// 加载弹幕
        /// </summary>
        public abstract void Load(IEnumerable danmakuList);

        /// <summary>
        /// 添加弹幕
        /// </summary>
        public abstract void Add(object danmakuItem, bool owner);

        /// <summary>
        /// 暂停
        /// </summary>
        public abstract void Pause();

        /// <summary>
        /// 继续
        /// </summary>
        public abstract void Resume();

        /// <summary>
        /// 更新大小
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public virtual void UpdateSize(double width, double height){}

        /// <summary>
        /// 更新时间
        /// </summary>
        /// <param name="position"></param>
        public virtual void UpdateTime(long position){}
    }
}
