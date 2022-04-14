using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;


namespace FontAwesome5.Extensions
{
    /// <summary>
    /// Control extensions
    /// </summary>
    public static class ControlExtensions
    {
        /// <summary>
        /// The key used for storing the spinner Storyboard.
        /// </summary>
        private static readonly string SpinnerStoryBoardName = String.Format("{0}Spinner", typeof(FontAwesome).Name);

        /// <summary>
        /// Start the spinning animation
        /// </summary>
        /// <typeparam name="T">FrameworkElement and ISpinable</typeparam>
        /// <param name="control">Control to apply the rotation </param>
        public static void BeginSpin<T>(this T control)
            where T : FrameworkElement, ISpinable
        {
            var transformGroup = control.RenderTransform as TransformGroup ?? new TransformGroup();

            var rotateTransform = transformGroup.Children.OfType<RotateTransform>().FirstOrDefault();

            if (rotateTransform != null)
            {
                rotateTransform.Angle = 0;
            }
            else
            {
                transformGroup.Children.Add(new RotateTransform() { Angle = 0.0 });
                control.RenderTransform = transformGroup;
                control.RenderTransformOrigin = new Point(0.5, 0.5);
            }

            var storyboard = new Storyboard();

            var animation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                AutoReverse = false,
                RepeatBehavior = RepeatBehavior.Forever,
                Duration = new Duration(TimeSpan.FromSeconds(control.SpinDuration))
            };
            storyboard.Children.Add(animation);

            Storyboard.SetTarget(animation, control);
#if WINDOWS_UWP
            Storyboard.SetTargetProperty(animation, "(FrameworkElement.RenderTransform).(TransformGroup.Children)[0].(RotateTransform.Angle)");
#else
            Storyboard.SetTargetProperty(animation,
                new PropertyPath("(0).(1)[0].(2)", UIElement.RenderTransformProperty,
                    TransformGroup.ChildrenProperty, RotateTransform.AngleProperty));
#endif
            storyboard.Begin();
            control.Resources.Add(SpinnerStoryBoardName, storyboard);
        }

        /// <summary>
        /// Stop the spinning animation 
        /// </summary>
        /// <typeparam name="T">FrameworkElement and ISpinable</typeparam>
        /// <param name="control">Control to stop the rotation.</param>
        public static void StopSpin<T>(this T control)
            where T : FrameworkElement, ISpinable
        {
            var storyboard = control.Resources[SpinnerStoryBoardName] as Storyboard;

            if (storyboard == null) return;

            storyboard.Stop();

            control.Resources.Remove(SpinnerStoryBoardName);
        }

        /// <summary>
        /// The key used for storing the spinner Storyboard.
        /// </summary>
        private static readonly string PulseStoryBoardName = String.Format("{0}Pulse", typeof(FontAwesome).Name);

        /// <summary>
        /// Start the pulse animation
        /// </summary>
        /// <typeparam name="T">FrameworkElement and IPulsable</typeparam>
        /// <param name="control">Control to apply the pulse animation</param>
        public static void BeginPulse<T>(this T control)
            where T : FrameworkElement, IPulsable
        {
            var transformGroup = control.RenderTransform as TransformGroup ?? new TransformGroup();

            var rotateTransform = transformGroup.Children.OfType<RotateTransform>().FirstOrDefault();

            if (rotateTransform != null)
            {
                rotateTransform.Angle = 0;
            }
            else
            {
                transformGroup.Children.Add(new RotateTransform() { Angle = 0.0 });
                control.RenderTransform = transformGroup;
                control.RenderTransformOrigin = new Point(0.5, 0.5);
            }

            var storyboard = new Storyboard();

#if WINDOWS_UWP
            var animation = new DoubleAnimationUsingKeyFrames()
            {
                AutoReverse = false,
                RepeatBehavior = RepeatBehavior.Forever,
                Duration = new Duration(TimeSpan.FromSeconds(control.PulseDuration)),
            };
            animation.KeyFrames.Add(new DiscreteDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(control.PulseDuration * 0)), Value = 0 });
            animation.KeyFrames.Add(new DiscreteDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(control.PulseDuration * 0.125)), Value = 45 });
            animation.KeyFrames.Add(new DiscreteDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(control.PulseDuration * 0.25)), Value = 90 });
            animation.KeyFrames.Add(new DiscreteDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(control.PulseDuration * 0.375)), Value = 135 });
            animation.KeyFrames.Add(new DiscreteDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(control.PulseDuration * 0.5)), Value = 180 });
            animation.KeyFrames.Add(new DiscreteDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(control.PulseDuration * 0.625)), Value = 225 });
            animation.KeyFrames.Add(new DiscreteDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(control.PulseDuration * 0.75)), Value = 270 });
            animation.KeyFrames.Add(new DiscreteDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(control.PulseDuration * 0.875)), Value = 315 });
            animation.KeyFrames.Add(new DiscreteDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(control.PulseDuration * 1.0)), Value = 360 });
