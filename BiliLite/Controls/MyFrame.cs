using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace BiliLite.Controls
{
    public class MyFrame:Frame
    {
        public event EventHandler ClosedPage;
        public void Close()
        {
            ClosedPage?.Invoke(this,null);
        }

    }
}
