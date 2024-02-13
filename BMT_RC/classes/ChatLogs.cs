using System.Collections.Generic;

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
