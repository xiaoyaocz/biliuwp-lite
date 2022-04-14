using System;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
using BiliLite.Helpers;

namespace BiliLite.Controls
{
    public sealed partial class Carousel : Control
    {
        private Panel _frame = null;
        private CarouselPanel _panel = null;

        private Grid _arrows = null;
        private Button _left = null;
        private Button _right = null;

        private LinearGradientBrush _gradient;
        private RectangleGeometry _clip;

        public Carousel()
        {
            this.DefaultStyleKey = typeof(Carousel);
            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
            this.SizeChanged += OnSizeChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            CreateFadeTimer();
            if (_slideTimer != null && this.SlideInterval > 150.0)
            {
                _slideTimer.Start();
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            DisposeFadeTimer();
            if (_slideTimer != null)
            {
                _slideTimer.Stop();
            }
        }

        protected override void OnApplyTemplate()
        {
            _frame = base.GetTemplateChild("frame") as Panel;
            _panel = base.GetTemplateChild("panel") as CarouselPanel;

            _arrows = base.GetTemplateChild("arrows") as Grid;
            _left = base.GetTemplateChild("left") as Button;
            _right = base.GetTemplateChild("right") as Button;

            _gradient = base.GetTemplateChild("gradient") as LinearGradientBrush;
            _clip = base.GetTemplateChild("clip") as RectangleGeometry;

            _frame.ManipulationDelta += OnManipulationDelta;
            _frame.ManipulationCompleted += OnManipulationCompleted;
            _frame.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.System;

            _frame.PointerMoved += OnPointerMoved;
            _left.Click += OnLeftClick;
            _right.Click += OnRightClick;
            _left.PointerEntered += OnArrowPointerEntered;
            _left.PointerExited += OnArrowPointerExited;
            _right.PointerEntered += OnArrowPointerEntered;
            _right.PointerExited += OnArrowPointerExited;

            base.OnApplyTemplate();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            availableSize = NormalizeSize(availableSize);

            double width = availableSize.Width / this.MaxItems;
            double height = width / this.AspectRatio;

            if (height < MinHeight)
            {
                height = MinHeight;
                width = height * this.AspectRatio;
            }

            if (height > MaxHeight)
            {
                height = MaxHeight;
                width = height * this.AspectRatio;
            }

            _panel.ItemWidth = Math.Round(width);
            _panel.ItemHeight = Math.Round(height);

            this.Position = -this.Index * width;

            return base.MeasureOverride(new Size(availableSize.Width, height));
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return base.ArrangeOverride(new Size(finalSize.Width, _panel.ItemHeight));
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _clip.Rect = new Rect(new Point(), e.NewSize);
            ApplyGradient();
        }

        private void ApplyGradient()
        {
            if (this.MaxItems > 2)
            {
                double factor = 1.0 / this.MaxItems;
                int index = this.MaxItems / 2;
                int count = 1;
                if (this.MaxItems % 2 == 0)
                {
                    index--;
                    count++;
                }
                _gradient.GradientStops[1].Offset = factor * index;
                _gradient.GradientStops[2].Offset = factor * (index + count);
            }
        }

        #region NormalizeSize
        private static Size NormalizeSize(Size size)
        {
            double width = size.Width;
            double height = size.Height;

            if (double.IsInfinity(width))
            {
                width = Window.Current.Bounds.Width;
            }
            if (double.IsInfinity(height))
            {
                height = Window.Current.Bounds.Height;
            }

            return new Size(width, height);
        }
        #endregion

        #region Move between items
        public void MoveBack()
        {
            if (_isBusy)
                return;
            _panel.TranslateDeltaX(0.01);
            AnimatePrev();
        }

        public void MoveForward()
        {
            if (_isBusy)
                return;
            _panel.TranslateDeltaX(-0.01);
            AnimateNext();
        }
        #endregion
    }
}
