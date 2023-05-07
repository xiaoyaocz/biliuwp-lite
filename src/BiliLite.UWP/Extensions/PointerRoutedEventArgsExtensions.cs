using BiliLite.Services;
using BiliLite.Models.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace BiliLite.Extensions
{
    public static class PointerRoutedEventArgsExtensions
    {
        public static bool IsUseMiddleButton(this PointerRoutedEventArgs e, object sender)
        {
            var par = e.GetCurrentPoint(sender as UIElement).Properties.PointerUpdateKind;
            return par == Windows.UI.Input.PointerUpdateKind.XButton1Pressed ||
                   par == Windows.UI.Input.PointerUpdateKind.MiddleButtonPressed;
        }

        public static bool IsMiddleButtonNewTap(this PointerRoutedEventArgs e, object sender)
        {
            return e.IsUseMiddleButton(sender) &&
                   SettingService.GetValue(SettingConstants.UI.MOUSE_MIDDLE_ACTION, (int)MouseMiddleActions.Back) ==
                   (int)MouseMiddleActions.NewTap;
        }
    }
}
