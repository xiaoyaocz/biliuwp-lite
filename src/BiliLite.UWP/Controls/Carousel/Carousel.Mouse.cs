using Microsoft.Toolkit.Uwp.UI.Animations;
using System;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace BiliLite.Controls
{
    partial class Carousel
    {
        private bool _isArrowVisible = false;
        private bool _isArrowOver = false;

        private DispatcherTimer _fadeTimer = null;

        #region Create/Dispose FadeTimer
        private void CreateFadeTimer()
        {
            _fadeTimer = new DispatcherTimer();
            _fadeTimer.Interval = TimeSpan.FromMilliseconds(1500);
            _fadeTimer.Tick += OnFadeTimerTick;
        }

        private void DisposeFadeTimer()
        {
            var fadeTimer = _fadeTimer;
            _fadeTimer = null;
            if (fadeTimer != null)
            {
                fadeTimer.Stop();
            }
        }
        #endregion

        #region ArrowPointerEntered/ArrowPointerExited
        private void OnArrowPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            _isArrowOver = true;
        }

        private void OnArrowPointerExited(object sender, PointerRoutedEventArgs e)
        {
            _isArrowOver = false;
        }
        #endregion

        #region LeftClick/RightClick
        private void OnLeftClick(object sender, RoutedEventArgs e)
        {
            MoveBack();
        }

        private void OnRightClick(object sender, RoutedEventArgs e)
        {
            MoveForward();
        }

        #endregion

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!_isArrowVisible)
            {
                // _arrows.Fade(easingMode: Windows.UI.Xaml.Media.Animation.EasingMode.EaseIn).Start();
                _arrows.FadeIn();
                _isArrowVisible = true;
            }
            this._fadeTimer?.Start();
        }

        private void OnFadeTimerTick(object sender, object e)
        {
            if (_isArrowVisible && !_isArrowOver)
            {
                _isArrowVisible = false;
                //_arrows.Fade(easingMode: Windows.UI.Xaml.Media.Animation.EasingMode.EaseOut).Start();
                _arrows.FadeOut();
            }
        }
    }
}
