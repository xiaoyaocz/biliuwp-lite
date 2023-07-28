using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using System.ComponentModel;
using BiliLite.Models.Common;
using BiliLite.Services;
using BiliLite.Extensions;
/*
* 参考文档:
* https://github.com/lovelyyoshino/Bilibili-Live-API/blob/master/API.WebSocket.md
* 
*/

namespace BiliLite.Modules.Live
{
    /// <summary>
    /// 消息类型
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// 连接成功
        /// </summary>
        ConnectSuccess,
        /// <summary>
        /// 在线人数
        /// </summary>
        Online,
        /// <summary>
        /// 弹幕
        /// </summary>
        Danmu,
        /// <summary>
        /// 赠送礼物
        /// </summary>
        Gift,
        /// <summary>
        /// 欢迎信息
        /// </summary>
        Welcome,
        /// <summary>
        /// 系统消息
        /// </summary>
        SystemMsg,
        /// <summary>
        /// 醒目留言
        /// </summary>
        SuperChat,
        /// <summary>
        /// 醒目留言（日文）
        /// </summary>
        SuperChatJpn,
        /// <summary>
        /// 抽奖开始
        /// </summary>
        AnchorLotteryStart,
        /// <summary>
        /// 抽奖结束
        /// </summary>
        AnchorLotteryEnd,
        /// <summary>
        /// 抽奖结果
        /// </summary>
        AnchorLotteryAward,
        /// <summary>
        /// 欢迎舰长
        /// </summary>
        WelcomeGuard,
        /// <summary>
        /// 上舰
        /// </summary>
        GuardBuy,
    }
    public class LiveMessage : IDisposable
    {
        private static readonly ILogger logger = GlobalLogger.FromCurrentType();

        public delegate void MessageHandler(MessageType type, object message);
        public event MessageHandler NewMessage;
        ClientWebSocket ws;
        public string ServerUrl { get; set; } = "wss://broadcastlv.chat.bilibili.com/sub";
        public LiveMessage()
        {
            ws = new ClientWebSocket();
        }
        private static System.Timers.Timer heartBeatTimer;
        public async Task Connect(int roomID, int uid, CancellationToken cancellationToken)
        {
            //连接
            await ws.ConnectAsync(new Uri(ServerUrl), cancellationToken);
            //进房
            await JoinRoomAsync(roomID, uid);
            //发送心跳
            await SendHeartBeatAsync();
            heartBeatTimer = new System.Timers.Timer(1000 * 30);
            heartBeatTimer.Elapsed += HeartBeatTimer_Elapsed;
            heartBeatTimer.Start();
            while (!cancellationToken.IsCancellationRequested && ws.State == WebSocketState.Open)
            {
                try
                {
                    var buffer = new byte[4096];
                    WebSocketReceiveResult result = await ws.ReceiveAsync(buffer, cancellationToken);
                    ParseData(buffer.Take(result.Count).ToArray());
                }
                catch (Exception ex)
                {
                    if (ex is OperationCanceledException)
                    {
                        return;
                    }
                    logger.Log("直播接收包出错", LogType.Error, ex);
                }
            }
        }

        private async void HeartBeatTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            await SendHeartBeatAsync();
        }
        /// <summary>
        /// 发送进房信息
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        private async Task JoinRoomAsync(int roomId, int uid = 0)
        {
            if (ws.State == WebSocketState.Open)
            {
                await ws.SendAsync(EncodeData(JsonConvert.SerializeObject(new
                {
                    roomid = roomId,
                    uid = uid
                }), 7), WebSocketMessageType.Binary, true, CancellationToken.None);
            }
        }
        /// <summary>
        /// 发送心跳包
        /// </summary>
        /// <returns></returns>
        private async Task SendHeartBeatAsync()
        {
            if (ws.State == WebSocketState.Open)
            {
                await ws.SendAsync(EncodeData("", 2), WebSocketMessageType.Binary, true, CancellationToken.None);
            }
        }
        /// <summary>
        /// 解析内容
        /// </summary>
        /// <param name="data"></param>
        private void ParseData(byte[] data)
        {
            //协议版本。
            //0为JSON，可以直接解析；
            //1为房间人气值,Body为Int32；
            //2为压缩过Buffer，需要解压再处理
            int protocolVersion = BitConverter.ToInt32(new byte[4] { data[7], data[6], 0, 0 }, 0);
            //操作类型。
            //3=心跳回应，内容为房间人气值；
            //5=通知，弹幕、广播等全部信息；
            //8=进房回应，空
            int operation = BitConverter.ToInt32(data.Skip(8).Take(4).Reverse().ToArray(), 0);
            //内容
            var body = data.Skip(16).ToArray();
            if (operation == 8)
            {
                NewMessage?.Invoke(MessageType.ConnectSuccess, "弹幕连接成功");
            }
            else if (operation == 3)
            {
                var online = BitConverter.ToInt32(body.Reverse().ToArray(), 0);
                NewMessage?.Invoke(MessageType.Online, online);
            }
            else if (operation == 5)
            {

                if (protocolVersion == 2)
                {
                    body = DecompressData(body);

                }
                var text = Encoding.UTF8.GetString(body);
                //可能有多条数据，做个分割
                var textLines = Regex.Split(text, "[\x00-\x1f]+").Where(x => x.Length > 2 && x[0] == '{').ToArray();
                foreach (var item in textLines)
                {
                    ParseMessage(item);

                }
            }
        }

        private void ParseMessage(string jsonMessage)
        {
            try
            {
                var obj = JObject.Parse(jsonMessage);
                var cmd = obj["cmd"].ToString();
                if (cmd.Contains("DANMU_MSG"))
                {
                    var msg = new DanmuMsgModel();
                    if (obj["info"] != null && obj["info"].ToArray().Length != 0)
                    {
                        msg.text = obj["info"][1].ToString();
                        if (obj["info"][2] != null && obj["info"][2].ToArray().Length != 0)
                        {
                            msg.username = obj["info"][2][1].ToString() + ":";

                            //msg.usernameColor = GetColor(obj["info"][2][0].ToString());
                            if (obj["info"][2][3] != null && Convert.ToInt32(obj["info"][2][3].ToString()) == 1)
                            {
                                msg.vip = "老爷";
                                msg.isVip = Visibility.Visible;
                            }
                            if (obj["info"][2][4] != null && Convert.ToInt32(obj["info"][2][4].ToString()) == 1)
                            {
                                msg.vip = "年费老爷";
                                msg.isVip = Visibility.Collapsed;
                                msg.isBigVip = Visibility.Visible;
                            }
                            if (obj["info"][2][2] != null && Convert.ToInt32(obj["info"][2][2].ToString()) == 1)
                            {
                                msg.vip = "房管";
                                msg.isAdmin = Visibility.Visible;
                            }
                        }
                        if (obj["info"][3] != null && obj["info"][3].ToArray().Length != 0)
                        {
                            msg.medal_name = obj["info"][3][1].ToString();
                            msg.medal_lv = obj["info"][3][0].ToString();
                            msg.medalColor = obj["info"][3][4].ToString();
                            msg.hasMedal = Visibility.Visible;
                        }
                        if (obj["info"][4] != null && obj["info"][4].ToArray().Length != 0)
                        {
                            msg.ul = "UL" + obj["info"][4][0].ToString();
                            msg.ulColor = obj["info"][4][2].ToString();
                        }
                        if (obj["info"][5] != null && obj["info"][5].ToArray().Length != 0)
                        {
                            msg.user_title = obj["info"][5][0].ToString();
                            msg.hasTitle = Visibility.Visible;
                        }

                        NewMessage?.Invoke(MessageType.Danmu, msg);
                        return;
                    }
                }
                if (cmd == "SEND_GIFT")
                {
                    var msg = new GiftMsgModel();
                    if (obj["data"] != null)
                    {
                        msg.uname = obj["data"]["uname"].ToString();
                        msg.action = obj["data"]["action"].ToString();
                        msg.giftId = Convert.ToInt32(obj["data"]["giftId"].ToString());
                        msg.giftName = obj["data"]["giftName"].ToString();
                        msg.num = obj["data"]["num"].ToString();
                        msg.uid = obj["data"]["uid"].ToString();
                        NewMessage?.Invoke(MessageType.Gift, msg);
                    }
                    return;
                }
                if (cmd == "COMBO_SEND")
                {
                    var msg = new GiftMsgModel();
                    if (obj["data"] != null)
                    {
                        msg.uname = obj["data"]["uname"].ToString();
                        msg.action = obj["data"]["action"].ToString();
                        msg.giftId = Convert.ToInt32(obj["data"]["gift_id"].ToString());
                        msg.giftName = obj["data"]["gift_name"].ToString();
                        msg.num = obj["data"]["total_num"].ToString();
                        msg.uid = obj["data"]["uid"].ToString();
                        NewMessage?.Invoke(MessageType.Gift, msg);
                    }
                    return;
                }
                if (cmd == "WELCOME")
                {
                    var w = new WelcomeMsgModel();
                    if (obj["data"] != null)
                    {
                        w.uname = obj["data"]["uname"].ToString();
                        w.uid = obj["data"]["uid"].ToString();
                        w.svip = obj["data"]["vip"].ToInt32() != 1;
                        NewMessage?.Invoke(MessageType.Welcome, w);
                    }

                    return;
                }
                if (cmd == "SYS_MSG")
                {
                    NewMessage?.Invoke(MessageType.SystemMsg, obj["msg"].ToString());
                    return;
                }
                if (cmd == "ANCHOR_LOT_START")
                {
                    if (obj["data"] != null)
                    {
                        NewMessage?.Invoke(MessageType.AnchorLotteryStart, obj["data"].ToString());
                    }
                    return;
                }
                if (cmd == "ANCHOR_LOT_AWARD")
                {
                    if (obj["data"] != null)
                    {
                        NewMessage?.Invoke(MessageType.AnchorLotteryAward, obj["data"].ToString());
                    }
                    return;
                }
                if (cmd == "SUPER_CHAT_MESSAGE")
                {
                    SuperChatMsgModel msg = new SuperChatMsgModel();
                    if (obj["data"] != null)
                    {
                        msg.background_bottom_color = obj["data"]["background_bottom_color"].ToString();
                        msg.background_color = obj["data"]["background_color"].ToString();
                        msg.background_image = obj["data"]["background_image"].ToString();
                        msg.end_time = obj["data"]["end_time"].ToInt32();
                        msg.start_time = obj["data"]["start_time"].ToInt32();
                        msg.time = obj["data"]["time"].ToInt32();
                        msg.max_time = msg.end_time - msg.start_time;
                        msg.face = obj["data"]["user_info"]["face"].ToString();
                        msg.face_frame = obj["data"]["user_info"]["face_frame"].ToString();
                        msg.font_color = obj["data"]["message_font_color"].ToString();
                        msg.message = obj["data"]["message"].ToString();
                        msg.price = obj["data"]["price"].ToInt32();
                        msg.username = obj["data"]["user_info"]["uname"].ToString();
                        NewMessage?.Invoke(MessageType.SuperChat, msg);
                    }
                    return;
                }
            }
            catch (Exception)
            {
            }

        }

        /// <summary>
        /// 对数据进行编码
        /// </summary>
        /// <param name="msg">文本内容</param>
        /// <param name="action">2=心跳，7=进房</param>
        /// <returns></returns>
        private ArraySegment<byte> EncodeData(string msg, int action)
        {
            var data = Encoding.UTF8.GetBytes(msg);
            //头部长度固定16
            var length = data.Length + 16;
            var buffer = new byte[length];
            using (var ms = new MemoryStream(buffer))
            {
                //数据包长度
                var b = BitConverter.GetBytes(buffer.Length).ToArray().Reverse().ToArray();
                ms.Write(b, 0, 4);
                //数据包头部长度,固定16
                b = BitConverter.GetBytes(16).Reverse().ToArray();
                ms.Write(b, 2, 2);
                //协议版本，0=JSON,1=Int32,2=Buffer
                b = BitConverter.GetBytes(0).Reverse().ToArray(); ;
                ms.Write(b, 0, 2);
                //操作类型
                b = BitConverter.GetBytes(action).Reverse().ToArray(); ;
                ms.Write(b, 0, 4);
                //数据包头部长度,固定1
                b = BitConverter.GetBytes(1).Reverse().ToArray(); ;
                ms.Write(b, 0, 4);
                //数据
                ms.Write(data, 0, data.Length);
                ArraySegment<byte> _bytes = new ArraySegment<byte>(ms.ToArray());
                ms.Flush();
                return _bytes;
            }

        }


        /// <summary>
        /// 解压数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private byte[] DecompressData(byte[] data)
        {
            using (MemoryStream outBuffer = new MemoryStream())
            {
                using (System.IO.Compression.DeflateStream compressedzipStream = new System.IO.Compression.DeflateStream(new MemoryStream(data, 2, data.Length - 2), System.IO.Compression.CompressionMode.Decompress))
                {
                    byte[] block = new byte[1024];
                    while (true)
                    {
                        int bytesRead = compressedzipStream.Read(block, 0, block.Length);
                        if (bytesRead <= 0)
                            break;
                        else
                            outBuffer.Write(block, 0, bytesRead);
                    }
                    compressedzipStream.Close();
                    return outBuffer.ToArray();

                }

            }


        }

        public void Dispose()
        {
            heartBeatTimer?.Stop();
            heartBeatTimer?.Dispose();
            ws.Dispose();
        }
    }

    public class LiveDanmuModel
    {

        public LiveDanmuTypes type { get; set; }
        public int viewer { get; set; }
        public object value { get; set; }

    }
    public class DanmuMsgModel
    {
        public string text { get; set; }
        public string username { get; set; }//昵称
                                            // public SolidColorBrush usernameColor { get; set; }//昵称颜色

        public string ul { get; set; }//等级
        public string ulColor { get; set; }//等级颜色
        public SolidColorBrush ul_color { get; set; }//等级颜色


        public string user_title { get; set; }//头衔id（对应的是CSS名）

        public string vip { get; set; }
        public string medal_name { get; set; }//勋章

        public string medal_lv { get; set; }//勋章
        public string medalColor { get; set; }//勋章颜色
        public SolidColorBrush medal_color { get; set; }//勋章颜色

        public Visibility isAdmin { get; set; } = Visibility.Collapsed;
        public Visibility isVip { get; set; } = Visibility.Collapsed;
        public Visibility isBigVip { get; set; } = Visibility.Collapsed;
        public Visibility hasMedal { get; set; } = Visibility.Collapsed;
        public Visibility hasTitle { get; set; } = Visibility.Collapsed;
        public Visibility hasUL { get; set; } = Visibility.Visible;
        public string titleImg
        {
            get
            {
                return LiveRoomVM.Titles.FirstOrDefault(x => x.id == user_title)?.img;
            }
        }
        public SolidColorBrush uname_color { get; set; }
        public SolidColorBrush content_color { get; set; }

    }
    public class GiftMsgModel
    {
        public string uname { get; set; }
        public string giftName { get; set; }
        public string action { get; set; }
        public string num { get; set; }
        public int giftId { get; set; }
        public string uid { get; set; }
        public string gif { get; set; }
    }
    public class WelcomeMsgModel
    {
        public string uname { get; set; }
        public string isadmin { get; set; }
        public string uid { get; set; }
        public bool svip { get; set; }

    }
    public class SuperChatMsgModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void DoPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public string username { get; set; }
        public string face { get; set; }
        public string face_frame { get; set; }
        public string message { get; set; }
        public string message_jpn { get; set; }
        public string background_image { get; set; }
        public int start_time { get; set; }
        public int end_time { get; set; }
        private int _time;
        public int time
        {
            get { return _time; }
            set { _time = value; DoPropertyChanged("time"); }
        }
        public int max_time { get; set; }
        public int price { get; set; }
        public int price_gold
        {
            get
            {
                return price * 100;
            }
        }
        public string background_color { get; set; }
        public string background_bottom_color { get; set; }
        public string font_color { get; set; }
    }
}
