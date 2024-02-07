using System.Collections.Generic;

namespace HawkSync_SM
{
    public class ob_playerList
    {
        public int slot { get; set; }
        public string name { get; set; }
        public string nameBase64 { get; set; }
        public string address { get; set; }
        public int ping { get; set; }
        public int kills { get; set; }
        public int deaths { get; set; }
        public int zonetime { get; set; }
        public int zonekills { get; set; }
        public int zonedefendkills { get; set; }
        public int playerrevives { get; set; }
        public int team { get; set; } // blue/red team
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
        public int totalshots { get; set; }
        public string PlayerClass { get; set; }
        public string selectedWeapon { get; set; }
        public string lastselectedWeapon { get; set; }
        public List<string> weapons { get; set; }

        // list of weapons assigned to an int.
        public enum WeaponStack
        {
            WPN_KNIFE = 1, // default weapon

            // secondary slot
            WPN_colt45 = 2,
            WPN_M9Beretta = 3,
            WPN_RemmingtonSG = 4,

            // primary slot
            WPN_CAR15_AUTO = 5,
            WPN_CAR15_SEMI = 6,
            WPN_CAR15_203_AUTO = 7,
            WPN_CAR15_203_SEMI = 8,
            WPN_CAR15_203_203 = 9,
            WPN_M16_Burst = 10,
            WPN_M16_SEMI = 11,
            WPN_M16_203_Burst = 12,
            WPN_M16_203_SEMI = 13,
            WPN_M16_203_203 = 14,
            WPN_M21 = 15,
            WPN_M24 = 16,
            WPN_MCRT_300_TACTICAL = 17,
            WPN_Barrett = 18,
            WPN_SAW = 19,
            WPN_M60 = 20,
            WPN_M240 = 21,
            WPN_MP5 = 22,

            // Team Sabre Expanssion Weapons
            WPN_G3_Auto = 23,
            WPN_G3_SEMI = 24,
            WPN_G36_AUTO = 25,
            WPN_G36_SEMI = 26,
            WPN_PSG1 = 27,

            // grenades
            WPN_XM84_STUN = 28,
            WPN_M67_FRAG = 29,
            WPN_AN_M8_SMOKE = 30,
            WPN_Satchel_CHARGE = 32,
            WPN_Radio_DETONATOR = 33,
            WPN_Claymore = 34,
            WPN_AT4 = 35,


            // medkit
            WPN_MEDPACK = 31,

            // mounts
            WPN_50_Hummer = 36,
            WPN_Mini_GUN = 37,
            WPN_GRENADELAUNCHER_M203 = 38,
            WPN_50_EMPLACEMENT_TRUCK = 40,
            WPN_50_EMPLACEMENT = 41,
            WPN_EMC_CANNON = 42,
        }
        public enum Teams
        {
            TEAM_GREEN = 0,
            TEAM_BLUE = 1,
            TEAM_RED = 2,
            TEAM_YELLOW = 3,
            TEAM_PURPLE = 4,
            TEAM_SPEC = 5 // this shouldn't be here because there is no spectator camera
        }
        public enum CharacterClass
        {
            CQB = 5,
            MEDIC = 6,
            SNIPER = 7,
            GUNNER = 8,
            SAS_CQB = 9,
            SAS_MEDIC = 10,
            SAS_SNIPER = 11,
            SAS_GUNNER = 12,
        }
        public virtual bool Equals(ob_playerList obj)
        {
            return obj.name == name;
        }
    }
}
