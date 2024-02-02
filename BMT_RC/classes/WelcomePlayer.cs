using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HawkSync_RC.classes
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
