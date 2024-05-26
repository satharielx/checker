using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace checker
{
    internal class TaskScheduler
    {
        private readonly Client currentClient;
        private Dictionary<string, object> _riotClientCredentials;
        private string _region;
        private static readonly object _lock = new object();
        public TaskScheduler() { 
            lock(_lock)
            {
                currentClient = new Client();
            }
        }

        public async Task<bool> InitCheck(Account a, string username, string password, string ritoClientPath = "C:\\Riot Games\\Riot Client\\RiotClientServices.exe")
        {
            try
            {
                Client client = new Client();
                RiotConnection ritoTest = new RiotConnection(username, password, client, ritoClientPath);



                var didSucceed = await ritoTest.RunAsynchronous();

                if (!didSucceed)
                {
                    
                    Console.WriteLine("Couldn't start Riot Client.");
                    ritoTest.Dispose();
                    return false;
                }
                else
                {
                    var ritoCredentials = ritoTest.GetCredentials();

                    RiotClient ritoClient = new RiotClient(ritoTest, client);

                    bool successfullyLoggedIn = await ritoClient.Login(username, password);
                    if (!successfullyLoggedIn)
                    {
                        Console.WriteLine("[DEBUG] Login: Wrong credentials.");
                        client.CloseClient(ritoTest.ProcessHandleId);
                        ritoTest.Dispose();
                        return false;

                    }
                    else
                    {
                        var region = await ritoTest.RequestRegionAsync();
                        

                        LeagueClientConnection lolConnection = new LeagueClientConnection("C:\\Riot Games\\League of Legends\\LeagueClient.exe", client, region, username, password)
                        {
                            ritoCredentials = ritoCredentials
                        };

                        var isSuccessfullyRunning = await lolConnection.Run();

                        if (isSuccessfullyRunning == AccountStatus.SUSPENDED || isSuccessfullyRunning == AccountStatus.SERVER_ERROR || isSuccessfullyRunning == AccountStatus.UNKNOWN)
                        {
                            Console.WriteLine($"[ACCOUNT INFO]");
                            Console.WriteLine($"Username: {ritoTest.Username}");
                            Console.WriteLine($"Password: {ritoTest.Password}");
                            Console.WriteLine($"Region: {region}");
                            Console.WriteLine($"Suspended: {isSuccessfullyRunning}");

                            client.CloseClients();
                            lolConnection.Dispose();
                            
                            
                            Console.WriteLine("[DEBUG] Session closed, because the account is either suspended or the server was offline.");
                            return false;
                        }
                        else
                        {
                            client.CloseClients(); 
                            lolConnection.Dispose();
                            ritoTest.Dispose();
                            Console.WriteLine($"[ACCOUNT INFO]");
                            Console.WriteLine($"Username: {ritoTest.Username}");
                            Console.WriteLine($"Password: {ritoTest.Password}");
                            Console.WriteLine($"Region: {region}");
                            Console.WriteLine($"Suspended: {isSuccessfullyRunning}");
                            a.Region = region;
                            a.Status = isSuccessfullyRunning;
                            return true;
                        }
                    }
                }

                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
            
        }
        
    }
}
