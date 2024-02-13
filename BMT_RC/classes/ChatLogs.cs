using System.Collections.Generic;

namespace HawkSync_RC.classes
{
    public class ChatLogs
    {
        public ChatLogs()
        {
            Messages = new List<PlayerChatLog>();
            CurrentIndex = 0;
        }

        public List<PlayerChatLog> Messages { get; set; }
        public int CurrentIndex { get; set; }
    }
}