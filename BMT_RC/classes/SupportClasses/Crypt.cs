using System;
using System.Security.Cryptography;
using System.Text;

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
            using (var md5 = MD5.Create())
            {
                var asciiBytes = Encoding.ASCII.GetBytes(input);
                var hashedBytes = MD5.Create().ComputeHash(asciiBytes);
                var hashedString = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
                return hashedString;
            }
        }
    }
}