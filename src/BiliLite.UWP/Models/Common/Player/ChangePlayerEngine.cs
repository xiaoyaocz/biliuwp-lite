namespace BiliLite.Models.Common.Player
{
    public class ChangePlayerEngine
    {
        public bool need_change { get; set; }
        /// <summary>
        /// 当前引擎
        /// </summary>
        public PlayEngine current_mode { get; set; }
        /// <summary>
        /// 更换引擎
        /// </summary>
        public PlayEngine change_engine { get; set; }

        public PlayMediaType play_type { get; set; }
        public string message { get; set; }
    }
}