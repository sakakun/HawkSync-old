using HawkSync_RC.classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HawkSync_RC
{
    public partial class Start_Game : Form
    {
        AppState _state;
        int ArrayID = -1;
        List<MapList> selectedMapList = new List<MapList>();
        List<MapList> selectedGameTypeMapList = new List<MapList>();
        Dictionary<int, MapList> avalMaps;
        RCSetup RCSetup = null;
        public Start_Game(AppState state, RCSetup setup, int arrayID)
        {
            InitializeComponent();
            _state = state;
            ArrayID = arrayID;
            RCSetup = setup;
            avalMaps = new Dictionary<int, MapList>();
        }

        private void Start_Game_Load(object sender, EventArgs e)
        {
            Dictionary<dynamic, dynamic> cmd = new Dictionary<dynamic, dynamic>
            {
                { "action", "BMTRC.GetAvalMaps" },
                { "serverID", _state.Instances[ArrayID].Id },
                { "SessionID", RCSetup.SessionID }
            };
            byte[] compressReply = RCSetup.SendCMD(cmd);
            Dictionary<dynamic, dynamic> data = JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(Encoding.Default.GetString(compressReply));
            if ((OpenClass.Status)data["Status"] == OpenClass.Status.SUCCESS)
            {
                avalMaps = JsonConvert.DeserializeObject<Dictionary<int, MapList>>(data["maps"]);
            }
            foreach (var gameType in _state.autoRes.gameTypes)
            {
                comboBox10.Items.Add(gameType.Value.Name);
            }
            comboBox10.SelectedIndex = 0;
            // fill slot
            int slotnum = 1;
            while (slotnum < 51)
            {
                comboBox2.Items.Add(slotnum);
                slotnum++;
            }
            // end fill slot
            // maxkills
            for (int maxkills = 1; maxkills < 501; maxkills++)
            {
                comboBox4.Items.Add(maxkills);
                comboBox5.Items.Add(maxkills);
            }
            //end maxkills
            for (int zonetimer = 1; zonetimer < 61; zonetimer++)
            {
                comboBox6.Items.Add(zonetimer);
                comboBox8.Items.Add(zonetimer);
            }
            for (int respawntime = 1; respawntime < 121; respawntime++)
            {
                comboBox7.Items.Add(respawntime);
            }
            textBox1.Text = _state.Instances[ArrayID].ServerName;
            textBox2.Text = _state.Instances[ArrayID].MOTD;
            textBox3.Text = _state.Instances[ArrayID].CountryCode;
            textBox4.Text = _state.Instances[ArrayID].Password;
            comboBox1.SelectedIndex = _state.Instances[ArrayID].SessionType;
            comboBox2.SelectedItem = _state.Instances[ArrayID].MaxSlots;
            comboBox3.SelectedIndex = _state.Instances[ArrayID].StartDelay;
            checkBox1.Checked = Convert.ToBoolean(_state.Instances[ArrayID].LoopMaps);
            comboBox4.SelectedIndex = _state.Instances[ArrayID].MaxKills;
            comboBox5.SelectedIndex = _state.Instances[ArrayID].GameScore;
            comboBox6.SelectedIndex = _state.Instances[ArrayID].ZoneTimer;
            comboBox7.SelectedIndex = _state.Instances[ArrayID].RespawnTime;
            comboBox8.SelectedIndex = _state.Instances[ArrayID].TimeLimit;
            checkBox2.Checked = _state.Instances[ArrayID].RequireNovaLogin;
            checkBox3.Checked = _state.Instances[ArrayID].WindowedMode;
            checkBox4.Checked = _state.Instances[ArrayID].AllowCustomSkins;
            checkBox5.Checked = _state.Instances[ArrayID].Dedicated;
            textBox5.Text = _state.Instances[ArrayID].BluePassword;
            textBox6.Text = _state.Instances[ArrayID].RedPassword;
            checkBox6.Checked = _state.Instances[ArrayID].FriendlyFire;
            checkBox8.Checked = _state.Instances[ArrayID].FriendlyFireWarning;
            checkBox7.Checked = _state.Instances[ArrayID].FriendlyTags;
            checkBox9.Checked = _state.Instances[ArrayID].AutoBalance;
            checkBox10.Checked = _state.Instances[ArrayID].ShowTracers;
            checkBox11.Checked = _state.Instances[ArrayID].ShowTeamClays;
            checkBox12.Checked = _state.Instances[ArrayID].AllowAutoRange;
            switch (_state.Instances[ArrayID].MinPing)
            {
                case false:
                    checkBox13.Checked = false;
                    textBox7.Text = "0";
                    break;
                case true:
                    textBox7.Text = Convert.ToString(_state.Instances[ArrayID].MinPingValue);
                    checkBox13.Checked = true;
                    break;
            }
            switch (_state.Instances[ArrayID].MaxPing)
            {
                case false:
                    checkBox14.Checked = false;
                    textBox8.Text = "0";
                    break;
                case true:
                    textBox8.Text = Convert.ToString(_state.Instances[ArrayID].MaxPingValue);
                    checkBox14.Checked = true;
                    break;
            }
            selectedMapList.Clear();
            foreach (var map in _state.Instances[ArrayID].MapList)
            {
                selectedMapList.Add(map.Value);
                listBox2.Items.Add("|" + map.Value.GameType + "| " + map.Value.MapName + " <" + map.Value.MapFile + ">");
            }
            label33.Text = listBox2.Items.Count.ToString() + " / 128";
        }

        private void comboBox10_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            selectedGameTypeMapList.Clear();

            foreach (var mapItem in avalMaps)
            {
                if (mapItem.Value.GameTypes.Contains(comboBox10.SelectedIndex))
                {
                    selectedGameTypeMapList.Add(new MapList
                    {
                        MapFile = mapItem.Value.MapFile,
                        MapName = mapItem.Value.MapName,
                        CustomMap = mapItem.Value.CustomMap
                    });
                    listBox1.Items.Add(mapItem.Value.MapName + " <" + mapItem.Value.MapFile + ">");
                }
            }
        }

        private void checkBox13_CheckedChanged(object sender, EventArgs e)
        {
            textBox7.Enabled = checkBox13.Checked;
        }

        private void checkBox14_CheckedChanged(object sender, EventArgs e)
        {
            textBox8.Enabled = checkBox14.Checked;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // reset maplist
            listBox2.Items.Clear();
            selectedMapList.Clear();
        }

        private void listBox2_DoubleClick(object sender, EventArgs e)
        {
            if (listBox2.SelectedIndex == -1)
            {
                return; // don't do anything since nothing is selected
            }
            selectedMapList.RemoveAt(listBox2.SelectedIndex);
            listBox2.Items.Remove(listBox2.SelectedItem);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            // update settings and start game
            // server name
            if (string.IsNullOrEmpty(textBox1.Text) || string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Please enter a valid Server Name!", "Error");
                return;
            } else if (string.IsNullOrEmpty(textBox2.Text) || string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Please enter a valid MOTD!", "Error");
                return;
            } else if (string.IsNullOrEmpty(textBox3.Text) || string.IsNullOrWhiteSpace(textBox3.Text) || textBox3.TextLength > 2)
            {
                MessageBox.Show("Please enter a valid 2 character Country Code!", "Error");
                return;
            } else if (listBox2.Items.Count == 0)
            {
                MessageBox.Show("Please enter at least 1 start up map!", "Error");
                return;
            }

            Dictionary<string, dynamic> request = new Dictionary<string, dynamic>()
            {
                { "SessionID", RCSetup.SessionID },
                { "action", "BMTRC.StartInstance" },
                { "serverID", _state.Instances[ArrayID].Id },
                { "ServerName", textBox1.Text },
                { "MOTD", textBox2.Text },
                { "CountryCode", textBox3.Text },
                { "password", textBox4.Text },
                { "SessionType", comboBox1.SelectedIndex },
                { "MaxSlots", Convert.ToInt32(comboBox2.SelectedItem) },
                { "StartDelay", comboBox3.SelectedIndex },
                { "MaxKills", comboBox4.SelectedIndex },
                { "GameScore", comboBox5.SelectedIndex },
                { "ZoneTimer", comboBox6.SelectedIndex },
                { "RespawnTimer", comboBox7.SelectedIndex },
                { "TimeLimit", comboBox8.SelectedIndex },
                { "RequireNovaLogin", checkBox2.Checked },
                { "WindowedMode", checkBox3.Checked },
                { "AllowCustomSkins", checkBox4.Checked },
                { "Dedicated", checkBox5.Checked },
                { "BluePassword", textBox5.Text },
                { "RedPassword", textBox6.Text },
                { "FriendlyFire", checkBox6.Checked },
                { "FriendlyFireWarning", checkBox8.Checked },
                { "FriendlyTags", checkBox7.Checked },
                { "AutoBalance", checkBox9.Checked },
                { "ShowTracers", checkBox10.Checked },
                { "ShowTeamClays", checkBox11.Checked },
                { "AllowAutoRange", checkBox12.Checked },
                { "MinPing", checkBox13.Checked },
                { "MinPingValue", Convert.ToInt32(textBox7.Text) },
                { "MaxPing", checkBox14.Checked },
                { "MaxPingValue", Convert.ToInt32(textBox8.Text) },
                { "StartList", JsonConvert.SerializeObject(selectedMapList) }
            };
            Dictionary<string, dynamic> response = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
            if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
            {
                MessageBox.Show("The server has been restarted successfully!\n\nPlease allow the RC a few moments to update.", "Success");
                this.Close();
            }
            else if ((OpenClass.Status)response["Status"] == OpenClass.Status.FAILURE)
            {
                MessageBox.Show("An error has occurred!", "Error");
                return;
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            string gameTypeShortCode = string.Empty;
            foreach (var gt in _state.autoRes.gameTypes)
            {
                if (gt.Value.Name == comboBox10.SelectedItem.ToString())
                {
                    gameTypeShortCode = gt.Key;
                    break;
                }
            }
            selectedMapList.Add(new MapList
            {
                CustomMap = selectedGameTypeMapList[listBox1.SelectedIndex].CustomMap,
                MapFile = selectedGameTypeMapList[listBox1.SelectedIndex].MapFile,
                MapName = selectedGameTypeMapList[listBox1.SelectedIndex].MapName,
                GameType = gameTypeShortCode
            });
            listBox2.Items.Add("|" + gameTypeShortCode + "| " + selectedGameTypeMapList[listBox1.SelectedIndex].MapName + " <" + selectedGameTypeMapList[listBox1.SelectedIndex].MapFile + ">");
        }
    }
}
