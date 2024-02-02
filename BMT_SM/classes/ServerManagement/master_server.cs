using System;

namespace HawkSync_SM
{
    public class master_server
    {
        public int ProfileID { get; set; }
        public int? PID { get; set; }
        public int ServerPort { get; set; }
        public string BindAddress { get; set; }
        public string ServerName { get; set; }
        public int CurrentPlayers { get; set; }
        public int MaxSlots { get; set; }
        public string Map { get; set; }
        public string MapType { get; set; }
        public string Status { get; set; }
        public int CurrentTime { get; set; }
        public int UnixNextUpdate { get; set; }
        public DateTime NextUpdate { get; set; }
    }
}
