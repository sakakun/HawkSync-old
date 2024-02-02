using System.ComponentModel;

namespace HawkSync_SM
{
    public class ChatLogs
    {
        public BindingList<PlayerChatLog> Messages { get; set; }
        public int CurrentIndex { get; set; }


        public ChatLogs()
        {
            Messages = new BindingList<PlayerChatLog>();
            CurrentIndex = 0;
        }
    }
}
