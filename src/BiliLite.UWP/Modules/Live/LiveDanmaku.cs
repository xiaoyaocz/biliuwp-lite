using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Services;

namespace BiliLite.Modules.Live
{
    public enum LiveDanmuTypes
    {
        /// <summary>
        /// 观众
        /// </summary>
        Viewer,
        /// <summary>
        /// 弹幕
        /// </summary>
        Danmu,
        /// <summary>
        /// 礼物
        /// </summary>
        Gift,
        /// <summary>
        /// 欢迎
        /// </summary>
        Welcome,
        /// <summary>
        /// 系统信息
        /// </summary>
        SystemMsg,
        /// <summary>
        /// 欢迎舰长
        /// </summary>
        WELCOME_GUARD,
        /// <summary>
        /// 开始抽奖
        /// </summary>
        ANCHOR_LOT_START,
        /// <summary>
        /// 抽奖结束
        /// </summary>
        ANCHOR_LOT_END,
        /// <summary>
        /// 开奖信息
        /// </summary>
        ANCHOR_LOT_AWARD,
        /// <summary>
        /// 更新粉丝数？
        /// </summary>
        ROOM_REAL_TIME_MESSAGE_UPDATE
    }
    public class LiveDanmaku : IDisposable
    {
        private static readonly ILogger logger = GlobalLogger.FromCurrentType();

        public event EventHandler<LiveDanmuModel> NewMessage;

        private StreamSocket _clientSocket;
        private DispatcherTimer _timer;
        public int delay = 20;
        private int _roomId;
        public LiveDanmaku()
        {

        }

        //开始
        public async void Start(int roomid, long userId)
        {
            try
            {
                _roomId = roomid;
                var server = await GetDanmuServer();
                HostName serverHost = new HostName(server.host);  //设置服务器IP  

                _clientSocket = new StreamSocket();
                await _clientSocket.ConnectAsync(serverHost, server.port.ToString());  //设置服务器端口号  
                _StartState = true;
                if (SendJoinChannel(roomid, userId))
                {
                    SendHeartbeatAsync();
                    _timer = new DispatcherTimer();
                    _timer.Interval = new TimeSpan(0, 0, 20);
                    _timer.Tick += Timer_Tick;
                    _timer.Start();
                    await Task.Run(() => { Listen(); });
                }
            }
            catch (Exception)
            {
                _StartState = false;
            }

        }

