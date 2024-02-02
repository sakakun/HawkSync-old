using System;

namespace HawkSync_SM.classes
{
    public class WelcomePlayer
    {
        public string playerName { get; set; }
        public bool ReturningPlayer { get; set; }
        public bool Processed { get; set; }
        public DateTime RunTime { get; set; }
        public int Slot { get; set; }
    }
}
