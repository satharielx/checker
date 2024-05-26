using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace checker
{
    static class Utils
    {
        public static string FindAvailablePort(ref HashSet<int> usedPortsDictionary, int rangeMin = 50000, int rangeMax = 65000)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Random rng = new Random();
            int attempts = rangeMax - rangeMin;

            while(attempts-- > 0)
            {
                int port = rng.Next(rangeMin, rangeMax);
                try {
                    if (usedPortsDictionary.Contains(port))
                        continue;
                    socket.Bind(new IPEndPoint(IPAddress.Loopback, port));
                    socket.Close();
                    usedPortsDictionary.Add(port);
                    return port.ToString();
                }
                catch (SocketException e)
                {
                    // check if port is in use
                    if (e.SocketErrorCode == SocketError.AddressAlreadyInUse)
                    {
                        // Retry
                        Thread.Sleep(1000);
                        continue;
                    }

                    // Retry with new Connection if socket refused
                    if (e.SocketErrorCode == SocketError.ConnectionRefused)
                    {
                        // Retry
                        Thread.Sleep(1000);
                        continue;
                    }

                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("HTTP Request exception at Socket Creation.");

                    //Retry
                    Thread.Sleep(1000);
                    continue;

                }
                catch (AggregateException e)
                {
                    Console.WriteLine("Aggregate exception at Socket Creation.");
                    //Retry
                    Thread.Sleep(1000);
                    continue;
                }
            }
            throw new Exception("No port available.");
        } 
    }
}
