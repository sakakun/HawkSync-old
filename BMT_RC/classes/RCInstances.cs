using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HawkSync_RC.classes
{
    public class RCInstances
    {
        public OpenClass.Status Status { get; set; }
        public Dictionary<int, Instance> Instances { get; set; }
        public Dictionary<int, ChatLogs> ChatLogs { get; set; }
    }
}
