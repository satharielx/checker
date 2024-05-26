using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using System.Net;
using System.Security.Authentication;
using Newtonsoft.Json.Linq;
using System.Data.Common;
using Polly.Timeout;

namespace checker
{
    public class AuthenticateResponse
    {
        public string Type { get; set; }
        public string Error { get; set; }
        public string Success { get; set; }
    }

    public class AuthenticateRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool PersistLogin { get; set; }
        public bool Remember { get; set; }
        public string Language { get; set; }
        public string Captcha { get; set; }
    }

    public class LoginRequest
    {
        public string AuthenticationType { get; set; }
        public string LoginToken { get; set; }
        public bool PersistLogin { get; set; }
    }

    public class CaptchaInfo
    {
        public string Key { get; set; }
        public string Data { get; set; }
    }

    public class StartAuthResponse
    {
        public CaptchaInfo Captcha { get; set; }
    }

    internal class RiotClient
    {
        private bool allowPatching = false;
        private string filePath;
        private int runningPort;
        private string userAgent;
        private readonly Client currentClient;
        private readonly RiotConnection connection;
        private readonly object _lock = new object();
        public RiotClient(RiotConnection connection, Client client)
        {
            lock (_lock) {
                this.connection = connection;
                this.currentClient = client;
            }
            

            //Tuple<string, bool> versionResult = Version.GetFileVersion(filePath);
            //Console.WriteLine($"[DEBUG] Version Info: {versionResult.Item1}, {versionResult.Item2}");
            //this.userAgent = $"Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) RiotClient/{versionResult.Item1} (CEF 74) Safari/537.36";

        }

        private async Task<bool> AuthenicateAsync(string username, string password, int timeout = 8)
        {
            var data = new Dictionary<string, object>()
            {
                {"username", username},
                {"password", password},
                {"persistLogin", false},
            };

            while(timeout > 0)
            {
                var response = await connection.RequestAsync(HttpMethod.Put, "/rso-auth/v1/session/credentials", data);
                var content = JToken.Parse(await response.Content.ReadAsStringAsync());
                if(response.IsSuccessStatusCode)
                {
                    if (Config.DEBUG_ENABLED)
                    {
                        Console.WriteLine($"[DEBUG] Username: {username} - {response.StatusCode} - \n{await response.Content.ReadAsStringAsync()}\n\n");
                    }
                    if(content.SelectToken("error") != null)
                    {
                        if (content["error"].ToString() == "auth_failure") {
                            MainInfo.FailedAccounts++;
                            return false;
                        }
                        if (content["error"].ToString() == "rate_limited")
                        {
                            Console.WriteLine("Rate limited. Use VPN.");
                            return false;
                        }
                    }
                    if (content.SelectToken("errorCode") != null)
                    {
                        if (content["errorCode"].ToString() == "RPC_ERROR")
                        {
                            Console.WriteLine("Riot Session error. Try again later.");
                            currentClient.CloseClients();
                            throw new Exception("Riot API Error. Try again in 5-10 minutes or use a VPN/Proxy");
                        }
                    }
                    
                    return true;

                }
                Thread.Sleep(1500);
                timeout--;
            }
            return false;
        }
        private async void AcceptEULA(int timeout = 10)
        {
            while (timeout > 0)
            {
                var response = await connection.RequestAsync(HttpMethod.Put, "/eula/v1/agreement/acceptance", null);

                if (response.IsSuccessStatusCode)
                    return;

                Thread.Sleep(1000);
                timeout--;
            }
        }

        public async Task<bool> Login(string username, string password) {
            bool canAuth = await AuthenicateAsync(username, password);

            if (!canAuth)
                return false;

            AcceptEULA();

            return true;
        }

    }
}
