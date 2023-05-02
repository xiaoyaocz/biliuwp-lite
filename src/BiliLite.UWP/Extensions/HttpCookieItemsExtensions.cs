using BiliLite.Models;
using System.Collections.Generic;
using Windows.Web.Http.Filters;

namespace BiliLite.Extensions
{
    public static class HttpCookieItemsExtensions
    {
        public static void SaveCookie(this List<HttpCookieItem> cookies)
        {
            if (cookies == null)
            {
                return;
            }
            var filter = new HttpBaseProtocolFilter();
            foreach (var cookieItem in cookies)
            {
                filter.CookieManager.SetCookie(new Windows.Web.Http.HttpCookie(cookieItem.Name, cookieItem.Domain, "/")
                {
                    HttpOnly = cookieItem.HttpOnly,
                    Secure = cookieItem.Secure,
                    Expires = cookieItem.Expires,
                    Value = cookieItem.Value,
                });
            }
        }
    }
}
