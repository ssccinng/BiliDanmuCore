using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BiliBiliDanmuCore
{
    enum Operation
    {
        HANDSHAKE = 0,
        HANDSHAKE_REPLY = 1,
        HEARTBEAT = 2,
        HEARTBEAT_REPLY = 3,
        SEND_MSG = 4,
        SEND_MSG_REPLY = 5,
        DISCONNECT_REPLY = 6,
        AUTH = 7,
        AUTH_REPLY = 8,
        RAW = 9,
        PROTO_READY = 10,
        PROTO_FINISH = 11,
        CHANGE_ROOM = 12,
        CHANGE_ROOM_REPLY = 13,
        REGISTER = 14,
        REGISTER_REPLY = 15,
        UNREGISTER = 16,
        UNREGISTER_REPLY = 17,
    }

    enum PROTOCOLVERSION
    {
        JSON = 0,
        POPULARITY = 1,
        ZLIB = 2,
        BROTLI = 3,
    }

    public class BiliBiliLiveDanmuClient
    {

        static string ROOM_INIT_URL = "https://api.live.bilibili.com/xlive/web-room/v1/index/getInfoByRoom";
        static string DANMAKU_SERVER_CONF_URL = "https://api.live.bilibili.com/xlive/web-room/v1/index/getDanmuInfo";


        protected ClientWebSocket _webSocket = new();
        // 房间信息
        protected int _roomId;
        protected int _roomShortId;
        protected int _roomOwnerId;

        protected string _roomTitle;


        public int _lastFans { get; private set; } = 0;
        public int Popularity { get; private set; } = 0;

        public LinkedList<BiliBiliDanmu> DanmuList = new();

        protected HttpClient _httpClient = new();
        CancellationTokenSource cts = new CancellationTokenSource(35000);



        protected JsonElement _hostServerList;
        protected string _hostServerToken;

        protected Timer _heartTimer;

        public BiliBiliLiveDanmuClient(int roomId)
        {
            _roomId = roomId;
        }

        protected virtual void AddDanmu(BiliBiliDanmu danmu)
        {
            DanmuList.AddLast(danmu);
            while (DanmuList.Count > 20) DanmuList.RemoveFirst();
        }

        //public async Task<string> GetNewDanmu()
        //{
        //    await Task.Run(async () =>
        //    {
        //        while (DanmuList.Count == 0)
        //        {
        //            await Task.Delay(10);
        //        }

        //    });
            
        //}

        public async Task Start()
        {
            await InitRoomIdAndOwner();
            await InitHostServer();
            // 开始连接
            int retryCnt = 0;

            _heartTimer = new Timer(new TimerCallback(async _ => await SendHeartMsg()), null, 10000, 10000);
            while (true)
            {
                try
                {

                    var BiliDanmuServer =
                    $"wss://{_hostServerList[retryCnt % _hostServerList.GetArrayLength()].GetProperty("host").GetString()}:{_hostServerList[retryCnt % _hostServerList.GetArrayLength()].GetProperty("wss_port").GetInt32()}/sub";
                    if (!Uri.TryCreate(BiliDanmuServer.Trim(), UriKind.Absolute, out Uri webSocketUri))
                    {
                    }
                    //_webSocket = new ClientWebSocket();
                    //await _webSocket
                    //    .ConnectAsync(webSocketUri, cts.Token);
                    await _webSocket
                        .ConnectAsync(webSocketUri, CancellationToken.None);
                    //await _webSocket
                    //    .ConnectAsync(webSocketUri, CancellationToken.None);
                    //Console.WriteLine("建立连接成功!");

                    await SendAuth();
                    await SendHeartMsg();
                    //Console.WriteLine("认证成功!");
                    var rcvBytes = new byte[25000];
                    var rcvBuffer = new ArraySegment<byte>(rcvBytes);
                    while (true)
                    {



                        WebSocketReceiveResult rcvResult = await _webSocket.ReceiveAsync(rcvBuffer, CancellationToken.None);
                        //Console.WriteLine("接受成功!");
                        if (rcvResult?.MessageType != WebSocketMessageType.Binary)
                        {
                            Console.WriteLine("未知信息");
                            continue;
                        }
                        byte[] msgBytes = rcvBuffer.Skip(rcvBuffer.Offset).Take(rcvResult.Count).ToArray();
                        //Console.WriteLine("转换成功!");
                        await ReadMessage(msgBytes);
                        //Console.WriteLine("处理成功!");


                    }
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("task"))
                    {
                        Console.WriteLine("orz");
                    }
                    //await _webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "client requeste", cts.Token);
                    _webSocket.Dispose();
                    _webSocket = new ClientWebSocket();
                    //_webSocket = null;
                    Console.WriteLine("连接失败... 尝试重连...");
                    retryCnt++;
                    Console.WriteLine($"重连次数: {retryCnt}");
                    await Task.Delay(5000);
                }


            }
            // 心跳包

        }




        //ArraySegment<byte>
        private byte[] MakePackage(object data, Operation operation)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));

            byte[] PacketLength = BitConverter.GetBytes(16 + body.Length);
            byte[] HeaderLength = BitConverter.GetBytes((short)16);
            byte[] Protocol = BitConverter.GetBytes((short)1);
            byte[] operationt = BitConverter.GetBytes((int)operation);
            byte[] SequenceId = BitConverter.GetBytes(1);

            Array.Reverse(PacketLength);
            Array.Reverse(HeaderLength);
            Array.Reverse(Protocol);
            Array.Reverse(operationt);
            Array.Reverse(SequenceId);

            //Console.WriteLine(JsonSerializer.Serialize(data));

            List<byte> res = new List<byte>();
            res.AddRange(PacketLength);
            res.AddRange(HeaderLength);
            res.AddRange(Protocol);
            res.AddRange(operationt);
            res.AddRange(SequenceId);

            res.AddRange(body);
            return res.ToArray();
        }

        private async Task ReadMessage(byte[] data)
        {
            int offset = 0;
            while (offset + 17 < data.Length)
            {

                int PacketLength = BitConverter.ToInt32(data[offset..(offset + 4)].Reverse().ToArray());
                short HeaderLength = BitConverter.ToInt16(data[(offset + 4)..(offset + 6)].Reverse().ToArray());
                PROTOCOLVERSION Protocol = (PROTOCOLVERSION)BitConverter.ToInt16(data[(offset + 6)..(offset + 8)].Reverse().ToArray());
                Operation operationt = (Operation)BitConverter.ToInt32(data[(offset + 8)..(offset + 12)].Reverse().ToArray());
                int SequenceId = BitConverter.ToInt32(data[(offset + 12)..(offset + 16)].Reverse().ToArray());
                var body = data[(offset + 16)..(offset + PacketLength)];

                if (operationt == Operation.AUTH_REPLY)
                {
                    await SendHeartMsg();
                }
                switch (Protocol)
                {
                    case PROTOCOLVERSION.JSON:
                        //Console.WriteLine($"Received: {Encoding.UTF8.GetString(body)}\n\n");
                        ParseCommand(Encoding.UTF8.GetString(body));
                        break;
                    case PROTOCOLVERSION.POPULARITY:
                        Popularity = BitConverter.ToInt32(body.Reverse().ToArray());
                        Console.WriteLine($"当前人气值: {Popularity}");
                        break;
                    case PROTOCOLVERSION.ZLIB:

                        //MemoryStream input = new MemoryStream(body);
                        MemoryStream input = new MemoryStream(body[2..^2]);
                        MemoryStream output = new MemoryStream();
                        using (DeflateStream dstream = new(input, CompressionMode.Decompress))
                        {
                            dstream.CopyTo(output);
                            var gg = output.ToArray();
                            await ReadMessage(gg);
                        }
                        //using (var inf = new Ionic.Zlib.ZlibStream(input, Ionic.Zlib.CompressionMode.Decompress, Ionic.Zlib.CompressionLevel.Default, true))
                        //{
                        //    inf.CopyTo(output);
                        //    var byteArray = new byte[output.Length];
                        //    var gg = output.ToArray();
                        //    await ReadMessage(gg);
                        //}
                        break;
                    case PROTOCOLVERSION.BROTLI:
                        Console.WriteLine($"欧拉2");
                        break;
                    default:
                        break;
                }
                offset += PacketLength;
            }
        }

        private bool ParseCommand(string command)
        {
            var cmd = JsonDocument.Parse(command);
            if (!cmd.RootElement.TryGetProperty("cmd", out JsonElement cmdType))
            {
                return false;
            }
            switch (cmdType.GetString())
            {
                case "INTERACT_WORD":
                    // 进入房间
                    var IWData = cmd.RootElement.GetProperty("data");
                    Console.WriteLine($"{IWData.GetProperty("uname").GetString()} 进入直播间");

                    BiliBiliDanmu indanmu = new BiliBiliDanmu
                    {
                        Username = IWData.GetProperty("uname").GetString(),
                        UID = IWData.GetProperty("uid").GetInt32(),
                        DanmuType = DanmuType.Join
                        //MedalName = DMInfo[3][1].GetString(),
                        //MedalLevel = DMInfo[3][0].GetInt32(),
                        //MedalUP = DMInfo[3][2].GetString(),
                    };

                    AddDanmu(indanmu);
                    break;
                case "ROOM_CHANGE":
                    // 进入房间
                    var RmCData = cmd.RootElement.GetProperty("data");
                    string title = RmCData.GetProperty("title").GetString();
                    _roomTitle = title;
                    Console.WriteLine($"---- 房间名切换为 {title} ----");
                    break;
                case "ROOM_BLOCK_MSG":
                    // 封禁
                    //var RmCData = cmd.RootElement.GetProperty("data");
                    //string title = RmCData.GetProperty("title").GetString();
                    //_roomTitle = title;
                    //Console.WriteLine($"---- 房间名切换为 {title} ----");
                    break;
                case "DANMU_MSG":
                    // 弹幕信息
                    var DMInfo = cmd.RootElement.GetProperty("info");
                    BiliBiliDanmu danmu = new BiliBiliDanmu
                    {
                        Message = DMInfo[1].GetString(),
                        Username = DMInfo[2][1].GetString(),
                        UID = DMInfo[2][0].GetInt32(),
                        DanmuType = DanmuType.Msg
                    };
                    if (DMInfo[3].GetArrayLength() > 2)
                    {
                        danmu.MedalName = DMInfo[3][1].GetString();
                        danmu.MedalLevel = DMInfo[3][0].GetInt32();
                        danmu.MedalUP = DMInfo[3][2].GetString();
                    }
                    AddDanmu(danmu);
                    Console.WriteLine($"---- {danmu} ----");
                    //Console.WriteLine($"---- {command} ----");
                    break;
                case "ROOM_REAL_TIME_MESSAGE_UPDATE":
                    var RRTFansData = cmd.RootElement.GetProperty("data");
                    _lastFans = RRTFansData.GetProperty("fans").GetInt32();
                    Console.WriteLine($"直播间粉丝变化: {_lastFans}人!");
                    
                    // 粉丝变化
                    break;

                case "SEND_GIFT":
                    var GiftData = cmd.RootElement.GetProperty("data");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"{GiftData.GetProperty("uname")} {GiftData.GetProperty("action")} {GiftData.GetProperty("giftName")} {GiftData.GetProperty("num")}个");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    BiliBiliDanmu Giftdanmu = new BiliBiliDanmu
                    {
                        Username = GiftData.GetProperty("uname").GetString(),
                        UID = GiftData.GetProperty("uid").GetInt32(),
                        GiftName = GiftData.GetProperty("giftName").GetString(),
                        GiftNum = GiftData.GetProperty("num").GetInt32(),
                        DanmuType = DanmuType.Gift
                    };
                    AddDanmu(Giftdanmu);
                    break;
                case "COMBO_SEND":
                    // 礼物连击

                    var ComboGiftData = cmd.RootElement.GetProperty("data");
                    Console.ForegroundColor = ConsoleColor.Red;
                    //Console.WriteLine("连击礼物");

                    BiliBiliDanmu Combodanmu = new BiliBiliDanmu
                    {
                        Username = ComboGiftData.GetProperty("uname").GetString(),
                        UID = ComboGiftData.GetProperty("uid").GetInt32(),
                        GiftName = ComboGiftData.GetProperty("gift_name").GetString(),
                        GiftNum = ComboGiftData.GetProperty("total_num").GetInt32(),
                        DanmuType = DanmuType.Gift
                    };
                    AddDanmu(Combodanmu);
                    Console.WriteLine($"{ComboGiftData.GetProperty("uname")} {ComboGiftData.GetProperty("action")} {ComboGiftData.GetProperty("gift_name")} 共{ComboGiftData.GetProperty("total_num")}个");
                    Console.ForegroundColor = ConsoleColor.Gray;

                    break;
                case "GUARD_BUY":
                    
                    var GUARDData = cmd.RootElement.GetProperty("data");
                    Console.WriteLine($"!!!!!!!!!!{GUARDData.GetProperty("username")} 成功购买 {GUARDData.GetProperty("gift_name")}!!!!!!!!!!!");
                    // very dont know
                    break;
                case "USER_TOAST_MSG":
                    Console.WriteLine("有人续费");
                    // very dont know
                    break;
                case "LIVE_INTERACTIVE_GAME":
                    //Console.WriteLine(command);
                    // very dont know
                    break;
                case "SUPER_CHAT_MESSAGE":
                    Console.WriteLine("****** 有SC ******");
                    // very dont know
                    break;
                case "SUPER_CHAT_MESSAGE_JPN":
                    Console.WriteLine("****** 有JPNSC ******");
                    // very dont know
                    break;
                case "SUPER_CHAT_MESSAGE_DELETE":
                    Console.WriteLine("****** SC结束 ******");
                    // very dont know
                    break;

                case "ENTRY_EFFECT":
                    //Console.WriteLine(command);
                    // 进场特效
                    break;
                case "NOTICE_MSG":
                    //Console.WriteLine(command);
                    break;
                case "STOP_LIVE_ROOM_LIST":
                //muda
                case "PK_BATTLE_PRE_NEW":
                case "PK_BATTLE_PRE":
                case "PK_BATTLE_START_NEW":
                case "PK_BATTLE_START":
                case "PK_BATTLE_END":
                case "ONLINE_RANK_COUNT":
                case "WIDGET_BANNER":
                case "COMMON_NOTICE_DANMAKU":
                case "PK_BATTLE_FINAL_PROCESS":
                case "PK_BATTLE_PROCESS_NEW":
                case "PK_BATTLE_PROCESS":
                case "PK_BATTLE_SETTLE_USER":
                case "PK_BATTLE_SETTLE_V2":
                // 什么pk的

                // 注意信息
                case "ONLINE_RANK_V2":
                case "ONLINE_RANK_TOP3":
                case "ROOM_SKIN_MSG":
                // 好像是房间皮肤什么的
                case "HOT_RANK_CHANGED":
                case "HOT_RANK_SETTLEMENT":
                    // 热度榜变化
                    // muda
                    break;
                case "ANCHOR_LOT_CHECKSTATUS":
                    Console.WriteLine("天选时刻检查");
                    break;
                case "ANCHOR_LOT_START":
                    Console.WriteLine("++++++++++++++++天选时刻开始++++++++++++++++");
                    break;
                case "ANCHOR_LOT_END":
                    Console.WriteLine("++++++++++++++++天选时刻结束++++++++++++++++");
                    break;
                case "ANCHOR_LOT_AWARD":
                    Console.WriteLine("天选之人");
                    break;
                case "LIVE":
                    Console.WriteLine("开始直播（？");
                    Console.WriteLine(command);
                    break;

                default:
                    // 让我康康
                    Console.WriteLine(command);
                    break;
            }
            return true;
        }

        // 分析弹幕信息 分析进房信息

        private async Task<bool> InitHostServer()
        {
            var res = await _httpClient.GetAsync($"{DANMAKU_SERVER_CONF_URL}?id={_roomId}&type=0");

            if (res.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return false;
            }
            //Console.WriteLine(await res.Content.ReadAsStringAsync());
            return ParseDanmakuServerConf(await res.Content.ReadAsStringAsync());
        }

        private bool ParseDanmakuServerConf(string data)
        {

            try
            {
                var json = JsonDocument.Parse(data);

                var jsondata = json.RootElement.GetProperty("data");
                _hostServerList = jsondata.GetProperty("host_list");
                _hostServerToken = jsondata.GetProperty("token").GetString();

            }
            catch
            {
                Console.WriteLine("获取服务器配置失败....");
                return false;
            }
            for (int i = 0; i < _hostServerList.GetArrayLength(); ++i)
            {
                //Console.WriteLine(_hostServerList[i]);
            }

            return true;
        }

        private bool Init()
        {
            InitHostServer().Wait();

            int retry_cnt = 0;

            var test = $"wss://{_hostServerList[retry_cnt % 3].GetProperty("host").GetString()}:{_hostServerList[retry_cnt % 3].GetProperty("wss_port").GetInt32()}/sub";
            Uri webSocketUri;
            if (!Uri.TryCreate(test.Trim(), UriKind.Absolute, out webSocketUri))
            {
                //NotifyUser("Error: Invalid URI", NotifyType.ErrorMessage);
                //return null;
            }
            _webSocket
                .ConnectAsync(webSocketUri, CancellationToken.None).Wait();


            //        _webSocket
            //.ConnectAsync(new Uri($"wss://{_host_server_list[retry_cnt % 3].GetProperty("host").GetString()}:{_host_server_list[retry_cnt%3].GetProperty("wss_port").GetInt32()}/sub"), CancellationToken.None).Wait();
            SendAuth().Wait();

            var rcvBytes = new byte[1280];
            var rcvBuffer = new ArraySegment<byte>(rcvBytes);

            _heartTimer = new Timer(new TimerCallback(async _ => await SendHeartMsg()), null, 0, 30000);
            while (true)
            {



                WebSocketReceiveResult rcvResult = _webSocket.ReceiveAsync(rcvBuffer, cts.Token).Result;
                //_webSocket.ReceiveAsync(rcvBuffer, CancellationToken.None).Wait();
                byte[] msgBytes = rcvBuffer.Skip(rcvBuffer.Offset).Take(rcvResult.Count).ToArray();
                byte[] msgBytes1 = rcvBuffer.Skip(16).Take(rcvResult.Count - 16).ToArray();
                string rcvMsg = Encoding.UTF8.GetString(msgBytes1);
                ReadMessage(msgBytes);
                //Console.WriteLine("Received: {0}", rcvMsg);

                //Task.Delay(1000).Wait();

            }

            return true;
        }

        private async Task<bool> SendAuth()
        {
            var authData = new
            {
                uid = 0,
                roomid = _roomId,
                protover = 2,
                platform = "web",
                clientver = "1.14.3",
                type = 2,
                key = _hostServerToken
            };
            //var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(authData));
            //await _webSocket?.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Binary, true, cts.Token);
            //await Task.Delay(500);
            //await _webSocket?.SendAsync(MakePackage(authData, Operation.AUTH), WebSocketMessageType.Binary, true, cts.Token);
            await _webSocket?.SendAsync(MakePackage(authData, Operation.AUTH), WebSocketMessageType.Binary, true, CancellationToken.None);
            return true;
        }

        private async Task<bool> SendHeartMsg()
        {
            //if (we)
            var HeartData = new
            {
            };
            try
            {
                //await _webSocket?.SendAsync(MakePackage(HeartData, Operation.HEARTBEAT), WebSocketMessageType.Binary, true, cts.Token);
                await _webSocket?.SendAsync(MakePackage(HeartData, Operation.HEARTBEAT), WebSocketMessageType.Binary, true, CancellationToken.None);
            }
            catch
            {
                Console.WriteLine("心跳失败");
                //throw;
            }
            //var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(authData));
            //await _webSocket?.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Binary, true, cts.Token);



            return true;
        }

        private async Task<bool> Init_room()
        {
            await InitHostServer();
            return true;
        }



        private async Task<bool> InitRoomIdAndOwner()
        {
            //HttpClient _httpClient1 = new HttpClient();
            //_httpClient1.DefaultRequestHeaders.Referrer = nu
            var res = await _httpClient.GetAsync($"{ROOM_INIT_URL}?room_id={_roomId}");

            if (res.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return false;
            }
            return ParseRoomInit(await res.Content.ReadAsStringAsync());
        }


        private bool ParseRoomInit(string data)
        {
            var json = JsonDocument.Parse(data);
            if (json.RootElement.GetProperty("code").GetInt32() != 0)
            {
                return false;
            }
            if (json.RootElement.TryGetProperty("data", out JsonElement roomData))
            {
                JsonElement roomInfo = roomData.GetProperty("room_info");
                _roomId = roomInfo.GetProperty("room_id").GetInt32();
                _roomShortId = roomInfo.GetProperty("short_id").GetInt32();
                _roomOwnerId = roomInfo.GetProperty("uid").GetInt32();
                _roomTitle = roomInfo.GetProperty("title").GetString();
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}
