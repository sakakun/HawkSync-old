using HawkSync_SM.classes.MapManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows.Forms;

namespace HawkSync_SM
{
    public partial class SM_RotationManager : Form
    {
        AppState _state;
        int ArrayID = -1;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        string selectedGameType = "";

        private bool deleteTriggered = false;
        private bool moveMap = false;

        List<MapList> avilableMaps = new List<MapList>();
        List<MapList> selectedMaps = new List<MapList>();

        public SM_RotationManager(AppState state, int instanceid)
        {
            InitializeComponent();
            _state = state;
            ArrayID = instanceid;
            label10.Text = "0 / 128";
        }

        private void RotationManager_Load(object sender, EventArgs e)
        {
            (new AvailMaps()).checkAvailableMaps(_state, ArrayID);

            foreach (var gameType in _state.autoRes.gameTypes)
            {
                drop_gameTypes.Items.Add(gameType.Value.Name);
            }
            drop_gameTypes.SelectedIndex = 0;
            list_savedMapRotation.Items.Add("New Rotation");

            if (_state.Instances[ArrayID].savedmaprotations != null)
            {
                foreach (var savedMapCycle in _state.Instances[ArrayID].savedmaprotations)
                {
                    list_savedMapRotation.Items.Add(savedMapCycle.Description);
                }
                list_savedMapRotation.SelectedIndex = 0;
            }
        }

