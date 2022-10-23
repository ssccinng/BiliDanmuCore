using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
namespace BiliBiliDanmuCore
{
    public static class BiliBiliTools
    {
        static DateTime DateTime = DateTime.Now.AddMinutes(-20);
        static DateTime lastDateTime = DateTime;
        public static async Task<string> GetAvatarURL(int uid)
        {
            if (DateTime.AddMinutes(16) > DateTime.Now) return null;
            while (DateTime.Now - lastDateTime < TimeSpan.FromSeconds(1)) await Task.Delay(1000);
            lastDateTime = DateTime.Now;
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage data;
            try
            {
                data = await httpClient.GetAsync($"https://api.bilibili.com/x/space/acc/info?mid={uid}");
            }
            catch
            {
                DateTime = DateTime.Now;
                return null;
            }
            if (data.StatusCode != System.Net.HttpStatusCode.OK)
            {
                DateTime = DateTime.Now;
                return null;
            }
            
            // 返回默认图片
            var jsondata = JsonDocument.Parse(await data.Content.ReadAsStringAsync()).RootElement.GetProperty("data");
            if (jsondata.TryGetProperty("face", out JsonElement url))
            {
                return url.GetString();
            }
            return null;
        }
    }
}
