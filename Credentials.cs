using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace checker
{
    static class Credentials
    {
        public static string GetAuthToken(int length = 22)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var token = new char[length];
            for (int i = 0; i < length; i++)
            {
                token[i] = chars[random.Next(chars.Length)];
            }
            return new string(token);
        }
    }
}
