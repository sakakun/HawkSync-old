using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using HawkSync_RC.classes;
using HawkSync_RC.TVFunctions;
using log4net;

namespace HawkSync_RC
{
    public partial class RC_RotationManager : Form
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly AppState _state;
        private readonly int ArrayID = -1;
        private List<MapList> avilableMaps;
        private readonly RCSetup RCSetup;
        private List<MapList> selectedMaps;
        private readonly TVRotationManagerCMD TVRotationManagerCMD;
        private string uploadDirectory;

        public RC_RotationManager(AppState state, RCSetup setup, int instanceID)
        {
            InitializeComponent();
            _state = state;
            RCSetup = setup;
            uploadDirectory = string.Empty;
            foreach (var instance in _state.Instances)
                if (instance.Value.Id == instanceID)
                {
                    ArrayID = instance.Key;
                    break;
                }

            if (ArrayID == -1)
            {
                MessageBox.Show("We couldn't detect the correct profile...", "Something went wrong...");
                return;
            }

            TVRotationManagerCMD = new TVRotationManagerCMD(RCSetup, _state, ArrayID);
            avilableMaps = new List<MapList>();
            selectedMaps = new List<MapList>();
            label1.Text = string.Empty;
        }

        private void RotationManager_Load(object sender, EventArgs e)
        {
            // load game types
            foreach (var gameType in _state.autoRes.gameTypes) comboBox8.Items.Add(gameType.Value.Name);
            comboBox8.SelectedIndex = 0;


            // load saved rotations
            listBox6.Items.Add("New Rotation");
            foreach (var item in _state.Instances[ArrayID].savedmaprotations) listBox6.Items.Add(item.Description);
            listBox6.SelectedIndex = 0;
        }

        private void comboBox8_SelectedIndexChanged(object sender, EventArgs e)
        {
            // get gametype ID
            var gameTypeIndex = -1;
            var selectedGameType = string.Empty;
            foreach (var gametype in _state.autoRes.gameTypes)
                if (gametype.Value.Name == comboBox8.SelectedItem.ToString())
                {
                    gameTypeIndex = gametype.Value.DatabaseId;
                    selectedGameType = gametype.Value.ShortName;
                    break;
                }

            listBox4.Items.Clear();
            avilableMaps.Clear();
            avilableMaps = new List<MapList>();
            foreach (var item in _state.Instances[ArrayID].availableMaps)
            {
                var found = false;
                foreach (var gametype in item.Value.GameTypes)
                    if (gametype == gameTypeIndex)
                    {
                        found = true;
                        break;
                    }
                    else
                    {
                    }

                if (found)
                {
                    listBox4.Items.Add(item.Value.MapName + " <" + item.Value.MapFile + ">");
                    avilableMaps.Add(new MapList
                    {
                        CustomMap = item.Value.CustomMap,
                        GameType = selectedGameType,
                        MapFile = item.Value.MapFile,
                        MapName = item.Value.MapName
                    });
                }
            }
        }

        private void listBox5_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (listBox5.SelectedIndex == -1) return;

                if (listBox5.SelectedItems.Count > 1)
                {
                    contextMenuStrip1.Show(listBox5, new Point(e.X, e.Y));
                }
                else
                {
                    listBox5.SelectedIndex = listBox5.IndexFromPoint(new Point(e.X, e.Y));
                    contextMenuStrip1.Show(listBox5, new Point(e.X, e.Y));
                }
            }
            else
            {
            }
        }

        private void listBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox6.SelectedIndex == -1) return; // do nothing since nothing was selected...

            // new entry
            if (listBox6.SelectedIndex == 0)
            {
                listBox5.Items.Clear();
                label1.Text = "0";
                textBox14.Enabled = true;
                selectedMaps.Clear();
                selectedMaps = new List<MapList>();
                textBox14.Text = string.Empty;
            }
            else
            {
                // selected a saved rotation
                listBox5.Items.Clear();
                var rotationIndex = listBox6.SelectedIndex;
                rotationIndex--;
                foreach (var item in _state.Instances[ArrayID].savedmaprotations[rotationIndex].mapcycle)
                    listBox5.Items.Add("|" + item.GameType + "| " + item.MapName + " <" + item.MapFile + ">");
                label1.Text = listBox5.Items.Count.ToString();
                textBox14.Text = _state.Instances[ArrayID].savedmaprotations[rotationIndex].Description;
                selectedMaps = _state.Instances[ArrayID].savedmaprotations[rotationIndex].mapcycle;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            // save rotation
            if (listBox5.Items.Count == 0)
            {
                MessageBox.Show("You need to add at least one map to the rotation before you can save it...", "Error");
                return;
            }

            if (listBox6.SelectedIndex == 0)
            {
                if (TVRotationManagerCMD.CreateRotation(selectedMaps, textBox14.Text) == OpenClass.Status.SUCCESS)
                {
                    listBox6.Items.Add(textBox14.Text);
                    MessageBox.Show("Rotation saved successfully!", "Success");
                    listBox6.SelectedIndex = listBox6.Items.Count - 1;
                }
                else
                {
                    MessageBox.Show("Something went wrong file trying to save the rotation.", "Error");
                }
            }
            else
            {
                if (TVRotationManagerCMD.UpdateRotation(selectedMaps, textBox14.Text, listBox6.SelectedIndex - 1) ==
                    OpenClass.Status.SUCCESS)
                {
                    MessageBox.Show("Rotation saved successfully!", "Success");
                }
                else
                {
                    MessageBox.Show("Something went wrong file trying to save the rotation.", "Error");
                }
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            // delete rotation
            if (listBox6.SelectedIndex == -1 ||
                listBox6.SelectedIndex == 0) return; // do nothing since nothing was selected...
            if (TVRotationManagerCMD.DeleteRotation(_state.Instances[ArrayID]
                    .savedmaprotations[listBox6.SelectedIndex - 1].RotationID) == OpenClass.Status.SUCCESS)
            {
                listBox6.Items.RemoveAt(listBox6.SelectedIndex);
                listBox6.SelectedIndex = listBox6.Items.Count - 1;
            }
            else
            {
                MessageBox.Show("Something went wrong while attempting to delete the rotation.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void listBox4_DoubleClick(object sender, EventArgs e)
        {
            if (listBox4.SelectedIndex == -1) return;

            // wtf - suggested by AI... so close..
            var mapName = avilableMaps[listBox4.SelectedIndex].MapName;
            var mapFile = avilableMaps[listBox4.SelectedIndex].MapFile;
            var gameType = avilableMaps[listBox4.SelectedIndex].GameType;
            // IT'S SO FKING SMART WTF
            listBox5.Items.Add("|" + gameType + "| " + mapName + " <" + mapFile + ">");
            label1.Text = listBox5.Items.Count.ToString();
            selectedMaps.Add(avilableMaps[listBox4.SelectedIndex]);
        }

        private void listBox5_DoubleClick(object sender, EventArgs e)
        {
            if (listBox5.SelectedIndex == -1) return; // do nothing since nothing was selected...
            selectedMaps.RemoveAt(listBox5.SelectedIndex);
            listBox5.Items.RemoveAt(listBox5.SelectedIndex);
        }
    }
}