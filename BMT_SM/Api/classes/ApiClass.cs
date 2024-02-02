using System.Collections.Generic;

namespace HawkSync_SM.Api.classes
{
    public class CurrentPlayers
    {
        public string name { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string isp { get; set; }
        public double lat { get; set; }
        public double lon { get; set; }
    }
    public class ImportantData
    {
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string isp { get; set; }
        public double lat { get; set; }
        public double lon { get; set; }
    }
    public class PlayerCount
    {
        public int numPlayers { get; set; }
    }
    public class ProfilesOnline
    {
        public int profilesOnline { get; set; }
    }
    public class CaughtVPNs
    {
        public int numVPN { get; set; }
    }
    public class UniquePlayersClass
    {
        public int num { get; set; }
    }
    public class ChartStatsClass
    {
        public Dictionary<int, monthstats> stat { get; set; }
    }
    public class monthstats
    {
        public Dictionary<int, int> month { get; set; }
    }

}
