using BiliBiliDanmuCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestRazor.Pages
{
    public class PrivacyModel : PageModel
    {
        private readonly ILogger<PrivacyModel> _logger;
        BiliBiliLiveDanmuClient BiliBiliLiveDanmuClient = new BiliBiliLiveDanmuClient(7317568);
        public PrivacyModel(ILogger<PrivacyModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            
            BiliBiliLiveDanmuClient.Start();
        }


    }
}
