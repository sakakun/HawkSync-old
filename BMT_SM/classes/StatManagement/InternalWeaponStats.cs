namespace HawkSync_SM
{
    public class InternalWeaponStats
    {
        public int kills { get; set; }
        public int shotsfired { get; set; }
        public int timeused { get; set; }

        public InternalWeaponStats()
        {
            kills = 0;
            shotsfired = 0;
            timeused = 0;
        }
    }
}
