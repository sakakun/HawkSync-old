using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows.Forms;

namespace HawkSync_SM
{
    public partial class SM_PopupSaveRotation : Form
    {
        int ArrayID = -1;
        AppState _state;
        List<MapList> mapLists;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public SM_PopupSaveRotation(AppState state, int instanceID, ref List<MapList> selectedMapList)
        {
            InitializeComponent();
            _state = state;
            ArrayID = instanceID;
            mapLists = selectedMapList;
        }

        private void Popup_SaveRotation_Load(object sender, EventArgs e)
        {
            if (_state.Instances[ArrayID].savedmaprotations.Count != 0)
            {
                foreach (var entry in _state.Instances[ArrayID].savedmaprotations)
                {
                    comboBox1.Items.Add(entry.Description);
                }
                comboBox1.SelectedIndex = 0;
            }
            else
            {
                checkBox1.Checked = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                db.Open();
                SQLiteCommand cmdInsert = new SQLiteCommand("INSERT INTO `instances_map_rotations` (`rotation_id`, `profile_id`, `description`, `mapcycle`) VALUES (NULL, @profileid, @description, @mapcycle);", db);
                cmdInsert.Parameters.AddWithValue("@profileid", _state.Instances[ArrayID].Id);
                cmdInsert.Parameters.AddWithValue("@description", textBox1.Text);
                cmdInsert.Parameters.AddWithValue("@mapcycle", JsonConvert.SerializeObject(mapLists));
                cmdInsert.ExecuteNonQuery();
                cmdInsert.Dispose();
                _state.Instances[ArrayID].savedmaprotations.Add(new savedmaprotations
                {
                    Description = textBox1.Text,
                    RotationID = (int)db.LastInsertRowId,
                    mapcycle = mapLists
                });
                db.Close();
                db.Dispose();
            }
            else
            {
                SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                db.Open();
                SQLiteCommand cmd = new SQLiteCommand("UPDATE `instances_map_rotations` SET `mapcycle` = @mapcycle WHERE `rotation_id` = @rotationID;", db);
                cmd.Parameters.AddWithValue("@mapcycle", JsonConvert.SerializeObject(mapLists));
                cmd.Parameters.AddWithValue("@rotationID", _state.Instances[ArrayID].savedmaprotations[comboBox1.SelectedIndex].RotationID);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                db.Close();
                db.Dispose();
                _state.Instances[ArrayID].savedmaprotations[comboBox1.SelectedIndex].mapcycle = mapLists;
            }
            MessageBox.Show("Rotation has been saved!", "Success");
            this.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                textBox1.Enabled = true;
                comboBox1.Enabled = false;
            }
            else
            {
                textBox1.Enabled = false;
                comboBox1.Enabled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
