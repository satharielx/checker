using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace checker
{
    internal class RiotConnection : Connection, IDisposable
    {
        public string RiotClientPath;
        public int ProcessHandleId { get; private set; }
        private static readonly object _lock = new object();
        private readonly Client CurrentClient;
        public string Username;
        public string Password;
        public RiotConnection(string username, string password, Client client, string riotClientPath)
        {
            lock(_lock)
            {
                this.Username = username;
                this.Password = password;
                this.RiotClientPath = riotClientPath;
                this.CurrentClient = client;
            }
        }

        public async Task<bool> RunAsynchronous() {
            CreateRCInstance();
            Thread.Sleep(2000);
            var connected = await WaitForConnectionAsync();
            return !connected ? false : true;
        }

        private async Task<bool> WaitForConnectionAsync(int timeout=10)
        {
            while(timeout > 0)
            {
                try {
                    Dictionary<string, object> data = new Dictionary<string, object>() {
                        { "clientId", "riot-client"},
                        { "trustLevels", new List<string> { "always_trusted" } }
                    };

                    var response = await RequestAsync(HttpMethod.Post, "/rso-auth/v2/authorizations", data);
                    if(Config.DEBUG_ENABLED)
                    {
                        Console.WriteLine($"[DEBUG] Username: {Username} - {response.StatusCode} - \n{await response.Content.ReadAsStringAsync()}\n\n");
                    }
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }

                    timeout--;
                    Thread.Sleep(1000);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("HTTP Request exception at Authorization. Re adding to queue");
                    
                    return false;
                }
                catch (AggregateException e)
                {
                    Console.WriteLine(e.StackTrace);
                    
                    return false;
                }
               
            }
            return false;
        }

        private void CreateRCInstance()
        {
            lock(_lock)
            {
                List<string> processArgs = new List<string>() { 
                    RiotClientPath,
                    "--app-port=" + port,
                    "--remoting-auth-token=" + authToken,
                    "--launch-product=league_of_legends",
                    "--launch-patchline=live",
                    "--locale=en_GB",
                    "--disable-auto-launch",
                    "--headless",
                    "--allow-multiple-clients"
                };

                ProcessHandleId = CurrentClient.CreateInstance(processArgs, RiotClientPath);
            }
        }

        public Dictionary<string, object> GetCredentials()
        {
            Dictionary<string, object> credentials = new Dictionary<string, object>()
            {
                  { "riotPort", port },
                  { "riotAuthToken", authToken }
            };
            return credentials;
        }

        public async Task<string> RequestRegionAsync(int timeout = 10)
        {
            while (timeout > 0)
            {
                var response = await RequestAsync(HttpMethod.Get, "/riotclient/get_region_locale", null);

                if (Config.DEBUG_ENABLED)
                {
                    Console.WriteLine($"[DEBUG] Username: {Username} - {response.StatusCode} - \n{await response.Content.ReadAsStringAsync()}\n\n");
                }

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = JToken.Parse(await response.Content.ReadAsStringAsync());

                    return jsonResponse["region"].ToString();
                }
                timeout--;
            }

            throw new Exception("Error requesting region locale.");
        }
    }
}
