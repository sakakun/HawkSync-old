using System;
using System.Collections.Generic;
using System.Text;

namespace HawkSync_RC.classes
{
    public class autoRestart
    {
        public Dictionary<string, GameType> gameTypes { get; set; }

        public List<MapList> ShuffleSelectedMapList(List<MapList> selectedMapList)
        {
            var rng = new Random();
            var n = selectedMapList.Count;
            while (n > 1)
            {
                n--;
                var k = rng.Next(n + 1);
                var value = selectedMapList[k];
                selectedMapList[k] = selectedMapList[n];
                selectedMapList[n] = value;
            }

            return selectedMapList;
        }

        public string StringToHex(string HexString, int totallength)
        {
            var StringToByte = Encoding.Default.GetBytes(HexString);
            var ConvertedHexString = BitConverter.ToString(StringToByte).Replace("-", " ");

            if (totallength != 0)
                for (var x = ConvertedHexString.Length / 3; x < totallength; x++)
                    ConvertedHexString += " 00";

            return ConvertedHexString;
        }

        public string IntToHex(int HexInt)
        {
            var IntToByte = BitConverter.GetBytes(HexInt);
            var ConvertedHexInt = BitConverter.ToString(IntToByte).Replace("-", " ");
            return ConvertedHexInt;
        }

        public string BoolToHex(bool val)
        {
            switch (val)
            {
                case true:
                    var newTrueVal = BitConverter.GetBytes(1);
                    var ConvertHexTrue = BitConverter.ToString(newTrueVal).Replace("-", " ").Replace(" 00", "");
                    return ConvertHexTrue;
                default:
                    var newValFalse = BitConverter.GetBytes(0);
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