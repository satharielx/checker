using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace checker
{
    enum AccountStatus { 
        SUSPENDED = 0,
        NOT_SUSPENDED = 1,
        SERVER_ERROR = 2,
        UNKNOWN = 3
    }
    internal class LeagueClientConnection : Connection, IDisposable
    {
        private readonly string exePath;
        public Dictionary<string, object> ritoCredentials = null;
        private readonly string region;
        private readonly Client client;
        private readonly string username;
        private readonly string password;
        public int ProcessHandleId { get; private set; }
        private static readonly object _lock = new object();

        public LeagueClientConnection(string exePath, Client client, string region, string username, string password)
        {
            lock (_lock) {
                this.exePath = exePath;
                this.region = region;
                this.client = client;
                this.username = username;
                this.password = password;
            }
        }

        public async Task<AccountStatus> Run()
        {
            CreateLeagueClient();
            Thread.Sleep(10000);
            var isCreated = await WaitForSession(2);
            if (isCreated == AccountStatus.SERVER_ERROR)
            {
                Console.WriteLine("Failed to create League session. Stopping thread.");
                return isCreated;
            }

            return isCreated;
        }

        private async Task<AccountStatus> WaitForSession(int timeout=12)
        {

            var state = AccountStatus.UNKNOWN;
            int attempts = 1;
            while(timeout > 0)
            {
                try
                {
                    //Console.WriteLine($"Initiating attempt of checking suspension #{attempts}");
                    var response = await RequestAsync(HttpMethod.Get, "/lol-login/v1/account-state", null);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = JToken.Parse(await response.Content.ReadAsStringAsync());

                        if (Config.DEBUG_ENABLED)
                            Console.WriteLine($"[DEBUG] Session - Username: {username} - {response.StatusCode} - \n{await response.Content.ReadAsStringAsync()}\n\n");

                        if (result["state"].ToString().ToLower() == "SUCCEEDED".ToLower())
                        {
                            Thread.Sleep(2000);

                            
                            
                            state = AccountStatus.NOT_SUSPENDED;
                            break;
                        }

                        if (result["state"].ToString().ToLower() == "ERROR".ToLower())
                        {
                            if (result["error"]["messageId"].ToString().ToLower() == "ACCOUNT_BANNED".ToLower())
                            {
                                MainInfo.FailedAccounts += 1;
                                
                                state = AccountStatus.SUSPENDED;
                                break;
                            }
                            else if (result["error"]["messageId"].ToString().ToLower() == "FAILED_TO_COMMUNICATE_WITH_LOGIN_QUEUE".ToLower())
                            {
                                Console.WriteLine("Failed to communicate with login queue. Re-added to queue");
                                state = AccountStatus.SERVER_ERROR;
                                break;
                            }
                        }
                        
                    }
                    Console.WriteLine($"[DEBUG] Response: {await response.Content.ReadAsStringAsync()}");
                    timeout--;
                    attempts++;
                    Thread.Sleep(1500);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());   
                    timeout--;
                    attempts++;
                }
            }
            //Console.WriteLine("Session Timed Out");
            return state;
        }

        private void CreateLeagueClient()
        {
            lock (_lock)
            {
                var processArgs = new List<string> {
                    exePath,
                    "--riotclient-app-port=" + ritoCredentials["riotPort"],
                    "--riotclient-auth-token=" + ritoCredentials["riotAuthToken"],
                    "--app-port=" + port,
                    "--remoting-auth-token=" + authToken,
                    "--allow-multiple-clients",
                    "--locale=en_GB",
                    "--disable-self-update",
                    "--region=" + region,
                    "--headless"
                };

               ProcessHandleId = client.CreateInstance(processArgs, exePath);
            }
        }
    }
}
