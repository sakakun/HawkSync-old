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
            updateNovaCC(ref _state);
            updateNovaHQ(ref _state);
        }

        internal void updateNovaCC(ref AppState _state)
        {
            foreach (var instance in _state.Instances)
            {
                if (instance.Value.ReportNovaCC == true && DateTime.Compare(_state.Instances[instance.Key].NextUpdateNovaCC, DateTime.Now) < 0 && instance.Value.instanceStatus != InstanceStatus.OFFLINE)
                {
                    List<NovaHQPlayerListClass> playerlistHQ = new List<NovaHQPlayerListClass>();
                    WebClient client = new WebClient
                    {
                        BaseAddress = "http://ext.novaworld.cc/"
                    };
                    client.Headers["User-Agent"] = "Babstats.net HawkSync";
                    //client.Headers["User-Agent"] = "NovaHQ Heartbeat DLL (1.0.9)";
                    client.Headers["Content-Type"] = "application/x-www-form-urlencoded";
                    NameValueCollection vars = new NameValueCollection
                    {
                        { "Encoding", "windows-1252" },
                        { "PKey", "eYkJaPPR-3WNbgPN93,(ZwxBCnEW" },
                        { "PVer", "1.0.9" },
                        { "SKey", "SECRET_KEY" },
                        { "DataType", "0x100" },
                        { "GameID", "dfbhd" },
                        { "Name", _state.Instances[instance.Key].gameServerName },
                        { "Port", _state.Instances[instance.Key].profileBindPort.ToString() },
                        { "CK", "0" },
                        { "Country", _state.Instances[instance.Key].gameCountryCode },
                        { "Type", "gameDedicated" },
                        { "profileServerType", _state.Instances[instance.Key].GameTypeName },
                        { "CurrentPlayers", _state.Instances[instance.Key].PlayerList.Count.ToString() },
                        { "MaxPlayers", _state.Instances[instance.Key].gameMaxSlots.ToString() },
                        { "MissionName", _state.Instances[instance.Key].infoCurrentMap.MapName },
                        { "MissionFile", _state.Instances[instance.Key].infoCurrentMap.MapFile },
                        { "infoMapTimeRemaining", (_state.Instances[instance.Key].gameStartDelay + _state.Instances[instance.Key].infoMapTimeRemaining * 60).ToString() },
                        { "gamePasswordLobby", (_state.Instances[instance.Key].gamePasswordLobby != string.Empty ? "Y" : "" )},
                        { "Message", _state.Instances[instance.Key].gameMOTD }
                    };
                    if (_state.Instances[instance.Key].infoTeamSabre == true)
                    {
                        vars.Add("profileGameMod", "TS:");
                    }
                    else
                    {
                        vars.Add("profileGameMod", "");
                    }
                    if (_state.Instances[instance.Key].PlayerList.Count > 0)
                    {
                        foreach (var player in _state.Instances[instance.Key].PlayerList)
                        {
                            playerlistHQ.Add(new NovaHQPlayerListClass
                            {
                                Deaths = player.Value.deaths,
                                Kills = player.Value.kills,
                                NameBase64Encoded = player.Value.nameBase64,
                                PlayerName = player.Value.name,
                                TeamId = player.Value.team,
                                TeamText = player.Value.team.ToString(),
                                WeaponId = Convert.ToInt32(Enum.Parse(typeof(ob_playerList.WeaponStack), player.Value.selectedWeapon)),
                                WeaponText = player.Value.selectedWeapon
                            });
                        }
                        vars.Add("PlayerList", Crypt.Base64Encode(JsonConvert.SerializeObject(playerlistHQ)));
                    }
                    else
                    {
                        vars.Add("PlayerList", "eyIwIjogeyJOYW1lIjoiSG9zdCIsIk5hbWVCYXNlNjRFbmNvZGVkIjoiU0c5emRBPT0iLCJLaWxscyI6IjAiLCJEZWF0aHMiOiIwIiwiV2VhcG9uSWQiOiI1IiwiV2VhcG9uVGV4dCI6IkNBUi0xNSIsIlRlYW1JZCI6IjUiLCJUZWFtVGV4dCI6Ik5vbmUiIH19");
                    }
                    try
                    {
                        byte[] response = client.UploadValues("nwapi.php", vars);
                        string getResponse = Encoding.Default.GetString(response);
                    }
                    catch
                    {

                    }
                    _state.Instances[instance.Key].NextUpdateNovaCC = DateTime.Now.AddMinutes(1.0);
                }
                else
                {
                    continue;
                }
            }
        }

        internal void updateNovaHQ(ref AppState _state)
        {
            foreach (var instance in _state.Instances)
            {
                if (instance.Value.ReportNovaHQ == true && DateTime.Compare(_state.Instances[instance.Key].NextUpdateNovaHQ, DateTime.Now) < 0 && instance.Value.instanceStatus != InstanceStatus.OFFLINE)
                {
                    List<NovaHQPlayerListClass> playerlistHQ = new List<NovaHQPlayerListClass>();
                    WebClient client = new WebClient
                    {
                        BaseAddress = "http://nw.novahq.net/"
                    };
                    /*client.Headers["User-Agent"] = "Babstats v4 " + ProgramConfig.ApplicationVersion;*/
                    client.Headers["User-Agent"] = "NovaHQ Heartbeat DLL (1.0.9)";
                    client.Headers["Content-Type"] = "application/x-www-form-urlencoded";
                    NameValueCollection vars = new NameValueCollection
                    {
                        { "Encoding", "windows-1252" },
                        { "PKey", "eYkJaPPR-3WNbgPN93,(ZwxBCnEW" },
                        { "PVer", "1.0.9" },
                        { "SKey", "SECRET_KEY" },
                        { "DataType", "0x100" },
                        { "GameID", "dfbhd" },
                        { "Name", _state.Instances[instance.Key].gameServerName },
                        { "Port", _state.Instances[instance.Key].profileBindPort.ToString() },
                        { "CK", "0" },
                        { "Country", _state.Instances[instance.Key].gameCountryCode },
                        { "Type", "gameDedicated" },
                        { "profileServerType", _state.Instances[instance.Key].GameTypeName },
                        { "CurrentPlayers", _state.Instances[instance.Key].PlayerList.Count.ToString() },
                        { "MaxPlayers", _state.Instances[instance.Key].gameMaxSlots.ToString() },
                        { "MissionName", _state.Instances[instance.Key].infoCurrentMap.MapName },
                        { "MissionFile", _state.Instances[instance.Key].infoCurrentMap.MapFile },
                        { "infoMapTimeRemaining", (_state.Instances[instance.Key].gameStartDelay + _state.Instances[instance.Key].infoMapTimeRemaining * 60).ToString() }
                    };
                    if (_state.Instances[instance.Key].gamePasswordLobby != string.Empty)
                    {
                        vars.Add("gamePasswordLobby", "Y");
                    }
                    else
                    {
                        vars.Add("gamePasswordLobby", "");
                    }
                    vars.Add("Message", _state.Instances[instance.Key].gameMOTD);
                    if (_state.Instances[instance.Key].infoTeamSabre == true)
                    {
                        vars.Add("profileGameMod", "TS:");
                    }
                    else
                    {
                        vars.Add("profileGameMod", "");
                    }
                    if (_state.Instances[instance.Key].PlayerList.Count > 0)
                    {
                        foreach (var player in _state.Instances[instance.Key].PlayerList)
                        {
                            playerlistHQ.Add(new NovaHQPlayerListClass
                            {
                                Deaths = player.Value.deaths,
                                Kills = player.Value.kills,
                                NameBase64Encoded = player.Value.nameBase64,
                                PlayerName = player.Value.name,
                                TeamId = player.Value.team,
                                TeamText = player.Value.team.ToString(),
                                WeaponId = Convert.ToInt32(Enum.Parse(typeof(ob_playerList.WeaponStack), player.Value.selectedWeapon)),
                                WeaponText = player.Value.selectedWeapon
                            });
                        }
                        vars.Add("PlayerList", Crypt.Base64Encode(JsonConvert.SerializeObject(playerlistHQ)));
                    }
                    else
                    {
                        vars.Add("PlayerList", "eyIwIjogeyJOYW1lIjoiSG9zdCIsIk5hbWVCYXNlNjRFbmNvZGVkIjoiU0c5emRBPT0iLCJLaWxscyI6IjAiLCJEZWF0aHMiOiIwIiwiV2VhcG9uSWQiOiI1IiwiV2VhcG9uVGV4dCI6IkNBUi0xNSIsIlRlYW1JZCI6IjUiLCJUZWFtVGV4dCI6Ik5vbmUiIH19");
                    }
                    try
                    {
                        byte[] response = client.UploadValues("server/heartbeat-dll", vars);
                        string getResponse = Encoding.Default.GetString(response);
                    }
                    catch
                    {

                    }
                    _state.Instances[instance.Key].NextUpdateNovaHQ = DateTime.Now.AddSeconds(30.0);
                }
                else
                {
                    continue;
                }
            }
        }



    }
}
