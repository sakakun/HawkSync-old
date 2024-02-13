using System;

namespace HawkSync_RC.classes
{
    public class PlayerChatLog
    {
        public DateTime dateSent { get; set; }
        public string PlayerName { get; set; }
        public string msgType { get; set; }
        public string team { get; set; }
        public string msg { get; set; }
    }
}
