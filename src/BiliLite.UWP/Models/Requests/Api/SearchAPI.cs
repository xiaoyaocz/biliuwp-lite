using BiliLite.Extensions;
using BiliLite.Services;
using System;
using System.Collections.Generic;

namespace BiliLite.Models.Requests.Api
{
    public class SearchAPI
    {
        /// <summary>
        /// 综合搜索
        /// </summary>
        /// <param name="keyword">关键字</param>
        /// <param name="order">排序</param>
        /// <param name="duration">时长</param>
        /// <param name="rid">分区</param>
        /// <param name="pn">页码</param>
        /// <param name="ps">页数</param>
        /// <returns></returns>
        public ApiModel Search(string keyword, string order = "", int duration = 0, int rid = 0, int pn = 1, int ps = 20)
        {
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://app.bilibili.com/x/v2/search",
                parameter = ApiHelper.MustParameter(ApiHelper.AndroidKey, true) + "&fnval=16&fnver=0&force_host=0&fourk=1&from_source=app_search&highlight=0&is_org_query=0&qn=112&recommend=1"
            };
            api.parameter += $"&keyword={Uri.EscapeDataString(keyword)}&local_time={TimeExtensions.GetTimestampS()}&pn={pn}&ps={ps}";
            if (string.IsNullOrEmpty(order))
            {
                api.parameter += $"&order={order}";
            }
            if (duration != 0)
            {
                api.parameter += $"&duration={duration}";
            }
            if (rid != 0)
            {
                api.parameter += $"&rid={rid}";
            }
            api.parameter += ApiHelper.GetSign(api.parameter, ApiHelper.AndroidKey);
            return api;
        }

        public ApiModel WebSearchVideo(string keyword, int pn = 1, string order = "", string duration = "", string region = "0", string area = "")
        {
            var baseUrl = ApiHelper.API_BASE_URL;
            if (!string.IsNullOrEmpty(area))
            {
                baseUrl = area.ChooseProxyServer();
            }
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                need_cookie = true,
                baseUrl = $"{baseUrl}/x/web-interface/search/type",
                parameter = $"context=&search_type=video&page={pn}&order={order}&keyword={Uri.EscapeDataString(keyword)}&duration={duration}&category_id=&tids_2=&__refresh__=true&tids={region}&highlight=1&single_column=0"
            };
            if (!string.IsNullOrEmpty(area))
            {
                api.parameter += $"&area={area}";
                api.ExtraCookies = new Dictionary<string, string>() { { "buvid3", "temp" } };
            }
            return api;
        }
        public ApiModel WebSearchAnime(string keyword, int pn = 1, string area = "")
        {
            var baseUrl = ApiHelper.API_BASE_URL;
            if (!string.IsNullOrEmpty(area))
            {
                baseUrl = area.ChooseProxyServer();
            }
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                need_cookie = true,
                baseUrl = $"{baseUrl}/x/web-interface/search/type",
                parameter = $"context=&search_type=media_bangumi&page={pn}&order=&keyword={Uri.EscapeDataString(keyword)}&category_id=&__refresh__=true&highlight=1&single_column=0"
            };
            if (!string.IsNullOrEmpty(area))
            {
                api.parameter += $"&area={area}";
                api.ExtraCookies = new Dictionary<string, string>() { { "buvid3", "temp" } };
            }
            return api;
        }
        public ApiModel WebSearchMovie(string keyword, int pn = 1, string area = "")
        {
            var baseUrl = ApiHelper.API_BASE_URL;
            if (!string.IsNullOrEmpty(area))
            {
                baseUrl = area.ChooseProxyServer();
            }
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                need_cookie = true,
                baseUrl = $"{baseUrl}/x/web-interface/search/type",
                parameter = $"context=&search_type=media_ft&page={pn}&order=&keyword={Uri.EscapeDataString(keyword)}&category_id=&__refresh__=true&highlight=1&single_column=0"
            };
            if (!string.IsNullOrEmpty(area))
            {
                api.parameter += $"&area={area}";
                api.ExtraCookies = new Dictionary<string, string>() { { "buvid3", "temp" } };
            }
            return api;
        }
        public ApiModel WebSearchUser(string keyword, int pn = 1, string order = "&order=&order_sort=", string type = "&user_type=", string area = "")
        {
            var baseUrl = ApiHelper.API_BASE_URL;
            if (!string.IsNullOrEmpty(area))
            {
                baseUrl = area.ChooseProxyServer();
            }
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                need_cookie = true,
                baseUrl = $"{baseUrl}/x/web-interface/search/type",
                parameter = $"context=&search_type=bili_user&page={pn}&keyword={Uri.EscapeDataString(keyword)}{order}{type}&__refresh__=true&changing=mid&highlight=1&single_column=0&category_id="
            };
            if (!string.IsNullOrEmpty(area))
            {
                api.parameter += $"&area={area}";
                api.ExtraCookies = new Dictionary<string, string>() { { "buvid3", "temp" } };
            }
            return api;
        }
        public ApiModel WebSearchLive(string keyword, int pn = 1, string area = "")
        {
            var baseUrl = ApiHelper.API_BASE_URL;
            if (!string.IsNullOrEmpty(area))
            {
                baseUrl = area.ChooseProxyServer();
            }
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                need_cookie = true,
                baseUrl = $"{baseUrl}/x/web-interface/search/type",
                parameter = $"context=&search_type=live&cover_type=user_cover&page={pn}&keyword={Uri.EscapeDataString(keyword)}&__refresh__=true&changing=mid&highlight=1&single_column=0"
            };
            if (!string.IsNullOrEmpty(area))
            {
                api.parameter += $"&area={area}";
                api.ExtraCookies = new Dictionary<string, string>() { { "buvid3", "temp" } };
            }
            return api;
        }
        public ApiModel WebSearchArticle(string keyword, int pn = 1, string order = "totalrank", string region = "0", string area = "")
        {
            var baseUrl = ApiHelper.API_BASE_URL;
            if (!string.IsNullOrEmpty(area))
            {
                baseUrl = area.ChooseProxyServer();
            }
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                need_cookie = true,
                baseUrl = $"{baseUrl}/x/web-interface/search/type",
                parameter = $"context=&search_type=article&page={pn}&order={order}&keyword={Uri.EscapeDataString(keyword)}&category_id={region}&__refresh__=true&highlight=1&single_column=0"
            };
            if (!string.IsNullOrEmpty(area))
            {
                api.parameter += $"&area={area}";
                api.ExtraCookies = new Dictionary<string, string>() { { "buvid3", "temp" } };
            }
            return api;
        }
        public ApiModel WebSearchTopic(string keyword, int pn = 1, string area = "")
        {
            var baseUrl = ApiHelper.API_BASE_URL;
            if (!string.IsNullOrEmpty(area))
            {
                baseUrl = area.ChooseProxyServer();
            }
            ApiModel api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                need_cookie = true,
                baseUrl = $"{baseUrl}/x/web-interface/search/type",
                parameter = $"context=&search_type=topic&page={pn}&order=&keyword={Uri.EscapeDataString(keyword)}&category_id=&__refresh__=true&highlight=1&single_column=0"
            };
            if (!string.IsNullOrEmpty(area))
            {
                api.parameter += $"&area={area}";
                api.ExtraCookies = new Dictionary<string, string>() { { "buvid3", "temp" } };
            }
            return api;
        }

        public ApiModel SearchSuggest(string content)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://s.search.bilibili.com/main/suggest",
                parameter = $"term={content}&main_ver=v1"
            };
            return api;
        }
    }
}
