using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.Helpers
{
    public class VideoPlayHistoryHelper
    {
        private const int ABPLAY_HISTORES_MAX = 200;
        private static Dictionary<string, ABPlayHistoryEntry> ABPlayHistories;

        public class ABPlayHistoryEntry {
            public double PointA { get; set; }
            public double PointB { get; set; } = double.MaxValue;
        }

        /// <summary>
        /// 加载AB播放记录
        /// </summary>
        /// <param name="cleanup">是否顺带清理一下历史记录</param>
        public static void LoadABPlayHistories(bool cleanup)
        {
            ABPlayHistories = SettingHelper.GetValue(SettingHelper.Player.PLAYER_ABPLAY_HISTORIES, new Dictionary<string, ABPlayHistoryEntry>(ABPLAY_HISTORES_MAX));

            if (ABPlayHistories.Count > ABPLAY_HISTORES_MAX && cleanup)
            {
                int toDelete = ABPlayHistories.Count - ABPLAY_HISTORES_MAX;

                for (int i = 0; i < toDelete; i++)
                {
                    ABPlayHistories.Remove(ABPlayHistories.Keys.First());
                }

                SettingHelper.SetValue(SettingHelper.Player.PLAYER_ABPLAY_HISTORIES, ABPlayHistories);
            }
        }

        /// <summary>
        /// 查找视频的AB播放区间
        /// </summary>
        /// <param name="videoID"></param>
        /// <returns>null 如果没有找到</returns>
        public static ABPlayHistoryEntry FindABPlayHistory(Controls.PlayInfo info)
        {
            return ABPlayHistories.GetValueOrDefault(info.season_id != 0 ? "ep" + info.ep_id : info.cid, null);
        }

        /// <summary>
        /// 设置AB播放历史
        /// </summary>
        /// <param name="info"></param>
        /// <param name="history">null移除AB播放历史</param>
        public static async void SetABPlayHistory(Controls.PlayInfo info, ABPlayHistoryEntry history)
        {
            if (history == null)
            {
                ABPlayHistories.Remove(info.season_id != 0 ? "ep" + info.ep_id : info.cid);
            } else
            {
                ABPlayHistories[info.season_id != 0 ? "ep" + info.ep_id : info.cid] = history;
            }

            //TODO: 这种信息或许用数据库存更合适
            await Task.Run(() => SettingHelper.SetValue(SettingHelper.Player.PLAYER_ABPLAY_HISTORIES, ABPlayHistories));
        }
    }
}
