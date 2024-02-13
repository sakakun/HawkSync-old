using System;
using System.Collections.Generic;

namespace HawkSync_RC.classes
{
    public class auto_messages
    {
        public int profile_id { get; set; }
        public bool enable_msg { get; set; }
        public int interval { get; set; }
        public int MsgNumber { get; set; }
        public List<string> messages = new List<string>();
        public DateTime NextMessage { get; set; }
    }
}
