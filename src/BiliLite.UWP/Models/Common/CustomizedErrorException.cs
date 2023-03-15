using System;

namespace BiliLite.Models.Common
{
    public class CustomizedErrorException : Exception
    {
        public CustomizedErrorException(string msg) : base(msg)
        {
        }
    }
}
