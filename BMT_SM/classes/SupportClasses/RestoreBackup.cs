using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using System.Reflection; // For WinForms

namespace HawkSync_SM
{
    public class GameBackupSettings 
    { 
        // Declare all objects of Instance that need to be backed up.
        public string   gameServerName          { get; set; } = "Untitled Server";
        public string   gameMOTD                { get; set; } = "Welcome to the server!";
        public string   gameCountryCode         { get; set; } = "US";
        public string   gameHostName            { get; set; } = "HostName";
        public bool     gameDedicated           { get; set; } = true;
        public bool     gameWindowedMode        { get; set; } = true;
        public string   gamePasswordLobby       { get; set; }
        public string   gamePasswordBlue        { get; set; }
        public string   gamePasswordRed         { get; set; }
        public int      gameSessionType         { get; set; } = 0;                              /* Session Type (Internet/LAN) - Currently not working. */
        public int      gameMaxSlots            { get; set; } = 50;                             /* Max Players 50 */
        public int      gameLoopMaps            { get; set; } = 1;                              /* 0 = Play One Map, 1 = Loop Maps, 2 = Cycle Maps (Starting Server requires a T/F bool) */
        public bool     gameRequireNova         { get; set; } = false;
        public bool     gameCustomSkins         { get; set; } = false;
        public int      gameScoreKills          { get; set; } = 20;                             /* Game Score needed for T/DM Matches to Win */
        public int      gameScoreFlags          { get; set; } = 10;                             /* Game Score needed for CTF & FB to Win */
        public int      gameScoreZoneTime       { get; set; } = 10;                             /* Game Score needed for T/KOTH to Win */
        public int      gameFriendlyFireKills   { get; set; } = 10;                             /* Game Friendly Fire Kills allow before punt. */
        public int      gameTimeLimit           { get; set; } = 22;                             /* Time limit per game, minutes */
        public int      gameStartDelay          { get; set; } = 2;                              /* Game start delay (minutes) */
        public int      gameRespawnTime         { get; set; } = 20;                             /* Respawn Time in Minutes */
        public int      gameScoreBoardDelay     { get; set; } = 20;                             /* Score Board Delay in Seconds */
        public int      gamePSPTOTimer          { get; set; } = 20;                             /* PSP Take Over Timer in Seconds */
        public int      gameFlagReturnTime      { get; set; } = 4;                              /* Flag retrun time in minutes */
        public int      gameMaxTeamLives        { get; set; } = 20;                             /* Max Team Lives, don't know what this is really used for... */
        // Game Settings: Misc Settings
        public bool     gameOneShotKills        { get; set; } = false;
        public bool     gameFatBullets          { get; set; } = false;
        public bool     gameDestroyBuildings    { get; set; } = false;
        public int      gameAllowLeftLeaning    { get; set; } = 0;
        // Game Settings: Ping Settings
        public bool     gameMinPing             { get; set; } = false;
        public bool     gameMaxPing             { get; set; } = false;
        public int      gameMinPingValue        { get; set; } = 0;
        public int      gameMaxPingValue        { get; set; } = 0;
        // Game Settings: Game Options
        public bool     gameOptionAutoBalance   { get; set; } = true;
        public bool     gameOptionFF            { get; set; } = false;
        public bool     gameOptionFFWarn        { get; set; } = false;
        public bool     gameOptionFriendlyTags  { get; set; } = true;
        public bool     gameOptionShowTracers   { get; set; } = false;
        public bool     gameShowTeamClays       { get; set; } = true;
        public bool     gameOptionAutoRange     { get; set; } = false;
    }

    internal class RestoreBackup
    {
        // Initalize variables
        public void ExportSettings(AppState _state, int instanceID)
        {
            GameBackupSettings backupObject = new GameBackupSettings();
            Instance instanceObject = _state.Instances[instanceID];

            PropertyInfo[] backupProperties = typeof(GameBackupSettings).GetProperties();
            PropertyInfo[] instanceProperties = typeof(Instance).GetProperties();

            foreach (var backupProp in backupProperties)
            {
                foreach (var instanceProp in instanceProperties)
                {
                    if (backupProp.Name == instanceProp.Name)
                    {
                        backupProp.SetValue(backupObject, instanceProp.GetValue(instanceObject));
                        break;
                    }
                }
            }

            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON files (*.json)|*.json";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Export specific object to selected file location
                ExportObject(backupObject, saveFileDialog.FileName);
            }

        }
        public void ImportSettings(AppState _state, int instanceID)
        {

            // Show OpenFileDialog to select import file location
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JSON files (*.json)|*.json";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Import specific object from selected file location
                    GameBackupSettings backupObject = ImportObject(openFileDialog.FileName);
                    
                    // Use the imported object as needed
                    Instance instanceObject = _state.Instances[instanceID];

                    PropertyInfo[] backupProperties = typeof(GameBackupSettings).GetProperties();
                    PropertyInfo[] instanceProperties = typeof(Instance).GetProperties();

                    foreach (var backupProp in backupProperties)
                    {
                        foreach (var instanceProp in instanceProperties)
                        {
                            if (backupProp.Name == instanceProp.Name)
                            {
                                instanceProp.SetValue(instanceObject, backupProp.GetValue(backupObject));
                                break;
                            }
                        }
                    }

                    MessageBox.Show("Settings restored successfully.");

                } catch (Exception ex)
                {
                    MessageBox.Show($"Failed to import object: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                
            }
        }
        private void ExportObject(GameBackupSettings obj, string filePath)
        {
            // Serialize the object to JSON
            string jsonString = JsonSerializer.Serialize(obj);
            // Write JSON string to file
            File.WriteAllText(filePath, Crypt.Base64Encode(jsonString));
        }

        private GameBackupSettings ImportObject(string filePath)
        {

            // Read JSON string from file
            string jsonString = File.ReadAllText(filePath);

            // Deserialize JSON string to object
            return JsonSerializer.Deserialize<GameBackupSettings>(Crypt.Base64Decode(jsonString));
            
        }
    }



}
