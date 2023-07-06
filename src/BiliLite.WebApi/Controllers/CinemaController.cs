using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BiliLite.WebApi.Models;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BiliLite.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CinemaController : ControllerBase
    {
        readonly IDistributedCache distributedCache;
        private readonly IMemoryCache m_memoryCache;
        readonly IHttpClientFactory client;
        public CinemaController(IHttpClientFactory httpContext,  IMemoryCache memoryCache)
        {
            client = httpContext;
            m_memoryCache = memoryCache;
        }
        [Route("Home")]
        public async Task<JsonResult> Cinema()
        {
            var cacheKey = "CinemaHome";
            var getCacheSuccess = m_memoryCache.TryGetValue(cacheKey, out var value);
            if (getCacheSuccess && value != null)
            {
                var cacheResult = value as CinemaHomeModel;
                return new JsonResult(new ApiModel<CinemaHomeModel>()
                {
                    code = 0,
                    message = "",
                    data = cacheResult
                });
            }

            var banners = await GetBanner();
            var datas = new CinemaHomeModel()
            {
                banners = banners,
                falls = new List<FallModel>() {
                    new FallModel(){
                        title="独家策划",
                        wid=117,
                        items=await GetFalls(117)
                    },
                },
                update=await GetTime(),
                movie = await GetHot(2),
                documentary =await GetHot(3),
                tv = await GetHot(5),
                variety= await GetHot(7)
            };
            m_memoryCache.Set(cacheKey, datas, TimeSpan.FromHours(2));
            return new JsonResult(new ApiModel<CinemaHomeModel>()
            {
                code = 0,
                message = "",
                data = datas
            });
        }
        [Route("Falls")]
        public async Task<JsonResult> BangumiFalls(int wid, long cursor = 0)
        {
            return new JsonResult(await GetFalls(wid, cursor));
        }
        private async Task<List<BannerModel>> GetBanner()
        {
            try
            {
                var http = client.CreateClient("http");
                var results = await http.GetStringAsync("https://www.bilibili.com/cinema");
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(results);
                var node = doc.DocumentNode.SelectSingleNode("//ul[@class='carou-images-wrapper clearfix']");
                var nodes = node.SelectNodes("li[@class='chief-recom-item']/a");
                List<BannerModel> banners = new List<BannerModel>();
                foreach (var item in nodes)
                {
                    var img = item.SelectSingleNode("picture/img");
                    banners.Add(new BannerModel()
                    {
                        img = img.Attributes["src"].Value.Split('@')[0].Replace("//", "https://"),
                        title = img.Attributes["alt"].Value,
                        url = item.Attributes["href"].Value
                    });

                }

                return banners.Distinct(new Compare()).ToList();
            }
            catch (Exception)
            {

                return new List<BannerModel>();
            }

        }
        private async Task<List<FallItemModel>> GetFalls(int wid, long cursor = 0)
        {

            try
            {
                var cacheKey = "Falls" + wid + "Cursor" + cursor;
                var getCacheSuccess = m_memoryCache.TryGetValue(cacheKey, out var value);
                if (getCacheSuccess && value != null)
                {
                    var cacheResult = value as List<FallItemModel>;
                    return cacheResult;
                }

                var http = client.CreateClient("http");
                var results = await http.GetStringAsync($"https://bangumi.bilibili.com/api/fall?appkey=1d8b6e7d45233436&build=5442100&cursor={ cursor}&mobi_app=android&pagesize=4&platform=android&ts={ Utils.GetTimestampS()}&wid={wid}");
                var obj = JObject.Parse(results);
                List<FallItemModel> list = new List<FallItemModel>();
                foreach (var item in obj["result"])
                {
                    list.Add(new FallItemModel()
                    {
                        cover = item["cover"].ToString(),
                        title = item["title"].ToString(),
                        desc = item["desc"] == null ? "" : item["desc"].ToString(),
                        link = item["link"].ToString(),
                        cursor = Convert.ToInt64(item["cursor"]),
                        wid = item["wid"].ToInt32(),
                    });
                }
                if (list.Count != 0)
                {
                    m_memoryCache.Set(cacheKey, list, TimeSpan.FromHours(1));
                }

                return list;
            }
            catch (Exception)
            {
                return new List<FallItemModel>();
            }


        }
        /// <summary>
        /// 热门推荐
        /// </summary>
        /// <returns></returns>
        private async Task<List<CinemaSeasonItem>> GetHot(int oid)
        {

            try
            {
                var cacheKey = "CinemaHot" + oid;
                var getCacheSuccess = m_memoryCache.TryGetValue(cacheKey, out var value);
                if (getCacheSuccess && value != null)
                {
                    var cacheResult = value as List<CinemaSeasonItem>;
                    return cacheResult;
                }

                var http = client.CreateClient("http");
                var results = await http.GetStringAsync( $"https://api.bilibili.com/pgc/season/rank/web/list?day=3&season_type={oid}");
                var obj = JObject.Parse(results);
                List<CinemaSeasonItem> list = JsonConvert.DeserializeObject<List<CinemaSeasonItem>>(obj["data"]["list"].ToString());
                list = list.Take(36).ToList();
                foreach (var item in list)
                {
                    if (oid == 5 || oid == 7)
                    {
                        item.cover = item.new_ep.cover;
                    }
                    if (oid == 2)
                    {
                        item.desc = item.new_ep.index_show;
                    }
                    item.stat.follow_view = item.stat.follow.NumberToString()+"人追剧";
                }
                if (list != null && list.Count != 0)
                {
                    list = list.Take(36).ToList();
                    m_memoryCache.Set(cacheKey, list, TimeSpan.FromHours(2));
                }

                return list;
            }
            catch (Exception)
            {
                return new List<CinemaSeasonItem>();
            }


        }

        /// <summary>
        /// 即将上线
        /// </summary>
        /// <returns></returns>
        private async Task<List<CinemaSeasonItem>> GetTime()
        {

            try
            {
                var cacheKey = "CinemaTime";
                var getCacheSuccess = m_memoryCache.TryGetValue(cacheKey, out var value);
                if (getCacheSuccess && value != null)
                {
                    var cacheResult = value as List<CinemaSeasonItem>;
                    return cacheResult;
                }

                var http = client.CreateClient("http");
                var results = await http.GetStringAsync($"https://api.bilibili.com/pgc/web/timeline/online?type=1");
                var obj = JObject.Parse(results);
                List<CinemaSeasonItem> list = JsonConvert.DeserializeObject<List<CinemaSeasonItem>>(obj["result"]["items"].ToString().Replace("follower", "follow"));
                foreach (var item in list)
                {
                    item.hat = item.desc;
                    item.stat.follow_view = item.stat.follow.NumberToString()+ "人追剧";
                }
                if (list != null && list.Count != 0)
                {
                    m_memoryCache.Set(cacheKey, list, TimeSpan.FromHours(2));
                }

                return list;
            }
            catch (Exception)
            {
                return new List<CinemaSeasonItem>();
            }


        }
    }

    public class CinemaHomeModel
    {
        public List<BannerModel> banners { get; set; }
        public List<FallModel> falls { get; set; }
        public List<CinemaSeasonItem> update { get; set; }
        /// <summary>
        /// 记录片 3
        /// </summary>
        public List<CinemaSeasonItem> documentary { get; set; }
        /// <summary>
        /// 电影 2
        /// </summary>
        public List<CinemaSeasonItem> movie { get; set; }
        /// <summary>
        /// 电视剧 5
        /// </summary>
        public List<CinemaSeasonItem> tv { get; set; }
        /// <summary>
        /// 综艺 7
        /// </summary>
        public List<CinemaSeasonItem> variety { get; set; }
    }



    public class CinemaSeasonItem
    {
        public string hat { get; set; }
        public string cover { get; set; }
        public string badge { get; set; }
        public int badge_type { get; set; }
        public string desc { get; set; }
        public string title { get; set; }
        public string link { get; set; }
        public int season_id { get; set; }
        public int season_type { get; set; }
        public string type { get; set; }
        public int wid { get; set; }
        public CinemaNewEPModel new_ep { get; set; }
        public CinemaStatModel stat { get; set; }
    }
    public class CinemaStatModel
    {
        public int view { get; set; }
        public string follow_view { get; set; }
        public int follow { get; set; }
        public int danmaku { get; set; }
    }
    public class CinemaNewEPModel
    {
        public string cover { get; set; }
        public string index_show { get; set; }
    }

}