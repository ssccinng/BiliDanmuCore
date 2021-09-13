using BiliBiliDanmuCore;
using System;

namespace BiliBiliTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");


            //BiliBiliLiveDanmuClient biliBiliLiveDanmuClient = new(153018);
            //BiliBiliLiveDanmuClient biliBiliLiveDanmuClient = new(23531171);
            //BiliBiliLiveDanmuClient biliBiliLiveDanmuClient = new(7317568);
            //BiliBiliLiveDanmuClient biliBiliLiveDanmuClient = new(22490788);
            //BiliBiliLiveDanmuClient biliBiliLiveDanmuClient = new(1128);
            BiliBiliLiveDanmuClient biliBiliLiveDanmuClient = new(21470918);

            biliBiliLiveDanmuClient.Start().Wait();
            //BiliDMCatch bili = new BiliDMCatch("4604871");
            //BiliDMCatch bili = new BiliDMCatch(7317568);
            //BiliDMCatch bili = new BiliDMCatch(153018);
        }
    }
}
