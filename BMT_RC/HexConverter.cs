using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HawkSync_RC
{
    static class HexConverter
    {
        public static byte[] ToByteArray(String hexString)
        {
            byte[] retval = new byte[hexString.Length / 2];
            for (int i = 0; i < hexString.Length; i += 2)
                retval[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            return retval;
        }
        public static string ToHexString(byte[] ByteArray)
        {
            string hexValues = BitConverter.ToString(ByteArray).Replace("-", "");
            return hexValues;
        }
    }
}
