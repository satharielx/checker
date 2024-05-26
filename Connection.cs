using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using Polly;
using System.Net;
using Newtonsoft.Json;
using System.Net.Security;

namespace checker
{
    internal class Connection : IDisposable
    {
        public string port;
        public string url;
        public string authToken;
        public readonly HttpClient webClient;
        private Process currentProcess;
        private HashSet<int> usedPorts = new HashSet<int>();
        public string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3";
        private static readonly object _lock = new object();
        public Connection()
        {
            lock (_lock) {
                this.port = Utils.FindAvailablePort(ref usedPorts);
                this.url = "https://127.0.0.1:" + this.port;
                this.authToken = Credentials.GetAuthToken();
                webClient = new HttpClient(BypassSSLHeader()) {
                    BaseAddress = new Uri(this.url)
                };
                webClient.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"riot:{authToken}")));
                if(Config.DEBUG_ENABLED)
                {
                    Console.WriteLine($"[DEBUG] New connection session spawned.\nAddress: {this.url}\nAuthenication Token (Base64): {authToken}");
                }
            }
            
            
        }

        public async Task<HttpResponseMessage> RequestAsync(HttpMethod method, string url, Dictionary<string, object> requestData)
        {
            HttpResponseMessage response = null;

            await Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.NotFound
                                                   || r.StatusCode == HttpStatusCode.InternalServerError
                                                   || (int)r.StatusCode == 429)
                .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
                .ExecuteAsync(() =>
                {
                    lock (_lock)
                    {
                        URLFixer(ref url);

                        var requestAddress = webClient.BaseAddress + url;
                        if(Config.DEBUG_ENABLED) 
                            Console.WriteLine($"Sending {method} request. URL: {url}");

                        switch (method.Method)
                        {
                            case "GET":
                                response = webClient.GetAsync(requestAddress).Result;
                                break;
                            case "POST":
                                var json = JsonConvert.SerializeObject(requestData);
                                var content = new StringContent(json, Encoding.UTF8, "application/json");
                                response = webClient.PostAsync(requestAddress, content).Result;
                                break;
                            case "PUT":
                                json = JsonConvert.SerializeObject(requestData);
                                content = new StringContent(json, Encoding.UTF8, "application/json");
                                response = webClient.PutAsync(requestAddress, content).Result;
                                break;
                            case "DELETE":
                                response = webClient.DeleteAsync(requestAddress).Result;
                                break;
                            default:
                                throw new Exception("Unsupported HTTP method.");
                        }
                        if (Config.DEBUG_ENABLED)
                            Console.WriteLine($"Response: {response.StatusCode}");
                        return Task.FromResult(response);
                    }
                });
            return response;
        }

        public async Task<HttpResponseMessage> RequestAsync(HttpMethod method, string url, List<string> requestData, bool isList)
        {
            HttpResponseMessage response = null;

            await Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.NotFound
                                                   || r.StatusCode == HttpStatusCode.InternalServerError
                                                   || (int)r.StatusCode == 429)
                .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
                .ExecuteAsync(() =>
                {
                    lock (_lock)
                    {
                        URLFixer(ref url);

                        var requestAddress = webClient.BaseAddress + url;
                        Console.WriteLine($"Sending {method} request. URL: {url}");

                        switch (method.Method)
                        {
                            case "GET":
                                response = webClient.GetAsync(requestAddress).Result;
                                break;
                            case "POST":
                                var json = JsonConvert.SerializeObject(requestData);
                                var content = new StringContent(json, Encoding.UTF8, "application/json");
                                response = webClient.PostAsync(requestAddress, content).Result;
                                break;
                            case "PUT":
                                json = JsonConvert.SerializeObject(requestData);
                                content = new StringContent(json, Encoding.UTF8, "application/json");
                                response = webClient.PutAsync(requestAddress, content).Result;
                                break;
                            case "DELETE":
                                response = webClient.DeleteAsync(requestAddress).Result;
                                break;
                            default:
                                throw new Exception("Unsupported HTTP method.");
                        }
                        Console.WriteLine($"Response: {response.StatusCode}");
                        return Task.FromResult(response);
                    }
                });
            return response;
        }

        private void URLFixer(ref string url)
        {
            if (url[0] == '/')
                url = url.Remove(0, 1);
        }

        private HttpClientHandler BypassSSLHeader()
        {
            return new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, SslPolicyErrors) => true
            };
        }

        public void StartClient(List<string> processArgs)
        {
            while (true)
            {
                try
                {
                    currentProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = processArgs[0],
                            Arguments = string.Join(" ", processArgs.ToArray()),
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = false
                        }
                    };
                    currentProcess.Start();
                    return;
                }
                catch (Exception ex)
                {
                    throw new Exception(this.GetType().Name, ex);
                }
            }
        }

        public void Kill()
        {
            if (currentProcess != null)
            {
                while (true)
                {
                    try
                    {
                        currentProcess.Kill();
                        currentProcess.WaitForExit(15000);
                        return;
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($"{this.GetType().Name}: Failed to terminate. Retrying...");
                    }
                }
            }
        }

        public void Dispose()
        {
            webClient.Dispose();
        }

    }
}
