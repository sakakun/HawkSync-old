using System.Collections.Generic;

namespace HawkSync_SM
{
    public class CustomMap
    {
        public string MapName { get; set; }
        public string FileName { get; set; }
        public int bitValueTotalSum { get; set; }
        public List<int> gameTypeBits { get; set; }

        public CustomMap()
        {
            gameTypeBits = new List<int>();
        }

    }
}