        private void dropGameTypeChanged(object sender, EventArgs e)
        {
            list_availableMaps.Items.Clear();
            avilableMaps.Clear();
            int gameTypeIndex = -1;
            ComboBox comboBox = (ComboBox)sender;
            foreach (var gametype in _state.autoRes.gameTypes)
            {
                if (gametype.Value.Name == comboBox.SelectedItem.ToString())
                {
                    gameTypeIndex = gametype.Value.DatabaseId;
                    selectedGameType = gametype.Value.ShortName;
                    break;
                }
            }
            foreach (var map in _state.Instances[ArrayID].availableMaps)
            {
                bool found = false;
                foreach (var gametype in map.Value.GameTypes)
                {
                    if (gametype == gameTypeIndex)
                    {
                        found = true;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (found == true)
                {
                    list_availableMaps.Items.Add(map.Value.MapName + " <" + map.Value.MapFile + ">");
                    avilableMaps.Add(new MapList
                    {
                        CustomMap = map.Value.CustomMap,
                        GameType = selectedGameType,
                        MapFile = map.Value.MapFile,
                        MapName = map.Value.MapName
                    });
                }
            }
        }

        private void availMaps_DoubleClick(object sender, EventArgs e)
        {
            if (list_selectedRotation.Items.Count == 128)
            {
                MessageBox.Show("Due to limitations set by NovaLogic,\nYou can only have 128 maps.", "Error");
                return;
            }
            if (list_selectedRotation.Items.Count > 128)
            {
                MessageBox.Show("Database Error, Start again...", "Error");
            }
            if (list_savedMapRotation.SelectedIndex != 0)
            {
                list_savedMapRotation.SelectedIndex = 0;
            }

            int selectedIndex = list_availableMaps.SelectedIndex;
            selectedMaps.Add(avilableMaps[selectedIndex]);
            list_selectedRotation.Items.Add("|" + selectedGameType + "| " + avilableMaps[selectedIndex].MapName + " <" + avilableMaps[selectedIndex].MapFile + ">");
            label10.Text = list_selectedRotation.Items.Count.ToString() + " / 128";

        }

        private void listBox2_DoubleClick(object sender, EventArgs e)
        {
            int selectedIndex = list_selectedRotation.SelectedIndex;
            selectedIndex--;
            selectedMaps.RemoveAt(selectedIndex);
            list_selectedRotation.Items.RemoveAt(selectedIndex);
            label10.Text = list_selectedRotation.Items.Count.ToString() + " / 128";
        }

        private void save_maprotation(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(text_RotationDesc.Text))
            {
                MessageBox.Show("Please enter a valid description.", "Missing Entry");
                return;
            }

            if (list_selectedRotation.Items.Count < 1)
            {
                MessageBox.Show("Please select maps to create a new rotation.", "Missing Maps");
                return;
            }

            SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
            db.Open();
            if (list_savedMapRotation.SelectedIndex == 0)
            {
                SQLiteCommand numChkCmd = new SQLiteCommand("SELECT COUNT(*) FROM `instances_map_rotations` WHERE `profile_id` = @profileid;", db);
                numChkCmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                int numChk = Convert.ToInt32(numChkCmd.ExecuteScalar());
                numChkCmd.Dispose();
                int nextID = -1;
                if (numChk == 0)
                {
                    nextID = 1;
                }
                else
                {
                    SQLiteCommand numCmd = new SQLiteCommand("SELECT `rotation_id` FROM `instances_map_rotations` WHERE `profile_id` = @profileid ORDER BY `rotation_id` DESC;", db);
                    numCmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                    nextID = Convert.ToInt32(numCmd.ExecuteScalar());
                    numCmd.Dispose();
                    nextID++;
                }
                if (ProgramConfig.Debug)
                {
                    log.Info("Creating new entry for instances_map_rotations: ID: " + nextID);
                }
                SQLiteCommand cmd = new SQLiteCommand("INSERT INTO `instances_map_rotations` (`rotation_id`, `profile_id`, `description`, `mapcycle`) VALUES (@entryid, @profileid, @description, @mapcycle);", db);
                cmd.Parameters.AddWithValue("@entryid", nextID);
                cmd.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                cmd.Parameters.AddWithValue("@description", text_RotationDesc.Text);
                cmd.Parameters.AddWithValue("@mapcycle", JsonConvert.SerializeObject(selectedMaps));
                cmd.ExecuteNonQuery();
                cmd.Dispose();

                _state.Instances[ArrayID].savedmaprotations.Add(new savedmaprotations
                {
                    Description = text_RotationDesc.Text,
                    RotationID = nextID,
                    mapcycle = selectedMaps
                });

                list_savedMapRotation.Items.Add(text_RotationDesc.Text);

                if (list_savedMapRotation.Items.Count == 0)
                {
                    list_savedMapRotation.SelectedIndex = 0;
                }
                else
                {
                    list_savedMapRotation.SelectedIndex = list_savedMapRotation.Items.Count - 1;
                }
            }
            else
            {
                if (ProgramConfig.Debug)
                {
                    log.Info("Updating entry for instances_map_rotations.");
                }
                int selectedCycle = list_savedMapRotation.SelectedIndex;
                selectedCycle++;
                SQLiteCommand cmd = new SQLiteCommand("UPDATE `instances_map_rotations` SET `description` = @description, `mapcycle` = @mapcycle WHERE `rotation_id` = @rotationID;", db);
                cmd.Parameters.AddWithValue("@description", text_RotationDesc.Text);
                cmd.Parameters.AddWithValue("@mapcycle", JsonConvert.SerializeObject(selectedMaps));
                cmd.Parameters.AddWithValue("@rotationID", _state.Instances[ArrayID].savedmaprotations[selectedCycle].RotationID);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                _state.Instances[ArrayID].savedmaprotations[selectedCycle].mapcycle = selectedMaps;
                _state.Instances[ArrayID].savedmaprotations[selectedCycle].Description = text_RotationDesc.Text;
                selectedMaps = new List<MapList>();
            }
            db.Close();
            db.Dispose();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (list_savedMapRotation.SelectedIndex == -1)
            {
                return;
            }
            if (moveMap == true)
            {
                return;
            }
            list_selectedRotation.Items.Clear();
            if (list_savedMapRotation.SelectedIndex == 0)
            {
                text_RotationDesc.Text = "New Entry";
                selectedMaps = new List<MapList>();
            }
            else
            {
                selectedMaps = new List<MapList>();
                if (deleteTriggered == true)
                {
                    return; // since we are not removing an entry, do not update list...
                }
                int selectedCycle = list_savedMapRotation.SelectedIndex;
                selectedCycle--;
                if (_state.Instances[ArrayID].savedmaprotations[selectedCycle].mapcycle.Count != 0)
                {
                    foreach (var mapCycle in _state.Instances[ArrayID].savedmaprotations[selectedCycle].mapcycle)
                    {
                        list_selectedRotation.Items.Add("|" + mapCycle.GameType + "| " + mapCycle.MapName + " <" + mapCycle.MapFile + ">");
                    }
                }
                text_RotationDesc.Text = _state.Instances[ArrayID].savedmaprotations[selectedCycle].Description;
                selectedMaps = _state.Instances[ArrayID].savedmaprotations[selectedCycle].mapcycle;
            }
            label10.Text = list_selectedRotation.Items.Count.ToString() + " / 128";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            deleteTriggered = true;
            // delete rotation
            int selectedCycle = list_savedMapRotation.SelectedIndex;
            if (selectedCycle == 0)
            {
                return; // do not delete "new rotation"
            }
            SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
            db.Open();
            SQLiteCommand cmd = new SQLiteCommand("DELETE FROM `instances_map_rotations` WHERE `rotation_id` = @rotationid;", db);
            int memoryLoc = selectedCycle - 1;
            if (ProgramConfig.Debug)
            {
                log.Info("Removing Rotation: " + _state.Instances[ArrayID].savedmaprotations[memoryLoc].RotationID);
            }
            cmd.Parameters.AddWithValue("@rotationid", _state.Instances[ArrayID].savedmaprotations[memoryLoc].RotationID);
            cmd.ExecuteNonQuery();
            _state.Instances[ArrayID].savedmaprotations.RemoveAt(memoryLoc);
            list_savedMapRotation.Items.RemoveAt(selectedCycle);
            list_selectedRotation.Items.Clear();
            list_availableMaps.ClearSelected();
            cmd.Dispose();
            db.Close();
            db.Dispose();
            deleteTriggered = false;
            if (list_savedMapRotation.Items.Count == 1)
            {
                list_savedMapRotation.SelectedIndex = 0;
            }
            else
            {
                list_savedMapRotation.SelectedIndex = list_savedMapRotation.Items.Count - 1;
            }
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (text_RotationDesc.Text == "" && list_savedMapRotation.SelectedIndex == 0)
            {
                text_RotationDesc.Text = "New Entry";
            }
            else if (text_RotationDesc.Text == "New Entry" && list_savedMapRotation.SelectedIndex != 0)
            {
                int selectedCycle = list_savedMapRotation.SelectedIndex;
                text_RotationDesc.Text = _state.Instances[ArrayID].savedmaprotations[selectedCycle].Description;
            }
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if ((text_RotationDesc.Text == "New Entry") && list_savedMapRotation.SelectedIndex == 0)
            {
                text_RotationDesc.Text = "";
            }
            else if ((text_RotationDesc.Text == "") || (string.IsNullOrEmpty(text_RotationDesc.Text) == true) || (string.IsNullOrWhiteSpace(text_RotationDesc.Text) == true) && list_savedMapRotation.SelectedIndex != 0)
            {
                int selectedCycle = list_savedMapRotation.SelectedIndex;
                text_RotationDesc.Text = _state.Instances[ArrayID].savedmaprotations[selectedCycle].Description;
            }
        }
        private void MoveMessage(int direction)
        {
            if (list_selectedRotation.SelectedItem == null || list_selectedRotation.SelectedIndex < 0)
            {
                return; // we haven't seleccted a message so it will return a -1 or NULL value.
            }
            else
            {
                int newIndex = list_selectedRotation.SelectedIndex + direction;

                // setup array bounds so we don't fuck up again...
                if (newIndex < 0 || newIndex >= list_selectedRotation.Items.Count)
                {
                    return; // Index out of range - nothing to do...
                }
                else
                {
                    // remove item then re-add at the correct position
                    object selected = list_selectedRotation.SelectedItem;
                    MapList mapListObj = selectedMaps[list_selectedRotation.SelectedIndex];
                    selectedMaps.Remove(mapListObj);
                    selectedMaps.Insert(newIndex, mapListObj);
                    list_selectedRotation.Items.Remove(selected);
                    list_selectedRotation.Items.Insert(newIndex, selected);
                    list_selectedRotation.SetSelected(newIndex, true);
                }
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            // move the selected item up the list...
            if (list_savedMapRotation.SelectedIndex != 0)
            {
                moveMap = true;
                list_savedMapRotation.SelectedIndex = 0;
                text_RotationDesc.Text = "New Entry";
            }
            MoveMessage(-1);
            moveMap = false;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (list_savedMapRotation.SelectedIndex != 0)
            {
                moveMap = true;
                list_savedMapRotation.SelectedIndex = 0;
                text_RotationDesc.Text = "New Entry";
            }
            MoveMessage(1);
            moveMap = false;
        }
    }
}
