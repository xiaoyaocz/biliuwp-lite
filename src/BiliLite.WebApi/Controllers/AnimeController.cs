using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BiliLite.WebApi.Models;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;

namespace BiliLite.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnimeController : ControllerBase
    {
        readonly IHttpClientFactory client;
        private readonly IMemoryCache m_memoryCache;
        public AnimeController(IHttpClientFactory httpContext, IMemoryCache memoryCache)
        {
            client = httpContext;
            m_memoryCache = memoryCache;
        }

        [Route("Bangumi")]
        public async Task<JsonResult> Anime()
        {
            var cacheKey = "AnimeHome";
            var getCacheSuccess = m_memoryCache.TryGetValue(cacheKey, out var value);
            if (getCacheSuccess && value!=null)
            {
                var cacheResult = value as BangumiHomeModel;
                return new JsonResult(new ApiModel<BangumiHomeModel>()
                {
                    code = 0,
                    message = "",
                    data = cacheResult
                });
            }

            var banners = await GetBanner(1);
            var ranks = await GetRanks(1);
            var timelines = await GetTimeLine(1);
            var datas = new BangumiHomeModel()
            {
                banners = banners,
                ranks = ranks,
                today = timelines.FirstOrDefault(x => x.is_today).seasons,
                hots = await GetHots(1),
                falls = new List<FallModel>() {
                    new FallModel(){
                        title="精彩推荐",
                        wid=81,
                        items=await GetFalls(81)
                    },
                     new FallModel(){
                        title="资讯档",
                        wid=78,
                        items=await GetFalls(78)
                    },
                      new FallModel(){
                        title="周末剧场",
                        wid=81,
                        items=await GetFalls(79)
                    },
                       new FallModel(){
                        title="泡面档",
                        wid=80,
                        items=await GetFalls(80)
                    },
                }
            };
            if (datas.ranks.Count != 0 && datas.banners.Count != 0)
            {
                m_memoryCache.Set(cacheKey, datas, TimeSpan.FromHours(2));
            }
            return new JsonResult(new ApiModel<BangumiHomeModel>()
            {
                code = 0,
                message = "",
                data = datas
            });
        }

        [Route("Guochuang")]
        public async Task<JsonResult> Guochuang()
        {
            var cacheKey = "GuochuangHome";
            var getCacheSuccess = m_memoryCache.TryGetValue(cacheKey, out var value);
            if (getCacheSuccess == true && value != null)
            {
                var cacheResult = value as BangumiHomeModel;
                return new JsonResult(new ApiModel<BangumiHomeModel>()
                {
                    code = 0,
                    message = "",
                    data = cacheResult
                });
            }

            var banners = await GetBanner(4);
            var ranks = await GetRanks(4);
            var timelines = await GetTimeLine(4);
            var datas = new BangumiHomeModel()
            {
                banners = banners,
                ranks = ranks,
                today = timelines.FirstOrDefault(x => x.is_today).seasons,
                hots = await GetHots(4),
                falls = new List<FallModel>() {
                    new FallModel(){
                        title="编辑推荐",
                        wid=59,
                        items=await GetFalls(59)
                    }
                }
            };
            if (datas.ranks.Count != 0 && datas.banners.Count != 0)
            {
                m_memoryCache.Set(cacheKey, datas, TimeSpan.FromHours(2));
            }
            return new JsonResult(new ApiModel<BangumiHomeModel>()
            {
                code = 0,
                message = "",
                data = datas
            });
        }

        [Route("Timeline")]
        public async Task<JsonResult> Timeline(int type = 1)
        {
            var cacheKey = $"Timeline-{type}";
            var getCacheSuccess = m_memoryCache.TryGetValue(cacheKey, out var value);
            if (getCacheSuccess == true && value != null)
            {
                var cacheResult = value as BangumiTimeline;
                return new JsonResult(new ApiModel<BangumiTimeline>()
                {
                    code = 0,
                    message = "",
                    data = cacheResult
                });
            }

            var timelines = await GetTimeLine(type);

            if (timelines.Count != 0)
            {
                m_memoryCache.Set(cacheKey, timelines, TimeSpan.FromHours(2));
            }
            return new JsonResult(new ApiModel<List<BangumiTimeline>>()
            {
                code = 0,
                message = "",
                data = timelines
            });
        }


        [Route("BangumiFalls")]
        public async Task<JsonResult> BangumiFalls(int wid, long cursor = 0)
        {
            return new JsonResult(await GetFalls(wid, cursor));
        }

        private async Task<List<BannerModel>> GetBanner(int type = 1)
        {
            try
            {
                var http = client.CreateClient("http");
                var results = await http.GetStringAsync("https://www.bilibili.com/" + (type == 1 ? "anime" : "guochuang"));
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
        private async Task<List<AnimeRank>> GetRanks(int type = 1)
        {
            try
            {
                var http = client.CreateClient("http");
                var results = await http.GetStringAsync("https://api.bilibili.com/pgc/web/rank/list?day=3&season_type=" + type);
                var obj = JObject.Parse(results);
                List<AnimeRank> ranks = new List<AnimeRank>();
                foreach (var item in obj["result"]["list"].Take(20))
                {
                    ranks.Add(new AnimeRank()
                    {
                        display = item["stat"]["follow"].NumberToString() + "人追番",
                        badge = item["badge"].ToString(),
                        cover = item["cover"].ToString(),
                        season_id = Convert.ToInt32(item["season_id"]),
                        danmaku = Convert.ToInt32(item["stat"]["danmaku"]),
                        follow = Convert.ToInt32(item["stat"]["follow"]),
                        view = Convert.ToInt32(item["stat"]["view"]),
                        index_show = item["new_ep"]["index_show"].ToString(),
                        title = item["title"].ToString(),
                    });
                }
                return ranks;
            }
            catch (Exception)
            {

                return new List<AnimeRank>();
            }
        }
        private async Task<List<AnimeRank>> GetHots(int type = 1)
        {
            try
            {
                var http = client.CreateClient("http");
                var results = await http.GetStringAsync($"https://api.bilibili.com/pgc/season/index/result?st=1&order=2&season_version=-1&area=-1&is_finish=-1&copyright=-1&season_status=-1&season_month=-1&year=-1&style_id=-1&sort=0&page=1&season_type={ type }&pagesize=20&type=1");
                var obj = JObject.Parse(results);
                List<AnimeRank> ranks = new List<AnimeRank>();
                foreach (var item in obj["data"]["list"].Take(20))
                {
                    ranks.Add(new AnimeRank()
                    {
                        badge = item["badge"].ToString(),
                        cover = item["cover"].ToString(),
                        season_id = Convert.ToInt32(item["season_id"]),
                        display = item["order"].ToString(),
                        index_show = item["index_show"].ToString(),
                        title = item["title"].ToString(),
                    });
                }
                return ranks;
            }
            catch (Exception)
            {

                return new List<AnimeRank>();
            }
        }
        /// <summary>
        /// 时间表
        /// </summary>
        /// <param name="type">1为番剧，2为国创</param>
        /// <returns></returns>
        private async Task<List<BangumiTimeline>> GetTimeLine(int type)
        {
            try
            {
                var http = client.CreateClient("http");
                var results = await http.GetStringAsync("https://bangumi.bilibili.com/web_api/" + (type == 1 ? "timeline_global" : "timeline_cn"));
                var obj = JObject.Parse(results);
                List<BangumiTimeline> timelines = new List<BangumiTimeline>();
                foreach (var item in obj["result"])
                {
                    var data = new BangumiTimeline()
                    {
                        date = item["date"].ToString(),
                        day_week = Convert.ToInt32(item["day_of_week"]),
                        is_today = Convert.ToInt32(item["is_today"]) == 1,
                    };
                    List<BangumiTimelineItem> seasons = new List<BangumiTimelineItem>();
                    foreach (var item2 in item["seasons"])
                    {

                        var pub_index = "";
                        if (Convert.ToInt32(item2["delay"]) == 1)
                        {
                            pub_index = item2["delay_reason"].ToString();
                        }
                        else
                        {
                            pub_index = item2["pub_index"].ToString();
                        }
                        seasons.Add(new BangumiTimelineItem()
                        {
                            cover = item2["cover"].ToString(),
                            pub_index = pub_index,
                            pub_time = item2["pub_time"].ToString(),
                            season_id = Convert.ToInt32(item2["season_id"]),
                            square_cover = item2["square_cover"].ToString(),
                            title = item2["title"].ToString()
                        });
                    }
                    data.seasons = seasons;
                    timelines.Add(data);
                }
                //var index = timelines.IndexOf(timelines.FirstOrDefault(x => x.day_week == 1));
                return timelines;//.Skip(index).Take(7).ToList();
            }
            catch (Exception ex)
            {
                return new List<BangumiTimeline>();
            }
        }

        private async Task<List<FallItemModel>> GetFalls(int wid, long cursor = 0)
        {

            try
            {
                var cacheKey = "Falls" + wid + "Cursor" + cursor;
                var getCacheSuccess = m_memoryCache.TryGetValue(cacheKey, out var value);
                if (getCacheSuccess == true && value != null)
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

    }
    public class Compare : IEqualityComparer<BannerModel>
    {
        public bool Equals(BannerModel x, BannerModel y)
        {
            return x.url==y.url ;
        }
        public int GetHashCode(BannerModel obj)
        {
            return obj.url.GetHashCode();
        }
    }
    public class BangumiHomeModel
    {
        public List<AnimeRank> hots { get; set; }
        public List<BannerModel> banners { get; set; }
        public List<AnimeRank> ranks { get; set; }
        public List<BangumiTimelineItem> today { get; set; }
        public List<FallModel> falls { get; set; }
    }
    public class FallModel
    {
        public int wid { get; set; }
        public string title { get; set; }
        public List<FallItemModel> items { get; set; }
    }
    public class FallItemModel
    {
        public string cover { get; set; }
        public string desc { get; set; }
        public string title { get; set; }
        public string link { get; set; }
        public long cursor { get; set; }
        public int wid { get; set; }
    }
    public class BannerModel
    {
        public string title { get; set; }
        public string img { get; set; }
        public string url { get; set; }
    }
    public class AnimeRank
    {
        public string display { get; set; }
        public string title { get; set; }
        public string cover { get; set; }
        public int season_id { get; set; }
        public string index_show { get; set; }
        public int follow { get; set; }
        public int danmaku { get; set; }
        public int view { get; set; }
        public bool show_badge
        {
            get
            {
                return !string.IsNullOrEmpty(badge);
            }
        }
        public string badge { get; set; }
    }

    public class BangumiTimeline
    {
        public int day_week { get; set; }
        public string date { get; set; }
        public bool is_today { get; set; }
        public string week
        {
            get
            {
                switch (day_week)
                {

                    case 1:
                        return "周一";
                    case 2:
                        return "周二";
                    case 3:
                        return "周三";
                    case 4:
                        return "周四";
                    case 5:
                        return "周五";
                    case 6:
                        return "周六";
                    case 7:
                        return "周日";
                    default:
                        return "未知";
                }
            }
        }
        public List<BangumiTimelineItem> seasons { get; set; }
    }
    public class BangumiTimelineItem
    {
        public int season_id { get; set; }
        public string cover { get; set; }
        public string square_cover { get; set; }
        public string pub_index { get; set; }
        public string pub_time { get; set; }
        public string title { get; set; }
    }
}