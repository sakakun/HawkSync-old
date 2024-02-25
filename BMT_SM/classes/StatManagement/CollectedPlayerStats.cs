using System.Collections.Generic;

namespace HawkSync_SM
{
    public class CollectedPlayerStats
    {
        public string name { get; set; }
        public string address { get; set; }
        public int ping { get; set; }
        public int kills { get; set; }
        public int deaths { get; set; }
        public int zonetime { get; set; }
        public int zonekills { get; set; }
        public int zonedefendkills { get; set; }
        public int playerrevives { get; set; }
        public string team { get; set; } // blue/red team
        public int flagcaptures { get; set; }
        public int suicides { get; set; }
        public int teamkills { get; set; }
        public int headshots { get; set; }
        public int knifekills { get; set; }
        public int revives { get; set; }
        public int pspattempts { get; set; }
        public int psptakeover { get; set; }
        public int doublekills { get; set; }
        public int flagcarrierkills { get; set; }
        public int flagcarrierdeaths { get; set; }
        public int exp { get; set; }
        public int ADTargetsDestroyed { get; set; }
        public int FlagSaves { get; set; }
        public int tkothdefensekills { get; set; }
        public int tkothattackkills { get; set; }
        public int totalshots { get; set; }
        public int sniperkills { get; set; }
        public string PlayerClass { get; set; }
        public string selectedWeapon { get; set; }
        public string lastselectedWeapon { get; set; }
        public Dictionary<string, InternalWeaponStats> weaponStats { get; set; }
    }
}
