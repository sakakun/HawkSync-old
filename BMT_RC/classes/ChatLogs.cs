using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HawkSync_RC.classes
{
    public class ChatLogs
    {
        public List<PlayerChatLog> Messages { get; set; }
        public int CurrentIndex { get; set; }


        public ChatLogs()
        {
            Messages = new List<PlayerChatLog>();
            CurrentIndex = 0;
        }
    }
}
