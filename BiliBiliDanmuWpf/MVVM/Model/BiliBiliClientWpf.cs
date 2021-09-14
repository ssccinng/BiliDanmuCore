using BiliBiliDanmuCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliBiliDanmuWpf.MVVM.Model
{
    public class BiliBiliClientWpf : BiliBiliDanmuCore.BiliBiliLiveDanmuClient
    {
        public BiliBiliClientWpf(int roomId): base(roomId)
        {
        }

        protected override void AddDanmu(BiliBiliDanmu danmu)
        {
            base.AddDanmu(danmu);
            soso?.Invoke(danmu);
        }

        public event Action<BiliBiliDanmu> soso;
    }
}
