using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
