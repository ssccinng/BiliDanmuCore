using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliBiliDanmuCore
{
    public class BiliBiliDanmu
    {
        public string Message { get; set; }
        public string Username { get; set; }
        public int UID { get; set; }

        public string MedalName { get; set; }
        public string MedalLevel { get; set; }
        public override string ToString()
        {
            return $"{Username}: {Message}";
        }
    }
}
