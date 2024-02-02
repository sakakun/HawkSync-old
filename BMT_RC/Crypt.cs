using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HawkSync_RC
{
    public static class Crypt
    {
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
        public static string CreateMD5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] asciiBytes = ASCIIEncoding.ASCII.GetBytes(input);
                byte[] hashedBytes = MD5CryptoServiceProvider.Create().ComputeHash(asciiBytes);
                string hashedString = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
                return hashedString;
            }
        }
    }
}
