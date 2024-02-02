using System;
using System.Collections.Generic;
using System.Text;

namespace HawkSync_SM
{
    public class autoRes
    {
        public Dictionary<string, GameType> gameTypes { get; set; }
        public List<MapList> ShuffleSelectedMapList(List<MapList> selectedMapList)
        {
            List<MapList> currentMapList = new List<MapList>();
            foreach (var item in selectedMapList)
            {
                currentMapList.Add(item);
            }
            try
            {
                Random rng = new Random();
                int n = selectedMapList.Count;
                while (n > 1)
                {
                    n--;
                    int k = rng.Next(n + 1);
                    MapList value = currentMapList[k];
                    currentMapList[k] = currentMapList[n];
                    currentMapList[n] = value;
                }
                return currentMapList;
            }
            catch
            {
                currentMapList = null;
                return null;
            }
        }
        public string StringToHex(string HexString, int totallength)
        {
            string ConvertedHexString = BitConverter.ToString(Encoding.Default.GetBytes(HexString)).Replace("-", " ");
            /*byte[] StringToByte = Encoding.Default.GetBytes(HexString);
            var ConvertedHexString = BitConverter.ToString(StringToByte).Replace("-", " ");*/

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
                    var ConvertHexTrue = BitConverter.ToString(newTrueVal).Replace("-", " ");
                    return ConvertHexTrue;
                default:
                    byte[] newValFalse = BitConverter.GetBytes(0);
                    var ConvertHexFalse = BitConverter.ToString(newValFalse).Replace("-", " ");
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
        public void sum_up(List<int> numbers, int target, ref CustomMap map)
        {
            sum_up_recursive(numbers, target, new List<int>(), ref map);
        }
        private void sum_up_recursive(List<int> numbers, int target, List<int> partial, ref CustomMap map)
        {
            int s = 0;
            foreach (int x in partial) s += x;

            if (s == target)
            {
                map.gameTypeBits = partial;
            }

            if (s >= target)
                return;

            for (int i = 0; i < numbers.Count; i++)
            {
                List<int> remaining = new List<int>();
                int n = numbers[i];
                for (int j = i + 1; j < numbers.Count; j++) remaining.Add(numbers[j]);

                List<int> partial_rec = new List<int>(partial);
                partial_rec.Add(n);
                sum_up_recursive(remaining, target, partial_rec, ref map);
            }
        }
    }
}
