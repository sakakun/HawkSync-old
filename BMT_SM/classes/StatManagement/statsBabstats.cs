using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Results;
using static System.Windows.Forms.AxHost;

namespace HawkSync_SM.classes.StatManagement
{
    public class ModInfo
    {
        public int ModId { get; set; }
        public string ModName { get; set; }
        public string ModVersion { get; set; }

        public ModInfo(int modId, string modName, string modVersion)
        {
            ModId = modId;
            ModName = modName;
            ModVersion = modVersion;
        }
    }

    public class WeaponInfo
    {
        public int ModId { get; set; }
        public int WpnId { get; set; }
        public int WpnPid { get; set; }
        public string WpnName { get; set; }
        public string ModWpnComment { get; set; }

    }

    internal class statsBabstats
    {
        public AppState _state;
        public int _instanceID;
        public ModInfo[] babstatsMods;
        public WeaponInfo[] babstatsWeapons;

        public void initBabstats(AppState state, int instanceID)
        {
            _state = state;
            _instanceID = instanceID;
            // Define the mods and weapons for babstats generation
            babstatsMods = new ModInfo[]
            {
                new ModInfo(7, "DFBHD", "1.5.0.5"),
                new ModInfo(8, "Team Sabre", "1.5.0.5"),
                new ModInfo(9, "Black Operations", ""),
                new ModInfo(10, "War on Terror", ""),
                new ModInfo(11, "ShocknAwe", "4.0")
            };
            babstatsWeapons = new WeaponInfo[]
            {
                new WeaponInfo() { ModId = 7, WpnId = 1, WpnPid = 1, WpnName = "Knife", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 2, WpnName = "Colt .45", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 3, WpnName = "M9 Beretta", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 4, WpnName = "Remington Shotgun", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 5, WpnPid = 5, WpnName = "CAR15", ModWpnComment = "Auto" },
                new WeaponInfo() { ModId = 7, WpnId = 6, WpnPid = 5, WpnName = "CAR15", ModWpnComment = "Semi" },
                new WeaponInfo() { ModId = 7, WpnId = 7, WpnPid = 7, WpnName = "CAR15/M203", ModWpnComment = "Auto" },
                new WeaponInfo() { ModId = 7, WpnId = 8, WpnPid = 7, WpnName = "CAR15/M203", ModWpnComment = "Semi" },
                new WeaponInfo() { ModId = 7, WpnId = 9, WpnPid = 7, WpnName = "CAR15/M203 - Grenade", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 10, WpnPid = 10, WpnName = "M16", ModWpnComment = "Burst" },
                new WeaponInfo() { ModId = 7, WpnId = 11, WpnPid = 10, WpnName = "M16", ModWpnComment = "Semi" },
                new WeaponInfo() { ModId = 7, WpnId = 12, WpnPid = 12, WpnName = "M16/M203", ModWpnComment = "Burst" },
                new WeaponInfo() { ModId = 7, WpnId = 13, WpnPid = 12, WpnName = "M16/M203", ModWpnComment = "Semi" },
                new WeaponInfo() { ModId = 7, WpnId = 14, WpnPid = 12, WpnName = "M16/M203 - Grenade", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 15, WpnName = "M21", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 16, WpnName = "M24", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 17, WpnName = "MCRT .300 Tactical", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 18, WpnName = "Barrett .50 Cal", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 19, WpnName = "M249 SAW", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 20, WpnName = "M60", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 21, WpnName = "M240", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 22, WpnName = "MP5", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 23, WpnName = "G3", ModWpnComment = "Auto" },
                new WeaponInfo() { ModId = 7, WpnId = 24, WpnName = "G3", ModWpnComment = "Semi" },
                new WeaponInfo() { ModId = 7, WpnId = 25, WpnName = "G36", ModWpnComment = "Auto" },
                new WeaponInfo() { ModId = 7, WpnId = 26, WpnName = "G36", ModWpnComment = "Semi" },
                new WeaponInfo() { ModId = 7, WpnId = 27, WpnName = "PSG1", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 28, WpnName = "Flash Bang", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 29, WpnName = "Frag Grenade", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 30, WpnName = "Smoke Grenade", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 31, WpnName = "Med Pack", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 32, WpnPid = 32, WpnName = "Satchel Charge", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 33, WpnPid = 32, WpnName = "Satchel Charge", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 34, WpnName = "Claymore", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 35, WpnName = "AT4", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 36, WpnPid = 36, WpnName = "50 Cal Humvee", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 37, WpnPid = 37, WpnName = "MiniGun", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 38, WpnName = "Grenade Launcher", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 39, WpnPid = 36, WpnName = "50 Cal Emplacement", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 40, WpnPid = 36, WpnName = "50 Cal Emplacement", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 41, WpnPid = 36, WpnName = "50 Cal Emplacement", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 42, WpnName = "E M Canon", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 43, WpnPid = 36, WpnName = "50 Cal Truck", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 44, WpnPid = 36, WpnName = "50 Cal Emplacement", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 45, WpnPid = 36, WpnName = "50 Cal Emplacement", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 46, WpnPid = 37, WpnName = "MiniGun", ModWpnComment = "" },
                new WeaponInfo() { ModId = 7, WpnId = 47, WpnPid = 36, WpnName = "50 Cal Emplacement", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 1, WpnName = "Knife", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 2, WpnName = "Colt .45", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 3, WpnName = "M9 Beretta", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 4, WpnName = "Remington Shotgun", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 5, WpnPid = 5, WpnName = "CAR15", ModWpnComment = "Auto" },
                new WeaponInfo() { ModId = 8, WpnId = 6, WpnPid = 5, WpnName = "CAR15", ModWpnComment = "Semi" },
                new WeaponInfo() { ModId = 8, WpnId = 7, WpnPid = 7, WpnName = "CAR15/M203", ModWpnComment = "Auto" },
                new WeaponInfo() { ModId = 8, WpnId = 8, WpnPid = 7, WpnName = "CAR15/M203", ModWpnComment = "Semi" },
                new WeaponInfo() { ModId = 8, WpnId = 9, WpnPid = 7, WpnName = "CAR15/M203 - Grenade", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 10, WpnPid = 10, WpnName = "M16", ModWpnComment = "Burst" },
                new WeaponInfo() { ModId = 8, WpnId = 11, WpnPid = 10, WpnName = "M16", ModWpnComment = "Semi" },
                new WeaponInfo() { ModId = 8, WpnId = 12, WpnPid = 12, WpnName = "M16/M203", ModWpnComment = "Burst" },
                new WeaponInfo() { ModId = 8, WpnId = 13, WpnPid = 12, WpnName = "M16/M203", ModWpnComment = "Semi" },
                new WeaponInfo() { ModId = 8, WpnId = 14, WpnPid = 12, WpnName = "M16/M203 - Grenade", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 15, WpnName = "M21", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 16, WpnName = "M24", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 17, WpnName = "MCRT .300 Tactical", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 18, WpnName = "Barrett .50 Cal", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 19, WpnName = "M249 SAW", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 20, WpnName = "M60", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 21, WpnName = "M240", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 22, WpnName = "MP5", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 23, WpnPid = 23, WpnName = "G3", ModWpnComment = "Auto" },
                new WeaponInfo() { ModId = 8, WpnId = 24, WpnPid = 23, WpnName = "G3", ModWpnComment = "Semi" },
                new WeaponInfo() { ModId = 8, WpnId = 25, WpnPid = 25, WpnName = "G36", ModWpnComment = "Auto" },
                new WeaponInfo() { ModId = 8, WpnId = 26, WpnPid = 25, WpnName = "G36", ModWpnComment = "Semi" },
                new WeaponInfo() { ModId = 8, WpnId = 27, WpnName = "PSG1", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 28, WpnName = "Flash Bang", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 29, WpnName = "Frag Grenade", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 30, WpnName = "Smoke Grenade", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 31, WpnName = "Med Pack", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 32, WpnPid = 32, WpnName = "Satchel Charge", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 33, WpnPid = 32, WpnName = "Satchel Charge", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 34, WpnName = "Claymore", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 35, WpnName = "AT4", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 36, WpnPid = 36, WpnName = "50 Cal Humvee", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 37, WpnPid = 37, WpnName = "MiniGun", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 38, WpnName = "Grenade Launcher", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 39, WpnPid = 36, WpnName = "50 Cal Emplacement", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 40, WpnPid = 36, WpnName = "50 Cal Emplacement", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 41, WpnPid = 36, WpnName = "50 Cal Emplacement", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 42, WpnName = "E M Canon", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 43, WpnPid = 36, WpnName = "50 Cal Truck", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 44, WpnPid = 36, WpnName = "50 Cal Emplacement", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 45, WpnPid = 36, WpnName = "50 Cal Emplacement", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 46, WpnPid = 37, WpnName = "MiniGun", ModWpnComment = "" },
                new WeaponInfo() { ModId = 8, WpnId = 47, WpnPid = 36, WpnName = "50 Cal Emplacement", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 1, WpnName = "Knife", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 2, WpnName = "M9 Beretta", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 3, WpnName = "Remington Shotgun", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 4, WpnName = "Spas 12", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 5, WpnName = "M21", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 6, WpnName = "M24", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 7, WpnName = "MCRT .300 Tactical", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 8, WpnName = "Barrett .50 Cal", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 9, WpnName = "M249 SAW", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 10, WpnName = "M60", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 11, WpnName = "M240", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 12, WpnName = "MP5 Silenced", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 13, WpnPid = 13, WpnName = "G3", ModWpnComment = "Auto" },
                new WeaponInfo() { ModId = 9, WpnId = 14, WpnPid = 13, WpnName = "G3", ModWpnComment = "Semi" },
                new WeaponInfo() { ModId = 9, WpnId = 15, WpnName = "PSG1", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 16, WpnName = "Flash Bang", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 17, WpnName = "Frag Grenade", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 18, WpnName = "Smoke Grenade", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 19, WpnName = "Med Pack", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 20, WpnPid = 20, WpnName = "Satchel Charge", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 21, WpnPid = 20, WpnName = "Satchel Charge", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 22, WpnName = "Claymore", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 23, WpnName = "P90", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 24, WpnName = "AUG", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 25, WpnName = "SOPMOD M4", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 26, WpnName = "SR-25", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 27, WpnName = "OICW", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 28, WpnName = "M4-SD", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 29, WpnName = "551", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 30, WpnName = "G37", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 31, WpnName = "G36c", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 32, WpnName = "FNMAG", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 33, WpnPid = 33, WpnName = "AK-47/203", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 34, WpnPid = 33, WpnName = "AK-47/203 - Grenade", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 35, WpnName = "AK-47", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 36, WpnName = "AK-74", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 37, WpnName = "M22", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 38, WpnName = "HR-LAW", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 39, WpnName = "L96", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 40, WpnName = "Glock 18-C", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 41, WpnName = "MP7", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 42, WpnName = "G11", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 43, WpnPid = 43, WpnName = "50 Cal Humvee", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 44, WpnPid = 44, WpnName = "MiniGun", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 45, WpnName = "Grenade Launcher", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 47, WpnPid = 43, WpnName = "50 Cal Emplacement", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 48, WpnPid = 43, WpnName = "50 Cal Emplacement", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 49, WpnName = "E M Canon", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 50, WpnPid = 43, WpnName = "50 Cal Truck", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 51, WpnPid = 43, WpnName = "50 Cal Humvee", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 52, WpnPid = 43, WpnName = "50 Cal Emplacement", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 53, WpnPid = 44, WpnName = "MiniGun", ModWpnComment = "" },
                new WeaponInfo() { ModId = 9, WpnId = 54, WpnPid = 43, WpnName = "50 Cal Emplacement", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 1, WpnName = "Knife", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 2, WpnName = "M9 Beretta", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 3, WpnName = "Remington Shotgun", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 4, WpnName = "Remington Shotgun", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 5, WpnName = "M240", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 6, WpnName = "M1 Rifle", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 7, WpnName = "MCRT .300 Tactical", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 8, WpnName = "M249 SAW", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 9, WpnName = "M60", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 10, WpnName = "M60 Silenced", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 11, WpnName = "MP5 Silenced", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 12, WpnPid = 12, WpnName = "G3", ModWpnComment = "Auto" },
                new WeaponInfo() { ModId = 10, WpnId = 13, WpnPid = 12, WpnName = "G3", ModWpnComment = "Semi" },
                new WeaponInfo() { ModId = 10, WpnId = 14, WpnPid = 14, WpnName = "G36", ModWpnComment = "Auto" },
                new WeaponInfo() { ModId = 10, WpnId = 15, WpnPid = 14, WpnName = "G36", ModWpnComment = "Semi" },
                new WeaponInfo() { ModId = 10, WpnId = 16, WpnPid = 16, WpnName = "PSG1", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 17, WpnPid = 16, WpnName = "PSG1 Silenced", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 18, WpnName = "Flash Bang", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 19, WpnName = "Frag Grenade", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 20, WpnName = "Smoke Grenade", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 21, WpnName = "Med Pack", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 22, WpnPid = 22, WpnName = "Satchel Charge", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 23, WpnPid = 22, WpnName = "Satchel Charge", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 24, WpnName = "Claymore", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 25, WpnName = "AT4", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 26, WpnName = "SKS Rifle", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 27, WpnName = "M4 Auto", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 28, WpnName = "LG55", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 29, WpnName = "BR55 Rifle", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 30, WpnName = "Glock 18-C", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 31, WpnName = "SIG SG552", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 32, WpnName = "SR-25", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 33, WpnName = "BR55.6 Rifle", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 34, WpnName = "AR-10", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 35, WpnName = "SOPMOD", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 36, WpnPid = 36, WpnName = "50 Cal Humvee", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 37, WpnPid = 37, WpnName = "MiniGun", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 38, WpnName = "Grenade Launcher", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 40, WpnPid = 36, WpnName = "50 Cal Emplacement", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 41, WpnPid = 36, WpnName = "50 Cal Emplacement", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 42, WpnName = "E M Canon", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 43, WpnPid = 36, WpnName = "50 Cal Truck", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 44, WpnPid = 36, WpnName = "50 Cal Emplacement", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 45, WpnPid = 36, WpnName = "50 Cal Emplacement", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 46, WpnPid = 37, WpnName = "MiniGun", ModWpnComment = "" },
                new WeaponInfo() { ModId = 10, WpnId = 47, WpnPid = 36, WpnName = "50 Cal Emplacement", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 1, WpnName = "Knife", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 2, WpnName = "M9 Beretta", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 3, WpnName = "Remington Shotgun", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 4, WpnName = "Throwing Knifes", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 5, WpnPid = 5, WpnName = "CAR15", ModWpnComment = "Auto" },
                new WeaponInfo() { ModId = 16, WpnId = 6, WpnPid = 5, WpnName = "CAR15", ModWpnComment = "Semi" },
                new WeaponInfo() { ModId = 16, WpnId = 7, WpnPid = 7, WpnName = "CAR15/M203", ModWpnComment = "Auto" },
                new WeaponInfo() { ModId = 16, WpnId = 8, WpnPid = 7, WpnName = "CAR15/M203", ModWpnComment = "Semi" },
                new WeaponInfo() { ModId = 16, WpnId = 9, WpnPid = 7, WpnName = "CAR15/M203 - Grenade", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 10, WpnPid = 10, WpnName = "M4A1SD", ModWpnComment = "Auto" },
                new WeaponInfo() { ModId = 16, WpnId = 11, WpnPid = 10, WpnName = "M4A1SD", ModWpnComment = "Burst" },
                new WeaponInfo() { ModId = 16, WpnId = 12, WpnPid = 10, WpnName = "M4A1SD", ModWpnComment = "Semi" },
                new WeaponInfo() { ModId = 16, WpnId = 13, WpnPid = 13, WpnName = "OICW", ModWpnComment = "Burst" },
                new WeaponInfo() { ModId = 16, WpnId = 14, WpnPid = 13, WpnName = "OICW", ModWpnComment = "Semi" },
                new WeaponInfo() { ModId = 16, WpnId = 15, WpnPid = 13, WpnName = "OICW - Grenade", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 16, WpnName = "M21", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 17, WpnName = "M24", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 20, WpnName = "M249 SAW", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 21, WpnName = "M60", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 22, WpnName = "M240", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 23, WpnName = "MP5 (UWU)", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 24, WpnPid = 24, WpnName = "G3", ModWpnComment = "Auto" },
                new WeaponInfo() { ModId = 16, WpnId = 25, WpnPid = 24, WpnName = "G3 Sniper", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 26, WpnName = "RPD", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 27, WpnName = "USAS12 Shotgun", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 28, WpnName = "PSG1", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 29, WpnName = "SIG 550 Sniper", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 30, WpnName = "M-20", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 31, WpnName = "Flash Bang", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 32, WpnName = "Frag Grenade", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 33, WpnName = "Smoke Grenade", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 34, WpnName = "Med Pack", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 35, WpnPid = 35, WpnName = "Satchel Charge", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 36, WpnPid = 35, WpnName = "Satchel Charge", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 37, WpnName = "Claymore", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 38, WpnName = "AT4", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 39, WpnPid = 39, WpnName = "50 Cal Emplacement", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 40, WpnPid = 40, WpnName = "MiniGun", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 41, WpnName = "Grenade Launcher", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 42, WpnPid = 39, WpnName = "50 Cal Emplacement", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 43, WpnPid = 39, WpnName = "50 Cal Emplacement", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 44, WpnPid = 39, WpnName = "50 Cal Emplacement", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 45, WpnName = "E M Canon", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 46, WpnPid = 39, WpnName = "50 Cal Emplacement", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 47, WpnPid = 39, WpnName = "50 Cal Emplacement", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 48, WpnPid = 39, WpnName = "50 Cal Emplacement", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 49, WpnName = "MiniGun", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 50, WpnName = "Mac10", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 51, WpnName = "Desert Eagle", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 52, WpnName = "SKS Paratrooper", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 53, WpnName = "P-99", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 54, WpnName = "Walther 2000", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 55, WpnName = "Bomb Vest", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 18, WpnName = "MCRT .300 Tactical", ModWpnComment = "" },
                new WeaponInfo() { ModId = 16, WpnId = 19, WpnName = "Barrett .50 Cal", ModWpnComment = "" }
            };

        }

