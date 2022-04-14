using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.Models
{
    public class ApiDataModel<T>
    {
        public int code { get; set; }
        private string _message;
        public string message
        {
            get {
                if (string.IsNullOrEmpty(_message))
                {
                    return msg;
                }
                else
                {
                    return _message;
                }
            }
            set { _message = value; }
        }
        public string msg { get; set; } = "";

        public bool success
        {
            get
            {
                return code == 0;
            }
        }
        public T data { get; set; }

        public bool proxy { get; set; } = false;
    }
    public class ApiResultModel<T>
    {
        public int code { get; set; }
        private string _message;
        public string message
        {
            get
            {
                if (string.IsNullOrEmpty(_message))
                {
                    return msg;
                }
                else
                {
                    return _message;
                }
            }
            set { _message = value; }
        }
        public bool success
        {
            get
            {
                return code == 0;
            }
        }
        public string msg { get; set; } = "";
        public T result { get; set; }
    }
}
