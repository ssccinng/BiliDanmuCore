using BiliBiliDanmuWpf.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiliBiliDanmuCore;
using BiliBiliDanmuWpf.MVVM.Model;
using System.Threading;

namespace BiliBiliDanmuWpf.MVVM.ViewModel
{
    class MainViewModel: ObservableObject
    {

        Timer _timer;
        
        private List<BiliBiliDanmuCore.BiliBiliDanmu> _biliBiliDanmus;
        public List<BiliBiliDanmuCore.BiliBiliDanmu> BiliBiliDanmus
        {
            get => _biliBiliDanmus; set
            {
                _biliBiliDanmus = value;
                OnPropertyChanged();

            }
        }
        public BiliBiliClientWpf _client;
        public MainViewModel()
        {
            //_client = new BiliBiliClientWpf(213);
            //_client = new BiliBiliClientWpf(697);
            //_client = new BiliBiliClientWpf(22746343);
            //_client = new BiliBiliClientWpf(697);
            //_client = new BiliBiliClientWpf(7317568);
            //_client = new BiliBiliClientWpf(38048);
            _client = new BiliBiliClientWpf(3444818);
            //_client = new BiliBiliClientWpf(21685677);
            //_client = new BiliBiliClientWpf(21452505);
            //_client = new BiliBiliClientWpf(22301377);
            //_client = new BiliBiliClientWpf(88615);
            //_client = new BiliBiliClientWpf(21320551);
            //_client = new BiliBiliClientWpf(47867);
            //_client = new BiliBiliClientWpf(55);
            //_client = new BiliBiliClientWpf(22333522);
            //_client = new BiliBiliClientWpf(153018);
            //_client = new BiliBiliClientWpf(22301377);
            //_client = new BiliBiliClientWpf(353398);
            _client.Start();
            _client.soso += RefreshDanmu;


        }

        public void RefreshDanmu(BiliBiliDanmu biliBiliDanmu)
        {
            // 可以直接add（？
            BiliBiliDanmus = _client.DanmuList.ToList();
        }
    }
}
