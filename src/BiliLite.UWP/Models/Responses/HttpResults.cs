using System.Collections.Generic;

namespace BiliLite.Models.Responses
{
    public class HttpResults
    {
        public int code { get; set; }
        public string message { get; set; }
        public string results { get; set; }
        public bool status { get; set; }
        public List<HttpCookieItem> cookies { get; set; }
    }
}
