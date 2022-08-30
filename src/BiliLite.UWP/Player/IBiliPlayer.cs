using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.Player
{
    public enum PlayerAspectRatio
    {
        /// <summary>
        /// 适应
        /// </summary>
        Uniform,
        /// <summary>
        /// 铺满屏幕
        /// </summary>
        Fill,
        /// <summary>
        /// 保持16：9
        /// </summary>
        S16_9,
        /// <summary>
        /// 保持4：3
        /// </summary>
        S4_3,
    }
    public enum PlayerState
    {
        /// <summary>
        /// 加载中
        /// </summary>
        Loading,
        /// <summary>
        /// 播放中
        /// </summary>
        Playing,
        /// <summary>
        /// 暂停
        /// </summary>
        Pause,
        /// <summary>
        /// 播放结束
        /// </summary>
        End,
        /// <summary>
        /// 播放错误
        /// </summary>
        Error
    }
    public interface IBiliPlayer
    {
        /// <summary>
        /// 播放进度变更
        /// </summary>
        event EventHandler<double> PositionChanged;
        /// <summary>
        /// 文件打开完毕
        /// </summary>
        event EventHandler MediaOpened;
        /// <summary>
        /// 文件播放有误
        /// </summary>
        event EventHandler<string> MediaError;
        /// <summary>
        /// 缓冲中
        /// </summary>
        event EventHandler<double> PlayBuffering;
        /// <summary>
        /// 缓冲完毕
        /// </summary>
        event EventHandler PlayBufferEnd;
        /// <summary>
        /// 播放状态变更
        /// </summary>
        event EventHandler<PlayerState> PlayStateChanged;
        /// <summary>
        /// 当前播放状态
        /// </summary>
        PlayerState PlayState { get; set; }
        /// <summary>
        /// 时长
        /// </summary>
        TimeSpan Duration { get; set; }
        /// <summary>
        /// 进度
        /// </summary>
        TimeSpan Position { get; set; }
        /// <summary>
        /// 媒体信息
        /// </summary>
        string MediaInfo { get; set; }
        /// <summary>
        /// 播放速率
        /// </summary>
        double Rate { get; set; }
        /// <summary>
        /// 宽高比
        /// </summary>
        PlayerAspectRatio AspectRatio { get; set; }
        /// <summary>
        /// 播放
        /// </summary>
         void Play();
        /// <summary>
        /// 暂停
        /// </summary>
        void Pause();
        /// <summary>
        /// 停止
        /// </summary>
        void Stop();
        /// <summary>
        /// 跳转进度到指定秒数
        /// </summary>
        /// <param name="position_s"></param>
        void Seek(double position_s);
        /// <summary>
        /// 设置倍速
        /// </summary>
        /// <param name="rate"></param>
        void SetRate(double rate);
        /// <summary>
        /// 设置视频音量
        /// </summary>
        void SetVolume(double volume);
        /// <summary>
        /// 截图
        /// </summary>
        void TakeScreenshot();
        /// <summary>
        /// 设置画面比例
        /// </summary>
        /// <param name="stretch"></param>
        void SetRatio(PlayerAspectRatio stretch);
        /// <summary>
        /// 设置指定文件
        /// </summary>
        /// <param name="media"></param>
        /// <param name="autoPlay"></param>
        /// <returns></returns>
        bool SetMedia(MediaItem media,bool autoPlay=false);

    }
    public enum MediaType
    {
        /// <summary>
        /// 单文件
        /// </summary>
        SingleFile,
        /// <summary>
        /// 音视频分离
        /// </summary>
        Separation,
        /// <summary>
        /// 直播
        /// </summary>
        Live,
        /// <summary>
        /// 多文件
        /// </summary>
        MultipleFile,
    }
    public class MediaItem
    {
        public string VideoUrl { get; set; }
        public string AudioUrl { get; set; }
        public List<string> Urls { get; set; }
        public string HttpReferrer { get; set; }
        public string HttpUserAgent { get; set; }
        public MediaType Type { get; set; }
    }

}
