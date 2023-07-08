using BiliLite.Extensions;
using BiliLite.Models;
using BiliLite.Models.Common;
using BiliLite.Services;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BiliLite.Modules
{
    public class IModules : INotifyPropertyChanged
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        public virtual ReturnModel<T> HandelError<T>(Exception ex, [CallerMemberName] string methodName = null)
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
                var type = new StackTrace().GetFrame(1).GetMethod().ReflectedType;
                _logger.Log(ex.Message, LogType.ERROR, ex, methodName, type.Name);
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
