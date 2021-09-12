using System;
using System.Collections.Generic;
using System.IO;
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


        private ClientWebSocket _webSocket = new();
        private int _roomId;
        private HttpClient _httpClient = new();
        CancellationTokenSource cts = new CancellationTokenSource(35000);

        private JsonElement _hostServerList;
        private string _hostServerToken;

        private Timer _heartTimer;

        public BiliBiliLiveDanmuClient(int roomId)
        {
            _roomId = roomId;
        }

        public async Task Start()
        {
            await InitHostServer();
            // 开始连接
            int retryCnt = 0;

            _heartTimer = new Timer(new TimerCallback(async _ => await SendHeartMsg()), null, 10000, 10000);
            while (true)
            {
                try
                {
                    //_httpClient = new();
                    //if (!await InitHostServer())
                    //{
                    //    int aa = 1;
                    //}


                        var BiliDanmuServer =
                        $"wss://{_hostServerList[retryCnt % _hostServerList.GetArrayLength()].GetProperty("host").GetString()}:{_hostServerList[retryCnt % _hostServerList.GetArrayLength()].GetProperty("wss_port").GetInt32()}/sub";
                    if (!Uri.TryCreate(BiliDanmuServer.Trim(), UriKind.Absolute, out Uri webSocketUri))
                    {
                    }
                    //_webSocket = new ClientWebSocket();
                    await _webSocket
                        .ConnectAsync(webSocketUri, cts.Token);
                    //await _webSocket
                    //    .ConnectAsync(webSocketUri, CancellationToken.None);
                    Console.WriteLine("建立连接成功!");
                    
                   await SendAuth();
                    await SendHeartMsg();
                    Console.WriteLine("认证成功!");
                    var rcvBytes = new byte[29000];
                    var rcvBuffer = new ArraySegment<byte>(rcvBytes);
                    while (true)
                    {


                        
                        WebSocketReceiveResult rcvResult =  await _webSocket.ReceiveAsync(rcvBuffer, CancellationToken.None);
                        Console.WriteLine("接受成功!");
                        if (rcvResult?.MessageType != WebSocketMessageType.Binary)
                        {
                            Console.WriteLine("未知信息");
                            continue;
                        }
                        byte[] msgBytes = rcvBuffer.Skip(rcvBuffer.Offset).Take(rcvResult.Count).ToArray();
                        Console.WriteLine("转换成功!");
                        //byte[] msgBytes1 = rcvBuffer.Skip(16).Take(rcvResult.Count - 16).ToArray();
                        //string rcvMsg = Encoding.UTF8.GetString(msgBytes1);
                        await ReadMessage(msgBytes);
                        Console.WriteLine("处理成功!");


                    }
                }
                catch (Exception e)
                {

                    //try

                    //{
                    //    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", cts.Token);
                    //}
                    //catch (Exception)
                    //{

                    //    //throw;
                    //}
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

            Console.WriteLine(JsonSerializer.Serialize(data));

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
            while (offset < data.Length)
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
                        Console.WriteLine($"Received: {Encoding.UTF8.GetString(body)}\n\n");
                        break;
                    case PROTOCOLVERSION.POPULARITY:
                        Console.WriteLine($"当前人气值: {BitConverter.ToInt32(body.Reverse().ToArray())}");
                        break;
                    case PROTOCOLVERSION.ZLIB:

                        MemoryStream input = new MemoryStream(body);
                        MemoryStream output = new MemoryStream();

                        using (var inf = new Ionic.Zlib.ZlibStream(input, Ionic.Zlib.CompressionMode.Decompress, Ionic.Zlib.CompressionLevel.Default, true))
                        {
                            inf.CopyTo(output);
                            var byteArray = new byte[output.Length];
                            var gg = output.ToArray();
                            await ReadMessage(gg);
                        }
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

        private async Task<bool> InitHostServer()
        {
            var res = await _httpClient.GetAsync($"{DANMAKU_SERVER_CONF_URL}?id={_roomId}&type=0");

            if (res.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return false;
            }
            //Console.WriteLine(await res.Content.ReadAsStringAsync());
            return Parse_danmaku_server_conf(await res.Content.ReadAsStringAsync());
        }

        private bool Parse_danmaku_server_conf(string data)
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
                Console.WriteLine(_hostServerList[i]);
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



        private async Task<bool> Init_room_id_and_owner()
        {
            return true;
        }
    }
}
