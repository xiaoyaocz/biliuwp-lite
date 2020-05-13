using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BiliLite.WebApi.Models;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BiliLite.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CinemaController : ControllerBase
    {
        readonly IDistributedCache distributedCache;
        readonly IHttpClientFactory client;
        public CinemaController(IHttpClientFactory httpContext, IDistributedCache cache)
        {
            client = httpContext;
            distributedCache = cache;
        }
        [Route("Home")]
        public async Task<JsonResult> Cinema()
        {
            var value = distributedCache.GetString("CinemaHome");
            if (value != null)
            {
                return new JsonResult(new ApiModel<CinemaHomeModel>()
                {
                    code = 0,
                    message = "",
                    data = JsonConvert.DeserializeObject<CinemaHomeModel>(value)
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
                documentary =await GetHot(87),
                movie = await GetHot(88),
                tv = await GetHot(89),
                variety= await GetHot(173)
            };
            distributedCache.SetString("CinemaHome", JsonConvert.SerializeObject(datas), new DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddDays(1).Date)
            });
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
                    var img = item.SelectSingleNode("img");
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
                var key = "Falls" + wid + "Cursor" + cursor;
                var value = distributedCache.GetString(key);
                if (value != null)
                {
                    return JsonConvert.DeserializeObject<List<FallItemModel>>(value);
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
                    distributedCache.SetString(key, JsonConvert.SerializeObject(list), new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddHours(2))
                    });
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
        private async Task<List<CinemaHotItem>> GetHot(int oid)
        {

            try
            {
                var key = "CinemaHot"+ oid;
                var value = distributedCache.GetString(key);
                if (value != null)
                {
                    return JsonConvert.DeserializeObject<List<CinemaHotItem>>(value);
                }
                var http = client.CreateClient("http");
                var results = await http.GetStringAsync(Utils.GetSign( $"https://api.bilibili.com/pgc/app/v2/page/exchange?appkey=1d8b6e7d45233436&build=5442100&mobi_app=android&platform=android&oid={oid}&ts={ Utils.GetTimestampS()}&type=1"));
                var obj = JObject.Parse(results);
                List<CinemaHotItem> list = JsonConvert.DeserializeObject<List<CinemaHotItem>>(obj["result"].ToString());

                if (list!=null&&list.Count != 0)
                {
                    distributedCache.SetString(key, JsonConvert.SerializeObject(list), new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddDays(1).Date)
                    });
                }

                return list;
            }
            catch (Exception)
            {
                return new List<CinemaHotItem>();
            }


        }

        /// <summary>
        /// 热门推荐
        /// </summary>
        /// <returns></returns>
        private async Task<List<CinemaHotItem>> GetTime()
        {

            try
            {
                var key = "CinemaTime";
                var value = distributedCache.GetString(key);
                if (value != null)
                {
                    return JsonConvert.DeserializeObject<List<CinemaHotItem>>(value);
                }
                var http = client.CreateClient("http");
                var results = await http.GetStringAsync(Utils.GetSign($"https://api.bilibili.com/pgc/app/v2/page/cinema/tab?appkey=1d8b6e7d45233436&build=5442100&mobi_app=android&platform=android&ts={ Utils.GetTimestampS()}"));
                var obj = JObject.Parse(results);
                List<CinemaHotItem> list = JsonConvert.DeserializeObject<List<CinemaHotItem>>(obj["result"]["modules"].FirstOrDefault(x=>x["title"].ToString() == "即将开播")["items"].ToString());

                if (list != null && list.Count != 0)
                {
                    distributedCache.SetString(key, JsonConvert.SerializeObject(list), new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddDays(1).Date)
                    });
                }

                return list;
            }
            catch (Exception)
            {
                return new List<CinemaHotItem>();
            }


        }
    }

    public class CinemaHomeModel
    {
        public List<BannerModel> banners { get; set; }
        public List<FallModel> falls { get; set; }
        public List<CinemaHotItem> update { get; set; }
        /// <summary>
        /// 记录片 87
        /// </summary>
        public List<CinemaHotItem> documentary { get; set; }
        /// <summary>
        /// 电影 88
        /// </summary>
        public List<CinemaHotItem> movie { get; set; }
        /// <summary>
        /// 电视剧 89
        /// </summary>
        public List<CinemaHotItem> tv { get; set; }
        /// <summary>
        /// 综艺 173
        /// </summary>
        public List<CinemaHotItem> variety { get; set; }
    }

    public class CinemaHotItem
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
        public CinemaStatModel stat { get; set; }
    }
    public class CinemaStatModel
    {
        public int view { get; set; }
        public string follow_view { get; set; }
        public int follow { get; set; }
        public int danmaku { get; set; }
    }

}