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
using BrotliSharpLib;
using System.IO.Compression;
using Google.Protobuf.WellKnownTypes;
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
        /// <summary>
        /// 房间信息更新
        /// </summary>
        RoomChange,
    }
    public class LiveMessage : IDisposable
    {
        private static readonly ILogger logger = GlobalLogger.FromCurrentType();

        public delegate void MessageHandler(MessageType type, object message);
        public event MessageHandler NewMessage;
        ClientWebSocket ws;

        public LiveMessage()
        {
            ws = new ClientWebSocket();
        }
        private static System.Timers.Timer heartBeatTimer;
        public async Task Connect(int roomID, int uid, string token, string buvid, string host, CancellationToken cancellationToken)
        {
            ws.Options.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36 Edg/116.0.1938.69");
            //连接
            await ws.ConnectAsync(new Uri("wss://" + host + "/sub"), cancellationToken);
            //进房
            await JoinRoomAsync(roomID, buvid, token, uid);
            //发送心跳
            await SendHeartBeatAsync();
            heartBeatTimer = new System.Timers.Timer(1000 * 30);
            heartBeatTimer.Elapsed += HeartBeatTimer_Elapsed;
            heartBeatTimer.Start();
            while (!cancellationToken.IsCancellationRequested && ws.State == WebSocketState.Open)
            {
                try
                {
                    WebSocketReceiveResult result;
                    using var ms = new MemoryStream();
                    var buffer = new byte[4096];
                    do
                    {
                        result = await ws.ReceiveAsync(buffer, cancellationToken);
                        ms.Write(buffer, 0, result.Count);
                    }
                    while (!result.EndOfMessage);

                    ms.Seek(0, SeekOrigin.Begin);
                    var receivedData = new byte[ms.Length];
                    ms.Read(receivedData, 0, receivedData.Length);

                    ParseData(receivedData);
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
        private async Task JoinRoomAsync(int roomId, string buvid, string token, int uid = 0)
        {
            if (ws.State == WebSocketState.Open)
            {
                await ws.SendAsync(EncodeData(JsonConvert.SerializeObject(new
                {
                    roomid = roomId,
                    uid = uid,
                    buvid = buvid,
                    key = token,
                    protover = 3,
                    //暂时不要加上platform，否则未登录时会隐藏用户名
                    //platform = "web"
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
            //2为zlib压缩过Buffer，需要解压再处理
            //3为brotli压缩过Buffer，需要解压再处理
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
            else if (operation == 5)
            {
                body = DecompressData(protocolVersion, body);

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
                        msg.Text = obj["info"][1].ToString();
                        var color = obj["info"][0][3].ToInt32();
                        if (color != 0)
                        {
                            msg.DanmuColor = color.ToString();
                        }
                       
                        if (obj["info"][2] != null && obj["info"][2].ToArray().Length != 0)
                        {
                            msg.UserName = obj["info"][2][1].ToString() + ":";
                            if (obj["info"][2][2] != null && Convert.ToInt32(obj["info"][2][2].ToString()) == 1)
                            {
                                msg.Role = "房管";
                                msg.ShowAdmin = Visibility.Visible;
                            }
                        }
                        if (obj["info"][3] != null && obj["info"][3].ToArray().Length != 0)
                        {
                            msg.MedalName = obj["info"][3][1].ToString();
                            msg.MedalLevel = obj["info"][3][0].ToString();
                            msg.MedalColor = obj["info"][3][4].ToString();
                            msg.ShowMedal = Visibility.Visible;
                        }
                        if (obj["info"][4] != null && obj["info"][4].ToArray().Length != 0)
                        {
                            msg.UserLevel = "UL" + obj["info"][4][0].ToString();
                            msg.UserLevelColor = obj["info"][4][2].ToString();
                        }
                        if (obj["info"][5] != null && obj["info"][5].ToArray().Length != 0)
                        {
                            msg.UserTitleID = obj["info"][5][0].ToString();
                            msg.ShowTitle = Visibility.Visible;
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
                        msg.UserName = obj["data"]["uname"].ToString();
                        msg.Action = obj["data"]["action"].ToString();
                        msg.GiftId = Convert.ToInt32(obj["data"]["giftId"].ToString());
                        msg.GiftName = obj["data"]["giftName"].ToString();
                        msg.Number = obj["data"]["num"].ToString();
                        msg.UserID = obj["data"]["uid"].ToString();
                        NewMessage?.Invoke(MessageType.Gift, msg);
                    }
                    return;
                }
                if (cmd == "COMBO_SEND")
                {
                    var msg = new GiftMsgModel();
                    if (obj["data"] != null)
                    {
                        msg.UserName = obj["data"]["uname"].ToString();
                        msg.Action = obj["data"]["action"].ToString();
                        msg.GiftId = Convert.ToInt32(obj["data"]["gift_id"].ToString());
                        msg.GiftName = obj["data"]["gift_name"].ToString();
                        msg.Number = obj["data"]["total_num"].ToString();
                        msg.UserID = obj["data"]["uid"].ToString();
                        NewMessage?.Invoke(MessageType.Gift, msg);
                    }
                    return;
                }
                if (cmd == "WELCOME")
                {
                    var w = new WelcomeMsgModel();
                    if (obj["data"] != null)
                    {
                        w.UserName = obj["data"]["uname"].ToString();
                        w.UserID = obj["data"]["uid"].ToString();
                       
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
                if(cmd== "ROOM_CHANGE")
                {
                    if (obj["data"] != null)
                    {
                        NewMessage?.Invoke(MessageType.RoomChange,new RoomChangeMsgModel()
                        {
                            Title = obj["data"]["title"].ToString(),
                            AreaID = obj["data"]["area_id"].ToInt32(),
                            AreaName = obj["data"]["area_name"].ToString(),
                            ParentAreaName = obj["data"]["parent_area_name"].ToString(),
                            ParentAreaID = obj["data"]["parent_area_id"].ToInt32(),
                        });
                    }
                    return;
                }
                if (cmd == "GUARD_BUY")
                {
                    if (obj["data"] != null)
                    {
                        NewMessage?.Invoke(MessageType.GuardBuy, new GuardBuyMsgModel()
                        {
                            GiftId = obj["data"]["gift_id"].ToInt32(),
                            GiftName = obj["data"]["gift_name"].ToString(),
                            Num = obj["data"]["num"].ToInt32(),
                            Price = obj["data"]["price"].ToInt32(),
                            UserName = obj["data"]["username"].ToString(),
                            UserID = obj["data"]["uid"].ToString(),
                            GuardLevel = obj["data"]["guard_level"].ToInt32(),
                        });
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                if (ex is JsonReaderException)
                {
                    logger.Error("直播解析JSON包出错", ex);
                }
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
            using var ms = new MemoryStream(buffer);
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

        /// <summary>
        /// 解压数据
        /// </summary>
        /// <param name="protocolVersion"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        private byte[] DecompressData(int protocolVersion, byte[] body)
        {
            body = protocolVersion switch
            {
                2 => DecompressDataWithDeflate(body),
                3 => DecompressDataWithBrotli(body),
                _ => body
            };
            return body;
        }

        /// <summary>
        /// 解压数据 (使用Deflate)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private byte[] DecompressDataWithDeflate(byte[] data)
        {
            using var outBuffer = new MemoryStream();
            using var compressedzipStream = new DeflateStream(new MemoryStream(data, 2, data.Length - 2), CompressionMode.Decompress);
            var block = new byte[1024];
            while (true)
            {
                var bytesRead = compressedzipStream.Read(block, 0, block.Length);
                if (bytesRead <= 0)
                    break;
                outBuffer.Write(block, 0, bytesRead);
            }
            compressedzipStream.Close();
            return outBuffer.ToArray();
        }

        /// <summary>
        /// 解压数据 (使用 Brotli)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private byte[] DecompressDataWithBrotli(byte[] data)
        {
            using var decompressedStream = new BrotliStream(new MemoryStream(data), CompressionMode.Decompress);
            using var outBuffer = new MemoryStream();
            var block = new byte[1024];
            while (true)
            {
                var bytesRead = decompressedStream.Read(block, 0, block.Length);
                if (bytesRead <= 0)
                    break;
                outBuffer.Write(block, 0, bytesRead);
            }
            return outBuffer.ToArray();
        }

        public void Dispose()
        {
            heartBeatTimer?.Stop();
            heartBeatTimer?.Dispose();
            ws.Dispose();
        }
    }

    public class DanmuMsgModel
    {
        /// <summary>
        /// 内容文本
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// 弹幕颜色，默认白色
        /// </summary>
        public string DanmuColor { get; set; }= "#FFFFFFFF";
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 用户名颜色,默认灰色
        /// </summary>
        public string UserNameColor { get; set; } = "#FF808080";
        /// <summary>
        /// 等级
        /// </summary>
        public string UserLevel { get; set; }

        /// <summary>
        /// 等级颜色,默认灰色
        /// </summary>
        public string UserLevelColor { get; set; } = "#FF808080";
        /// <summary>
        /// 用户头衔id（对应的是CSS名）
        /// </summary>
        public string UserTitleID { get; set; }
        /// <summary>
        /// 用户头衔图片
        /// </summary>
        public string UserTitleImage
        {
            get
            {
                return LiveRoomVM.Titles.FirstOrDefault(x => x.id == UserTitleID)?.img;
            }
        }
        /// <summary>
        /// 用户角色
        /// </summary>
        public string Role { get; set; }
        /// <summary>
        /// 勋章名称
        /// </summary>
        public string MedalName { get; set; }
        /// <summary>
        /// 勋章等级
        /// </summary>
        public string MedalLevel { get; set; }
        /// <summary>
        /// 勋章颜色
        /// </summary>
        public string MedalColor { get; set; }

        /// <summary>
        /// 是否显示房管
        /// </summary>
        public Visibility ShowAdmin { get; set; } = Visibility.Collapsed;
        /// <summary>
        /// 是否显示勋章
        /// </summary>
        public Visibility ShowMedal { get; set; } = Visibility.Collapsed;
        /// <summary>
        /// 是否显示用户等级
        /// </summary>
        public Visibility ShowTitle { get; set; } = Visibility.Collapsed;
        /// <summary>
        /// 是否显示用户等级
        /// </summary>
        public Visibility ShowUserLevel { get; set; } = Visibility.Collapsed;
        
    }
    public class GiftMsgModel
    {
        /// <summary>
        /// 用户名称
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 礼物的名称
        /// </summary>
        public string GiftName { get; set; }
        /// <summary>
        /// 礼物操作
        /// </summary>
        public string Action { get; set; }
        /// <summary>
        /// 礼物数量
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// 礼物ID
        /// </summary>
        public int GiftId { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserID { get; set; }
        /// <summary>
        /// GIF图片
        /// </summary>
        public string Gif { get; set; }
    }
    public class WelcomeMsgModel
    {
        public string UserName { get; set; }
        public string IsAdmin { get; set; }
        public string UserID { get; set; }
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
    public class RoomChangeMsgModel
    {
        /// <summary>
        /// 房间标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 分区ID
        /// </summary>
        public int AreaID { get; set; }
        /// <summary>
        /// 父分区ID
        /// </summary>
        public int ParentAreaID { get; set; }
        /// <summary>
        /// 分区名称
        /// </summary>
        public string AreaName { get; set; }
        /// <summary>
        /// 父分区名称
        /// </summary>
        public string ParentAreaName { get; set; }
        /// <summary>
        /// 未知
        /// </summary>
        public string LiveKey { get; set; }
        /// <summary>
        /// 未知
        /// </summary>
        public string SubSessionKey { get; set; }

    }
    public class GuardBuyMsgModel
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserID { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 大航海等级1: 总督 2: 提督 3:舰长
        /// </summary>
        public int GuardLevel { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Num { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public int Price { get; set; }
        /// <summary>
        /// 礼物ID
        /// </summary>
        public int GiftId { get; set; }
        /// <summary>
        /// 礼物名称
        /// </summary>
        public string GiftName { get; set; }
    }
}