        private async void Listen()
        {
            Stream _netStream = _clientSocket.InputStream.AsStreamForRead(1024);
            byte[] stableBuffer = new byte[1024];
            while (true)
            {

                if (!_StartState)
                {
                    return;
                }
                try
                {
                    _netStream.ReadB(stableBuffer, 0, 4);
                    var packetlength = BitConverter.ToInt32(stableBuffer, 0);
                    packetlength = IPAddress.NetworkToHostOrder(packetlength);

                    if (packetlength < 16)
                    {
                        throw new NotSupportedException("协议失败: (L:" + packetlength + ")");
                    }

                    _netStream.ReadB(stableBuffer, 0, 2);//magic
                    _netStream.ReadB(stableBuffer, 0, 2);//protocol_version 

                    _netStream.ReadB(stableBuffer, 0, 4);
                    var typeId = BitConverter.ToInt32(stableBuffer, 0);
                    typeId = IPAddress.NetworkToHostOrder(typeId);

                    _netStream.ReadB(stableBuffer, 0, 4);//magic, params?

                    var playloadlength = packetlength - 16;
                    if (playloadlength == 0)
                    {
                        continue;//没有内容了
                    }

                    typeId = typeId - 1;


                    var buffer = new byte[playloadlength];
                    _netStream.ReadB(buffer, 0, playloadlength);
                    if (typeId == 2)
                    {
                        var viewer = BitConverter.ToUInt32(buffer.Take(4).Reverse().ToArray(), 0); //观众人数
                        if (NewMessage != null)
                        {
                            NewMessage(null, new LiveDanmuModel() { type = LiveDanmuTypes.Viewer, viewer = Convert.ToInt32(viewer) });
                        }
                        Debug.WriteLine(viewer);
                        continue;
                    }
                    var json_str = "";
                    try
                    {
                        //临时解决方案，可以优化
                        //参考https://github.com/Bililive/BililiveRecorder
                        using (MemoryStream outBuffer = new MemoryStream())
                        {
                            using (System.IO.Compression.DeflateStream compressedzipStream = new System.IO.Compression.DeflateStream(new MemoryStream(buffer, 2, playloadlength - 2), System.IO.Compression.CompressionMode.Decompress))
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
                                buffer = outBuffer.ToArray();
                            }
                        }
                        json_str = Regex.Replace(Encoding.UTF8.GetString(buffer, 16, buffer.Length - 16), "}\\0\\0.*?\\0\\0{", "},{");
                    }
                    catch (Exception)
                    {
                        json_str = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                    }

                    if (json_str.Trim().Length != 0)
                    {
                        json_str = "[" + json_str + "]";
                        Debug.WriteLine(json_str);
                        JArray json_array = JArray.Parse(json_str);
                        foreach (var obj in json_array)
                        {
                            if (obj["cmd"] == null)
                            {
                                continue;
                            }
                            if (obj["cmd"].ToString().Contains("DANMU_MSG"))
                            {
                                var v = new DanmuMsgModel();
                                if (obj["info"] != null && obj["info"].ToArray().Length != 0)
                                {
                                    v.text = obj["info"][1].ToString();
                                    if (obj["info"][2] != null && obj["info"][2].ToArray().Length != 0)
                                    {
                                        v.username = obj["info"][2][1].ToString() + ":";

                                        //v.usernameColor = GetColor(obj["info"][2][0].ToString());
                                        if (obj["info"][2][3] != null && Convert.ToInt32(obj["info"][2][3].ToString()) == 1)
                                        {
                                            v.vip = "老爷";
                                            v.isVip = Visibility.Visible;
                                        }
                                        if (obj["info"][2][4] != null && Convert.ToInt32(obj["info"][2][4].ToString()) == 1)
                                        {
                                            v.vip = "年费老爷";
                                            v.isVip = Visibility.Collapsed;
                                            v.isBigVip = Visibility.Visible;
                                        }
                                        if (obj["info"][2][2] != null && Convert.ToInt32(obj["info"][2][2].ToString()) == 1)
                                        {
                                            v.vip = "房管";
                                            v.isAdmin = Visibility.Visible;
                                        }
                                    }
                                    if (obj["info"][3] != null && obj["info"][3].ToArray().Length != 0)
                                    {
                                        v.medal_name = obj["info"][3][1].ToString();
                                        v.medal_lv = obj["info"][3][0].ToString();
                                        v.medalColor = obj["info"][3][4].ToString();
                                        v.hasMedal = Visibility.Visible;
                                    }
                                    if (obj["info"][4] != null && obj["info"][4].ToArray().Length != 0)
                                    {
                                        v.ul = "UL" + obj["info"][4][0].ToString();
                                        v.ulColor = obj["info"][4][2].ToString();
                                    }
                                    if (obj["info"][5] != null && obj["info"][5].ToArray().Length != 0)
                                    {
                                        v.user_title = obj["info"][5][0].ToString();
                                        v.hasTitle = Visibility.Visible;
                                    }

                                    if (NewMessage != null)
                                    {
                                        NewMessage(null, new LiveDanmuModel() { type = LiveDanmuTypes.Danmu, value = v });
                                    }
                                }
                            }
                            //19/10/01,cmd DANMU_MSG变成了DANMU_MSG:4:0:2:2:2:0
                            switch (obj["cmd"].ToString())
                            {
                                //case "DANMU_MSG":
                                //    break;
                                case "SEND_GIFT":
                                    var g = new GiftMsgModel();
                                    if (obj["data"] != null)
                                    {
                                        g.uname = obj["data"]["uname"].ToString();
                                        g.action = obj["data"]["action"].ToString();
                                        g.giftId = Convert.ToInt32(obj["data"]["giftId"].ToString());
                                        g.giftName = obj["data"]["giftName"].ToString();
                                        g.num = obj["data"]["num"].ToString();
                                        g.uid = obj["data"]["uid"].ToString();
                                        if (NewMessage != null)
                                        {
                                            NewMessage(null, new LiveDanmuModel() { type = LiveDanmuTypes.Gift, value = g });
                                        }
                                    }

                                    break;
                                case "WELCOME":
                                    var w = new WelcomeMsgModel();
                                    if (obj["data"] != null)
                                    {
                                        w.uname = obj["data"]["uname"].ToString();
                                        w.uid = obj["data"]["uid"].ToString();
                                        w.svip = obj["data"]["vip"].ToInt32() != 1;
                                        if (NewMessage != null)
                                        {
                                            NewMessage(null, new LiveDanmuModel() { type = LiveDanmuTypes.Welcome, value = w });
                                        }
                                    }
                                    break;
                                case "SYS_MSG":
                                    if (obj["msg"] != null)
                                    {
                                        if (NewMessage != null)
                                        {
                                            NewMessage(null, new LiveDanmuModel() { type = LiveDanmuTypes.SystemMsg, value = obj["msg"].ToString() });
                                        }
                                    }

                                    break;
                                case "ANCHOR_LOT_START":
                                    if (obj["data"] != null)
                                    {
                                        if (NewMessage != null)
                                        {
                                            NewMessage(null, new LiveDanmuModel() { type = LiveDanmuTypes.ANCHOR_LOT_START, value = obj["data"].ToString() });
                                        }
                                    }
                                    break;
                                case "ANCHOR_LOT_AWARD":
                                    if (obj["data"] != null)
                                    {
                                        if (NewMessage != null)
                                        {
                                            NewMessage(null, new LiveDanmuModel() { type = LiveDanmuTypes.ANCHOR_LOT_AWARD, value = obj["data"].ToString() });
                                        }
                                    }
                                    break;
                                default:

                                    break;
                            }
                            await Task.Delay(delay);
                        }

                    }




                    // }

                }
                catch (Exception ex)
                {
                    logger.Log("加载直播弹幕失败", LogType.Error, ex);
                }

                await Task.Delay(delay);
            }



        }
        ///// <summary>
        /////十进制转SolidColorBrush
        ///// </summary>
        ///// <param name="_color">输入10进制颜色</param>
        ///// <returns></returns>
        //public async Task<SolidColorBrush> GetColor(string _color)
        //{
        //    SolidColorBrush solid = new SolidColorBrush(new Color()
        //    {
        //        A = 255,
        //        R = 255,
        //        G = 255,
        //        B = 255
        //    });
        //    await Task.Run(() => {
        //        try
        //        {
        //            _color = Convert.ToInt32(_color).ToString("X2");
        //            if (_color.StartsWith("#"))
        //                _color = _color.Replace("#", string.Empty);
        //            int v = int.Parse(_color, System.Globalization.NumberStyles.HexNumber);
        //             solid = new SolidColorBrush(new Color()
        //            {
        //                A = Convert.ToByte(255),
        //                R = Convert.ToByte((v >> 16) & 255),
        //                G = Convert.ToByte((v >> 8) & 255),
        //                B = Convert.ToByte((v >> 0) & 255)
        //            });
        //            // color = solid;
        //            return solid;
        //        }
        //        catch (Exception)
        //        {
        //            return solid;
        //            // color = solid;

