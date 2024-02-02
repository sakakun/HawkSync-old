using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HawkSync_RC.classes
{
    public class PlayerChatLog
    {
        public string PlayerName { get; set; }
        public string msg { get; set; }
        public DateTime dateSent { get; set; }
        public string msgType { get; set; }
        public string team { get; set; }
    }
}
