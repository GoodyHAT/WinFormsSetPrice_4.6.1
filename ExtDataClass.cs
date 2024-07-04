using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsSetPrice
{
    internal class ExtDataClass
    {
        public static string token = "";
        public static long token_expires_in = 0;
        public static string webaddress = "";
        public static string user = "";
        public static string pass = "";

        internal static async Task GetTokenAsync()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{webaddress}/Token");
            //request.Headers.Add("Authorization", "Bearer null");
            //request.Headers.Add("Cookie", ".AspNet.Cookies=TfX_ujaipgORcrfXS6RtndsAynKcV37plb_4OhEYGdcYl3boilmFPBDJInEK6xfEz9r_dwpRzzJPQVCg4Udz3nBcj3xgOVWhLpJ8FzomQNg-Q4lUjB_518-mA3rpp-J3LzFwzBNLOfRI3NTYBBR-0o5y0s1ae2A7TQYcLn2Iy2sfv4kY6iN55pxEB92Hm3PNlt4oEK3FJ05xK2TAlNw6bw_NMCTa3Fr3Q6FqLuo4U2tugSyMOLWeFkp5CgDC_uOQFzxdnpsE9hnZVQlsiNulNnfDPYzw1EgcpDkgbzKJUTK8kKZV_IghJ0KhOBwWPph7tCF5k0rnRBqy_XN2CCm3Kmjvl0tfR9zFX8buhfaUB7vy9FFQ6beOlR3wtURu2BtSiXiIdzjVJCinrQG3fgdOvVMWSXgm5FdNe3577Dnd6OONUQ734ZpJKGgkTZqAiDzaj8eCdY50Wm5zjz_6Q4Gwdj5p-8gVlIAoQevi_OXBK6o");
            var collection = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", user),
                new KeyValuePair<string, string>("password", pass)
            };
            var content = new FormUrlEncodedContent(collection);
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            JObject data = JObject.Parse(json);

            if (data != null)
            {
                var access_token = ((JValue)data["access_token"]).Value;
                var expires_in = ((JValue)data["expires_in"]).Value;

                if (access_token != null && expires_in != null)
                {
                    token = access_token.ToString();
                    token_expires_in = DateTime.Now.Ticks + (long)expires_in - 60;
                }
            }
        }

        public static async Task<List<Models.agzsClass>> GetAgzsAsync()
        {
            try
            {
                if (token_expires_in == 0 || DateTime.Now.Ticks < token_expires_in)
                    await GetTokenAsync();

                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, $"{webaddress}/api/GasStations");
                request.Headers.Add("Authorization", $"Bearer {token}");
                var content = new StringContent("", null, "application/json");
                //request.Content = content;
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                JObject data = JObject.Parse(json);
                var v = data["agzs"].ToObject<List<Models.agzsClass>>();
                if (v != null)
                    return v;

                return new List<Models.agzsClass>();
            }
            catch
            { return null; }
        }

        internal static async Task<bool> SetPrice(string agzsid, decimal d)
        {
            if (token_expires_in == 0 || DateTime.Now.Ticks < token_expires_in)
                await GetTokenAsync();

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{webaddress}/api/GasStations/SetPrice");
            request.Headers.Add("Authorization", $"Bearer {token}");
            var content = new StringContent($"{{\"price_change\":\"{(d.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture))}\", \"agzsid\":\"{agzsid}\"}}", null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();            

            return true;
        }
    }
}
