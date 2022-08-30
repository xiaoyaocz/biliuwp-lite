using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace BiliLite.Controls
{
    public static class CarouselHelper
    {
        public static void TranslateX(this FrameworkElement elem, double x)
        {
            elem.GetCompositeTransform().TranslateX = x;
        }
        public static void TranslateY(this FrameworkElement elem, double y)
        {
            elem.GetCompositeTransform().TranslateY = y;
        }
        public static double GetTranslateX(this FrameworkElement elem)
        {
            return elem.GetCompositeTransform().TranslateX;
        }
        public static double GetTranslateY(this FrameworkElement elem)
        {
            return elem.GetCompositeTransform().TranslateY;
        }
        public static CompositeTransform GetCompositeTransform(this FrameworkElement elem)
        {
            if (elem == null)
            {
                throw new ArgumentNullException("elem");
            }

            var trans = elem.RenderTransform as CompositeTransform;
            if (trans == null)
            {
                trans = new CompositeTransform();
                elem.RenderTransform = trans;
            }
            return trans;
        }
        public static void TranslateDeltaX(this FrameworkElement elem, double x)
        {
            elem.GetCompositeTransform().TranslateX += x;
        }
        public static void TranslateDeltaY(this FrameworkElement elem, double y)
        {
            elem.GetCompositeTransform().TranslateY += y;
        }
        public static async Task AnimateXAsync(this FrameworkElement element, double x, double duration = 250, EasingFunctionBase easingFunction = null)
        {
            if (element.GetTranslateX() != x)
            {
               // await element.Offset(offsetX: (float)element.GetTranslateX(), offsetY: 0, duration: duration, delay: 0, easingType: EasingType.Default).StartAsync();  //Offset animation can be awaited
                await AnimateDoublePropertyAsync(element.GetCompositeTransform(), "TranslateX", element.GetTranslateX(), x, duration, easingFunction);
            }
        }
        public static async Task AnimateYAsync(this FrameworkElement element, double from, double to, double duration = 250, EasingFunctionBase easingFunction = null)
        {
            await AnimateDoublePropertyAsync(element.GetCompositeTransform(), "TranslateY", from, to, duration, easingFunction);
        }
        public static int Mod(this int value, int module)
        {
            if (module==0)
            {
                return 0;
            }
            int res = value % module;
            return res >= 0 ? res : (res + module) % module;
        }

        public static Storyboard FadeIn(this UIElement element, double duration = 250, EasingFunctionBase easingFunction = null)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (element.Opacity < 1.0)
            {
                return AnimateDoubleProperty(element, "Opacity", element.Opacity, 1.0, duration, easingFunction);
            }
            return null;
        }
        public static async Task FadeInAsync(this UIElement element, double duration = 250, EasingFunctionBase easingFunction = null)
        {
            if (element.Opacity < 1.0)
            {
                await AnimateDoublePropertyAsync(element, "Opacity", element.Opacity, 1.0, duration, easingFunction);
            }
        }

        public static Storyboard FadeOut(this UIElement element, double duration = 250, EasingFunctionBase easingFunction = null)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (element.Opacity > 0.0)
            {
                return AnimateDoubleProperty(element, "Opacity", element.Opacity, 0.0, duration, easingFunction);
            }
            return null;
        }
        public static async Task FadeOutAsync(this UIElement element, double duration = 250, EasingFunctionBase easingFunction = null)
        {
            if (element.Opacity > 0.0)
            {
                await AnimateDoublePropertyAsync(element, "Opacity", element.Opacity, 0.0, duration, easingFunction);
            }
        }
        public static Task AnimateDoublePropertyAsync(this DependencyObject target, string property, double from, double to, double duration = 250, EasingFunctionBase easingFunction = null)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            Storyboard storyboard = AnimateDoubleProperty(target, property, from, to, duration, easingFunction);
            storyboard.Completed += (sender, e) =>
            {
                tcs.SetResult(true);
            };
            return tcs.Task;
        }
        public static Storyboard AnimateDoubleProperty(this DependencyObject target, string property, double from, double to, double duration = 250, EasingFunctionBase easingFunction = null)
        {
            var storyboard = new Storyboard();
            var animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromMilliseconds(duration),
                EasingFunction = easingFunction ?? new SineEase(),
                FillBehavior = FillBehavior.HoldEnd,
                EnableDependentAnimation = true
            };

            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, property);

            storyboard.Children.Add(animation);
            storyboard.FillBehavior = FillBehavior.HoldEnd;
            storyboard.Begin();

            return storyboard;
        }
    }
}
