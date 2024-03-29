using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using static HawkSync_SM.ob_playerList;

namespace HawkSync_SM.classes.StatManagement
{
    public class PlayerStats
    {
        public string PlayerId { get; set; } // Unique identifier for the player
        public string PlayerName { get; set; }
        public int PlayerSlot { get; set; }
        public int Team { get; set; }
        public ob_playerList PlayerData { get; set; }
        public DateTime FirstSeen { get; set; }
        public DateTime LastSeen { get; set; }

        public PlayerStats(string playerName, int playerSlot, int team, ob_playerList playerData)
        {
            PlayerId = GeneratePlayerId(playerName, playerSlot, team);
            PlayerName = playerName;
            PlayerSlot = playerSlot;
            Team = team;
            PlayerData = playerData;
            FirstSeen = DateTime.Now;
            LastSeen = DateTime.Now;
        }

        private static string GeneratePlayerId(string playerName, int playerSlot, int team)
        {
            string combinedInfo = $"{playerName}-{playerSlot}-{team}";
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(combinedInfo));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }

    
    public class PlayerStatsManager
    {
        public void RecordPlayerStats(AppState _state, int instanceID, string playerId, PlayerStats currentStats)
        {
            int weaponID;
            weaponID = (int)Enum.Parse(typeof(WeaponStack), currentStats.PlayerData.selectedWeapon);
            DateTime currentTime = DateTime.Now;
            DateTime firstSeen = DateTime.Now;
            WeaponStats recordWeaponStats = new WeaponStats();

            try
            {

                if (!_state.Instances[instanceID].PlayerStats.ContainsKey(playerId))
                {
                    _state.Instances[instanceID].PlayerStats[playerId] = currentStats;
                    WeaponStats.RecordWeaponStats(_state, instanceID, playerId, weaponID, currentStats.PlayerData.kills, currentStats.PlayerData.totalshots, 0);
                
                } else
                {
                    PlayerStats oldStats = _state.Instances[instanceID].PlayerStats[playerId];
                    int diffKills = currentStats.PlayerData.kills - oldStats.PlayerData.kills;
                    int diffShotsFired = currentStats.PlayerData.totalshots - oldStats.PlayerData.totalshots;
                    double diffTimer = ((currentTime - _state.Instances[instanceID].PlayerStats[playerId].LastSeen).TotalSeconds);
                    WeaponStats.RecordWeaponStats(_state, instanceID, playerId, weaponID, diffKills, diffShotsFired, diffTimer);

                    firstSeen = _state.Instances[instanceID].PlayerStats[playerId].FirstSeen;
                    _state.Instances[instanceID].PlayerStats[playerId] = currentStats;
                }

                currentStats.FirstSeen = firstSeen; // fix the first seen time stamp
                currentStats.LastSeen = currentTime; // Update the last seen timestamp

            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message} {e.Source}");
            }

        }

        public void CollectPlayerStats(AppState _state, int instanceID)
        {

            // Cycle through all the players in the game and collect their currentStats
            // _state.Instances[instanceID].PlayerList[] contains the player data as ob_playerList object
            // Needs to be able to pass the whole object using its key to the RecordPlayerStats method
            if (_state.Instances[instanceID].PlayerList.Count > 0)
            {
                foreach (var player in _state.Instances[instanceID].PlayerList)
                {
                    // Create a new PlayerStats object
                    PlayerStats stats = new PlayerStats(player.Value.name, player.Value.slot, player.Value.team,_state.Instances[instanceID].PlayerList[player.Key]);
                    // Record the currentStats
                    RecordPlayerStats(_state, instanceID, stats.PlayerId, stats);
                }
            }
            
        }

    }
    public class PlayerWeaponStats
    {
        public string PlayerId { get; set; }
        public List<InternalWeaponStats> WeaponStatsList { get; set; }

        public PlayerWeaponStats(string playerId)
        {
            PlayerId = playerId;
            WeaponStatsList = new List<InternalWeaponStats>();
        }

    }

    public class WeaponStats
    {
        public static void RecordWeaponStats(AppState _state, int instanceID, string playerId, int weaponId, int kills, int shotsFired, double timer)
        {
            // Check if the player already has a weapon currentStats record
            if (!_state.Instances[instanceID].PlayerWeaponStats.ContainsKey(playerId))
            {
                _state.Instances[instanceID].PlayerWeaponStats[playerId] = new PlayerWeaponStats(playerId);
            }

            List<InternalWeaponStats> weaponStats = _state.Instances[instanceID].PlayerWeaponStats[playerId].WeaponStatsList;

            //Console.WriteLine(JsonConvert.SerializeObject(_state.Instances[instanceID].PlayerWeaponStats[playerId]));

            // Check if the weapon ID exists in the weaponStats list
            InternalWeaponStats weaponStatsEntry = weaponStats.FirstOrDefault(ws => ws.weaponid == weaponId);

            // If the weapon ID exists, update the statistics
            if (weaponStatsEntry != null)
            {
                weaponStatsEntry.kills += kills;
                weaponStatsEntry.shotsfired += shotsFired;
                weaponStatsEntry.timer += timer;
            }
            else
            {
                // If the weapon ID doesn't exist, add a new entry
                weaponStats.Add(new InternalWeaponStats
                {
                    weaponid = weaponId,
                    kills = kills,
                    shotsfired = shotsFired,
                    timer = timer
                });
            }

        }
    }

    public class scoreManagement
    {
        public int totalScores { get; set; }
        public int blueScore { get; set; }
        public int redScore { get; set; }

        public scoreManagement(int _totalScores = 0, int _blueScores = 0, int _redScores = 0)
        {
            totalScores = _totalScores;
            blueScore = _redScores;
            redScore = _blueScores;
        }
    }
}