        //        }
        //    });


        //    return solid;

        //}


        private async Task<(string token, string host, int port)> GetDanmuServer()
        {
            try
            {
                var chat = $"https://api.live.bilibili.com/room/v1/Danmu/getConf?room_id={_roomId}&platform=pc&player=web";
                string results = await chat.GetString();
                var obj = JObject.Parse(results);
                if (obj["code"].ToInt32() == 0)
                {
                    return (obj["data"]["token"].ToString(), obj["data"]["host"].ToString(), obj["data"]["port"].ToInt32());
                }
                else
                {
                    return (string.Empty, "broadcastlv.chat.bilibili.com", 2243);
                }
            }
            catch (Exception)
            {
                return (string.Empty, "broadcastlv.chat.bilibili.com", 2243);
            }



        }



        private void Timer_Tick(object sender, object e)
        {
            SendHeartbeatAsync();
        }

        private void SendHeartbeatAsync()
        {
            SendSocketData(2);
        }

        private bool SendJoinChannel(int channelId, long userId)
        {
            var r = new Random();

            long tmpuid = 0;
            if (userId == 0)
            {
                tmpuid = (long)(1e14 + 2e14 * r.NextDouble());
            }
            else
            {
                tmpuid = userId;
            }
            var packetModel = new { roomid = channelId, uid = tmpuid };
            var playload = JsonConvert.SerializeObject(packetModel);
            SendSocketData(7, playload);
            return true;
        }

        private void SendSocketData(int action, string body = "")
        {
            SendSocketData(0, 16, 1, action, 1, body);
        }
        private async void SendSocketData(int packetlength, short magic, short ver, int action, int param = 1, string body = "")
        {
            try
            {
                if (_clientSocket == null) return;
                var playload = Encoding.UTF8.GetBytes(body);
                if (packetlength == 0)
                {
                    packetlength = playload.Length + 16;
                }
                var buffer = new byte[packetlength];
                using (var ms = new MemoryStream(buffer))
                {
                    //Array.Reverse(a)
                    var b = BitConverter.GetBytes(buffer.Length).ToArray().Reverse().ToArray();

                    ms.Write(b, 0, 4);
                    b = BitConverter.GetBytes(magic).ToArray().Reverse().ToArray();
                    ms.Write(b, 0, 2);
                    b = BitConverter.GetBytes(ver).ToArray().Reverse().ToArray();
                    ms.Write(b, 0, 2);
                    b = BitConverter.GetBytes(action).ToArray().Reverse().ToArray();
                    ms.Write(b, 0, 4);
                    b = BitConverter.GetBytes(param).ToArray().Reverse().ToArray();
                    ms.Write(b, 0, 4);
                    if (playload.Length > 0)
                    {
                        ms.Write(playload, 0, playload.Length);
                    }
                    DataWriter writer = new DataWriter(_clientSocket.OutputStream);  //实例化writer对象，以StreamSocket的输出流作为writer的方向  
                                                                                     // string content = "ABCDEFGH";  //发送一字符串  
                                                                                     //byte[] data = Encoding.UTF8.GetBytes(content);  //将字符串转换为字节类型，完全可以不用转换  
                    writer.WriteBytes(buffer);  //写入字节流，当然可以使用WriteString直接写入字符串  
                    await writer.StoreAsync();  //异步发送数据  
                    writer.DetachStream();  //分离  
                    writer.Dispose();  //结束writer  



                    // _netStream.WriteAsync(buffer, 0, buffer.Length);
                    //  _netStream.FlushAsync();
                }
            }
            catch (Exception)
            {
            }

        }





        bool _StartState = false;
        public void Dispose()
        {
            _StartState = false;
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }
            if (_clientSocket != null)
            {
                _clientSocket.Dispose();
                _clientSocket = null;
            }
        }



    }



}
