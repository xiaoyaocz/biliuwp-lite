using BiliLite.Api;
using BiliLite.Controls;
using BiliLite.Helpers;
using BiliLite.Models;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Toolkit.Uwp.UI.Animations.Behaviors;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Services.Maps;
using ZXing;

namespace BiliLite.Modules.Player.Playurl
{

    public class BiliPlayUrl
    {
        public bool IsDownload { get; set; } = false;
        public BiliPlayUrl(bool isDownload)
        {
            IsDownload = isDownload;
        }

        public async Task<BiliPlayUrlQualitesInfo> GetPlayUrl(PlayInfo playInfo, int qualityID)
        {
            BiliPlayUrlRequest request;
            if (playInfo.play_mode == VideoPlayType.Season)
            {
                request = new BiliSeasonPlayUrlRequest(IsDownload);
            }
            else
            {
                request = new BiliVideoPlayUrlRequest(IsDownload);
            }
            return await request.GetPlayUrlInfo(playInfo, qualityID);
        }

    }
  
    

}