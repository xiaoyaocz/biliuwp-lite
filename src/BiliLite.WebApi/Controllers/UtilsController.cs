using BiliLite.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BiliLite.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilsController : ControllerBase
    {
        private readonly UtilsService m_utilsService;

        public UtilsController(UtilsService utilsService)
        {
            m_utilsService = utilsService;
        }

        [HttpGet("BuildCorrespondPath")]
        public ActionResult<string> BuildCorrespondPath([FromQuery]long timestamp)
        {
            var result = m_utilsService.BuildCorrespondPath(timestamp);
            return Ok(result);
        }
    }
}