        private string line_ServerID()
        {
            string line = $"ServerID {_state.Instances[_instanceID].WebStatsId}\n";
            return line;
        }

        private string line_GameInfo()
        {
            /*
             * Game #2__&__#3__&__#4__&__#5__&__#6__&__#7__&__#8__&__#9__&__#10
             * #2  = Game Timer
               #3  = DateTime (Y-m-d H:i:s)
               #4  = GameType (See GameType Ref)
               #5  = Is Dedicated (0=No,1=Yes)
               #6  = Server Name 
               #7  = Map Name
               #8  = Maxplayers
               #9  = Numplayers
               #10 = Mod (See Reference Numbers)
             */
            int timer = _state.Instances[_instanceID].TimeLimit - _state.Instances[_instanceID].TimeRemaining;
            string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            int gameMapType = _state.Instances[_instanceID].gameMapType;
            int dedicated = Convert.ToInt32(_state.Instances[_instanceID].Dedicated);
            string serverName = _state.Instances[_instanceID].ServerName;
            string mapName = _state.Instances[_instanceID].CurrentMap.MapName;
            int maxPlayers = _state.Instances[_instanceID].MaxSlots;
            int numPlayers = _state.Instances[_instanceID].PlayerList.Count;

            bool TeamSabre = _state.Instances[_instanceID].GameType == 0 && File.Exists(_state.Instances[_instanceID].GamePath + "\\EXP1.pff");
            int mod = TeamSabre ? 8 : 7;

            string line = $"Game {timer}__&__{date}__&__{gameMapType}__&__{dedicated}__&__{serverName}__&__{mapName}__&__{maxPlayers}__&__{numPlayers}__&__{mod}\n";
            return line;
        }

