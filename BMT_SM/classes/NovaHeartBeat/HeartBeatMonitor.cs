using HawkSync_SM.classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;

namespace HawkSync_SM.classes
{
    internal class HeartBeatMonitor
    {

        public void ProcessHeartBeats(ref AppState _state)
        {
            foreach (var instance in _state.Instances)
            {
                if (instance.Value.ReportNovaCC == true &&
                    DateTime.Compare(_state.Instances[instance.Key].NextUpdateNovaCC, DateTime.Now) < 0
                    && instance.Value.instanceStatus != InstanceStatus.OFFLINE)
                {
                    Instance passInstance = _state.Instances[instance.Key];
                    sendHeartBeat(ref passInstance, "http://ext.novaworld.cc/", "nwapi.php");

                    _state.Instances[instance.Key].NextUpdateNovaCC = DateTime.Now.AddMinutes(30.0);
                }
                if (instance.Value.ReportNovaHQ == true && 
                    DateTime.Compare(_state.Instances[instance.Key].NextUpdateNovaHQ, DateTime.Now) < 0 && 
                    instance.Value.instanceStatus != InstanceStatus.OFFLINE)
                {
                    Instance passInstance = _state.Instances[instance.Key];
                    sendHeartBeat(ref passInstance, "http://nw.novahq.net/", "server/heartbeat-dll");

                    _state.Instances[instance.Key].NextUpdateNovaHQ = DateTime.Now.AddSeconds(30.0);
                }
            }

        }
        //BaseAddress = "http://ext.novaworld.cc/" "nwapi.php"
        //BaseAddress = "http://nw.novahq.net/" "server/heartbeat-dll"

        internal void sendHeartBeat(ref Instance instance, string strBaseAddress, string strPath)
        {
            List<NovaHQPlayerListClass> playerlistHQ = new List<NovaHQPlayerListClass>();
            WebClient client = new WebClient
            {
                BaseAddress = strBaseAddress
            };
            client.Headers["User-Agent"] = "NovaHQ Heartbeat DLL (1.0.9)";
            client.Headers["Content-Type"] = "application/x-www-form-urlencoded";
            NameValueCollection vars = new NameValueCollection
                    {
                        { "Encoding", "Windows-1252" },
                        { "PKey", "eYkJaPPR-3WNbgPN93,(ZwxBCnEW" },
                        { "PVer", "1.0.9" },
                        { "SKey", "wKyPoIh4NSenfXimdnjs" },
                        { "DataType", "0x100" },
                        { "GameID", "dfbhd" },
                        { "Name", instance.gameServerName },
                        { "Port", instance.profileBindPort.ToString() },
                        { "CK", "0" },
                        { "Country", instance.gameCountryCode },
                        { "Type", "Dedicated" },
                        { "GameType", instance.GameTypeName },
                        { "CurrentPlayers", instance.PlayerList.Count.ToString() },
                        { "MaxPlayers", instance.gameMaxSlots.ToString() },
                        { "MissionName", instance.infoCurrentMap.MapName },
                        { "MissionFile", instance.infoCurrentMap.MapFile },
                        { "TimeRemaining", (instance.gameStartDelay + instance.infoMapTimeRemaining * 60).ToString() }
                    };
            if (instance.gamePasswordLobby != string.Empty)
            {
                vars.Add("Password", "Y");
            }
            else
            {
                vars.Add("Password", "");
            }
            vars.Add("Message", instance.gameMOTD);
            if (instance.infoTeamSabre == true)
            {
                vars.Add("Mod", "TS:");
            }
            else
            {
                vars.Add("Mod", "");
            }
            if (instance.PlayerList.Count > 0)
            {
                foreach (var player in instance.PlayerList)
                {
                    playerlistHQ.Add(new NovaHQPlayerListClass
                    {
                        Name = player.Value.name,
                        NameBase64Encoded = player.Value.nameBase64,
                        Kills = player.Value.kills,
                        Deaths = player.Value.deaths,
                        WeaponId = Convert.ToInt32(Enum.Parse(typeof(ob_playerList.WeaponStack), player.Value.selectedWeapon)),
                        WeaponText = player.Value.selectedWeapon,
                        TeamId = player.Value.team,
                        TeamText = player.Value.team.ToString()
                    });
                }
                vars.Add("PlayerList", Crypt.Base64Encode(JsonConvert.SerializeObject(playerlistHQ)));
            }
            else
            {
                vars.Add("PlayerList", "eyIwIjogeyJOYW1lIjoiSG9zdCIsIk5hbWVCYXNlNjRFbmNvZGVkIjoiU0c5emRBPT0iLCJLaWxscyI6IjAiLCJEZWF0aHMiOiIwIiwiV2VhcG9uSWQiOiI1IiwiV2VhcG9uVGV4dCI6IkNBUi0xNSIsIlRlYW1JZCI6IjEiLCJUZWFtVGV4dCI6IkJsdWUiIH19");
            }
            try
            {
                byte[] response = client.UploadValues(strPath, vars);
                string getResponse = Encoding.Default.GetString(response);
            }
            catch
            {

            }
        }



    }
}
