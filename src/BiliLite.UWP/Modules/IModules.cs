using BiliLite.Extensions;
using BiliLite.Models;
using BiliLite.Models.Common;
using BiliLite.Services;
using System;
using System.ComponentModel;

namespace BiliLite.Modules
{
    public class IModules : INotifyPropertyChanged
    { 
        public virtual ReturnModel<T> HandelError<T>(Exception ex)
        {
            if (ex.IsNetworkError())
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
