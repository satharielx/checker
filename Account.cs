using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace checker
{
    internal class Account
    {
        public string Username;
        public string Password;
        public string Region;
        public AccountStatus Status;
        public string rawLine;
        public Account(string username, string password, string rawLine) { 
            this.Username = username;
            this.Password = password;
            this.rawLine = rawLine;
        }
    }
    internal class Accounts { 
        public List<Account> accounts = new List<Account>();
        public Accounts(params Account[] accounts) { 
            foreach (var account in accounts)
            {
                this.accounts.Add(account);
            }
        }
        public Accounts() { }
        public void AddAccount(Account a) { this.accounts.Add(a); }
        public void RemoveAccount(Account a) { this.accounts.Remove(a); }
        public static Accounts FromFile(string filePath, params char[] delimeters) { 
            Accounts result = new Accounts();
            if(File.Exists(filePath))
            {
                string[] fileContent = File.ReadAllLines(filePath);
                foreach (string line in fileContent)
                {
                    if (line.Replace(" ", "").Length < 1) continue;
                    string[] splitted = line.Split(delimeters);
                    string username = splitted[0];
                    string password = splitted[1].Split(' ')[0];
                    Account account = new Account(username, password, line);
                    result.AddAccount(account);
                }
            }
            return result;
        }

    }
}
