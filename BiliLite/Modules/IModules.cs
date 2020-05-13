using BiliLite.Helpers;
using BiliLite.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace BiliLite.Modules
{
    public class IModules : INotifyPropertyChanged
    { 
        public virtual ReturnModel<T> HandelError<T>(Exception ex)
        {
            if (LogHelper.IsNetworkError(ex))
            {
                return new ReturnModel<T>()
                {
                    success = false,
                    message = "请检查你的网络连接"
                };
            }
            else
            {
                LogHelper.Log(ex.Message, LogType.ERROR, ex);
                return new ReturnModel<T>()
                {
                    success = false,
                    message = "出现了一个未处理错误，已记录"
                };
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void DoPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
