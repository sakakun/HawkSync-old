﻿using System.Collections.Generic;

namespace HawkSync_RC.classes
{
    public class RCInstances
    {
        public OpenClass.Status Status { get; set; }
        public Dictionary<int, Instance> Instances { get; set; }
        public Dictionary<int, ob_PlayerChatLog> ChatLogs { get; set; }
    }
}