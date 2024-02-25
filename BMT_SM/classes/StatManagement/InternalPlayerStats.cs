using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace HawkSync_SM
{
    public class InternalPlayerStats
    {
        public string PlayerName { get; set; }
        public int ping { get; set; }
        public int CharacterClass { get; set; }
        public int SelectedWeapon { get; set; }
        public List<string> PlayerWeapons { get; set; }
        public int TotalShotsFired { get; set; }
        public int kills { get; set; }
        public int deaths { get; set; }
        public int suicides { get; set; }
        public int teamkills { get; set; }
        public int headshots { get; set; }
        public int knifekills { get; set; }
        public int exp { get; set; }
        public int revives { get; set; }
        public int pspattempts { get; set; }
        public int psptakeover { get; set; }
        public int doublekills { get; set; }
        public int playerrevives { get; set; }
        public int FBCaptures { get; set; }
        public int FBCarrierKills { get; set; }
        public int FBCarrierDeaths { get; set; }
        public int ZoneTime { get; set; }
        public int ZoneKills { get; set; }
        public int ZoneDefendKills { get; set; }
        public int ADTargetsDestroyed { get; set; }
        public int FlagSaves { get; set; }
        public int sniperkills { get; set; }
        public int tkothdefensekills { get; set; }
        public int tkothattackkills { get; set; }
    }
}
