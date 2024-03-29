using System.Threading;

namespace HawkSync_SM
{
    public class InternalWeaponStats
    {
        public int weaponid { get; set; }
        public int kills { get; set; }
        public int shotsfired { get; set; }
        public double timer { get; set; }

        public InternalWeaponStats()
        {
            weaponid = 0;
            timer = 0;
            kills = 0;
            shotsfired = 0;
        }
    }
}
