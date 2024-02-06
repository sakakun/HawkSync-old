using System;
using System.Data.SQLite;

namespace HawkSync_SM
{
    public class playerHistory
    {
        public int DatabaseId { get; set; }
        public string playerName { get; set; }
        public string playerIP { get; set; }
        public DateTime firstSeen { get; set; }

    }



}
