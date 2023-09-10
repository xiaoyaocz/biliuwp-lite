using System;

namespace BiliLite.Models.Exceptions
{
    public class CustomizedErrorException : Exception
    {
        public CustomizedErrorException(string msg) : base(msg)
        {
        }
    }
}
