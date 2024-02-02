using System.Collections.Generic;

namespace HawkSync_SM
{
    public class MapList
    {
        public string MapFile { get; set; }
        public string MapName { get; set; }
        public string GameType { get; set; }
        public bool CustomMap { get; set; }
        public List<int> GameTypes { get; set; }
    }
}
