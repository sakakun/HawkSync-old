using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;

namespace HawkSync_SM.classes.ChatManagement
{
    public class ChatManagement
    {
        
        public static string[] ChatChannels = new string[] {
            "6A01", // 0 = Global Chat (Blue)
            "6A03", // 1 = Global Chat (Yellow)
            "6A08", // 2 = Global Chat (Orange)
            "6A04", // 3 = Team Chat Red
            "6A05"  // 4 = Team Chat Blue
        };

        public void ProcessAutoMessages(ref AppState _state, int id)
        {
            if (_state.Instances[id].AutoMessages.enable_msg == true)
            {
                if (_state.Instances[id].AutoMessages.messages.Count != 0)
                {
                    if (DateTime.Compare(_state.Instances[id].AutoMessages.NextMessage, DateTime.Now) < 0)
                    {
                        if (_state.Instances[id].AutoMessages.MsgNumber.Equals(_state.Instances[id].AutoMessages.messages.Count))
                        {
                            _state.Instances[id].AutoMessages.MsgNumber = 0;
                        }

                        string msg = _state.Instances[id].AutoMessages.messages[_state.Instances[id].AutoMessages.MsgNumber];

                        // don't do anything if no one is playing in the server
                        if (_state.Instances[id].PlayerList.Count == 0)
                        {
                            _state.Instances[id].AutoMessages.NextMessage = DateTime.Now.AddMinutes(_state.Instances[id].AutoMessages.interval);
                            return;
                        }

                        if (msg.Contains("$(NextMap)"))
                        {
                            int mapIndex = 0;
                            if (_state.Instances[id].mapIndex != _state.Instances[id].MapList.Count)
                            {
                                mapIndex = _state.Instances[id].mapIndex + 1;
                            }
                            msg = msg.Replace("$(NextMap)", "Next Map: " + _state.Instances[id].MapList[mapIndex].MapName);
                        }
                        else if (msg.Contains("$(HighestEXP)"))
                        {
                            string playerName = string.Empty;
                            int playerExp = 0;
                            foreach (var player in _state.Instances[id].PlayerList)
                            {
                                if (player.Value.exp > playerExp)
                                {
                                    playerName = player.Value.name;
                                    playerExp = player.Value.exp;
                                }
                            }
                            msg = msg.Replace("$(HighestEXP)", "Highest Exp: " + playerName + ": " + playerExp.ToString() + " EXP");
                        }
                        else if (msg.Contains("$(LowestEXP)"))
                        {
                            string playerName = string.Empty;
                            int playerExp = 0;
                            foreach (var player in _state.Instances[id].PlayerList)
                            {
                                if (player.Value.exp < playerExp)
                                {
                                    playerName = player.Value.name;
                                    playerExp = player.Value.exp;
                                }
                            }
                            msg.Replace("$(LowestEXP)", "Lowest Exp: " + playerName + ": " + playerExp.ToString() + " EXP");
                        }
                        else if (msg.Contains("$(MostKills)"))
                        {
                            string playerName = string.Empty;
                            int playerKills = 0;
                            foreach (var player in _state.Instances[id].PlayerList)
                            {
                                if (player.Value.kills > playerKills)
                                {
                                    playerName = player.Value.name;
                                    playerKills = player.Value.kills;
                                }
                            }
                            msg.Replace("$(MostKills)", "Most Kills: " + playerName + ": " + playerKills);
                        }
                        else if (msg.Contains("$(MostDeaths)"))
                        {
                            string playerName = string.Empty;
                            int playerDeaths = 0;
                            foreach (var player in _state.Instances[id].PlayerList)
                            {
                                if (player.Value.deaths > playerDeaths)
                                {
                                    playerName = player.Value.name;
                                    playerDeaths = player.Value.deaths;
                                }
                            }
                            msg.Replace("$(MostDeaths)", "Most Deaths: " + playerName + ": " + playerDeaths);
                        }
                        else if (msg.Contains("$(BestKDR)"))
                        {
                            string playerName = string.Empty;
                            decimal playerKDR = 0;
                            foreach (var player in _state.Instances[id].PlayerList)
                            {
                                if ((decimal)(player.Value.kills / player.Value.deaths) > playerKDR)
                                {
                                    playerName = player.Value.name;
                                    playerKDR = (decimal)(player.Value.kills / player.Value.deaths);
                                }
                            }
                            msg.Replace("$(BestKDR)", "Best KDR: " + playerName + ": " + playerKDR);
                        }
                        else if (msg.Contains("$(MostRevives)"))
                        {
                            string playerName = string.Empty;
                            int playerRevives = 0;
                            foreach (var player in _state.Instances[id].PlayerList)
                            {
                                if (player.Value.playerrevives > playerRevives)
                                {
                                    playerName = player.Value.name;
                                    playerRevives = player.Value.playerrevives;
                                }
                            }
                            msg.Replace("$(MostRevives)", "Most Player Revives: " + playerName + ": " + playerRevives);
                        }
                        else if (msg.Contains("$(MostHeadshots)"))
                        {
                            string playerName = string.Empty;
                            int playerHeadshots = 0;
                            foreach (var player in _state.Instances[id].PlayerList)
                            {
                                if (player.Value.headshots > playerHeadshots)
                                {
                                    playerName = player.Value.name;
                                    playerHeadshots = player.Value.headshots;
                                }
                            }
                            msg.Replace("$(MostHeadshots)", "Most Headshots: " + playerName + ": " + playerHeadshots);
                        }
                        else if (msg.Contains("$(MostSuicides)"))
                        {
                            string playerName = string.Empty;
                            int playerSuicides = 0;
                            foreach (var player in _state.Instances[id].PlayerList)
                            {
                                if (player.Value.suicides > playerSuicides)
                                {
                                    playerName = player.Value.name;
                                    playerSuicides = player.Value.suicides;
                                }
                            }
                            msg.Replace("$(MostSuicides)", "Most Suicides: " + playerName + ": " + playerSuicides);
                        }
                        else if (msg.Contains("$(MostTKs)"))
                        {
                            string playerName = string.Empty;
                            int playerTKs = 0;
                            foreach (var player in _state.Instances[id].PlayerList)
                            {
                                if (player.Value.teamkills > playerTKs)
                                {
                                    playerName = player.Value.name;
                                    playerTKs = player.Value.teamkills;
                                }
                            }
                            msg.Replace("$(MostTKs)", "Most Team Kills: " + playerName + ": " + playerTKs);
                        }

                        if (msg == "" || msg == string.Empty)
                        {
                            return;
                        }

                        // Send the message to the Server
                        (new ServerManagement()).SendChatMessage(ref _state, id, ChatChannels[1], msg);


                        // Set the next interval for AutoMessages
                        _state.Instances[id].AutoMessages.NextMessage = DateTime.Now.AddMinutes(_state.Instances[id].AutoMessages.interval);
                        _state.Instances[id].AutoMessages.MsgNumber++;
                    }
                }
                else
                {
                    return; // return if there are no messages present
                }
            }
            else
            {
                return; // return if the AutoMessages are disabled
            }
        }

    }
}
