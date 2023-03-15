using BiliLite.Controls;
using System.Threading.Tasks;

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