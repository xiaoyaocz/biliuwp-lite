using BiliLite.Modules.User;
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
    public sealed partial class DynamicRepostControl : UserControl
    {
        public readonly DynamicRepostVM dynamicRepostVM;
        public DynamicRepostControl()
        {
            this.InitializeComponent(); 
            dynamicRepostVM = new DynamicRepostVM();
        }
        public async void LoadData(string id)
        {
            dynamicRepostVM.ID = id;
            await dynamicRepostVM.GetDynamicItems();
        }
        
    }
}