#else
            var animation = new DoubleAnimationUsingKeyFrames
            {
                KeyFrames = new DoubleKeyFrameCollection
                {
                    new DiscreteDoubleKeyFrame { KeyTime = KeyTime.FromPercent(0),     Value = 0 },
                    new DiscreteDoubleKeyFrame { KeyTime = KeyTime.FromPercent(0.125), Value = 45 },
                    new DiscreteDoubleKeyFrame { KeyTime = KeyTime.FromPercent(0.25),  Value = 90 },
                    new DiscreteDoubleKeyFrame { KeyTime = KeyTime.FromPercent(0.375), Value = 135 },
                    new DiscreteDoubleKeyFrame { KeyTime = KeyTime.FromPercent(0.5),   Value = 180 },
                    new DiscreteDoubleKeyFrame { KeyTime = KeyTime.FromPercent(0.625), Value = 225 },
                    new DiscreteDoubleKeyFrame { KeyTime = KeyTime.FromPercent(0.75),  Value = 270 },
                    new DiscreteDoubleKeyFrame { KeyTime = KeyTime.FromPercent(0.875), Value = 315 },
                    new DiscreteDoubleKeyFrame { KeyTime = KeyTime.FromPercent(1.0),   Value = 360 },
                },
                AutoReverse = false,
                RepeatBehavior = RepeatBehavior.Forever,
                Duration = new Duration(TimeSpan.FromSeconds(control.PulseDuration))
            };
#endif
            storyboard.Children.Add(animation);

            Storyboard.SetTarget(animation, control);
#if WINDOWS_UWP
            Storyboard.SetTargetProperty(animation, "(FrameworkElement.RenderTransform).(TransformGroup.Children)[0].(RotateTransform.Angle)");
#else
            Storyboard.SetTargetProperty(animation,
                new PropertyPath("(0).(1)[0].(2)", UIElement.RenderTransformProperty,
                    TransformGroup.ChildrenProperty, RotateTransform.AngleProperty));
#endif

            storyboard.Begin();
            control.Resources.Add(PulseStoryBoardName, storyboard);
        }

        /// <summary>
        /// Stop the pulse animation 
        /// </summary>
        /// <typeparam name="T">FrameworkElement and IPulsable</typeparam>
        /// <param name="control">Control to stop the pulse animation</param>
        public static void StopPulse<T>(this T control)
            where T : FrameworkElement, IPulsable
        {
            var storyboard = control.Resources[PulseStoryBoardName] as Storyboard;

            if (storyboard == null) return;

            storyboard.Stop();

            control.Resources.Remove(PulseStoryBoardName);
        }

        /// <summary>
        /// Sets the rotation for the control
        /// </summary>
        /// <typeparam name="T">FrameworkElement and IRotatable</typeparam>
        /// <param name="control">Control to apply the rotation</param>
        public static void SetRotation<T>(this T control)
            where T : FrameworkElement, IRotatable
        {
            var transformGroup = control.RenderTransform as TransformGroup ?? new TransformGroup();

            var rotateTransform = transformGroup.Children.OfType<RotateTransform>().FirstOrDefault();

            if (rotateTransform != null)
            {
                rotateTransform.Angle = control.Rotation;
            }
            else
            {
                transformGroup.Children.Add(new RotateTransform() { Angle = control.Rotation });
                control.RenderTransform = transformGroup;
                control.RenderTransformOrigin = new Point(0.5, 0.5);
            }
        }

        /// <summary>
        /// Sets the flip orientation for the control
        /// </summary>
        /// <typeparam name="T">FrameworkElement and IRotatable</typeparam>
        /// <param name="control">Control to apply the rotation</param>
        public static void SetFlipOrientation<T>(this T control)
            where T : FrameworkElement, IFlippable
        {
            var transformGroup = control.RenderTransform as TransformGroup ?? new TransformGroup();

            var scaleX = control.FlipOrientation == EFlipOrientation.Normal || control.FlipOrientation == EFlipOrientation.Vertical ? 1 : -1;
            var scaleY = control.FlipOrientation == EFlipOrientation.Normal || control.FlipOrientation == EFlipOrientation.Horizontal ? 1 : -1;

            var scaleTransform = transformGroup.Children.OfType<ScaleTransform>().FirstOrDefault();

            if (scaleTransform != null)
            {
                scaleTransform.ScaleX = scaleX;
                scaleTransform.ScaleY = scaleY;
            }
            else
            {
                transformGroup.Children.Add(new ScaleTransform() { ScaleX = scaleX, ScaleY = scaleY });
                control.RenderTransform = transformGroup;
                control.RenderTransformOrigin = new Point(0.5, 0.5);
            }
        }
    }
}
