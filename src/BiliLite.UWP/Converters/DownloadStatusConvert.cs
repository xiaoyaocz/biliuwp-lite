using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.UI.Xaml.Data;

namespace BiliLite.Converters
{
    public class DownloadStatusConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return "";
            }
            var Status = (BackgroundTransferStatus)value;
            switch (Status)
            {
                case BackgroundTransferStatus.Idle:
                    return "空闲中";
                case BackgroundTransferStatus.Running:
                    return "下载中";
                case BackgroundTransferStatus.PausedByApplication:
                    return "暂停中";
                case BackgroundTransferStatus.PausedCostedNetwork:
                    return "已暂停，因为正在使用数据";
                case BackgroundTransferStatus.PausedNoNetwork:
                    return "挂起";
                case BackgroundTransferStatus.Completed:
                    return "完成";
                case BackgroundTransferStatus.Canceled:
                    return "取消";
                case BackgroundTransferStatus.Error:
                    return "下载错误";
                case BackgroundTransferStatus.PausedSystemPolicy:
                    return "因系统问题暂停";
                default:
                    return "未知状态";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
