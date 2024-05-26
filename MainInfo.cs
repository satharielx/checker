using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace checker
{
    static class MainInfo
    {
        public static int FailedAccounts = 0;
        public static Dictionary<Account, string> SuccesfulLoginAccountsToLine = new Dictionary<Account, string>(); 
    }
}
