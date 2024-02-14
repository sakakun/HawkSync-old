using System;

namespace HawkSync_RC.classes
{
    public class playerHistory
    {
        public int DatabaseId { get; set; }
        public string playerName { get; set; }
        public string playerIP { get; set; }
        public DateTime firstSeen { get; set; }
    }
}