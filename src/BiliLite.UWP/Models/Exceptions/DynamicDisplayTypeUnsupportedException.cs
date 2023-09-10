using System;
using BiliLite.Controls.Dynamic;

namespace BiliLite.Models.Exceptions
{
    public class DynamicDisplayTypeUnsupportedException : Exception
    {
        public UserDynamicItemDisplayViewModel ViewModel { get; private set; }

        public DynamicDisplayTypeUnsupportedException(UserDynamicItemDisplayViewModel viewModel)
        {
            ViewModel = viewModel;
        }
    }
}
