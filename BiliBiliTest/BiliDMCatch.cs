using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

using System.Net.Http;
using System.Threading;
using System.IO.Compression;
using System.IO;

namespace BiliBiliTest
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
    class BiliDMCatch
    {
        static string ROOM_INIT_URL = "https://api.live.bilibili.com/xlive/web-room/v1/index/getInfoByRoom";
        static string DANMAKU_SERVER_CONF_URL = "https://api.live.bilibili.com/xlive/web-room/v1/index/getDanmuInfo";


        private ClientWebSocket _webSocket = new();
        private int _roomId;
        private HttpClient _httpClient = new();
        CancellationTokenSource cts = new CancellationTokenSource(350000);

        private JsonElement _host_server_list;
        private string _host_server_token;

        private Timer _heartTimer;
        public BiliDMCatch(int roomId)
        {
            _roomId = roomId;
            Init();
        }

        //ArraySegment<byte>
        private byte[] MakePackage(object data, Operation operation)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));

            byte[] PacketLength = BitConverter.GetBytes(16 + body.Length);
            byte[] HeaderLength = BitConverter.GetBytes((short)16);
            byte[] Protocol = BitConverter.GetBytes((short)1);
            byte[] operationt = BitConverter.GetBytes((int)operation);
            byte[] SequenceId= BitConverter.GetBytes(1);

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

            for (int i = 0; i < res.Count; ++i)
            {
                Console.Write($"\\x{res[i]:X2}");
            }
            Console.WriteLine();
            //Console.WriteLine(res);
            res.AddRange(body);
            return res.ToArray();
        }
        
        private async Task ReadMessage(byte[] data)
        {
            int PacketLength = BitConverter.ToInt32(data[..4].Reverse().ToArray());
            short HeaderLength = BitConverter.ToInt16(data[4..6].Reverse().ToArray());
            PROTOCOLVERSION Protocol = (PROTOCOLVERSION)BitConverter.ToInt16(data[6..8].Reverse().ToArray());
            Operation operationt = (Operation)BitConverter.ToInt32(data[8..12].Reverse().ToArray());
            int SequenceId = BitConverter.ToInt32(data[12..16].Reverse().ToArray());
            var body = data[16..];

            switch (operationt)
            {
                case Operation.HANDSHAKE:
                    break;
                case Operation.HANDSHAKE_REPLY:
                    break;
                case Operation.HEARTBEAT:
                    break;
                case Operation.HEARTBEAT_REPLY:
                    break;
                case Operation.SEND_MSG:
                    break;
                case Operation.SEND_MSG_REPLY:
                    break;
                case Operation.DISCONNECT_REPLY:
                    break;
                case Operation.AUTH:
                    break;
                case Operation.AUTH_REPLY:
                    await SendHeartMsg();
                    break;
                case Operation.RAW:
                    break;
                case Operation.PROTO_READY:
                    break;
                case Operation.PROTO_FINISH:
                    break;
                case Operation.CHANGE_ROOM:
                    break;
                case Operation.CHANGE_ROOM_REPLY:
                    break;
                case Operation.REGISTER:
                    break;
                case Operation.REGISTER_REPLY:
                    break;
                case Operation.UNREGISTER:
                    break;
                case Operation.UNREGISTER_REPLY:
                    break;
                default:
                    break;
            }

            switch (Protocol)
            {
                case PROTOCOLVERSION.JSON:
                    Console.WriteLine("Received: {0}", Encoding.UTF8.GetString(body));
                    break;
                case PROTOCOLVERSION.POPULARITY:
                    Console.WriteLine($"当前人气值: {BitConverter.ToInt32(body.Reverse().ToArray())}");
                    break;
                case PROTOCOLVERSION.ZLIB:

                    Console.WriteLine($"bodylen = {body.Length}");

                    MemoryStream input = new MemoryStream(body);
                    MemoryStream output = new MemoryStream();

                    using (var inf = new Ionic.Zlib.ZlibStream(input, Ionic.Zlib.CompressionMode.Decompress, Ionic.Zlib.CompressionLevel.Default, true))
                    {
                        inf.CopyTo(output);
                        var byteArray = new byte[output.Length];
                        var gg = output.ToArray();
                        await ReadMessage(gg);
                    }
                    //using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
                    //{
                    //    dstream.CopyTo(output);
                    //    var gg = output.ToArray();
                    //    ReadMessage(gg);
                        
                    //    //output.Read(byteArray, 0, (int)output.Length);
                    //}
                    //if (body.Length < 300) return;
                    //MemoryStream input = new MemoryStream(1000);
                    //MemoryStream input = new MemoryStream(body);
                    ////input.Write(body, 0, body.Length);
                    //MemoryStream output = new MemoryStream(1000);
                    //using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
                    //{
                    //    dstream.CopyTo(output);
                    //    var byteArray = new byte[output.Length];
                    //    output.Read(byteArray, 0, (int)output.Length);
                    //}

                    //using (MemoryStream memStream = new MemoryStream(1000))
                    //{
                    //    // Write the first string to the stream.
                    //    var byteArray = new byte[output.Length];
                    //    memStream.Read(byteArray, 0, (int)output.Length);


                    //    //memStream.Write(body, 0, body.Length);
                    //    //DeflateStream decompressionStream = new(memStream, CompressionMode.Decompress);
                    //    //StreamReader reader = new StreamReader(decompressionStream);

                    //    //var byteArray = new byte[memStream.Length];
                    //    //memStream.Read(byteArray, 0, (int)memStream.Length);
                    //    //string text = reader.ReadToEnd();
                    //    //Console.WriteLine(text);
                    //}
                    //using (DeflateStream decompressionStream = new DeflateStream(body.getse, CompressionMode.Decompress))
                    //{
                    //    decompressionStream.CopyTo(decompressedFileStream);
                    //    Console.WriteLine("Decompressed: {0}", fileToDecompress.Name);
                    //}
                    //using (FileStream originalFileStream = fileToDecompress.OpenRead())
                    //{
                    //    string currentFileName = fileToDecompress.FullName;
                    //    string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

                    //    using (FileStream decompressedFileStream = File.Create(newFileName))
                    //    {
                    //        using (DeflateStream decompressionStream = new DeflateStream(originalFileStream, CompressionMode.Decompress))
                    //        {
                    //            decompressionStream.CopyTo(decompressedFileStream);
                    //            Console.WriteLine("Decompressed: {0}", fileToDecompress.Name);
                    //        }
                    //    }
                    //}

                    break;
                case PROTOCOLVERSION.BROTLI:
                    Console.WriteLine($"欧拉2");
                    break;
                default:
                    break;
            }
        }

        private async Task<bool> Init_host_server()
        {
            var res = await _httpClient.GetAsync($"{DANMAKU_SERVER_CONF_URL}?id={_roomId}&type=0");

            if (res.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return false;
            }
            //Console.WriteLine(await res.Content.ReadAsStringAsync());
            Parse_danmaku_server_conf(await res.Content.ReadAsStringAsync());
            return true;
        }

        private bool Parse_danmaku_server_conf(string data)
        {

            try
            {
                var json = JsonDocument.Parse(data);

                var jsondata = json.RootElement.GetProperty("data");
                _host_server_list = jsondata.GetProperty("host_list");
                _host_server_token = jsondata.GetProperty("token").GetString();

            }
            catch
            {
                Console.WriteLine("获取服务器配置失败....");
                return false;
            }
            for (int i = 0; i < _host_server_list.GetArrayLength(); ++i)
            {
                Console.WriteLine(_host_server_list[i]);
            }

            return true;
        }

        private bool Init()
        {
            Init_host_server().Wait();

            int retry_cnt = 0;

            //var test = $"wss://{_host_server_list[retry_cnt].GetProperty("host").GetString()}:{_host_server_list[retry_cnt].GetProperty("wss_port").GetInt32()}/sub";




            //_webSocket
            //.ConnectAsync(new Uri($"wss://hw-sh-live-comet-02.chat.bilibili.com/sub"), CancellationToken.None).Wait();


            var test = $"wss://{_host_server_list[retry_cnt % 3].GetProperty("host").GetString()}:{_host_server_list[retry_cnt % 3].GetProperty("wss_port").GetInt32()}/sub";
            Uri webSocketUri;
            if (!Uri.TryCreate(test.Trim(), UriKind.Absolute, out webSocketUri))
            {
                //NotifyUser("Error: Invalid URI", NotifyType.ErrorMessage);
                //return null;
            }
            //_webSocket
            //    .ConnectAsync(webSocketUri, CancellationToken.None).Wait();
            _webSocket
                .ConnectAsync(webSocketUri, cts.Token).Wait();


            //        _webSocket
            //.ConnectAsync(new Uri($"wss://{_host_server_list[retry_cnt % 3].GetProperty("host").GetString()}:{_host_server_list[retry_cnt%3].GetProperty("wss_port").GetInt32()}/sub"), CancellationToken.None).Wait();
            SendAuth().Wait();

            var rcvBytes = new byte[1280];
            var rcvBuffer = new ArraySegment<byte>(rcvBytes);

            _heartTimer = new Timer(new TimerCallback(async _ => await SendHeartMsg()), null, 0, 20000);
            while (true)
            {

                

                WebSocketReceiveResult rcvResult = _webSocket.ReceiveAsync(rcvBuffer, cts.Token).Result;
                //_webSocket.ReceiveAsync(rcvBuffer, CancellationToken.None).Wait();
                byte[] msgBytes = rcvBuffer.Skip(rcvBuffer.Offset).Take(rcvResult.Count).ToArray();
                byte[] msgBytes1 = rcvBuffer.Skip(16).Take(rcvResult.Count - 16).ToArray();
                string rcvMsg = Encoding.UTF8.GetString(msgBytes1);
                ReadMessage(msgBytes).Wait();
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
                key = _host_server_token
            };
            //var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(authData));
            //await _webSocket?.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Binary, true, cts.Token);
            await Task.Delay(1000);
            await _webSocket?.SendAsync(MakePackage(authData, Operation.AUTH), WebSocketMessageType.Binary, true, cts.Token);
            return true;
        }

        private async Task<bool> SendHeartMsg()
        {
            var HeartData = new
            {
            };
            //var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(authData));
            //await _webSocket?.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Binary, true, cts.Token);
            await _webSocket?.SendAsync(MakePackage(HeartData, Operation.HEARTBEAT), WebSocketMessageType.Binary, true, cts.Token);


            return true;
        }

        private async Task<bool> Init_room()
        {
            await Init_host_server();
            return true;
        }



        private async Task<bool> Init_room_id_and_owner()
        {
            return true;
        }
    }
}
