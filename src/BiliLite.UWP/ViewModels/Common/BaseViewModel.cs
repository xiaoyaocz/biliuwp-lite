using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using BiliLite.Extensions;
using BiliLite.Models;
using BiliLite.Models.Common;
using BiliLite.Services;

namespace BiliLite.ViewModels.Common
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                try
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
                catch (Exception ex)
                {

                }
            }
        }

        protected void OnLocalPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected void Set<T>(ref T item, T value, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(item, value))
            {
                item = value;
                OnLocalPropertyChanged(propertyName);
            }
        }
        protected void Set([CallerMemberName] string propertyName = null)
        {
            OnLocalPropertyChanged(propertyName);
        }

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
                _logger.Log(ex.Message, LogType.Error, ex, methodName, type.Name);
                return new ReturnModel<T>()
                {
                    success = false,
                    message = "出现了一个未处理错误，已记录"
                };
            }
        }
    }
}
