using System.ComponentModel;

namespace HawkSync_SM
{
    public class ob_ChatLogs
    {
        public BindingList<ob_PlayerChatLog> Messages { get; set; }
        public int CurrentIndex { get; set; }

        public ob_ChatLogs()
        {
            Messages = new BindingList<ob_PlayerChatLog>();
            CurrentIndex = 0;
        }
    }
}
