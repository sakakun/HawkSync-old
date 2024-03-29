using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace HawkSync_SM
{
    public partial class SM_PopupLoadRotation : Form
    {
        AppState _state;
        int ArrayID = -1;
        public static List<MapList> _mapList = new List<MapList>();
        public SM_PopupLoadRotation(AppState state, int instanceID)
        {
            InitializeComponent();
            _state = state;
            ArrayID = instanceID;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_state.Instances[ArrayID].MapListRotationDB.Count == 0)
            {
                return;
            }

            int selectedMapCycle = comboBox1.SelectedIndex;

            _state.Instances[ArrayID].MapListPrevious = new Dictionary<int, MapList>();
            foreach (var mapEntry in _state.Instances[ArrayID].MapListCurrent)
            {
                _state.Instances[ArrayID].MapListPrevious.Add(_state.Instances[ArrayID].MapListPrevious.Count, mapEntry.Value);
            }
            _state.Instances[ArrayID].MapListCurrent = new Dictionary<int, MapList>();
            foreach (var map in _state.Instances[ArrayID].MapListRotationDB[selectedMapCycle].mapcycle)
            {
                _state.Instances[ArrayID].MapListCurrent.Add(_state.Instances[ArrayID].MapListCurrent.Count, map);
            }
            _mapList = _state.Instances[ArrayID].MapListRotationDB[selectedMapCycle].mapcycle;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Popup_LoadRotation_Load(object sender, EventArgs e)
        {
            if (_state.Instances[ArrayID].MapListRotationDB.Count != 0)
            {
                foreach (var entry in _state.Instances[ArrayID].MapListRotationDB)
                {
                    comboBox1.Items.Add(entry.Description);
                }
                comboBox1.SelectedIndex = 0;
            }
            else
            {
                comboBox1.Items.Add("No Rotations Saved.");
                comboBox1.SelectedIndex = 0;
                comboBox1.Enabled = false;
            }
        }
    }
}
