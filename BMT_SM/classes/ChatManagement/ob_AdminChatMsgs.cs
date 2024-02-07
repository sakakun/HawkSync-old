using System;

namespace HawkSync_SM
{
    public class ob_AdminChatMsgs
    {
        public int MsgID { get; set; }
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Msg { get; set; }
        public DateTime DateSent { get; set; }
    }
}
