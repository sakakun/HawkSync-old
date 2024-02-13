using System;
using System.Collections.Generic;
using System.Text;

namespace HawkSync_RC.classes
{
    public class autoRes
    {
        public Dictionary<string, GameType> gameTypes { get; set; }
        public List<MapList> ShuffleSelectedMapList(List<MapList> selectedMapList)
        {
            Random rng = new Random();
            int n = selectedMapList.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                MapList value = selectedMapList[k];
                selectedMapList[k] = selectedMapList[n];
                selectedMapList[n] = value;
            }
            return selectedMapList;
        }
        public string StringToHex(string HexString, int totallength)
        {
            byte[] StringToByte = Encoding.Default.GetBytes(HexString);
            var ConvertedHexString = BitConverter.ToString(StringToByte).Replace("-", " ");

            if (totallength != 0)
            {
                for (int x = ConvertedHexString.Length / 3; x < totallength; x++)
                {
                    ConvertedHexString += " 00";
                }
            }

            return ConvertedHexString;
        }
        public string IntToHex(int HexInt)
        {
            byte[] IntToByte = BitConverter.GetBytes(HexInt);
            var ConvertedHexInt = BitConverter.ToString(IntToByte).Replace("-", " ");
            return ConvertedHexInt;
        }
        public string BoolToHex(bool val)
        {
            switch (val)
            {
                case true:
                    byte[] newTrueVal = BitConverter.GetBytes(1);
                    var ConvertHexTrue = BitConverter.ToString(newTrueVal).Replace("-", " ").Replace(" 00", "");
                    return ConvertHexTrue;
                default:
                    byte[] newValFalse = BitConverter.GetBytes(0);
                    var ConvertHexFalse = BitConverter.ToString(newValFalse).Replace("-", " ").Replace(" 00", "");
                    return ConvertHexFalse;
            }
        }
        public bool IsMapTeamBased(int gametype)
        {
            switch (gametype)
            {
                case 0: // DM
                    return false;
                case 1: // TDM
                    return true;
                case 3: //TKOTH
                    return true;
                case 4: // King of The Hill
                    return false;
                case 5: // Search and Destroy
                    return true;
                case 6: // Attack and Defend
                    return true;
                case 7: // Capture the Flag
                    return true;
                case 8: // Flagball
                    return true;
                default: // all others are assumed to be invalid game types...
                    return false;
            }
        }
    }
}
