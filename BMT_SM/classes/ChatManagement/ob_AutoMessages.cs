using System;
using System.Collections.Generic;

namespace HawkSync_SM
{
    public class ob_AutoMessages
    {
        public int profile_id { get; set; }
        public bool enable_msg { get; set; } = false;
        public int interval { get; set; } = 3;
        public int MsgNumber { get; set; } = 0;
        public List<string> messages = new List<string>();
        public DateTime NextMessage { get; set; } = DateTime.Now;
    }
}
