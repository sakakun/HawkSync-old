using System.Threading;

namespace HawkSync_SM
{
    public class InternalWeaponStats
    {
        public int kills { get; set; }
        public int shotsfired { get; set; }
        public int timeused { get; set; }
        public string name { get; set; }
        public int code { get; set; }
        public int timer { get; set; }

        public InternalWeaponStats()
        {
            name = "";
            timer = 0;
            kills = 0;
            shotsfired = 0;
            timeused = 0;
        }
    }
}
