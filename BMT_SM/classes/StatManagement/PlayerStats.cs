using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

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
        public void RecordPlayerStats(AppState _state, int instanceID, string playerId, PlayerStats stats)
        {
            try
            {
                if (!_state.Instances[instanceID].playerStats.ContainsKey(playerId))
                {
                    _state.Instances[instanceID].playerStats[playerId] = stats;
                }

                DateTime firstSeen = _state.Instances[instanceID].playerStats[playerId].FirstSeen;
                _state.Instances[instanceID].playerStats[playerId] = stats;
                stats.FirstSeen = firstSeen; // fix the first seen time stamp
                stats.LastSeen = DateTime.Now; // Update the last seen timestamp
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message} {e.Source}");
            }

        }

        public void CollectPlayerStats(AppState _state, int instanceID)
        {

            // Cycle through all the players in the game and collect their stats
            // _state.Instances[instanceID].PlayerList[] contains the player data as ob_playerList object
            // Needs to be able to pass the whol object using its key to the RecordPlayerStats method
            if (_state.Instances[instanceID].PlayerList.Count > 0)
            {
                foreach (var player in _state.Instances[instanceID].PlayerList)
                {
                    // Create a new PlayerStats object
                    PlayerStats stats = new PlayerStats(player.Value.name, player.Value.slot, player.Value.team,
                        _state.Instances[instanceID].PlayerList[player.Key]);
                    // Record the stats
                    RecordPlayerStats(_state, instanceID, stats.PlayerId, stats);
                }
            }
            
        }

    }

}