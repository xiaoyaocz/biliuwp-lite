using System.Collections.Generic;
using BiliLite.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace BiliLite.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RankController : ControllerBase
    {
        public RankController()
        {
        }

        [Route("RankRegion")]
        public JsonResult RankRegion()
        {
            return new JsonResult(new ApiModel<List<RankRegionModel>>()
            {
                code=0,
                message="",
                data=new List<RankRegionModel>()
                {
                    new RankRegionModel(0,"全站"),
                    new RankRegionModel(1,"动画"),
                    new RankRegionModel(168,"国创相关"),
                    new RankRegionModel(3,"音乐"),
                    new RankRegionModel(129,"舞蹈"),
                    new RankRegionModel(4,"游戏"),
                    new RankRegionModel(36,"科技"),
                    new RankRegionModel(188,"数码"),
                    new RankRegionModel(160,"生活"),
                    new RankRegionModel(119,"鬼畜"),
                    new RankRegionModel(155,"时尚"),
                    new RankRegionModel(5,"娱乐"),
                    new RankRegionModel(181,"影视"),
                    new RankRegionModel(211,"美食"),
                }
            });
        }
       
       


    }
    public class RankRegionModel
    {
        public RankRegionModel(int id,string rname)
        {
            rid = id;
            name = rname;
        }
        public string name { get; set; }
        public int rid { get; set; }
    }

  
}