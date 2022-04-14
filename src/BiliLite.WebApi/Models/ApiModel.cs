using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiliLite.WebApi.Models
{
    public class ApiModel<T>
    {
        public int code { get; set; }
        public string message { get; set; }
        public T data { get; set; }
    }
}
