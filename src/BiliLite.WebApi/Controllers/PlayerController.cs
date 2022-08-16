using Microsoft.AspNetCore.Mvc;
using System;

namespace BiliLite.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerController : Controller
    {
        const string TEMPLATE = @"<MPD xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" 
xmlns=""urn:mpeg:dash:schema:mpd:2011"" 
xsi:schemaLocation=""urn:mpeg:dash:schema:mpd:2011 http://standards.iso.org/ittf/PubliclyAvailableStandards/MPEG-DASH_schema_files/DASH-MPD.xsd"" 
type=""static"" 
mediaPresentationDuration=""$Duration"" 
timeShiftBufferDepth=""PT1S"" minimumUpdatePeriod=""PT1H"" maxSegmentDuration=""PT3S"" minBufferTime=""PT1S"" 
profiles=""urn:mpeg:dash:profile:isoff-live:2011,urn:com:dashif:dash264"">
  <Period id=""1"" start=""PT0S"">
    <AdaptationSet group=""1"" mimeType=""audio/mp4"" segmentAlignment=""true"">
      <Representation id=""$AudioID"" bandwidth=""$AudioBandwidth"" codecs=""$AudioCodec"" >
        <SegmentTemplate duration=""$Length"" media=""$AudioUrl"" startNumber=""1""/>
      </Representation>
    </AdaptationSet>
    <AdaptationSet group=""2"" mimeType=""video/mp4""  segmentAlignment=""true"" startWithSAP=""1"">
      <Representation id=""$VideoID"" frameRate=""$VideoFrameRate"" bandwidth=""$VideoBandwidth"" codecs=""$VideoCodec"" width=""$VideoWidth"" height=""$VideoHeight"">
        <SegmentTemplate timescale=""1000"" duration=""$Length"" media=""$VideoUrl""  startNumber=""1""/>
      </Representation>
    </AdaptationSet>
  </Period>
</MPD>";
        [HttpGet]
        [Route("GenerateMPD")]
        public IActionResult GenerateMPD(string par)
        {
            var dash=Newtonsoft.Json.JsonConvert.DeserializeObject<DashModel>(par);
            dash.AudioUrl=System.Web.HttpUtility.HtmlEncode(dash.AudioUrl);
            dash.VideoUrl = System.Web.HttpUtility.HtmlEncode(dash.VideoUrl);
            var ts = "PT" + TimeSpan.FromMilliseconds(dash.DurationMS).ToString(@"hh\Hmm\Mss\S");
            var content = TEMPLATE
                .Replace("$Duration", ts)
                .Replace("$AudioID", dash.AudioID)
                .Replace("$AudioBandwidth", dash.AudioBandwidth)
                .Replace("$AudioCodec", dash.AudioCodec)
                .Replace("$Length", dash.Duration.ToString())
                .Replace("$AudioUrl", dash.AudioUrl)
                .Replace("$VideoID", dash.VideoID)
                .Replace("$VideoFrameRate", dash.VideoFrameRate)
                .Replace("$VideoBandwidth", dash.VideoBandwidth)
                .Replace("$VideoCodec", dash.VideoCodec)
                .Replace("$VideoWidth", dash.VideoWidth.ToString())
                .Replace("$VideoHeight", dash.VideoHeight.ToString())
                .Replace("$VideoUrl", dash.VideoUrl);
            return new ContentResult()
            {
                Content = content,
                ContentType = "application/dash+xml",
                StatusCode = 200,
            };
        }
    }
    public class DashModel
    {
        public long Duration { get; set; }
        public long DurationMS { get; set; }
        public string AudioID { get; set; }
        public string AudioBandwidth { get; set; }
        public string AudioCodec { get; set; }
        public string AudioUrl { get; set; }

        public string VideoID { get; set; }
        public string VideoBandwidth { get; set; }
        public string VideoCodec { get; set; }
        public string VideoUrl { get; set; }
        public string VideoFrameRate { get; set; }
        public int VideoWidth { get; set; }
        public int VideoHeight { get; set; }
    }
}