        public void generateBabstats_EOM(AppState state, int instanceID)
        {
            initBabstats(state, instanceID);
            // Generate data to send to Babstats Server via POST.
            // This is for End of Map data
            string ServerLine = line_ServerID();
            string GameLine = line_GameInfo();
            string playerLines = "";
            foreach (var player in _state.Instances[instanceID].PlayerList)
            {
                /* PlayerStats #13 #14 #15 #16 #17 #18 #19 #20 #21 #22 #23 #24 #25 #26 #27 #28 #29 #30 #31 #32 #33 #34 #35 #36 #37 #38 #39 #40
                 * #13 = suicides
                   #14 = murders
                   #15 = kills
                   #16 = deaths
                   #17 = zonetime
                   #18 = flags
                   #19 = flagsaves
                   #20 = targets
                   #21 = revives
                   #22 = medsaves
                   #23 = pspattempts
                   #24 = psptakeovers
                   #25 = flagcarrierkills
                   #26 = doublekills
                   #27 = headshots
                   #28 = knifings
                   #29 = sniperkills
                   #30 = tkothattackkills
                   #31 = tkothdefendkills
                   #32 = sdaddefendkills
                   #33 = sdadpolicekills
                   #34 = sdadattackkills
                   #35 = sdadsecurekills
                   #36 = shotsperkill
                   #37 = experience
                   #38 = team
                   #39 = playedtillend
                   #40 = timer
                 *
                 */
                string v01 = player.Value.suicides.ToString();
                string v02 = player.Value.teamkills.ToString();
                string v03 = player.Value.kills.ToString();
                string v04 = player.Value.deaths.ToString();
                string v05 = player.Value.zonetime.ToString();
                string v06 = player.Value.flagcaptures.ToString();
                string v07 = player.Value.FlagSaves.ToString();
                string v08 = player.Value.ADTargetsDestroyed.ToString();
                string v09 = player.Value.revives.ToString();
                string v10 = player.Value.playerrevives.ToString();
                string v11 = player.Value.pspattempts.ToString();
                string v12 = player.Value.psptakeover.ToString();
                string v13 = player.Value.flagcarrierkills.ToString();
                string v14 = player.Value.doublekills.ToString();
                string v15 = player.Value.headshots.ToString();
                string v16 = player.Value.knifekills.ToString();
                string v17 = player.Value.sniperkills.ToString();
                string v18 = player.Value.tkothattackkills.ToString(); // tkothattackkills
                string v19 = player.Value.tkothdefensekills.ToString(); // tkothdefendkills
                string v20 = "0"; // sdaddefendkills
                string v21 = "0"; // sdadpolicekills
                string v22 = "0"; // sdadattackkills
                string v23 = "0"; // sdadsecurekills
                string v24 = (player.Value.totalshots / player.Value.kills).ToString();
                string v25 = player.Value.exp.ToString();
                string v26 = player.Value.team.ToString();
                string v27 = "1"; // playedtillend technically, and player that is in the active player list EOM.
                string v28 = "0"; // timer seconds
                playerLines += $"Player {player.Value.name}__&__{player.Value.address}\n";
                playerLines += $"PlayerStats {v01} {v02} {v03} {v04} {v05} {v06} {v07} {v08} {v09} {v10} {v11} {v12} {v13} {v14} {v15} {v16} {v17} {v18} {v19} {v20} {v21} {v22} {v23} {v24} {v25} {v26} {v27} {v28}";
            }

            Console.WriteLine(ServerLine);
            Console.WriteLine(GameLine);

        }

        public void sendBabstatsData()
        {
            // Take data from generateBabstats_EOM or generateBabstats_APM to send to specified Babstats Server via POST.
        }
    }
}
