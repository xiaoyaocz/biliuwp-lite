using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls.Dynamic
{
    public sealed partial class DynamicItemControl : UserControl
    {
        public DynamicItemControl()
        {
            this.InitializeComponent();
        }
      
        public UserDynamicItemDisplayViewModel ViewModel
        {
            get => (UserDynamicItemDisplayViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        // Using a DependencyProperty as the backing store for Model.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(UserDynamicItemDisplayViewModel), typeof(DynamicItemControl), new PropertyMetadata(new UserDynamicItemDisplayViewModel()));

        public FrameworkElement CardContent
        {
            get => (FrameworkElement)GetValue(CardContentProperty);
            set => SetValue(CardContentProperty, value);
        }

        public static readonly DependencyProperty CardContentProperty =
            DependencyProperty.Register(nameof(CardContent), typeof(FrameworkElement), typeof(DynamicItemControl), new PropertyMetadata(null));
    }
}
