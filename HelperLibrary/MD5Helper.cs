using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HelperLibrary
{
    public static class MD5Helper
    {
        //Get MD5 Hash of a string in string form
        public static string GetHash(string s)
        {
            byte[] asciiBytes = Encoding.ASCII.GetBytes(s);
            byte[] hashedBytes = MD5.Create().ComputeHash(asciiBytes);
            string hashedString = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();

            return hashedString;
        }
    }
}
