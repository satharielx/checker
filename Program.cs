using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Threading.Tasks;

namespace checker
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.Write("The combolist file should be placed in the checker's folder.\nEnter combolist file name: ");
                string fileName = Console.ReadLine();
                Accounts accounts = Accounts.FromFile(Directory.GetCurrentDirectory() + $"\\{fileName}", ':');
                Console.Write("Enter by what region should be the accounts sorted: ");
                string regionToSort = Console.ReadLine();
                string fileNameExported = $"{regionToSort.ToUpper()}_notsuspended_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
                TaskScheduler scheduler = new TaskScheduler();
                Console.WriteLine($"Checking the account status of username - {"sjfrann"}:{"ivanyalan123"}.. Please Wait");
                Account a = new Account("sjfrann", "ivanyalan123", "");
                var result = await scheduler.InitCheck(a, a.Username, a.Password);

                /*foreach (var account in accounts.accounts)
                {
                    TaskScheduler scheduler = new TaskScheduler();
                    Console.WriteLine($"Checking the account status of username - {account.Username}:{account.Password}.. Please Wait");
                    var result = await scheduler.InitCheck(account, account.Username, account.Password);

                    if (result)
                    {
                        Console.WriteLine($"[ACCOUNT INFO]");
                        Console.WriteLine($"Username: {account.Username}");
                        Console.WriteLine($"Password: {account.Password}");
                        Console.WriteLine($"Region: {account.Region}");
                        Console.WriteLine($"Suspended: {account.Status}");

                        // Create or append to the export file
                        
                        File.AppendAllText(fileNameExported, $"{account.rawLine}\n");
                        Console.WriteLine($"Account information has been exported to: {fileNameExported}");
                    }
                }*/
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
