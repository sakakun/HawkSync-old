using System;

namespace HawkSync_RC.classes
{
    public class ob_playerBanList
    {
        public int id { get; set; }
        public string player { get; set; }
        public string ipaddress { get; set; }
        public DateTime lastseen { get; set; }
        public string reason { get; set; }
        public DateTime retry { get; set; }
        public DateTime addedDate { get; set; }
        public string expires { get; set; }
        public string bannedBy { get; set; }
        public bool newBan { get; set; }
        public bool onlykick { get; set; }
        public bool VPNBan { get; set; }
    }
}