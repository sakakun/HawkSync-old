using System;

namespace HawkSync_RC
{
    internal static class HexConverter
    {
        public static byte[] ToByteArray(string hexString)
        {
            var retval = new byte[hexString.Length / 2];
            for (var i = 0; i < hexString.Length; i += 2)
                retval[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            return retval;
        }

        public static string ToHexString(byte[] ByteArray)
        {
            var hexValues = BitConverter.ToString(ByteArray).Replace("-", "");
            return hexValues;
        }
    }
}