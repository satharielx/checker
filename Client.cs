using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace checker
{
    internal class Client
    {
        private readonly ConcurrentBag<Process> runningProcesses = new ConcurrentBag<Process>();
        private static readonly object _lock = new object();

        public Client() { }

        public int CreateInstance(List<string> args, string path)
        {
            lock (_lock)
            {
                Process process = new Process();

                try
                {
                    ProcessStartInfo processInfo = new ProcessStartInfo()
                    {
                        FileName = path,
                        Arguments = string.Join(" ", args),
                        UseShellExecute = false
                    };
                    process.StartInfo = processInfo;

                    Thread.Sleep(1000);

                    process.Start();                    

                    // Process is considered fully loaded
                    runningProcesses.Add(process);

                    Thread.Sleep(1000);

                    return process.Id;
                }
                catch (Exception ex)
                {
                    CloseClients();
                    throw new Exception("Error starting process.", ex);
                }
            }
        }



        public void CloseClient(int pid)
        {
            lock (_lock)
            {
                foreach (Process process in runningProcesses)
                {
                    if (process.Id == pid)
                    {
                        process.Kill();
                    }
                }
            }
        }
        public void CloseClients()
        {
            foreach (Process process in runningProcesses)
            {
                process.Kill();
            }
        }
    }
}
