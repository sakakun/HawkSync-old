using HawkSync_SM.classes;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;

namespace HawkSync_SM
{
    internal class PlayerManagement
    {
        /*
         * checkPlayerHistory
         * - This will update the internal player history list and the database with any new players that have joined the server.
         * - Player Related Checks (Not critical to Game Play should be done here.)
         */
        internal void checkPlayerHistory(ref AppState _state)
        {
            SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
            db.Open();
            foreach (var instance in _state.Instances)
            {
                if (instance.Value.PlayerList == null)
                {
                    continue;
                }
                foreach (var playerObj in instance.Value.PlayerList)
                {
                    bool found = false;
                    foreach (var playerHistory in _state.playerHistories)
                    {
                        if (playerHistory.playerName == playerObj.Value.name && playerHistory.playerIP == playerObj.Value.address)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found == false)
                    {
                        SQLiteCommand cmd = new SQLiteCommand("INSERT INTO `playerhistory` (`id`, `playername`, `ip`, `firstseen`) VALUES (NULL, @playername, @ip, @firstseen);", db);
                        cmd.Parameters.AddWithValue("@playername", playerObj.Value.name);
                        cmd.Parameters.AddWithValue("@ip", playerObj.Value.address);
                        cmd.Parameters.AddWithValue("@firstseen", DateTime.Now);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        _state.playerHistories.Add(new ob_playerHistory
                        {
                            DatabaseId = (int)db.LastInsertRowId,
                            firstSeen = DateTime.Now,
                            playerIP = playerObj.Value.address,
                            playerName = playerObj.Value.name
                        });
                        // HOOK: WelcomePlayers
                        if (_state.Instances[instance.Key].Plugins.WelcomeMessage)
                        {
                            _state.Instances[instance.Key].WelcomeQueue.Add(new WelcomePlayer
                            {
                                playerName = playerObj.Value.name,
                                ReturningPlayer = false,
                                Processed = false,
                                RunTime = DateTime.Now.AddSeconds(30.0),
                                Slot = playerObj.Value.slot
                            });
                        }
                    }
                    else
                    {
                        // HOOK: WelcomePlayers
                        if (_state.Instances[instance.Key].Plugins.WelcomeMessage)
                        {
                            bool plFound = false;
                            int index = -1;
                            foreach (var item in _state.Instances[instance.Key].WelcomeQueue)
                            {
                                index++;
                                if (item.playerName == playerObj.Value.name)
                                {
                                    plFound = true;
                                    break;
                                }
                            }
                            if (plFound != true)
                            {
                                _state.Instances[instance.Key].WelcomeQueue.Add(new WelcomePlayer
                                {
                                    playerName = playerObj.Value.name,
                                    ReturningPlayer = true,
                                    Processed = false,
                                    RunTime = DateTime.Now.AddSeconds(30.0),
                                    Slot = playerObj.Value.slot
                                });
                            }
                        }
                    }
                }
            }

            db.Close();
            db.Dispose();
        }

        internal void checkExpiredBans(ref AppState _state)
        {
            if (DateTime.Compare(ProgramConfig.checkExpiredBans, DateTime.Now) > 0)
            {
                return;
            }

            foreach (var inst in _state.Instances)
            {
                Instance instance = inst.Value;
                int InstanceID = instance.instanceID;
                try
                {
                    foreach (var ban in instance.PlayerListBans)
                    {
                        // remove if it's expired, even if the player isn't on the server...
                        if (ban.expires != "-1" && ban.expires != null)
                        {
                            if (DateTime.Compare(DateTime.Parse(ban.expires), DateTime.Now) < 0)
                            {
                                _state.Instances[inst.Key].PlayerListBans.Remove(ban);
                                // remove from SQLiteDB
                                SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                                db.Open();
                                SQLiteCommand cmdRemove = new SQLiteCommand("DELETE FROM `playerbans` WHERE `id` = @banid AND `profileid` = @profileid;", db);
                                cmdRemove.Parameters.AddWithValue("@banid", ban.id);
                                cmdRemove.Parameters.AddWithValue("@profileid", InstanceID);
                                cmdRemove.ExecuteNonQuery();
                                cmdRemove.Dispose();
                                db.Close();
                                db.Dispose();
                            }
                        }
                    }
                }
                catch
                {
                    return;
                }
            }
            // reset checkExpiredBansChecker
            ProgramConfig.checkExpiredBans = DateTime.Now.AddMinutes(5);
        }


    }
}
