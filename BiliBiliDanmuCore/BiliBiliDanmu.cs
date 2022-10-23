using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliBiliDanmuCore
{

    public enum DanmuType
    {
        Join,
        Msg,
        Gift,
    }

    public class BiliBiliDanmu
    {
        public string Message { get; set; }
        public string Username { get; set; }
        public int UID { get; set; }

        public string MedalName { get; set; }
        public int MedalLevel { get; set; }
        public string GiftName { get; set; }
        public int GiftNum { get; set; }
        public string MedalUP { get; set; }
        public DanmuType DanmuType { get; set; }

        public override string ToString() => DanmuType switch
        {
            DanmuType.Join => $"欢迎 {Username} 进入直播间",
            DanmuType.Msg => $"{Username}: {Message}",
            DanmuType.Gift => $"感谢 {Username} 赠送的 {GiftName}{GiftNum}个",
            _ => base.ToString()
        };
    }
}
