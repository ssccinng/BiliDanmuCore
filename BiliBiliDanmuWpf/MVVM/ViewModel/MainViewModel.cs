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
            _client = new BiliBiliClientWpf(7317568);
            //_client = new BiliBiliClientWpf(153018);
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
