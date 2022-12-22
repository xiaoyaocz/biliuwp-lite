using System;

namespace BiliLite.Models
{
    public class HttpCookieItem
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool HttpOnly { get; set; }
        public DateTimeOffset? Expires { get; set; }
        public bool Secure { get; set; }
        public string Domain { get; set; }
    }
}
