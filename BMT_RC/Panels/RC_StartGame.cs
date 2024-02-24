using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using HawkSync_RC.classes;
using Newtonsoft.Json;

namespace HawkSync_RC
{
    public partial class RC_StartGame : Form
    {
        private readonly AppState _state;
        private readonly int ArrayID = -1;
        private Dictionary<int, MapList> avalMaps;
        private readonly RCSetup RCSetup;
        private readonly List<MapList> selectedGameTypeMapList = new List<MapList>();
        private readonly List<MapList> selectedMapList = new List<MapList>();

        public RC_StartGame(AppState state, RCSetup setup, int arrayID)
        {
            InitializeComponent();
            _state = state;
            ArrayID = arrayID;
            RCSetup = setup;
            avalMaps = new Dictionary<int, MapList>();
        }

        private void Start_Game_Load(object sender, EventArgs e)
        {
            var cmd = new Dictionary<dynamic, dynamic>
            {
                { "action", "BMTRC.GetAvalMaps" },
                { "serverID", _state.Instances[ArrayID].Id },
                { "SessionID", RCSetup.SessionID }
            };
            var compressReply = RCSetup.SendCMD(cmd);
            var data = JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(
                Encoding.Default.GetString(compressReply));
            if ((OpenClass.Status)data["Status"] == OpenClass.Status.SUCCESS)
                avalMaps = JsonConvert.DeserializeObject<Dictionary<int, MapList>>(data["maps"]);
            foreach (var gameType in _state.autoRes.gameTypes) comboBox10.Items.Add(gameType.Value.Name);
            comboBox10.SelectedIndex = 0;
            // fill slot
            var slotnum = 1;
            while (slotnum < 51)
            {
                comboBox_maxPlayers.Items.Add(slotnum);
                slotnum++;
            }

            // end fill slot
            // maxkills
            for (var maxkills = 1; maxkills < 501; maxkills++)
            {
                comboBox_maxKills.Items.Add(maxkills);
                comboBox_flagsScored.Items.Add(maxkills);
            }

            //end maxkills
            for (var zonetimer = 1; zonetimer < 61; zonetimer++)
            {
                comboBox_zoneTimer.Items.Add(zonetimer);
                comboBox_timeLimit.Items.Add(zonetimer);
            }

            for (var respawntime = 1; respawntime < 121; respawntime++) comboBox_respawnTime.Items.Add(respawntime);
            textBox_serverName.Text = _state.Instances[ArrayID].ServerName;
            textBox_MOTD.Text = _state.Instances[ArrayID].MOTD;
            textBox_countryCode.Text = _state.Instances[ArrayID].CountryCode;
            textBox_serverPassword.Text = _state.Instances[ArrayID].Password;
            comboBox_sessionType.SelectedIndex = 0;
            comboBox_sessionType.Enabled = false;
            comboBox_maxPlayers.SelectedItem = _state.Instances[ArrayID].MaxSlots;
            comboBox_startDelay.SelectedIndex = _state.Instances[ArrayID].StartDelay;
            checkBox_loopMaps.Checked = Convert.ToBoolean(_state.Instances[ArrayID].LoopMaps);
            comboBox_maxKills.SelectedIndex = _state.Instances[ArrayID].MaxKills;
            comboBox_flagsScored.SelectedIndex = _state.Instances[ArrayID].GameScore;
            comboBox_zoneTimer.SelectedIndex = _state.Instances[ArrayID].ZoneTimer;
            comboBox_respawnTime.SelectedIndex = _state.Instances[ArrayID].RespawnTime;
            comboBox_timeLimit.SelectedIndex = _state.Instances[ArrayID].TimeLimit;
            checkBox_reqNova.Checked = _state.Instances[ArrayID].RequireNovaLogin;
            checkBox_windowMode.Checked = _state.Instances[ArrayID].WindowedMode;
            checkBox_customSkin.Checked = _state.Instances[ArrayID].AllowCustomSkins;
            checkBox_runDedicated.Checked = _state.Instances[ArrayID].Dedicated;
            textBox_passBlue.Text = _state.Instances[ArrayID].BluePassword;
            textBox_passRed.Text = _state.Instances[ArrayID].RedPassword;
            checkBox_ffire.Checked = _state.Instances[ArrayID].FriendlyFire;
            checkBox_ffireWarn.Checked = _state.Instances[ArrayID].FriendlyFireWarning;
            checkBox_ftags.Checked = _state.Instances[ArrayID].FriendlyTags;
            checkBox_autoBal.Checked = _state.Instances[ArrayID].AutoBalance;
            checkBox_showTrace.Checked = _state.Instances[ArrayID].ShowTracers;
            checkBox_showTeamClays.Checked = _state.Instances[ArrayID].ShowTeamClays;
            checkBox_autoRange.Checked = _state.Instances[ArrayID].AllowAutoRange;
            switch (_state.Instances[ArrayID].MinPing)
            {
                case false:
                    checkBox_minPing.Checked = false;
                    textBox_minPing.Text = "0";
                    break;
                case true:
                    textBox_minPing.Text = Convert.ToString(_state.Instances[ArrayID].MinPingValue);
                    checkBox_minPing.Checked = true;
                    break;
            }

            switch (_state.Instances[ArrayID].MaxPing)
            {
                case false:
                    checkBox_maxPing.Checked = false;
                    textBox_maxPing.Text = "0";
                    break;
                case true:
                    textBox_maxPing.Text = Convert.ToString(_state.Instances[ArrayID].MaxPingValue);
                    checkBox_maxPing.Checked = true;
                    break;
            }

            selectedMapList.Clear();
            foreach (var map in _state.Instances[ArrayID].MapList)
            {
                selectedMapList.Add(map.Value);
                listBox2.Items.Add("|" + map.Value.GameType + "| " + map.Value.MapName + " <" + map.Value.MapFile +
                                   ">");
            }

            label_mapCount.Text = listBox2.Items.Count + " / 128";
        }

        private void comboBox10_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            selectedGameTypeMapList.Clear();

            foreach (var mapItem in avalMaps)
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

        private void checkBox13_CheckedChanged(object sender, EventArgs e)
        {
            textBox_minPing.Enabled = checkBox_minPing.Checked;
        }

        private void checkBox14_CheckedChanged(object sender, EventArgs e)
        {
            textBox_maxPing.Enabled = checkBox_maxPing.Checked;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // reset maplist
            listBox2.Items.Clear();
            selectedMapList.Clear();
        }

        private void listBox2_DoubleClick(object sender, EventArgs e)
        {
            if (listBox2.SelectedIndex == -1) return; // don't do anything since nothing is selected
            selectedMapList.RemoveAt(listBox2.SelectedIndex);
            listBox2.Items.Remove(listBox2.SelectedItem);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // update settings and start game
            // server name
            if (string.IsNullOrEmpty(textBox_serverName.Text) || string.IsNullOrWhiteSpace(textBox_serverName.Text))
            {
                MessageBox.Show("Please enter a valid Server Name!", "Error");
                return;
            }

            if (string.IsNullOrEmpty(textBox_MOTD.Text) || string.IsNullOrWhiteSpace(textBox_MOTD.Text))
            {
                MessageBox.Show("Please enter a valid MOTD!", "Error");
                return;
            }

            if (string.IsNullOrEmpty(textBox_countryCode.Text) || string.IsNullOrWhiteSpace(textBox_countryCode.Text) ||
                textBox_countryCode.TextLength > 2)
            {
                MessageBox.Show("Please enter a valid 2 character Country Code!", "Error");
                return;
            }

            if (listBox2.Items.Count == 0)
            {
                MessageBox.Show("Please enter at least 1 start up map!", "Error");
                return;
            }

            var request = new Dictionary<string, dynamic>
            {
                { "SessionID", RCSetup.SessionID },
                { "action", "BMTRC.StartInstance" },
                { "serverID", _state.Instances[ArrayID].Id },
                { "ServerName", textBox_serverName.Text },
                { "MOTD", textBox_MOTD.Text },
                { "CountryCode", textBox_countryCode.Text },
                { "password", textBox_serverPassword.Text },
                { "SessionType", comboBox_sessionType.SelectedIndex },
                { "MaxSlots", Convert.ToInt32(comboBox_maxPlayers.SelectedItem) },
                { "StartDelay", comboBox_startDelay.SelectedIndex },
                { "MaxKills", comboBox_maxKills.SelectedIndex },
                { "GameScore", comboBox_flagsScored.SelectedIndex },
                { "ZoneTimer", comboBox_zoneTimer.SelectedIndex },
                { "RespawnTimer", comboBox_respawnTime.SelectedIndex },
                { "TimeLimit", comboBox_timeLimit.SelectedIndex },
                { "RequireNovaLogin", checkBox_reqNova.Checked },
                { "WindowedMode", checkBox_windowMode.Checked },
                { "AllowCustomSkins", checkBox_customSkin.Checked },
                { "Dedicated", checkBox_runDedicated.Checked },
                { "BluePassword", textBox_passBlue.Text },
                { "RedPassword", textBox_passRed.Text },
                { "FriendlyFire", checkBox_ffire.Checked },
                { "FriendlyFireWarning", checkBox_ffireWarn.Checked },
                { "FriendlyTags", checkBox_ftags.Checked },
                { "AutoBalance", checkBox_autoBal.Checked },
                { "ShowTracers", checkBox_showTrace.Checked },
                { "ShowTeamClays", checkBox_showTeamClays.Checked },
                { "AllowAutoRange", checkBox_autoRange.Checked },
                { "MinPing", checkBox_minPing.Checked },
                { "MinPingValue", Convert.ToInt32(textBox_minPing.Text) },
                { "MaxPing", checkBox_maxPing.Checked },
                { "MaxPingValue", Convert.ToInt32(textBox_maxPing.Text) },
                { "StartList", JsonConvert.SerializeObject(selectedMapList) }
            };
            var response =
                JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                    Encoding.ASCII.GetString(RCSetup.SendCMD(request)));
            if ((OpenClass.Status)response["Status"] == OpenClass.Status.SUCCESS)
            {
                MessageBox.Show(
                    "The server has been restarted successfully!\n\nPlease allow the RC a few moments to update.",
                    "Success");
                Close();
            }
            else if ((OpenClass.Status)response["Status"] == OpenClass.Status.FAILURE)
            {
                MessageBox.Show("An error has occurred!", "Error");
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            var gameTypeShortCode = string.Empty;
            foreach (var gt in _state.autoRes.gameTypes)
                if (gt.Value.Name == comboBox10.SelectedItem.ToString())
                {
                    gameTypeShortCode = gt.Key;
                    break;
                }

            selectedMapList.Add(new MapList
            {
                CustomMap = selectedGameTypeMapList[listBox1.SelectedIndex].CustomMap,
                MapFile = selectedGameTypeMapList[listBox1.SelectedIndex].MapFile,
                MapName = selectedGameTypeMapList[listBox1.SelectedIndex].MapName,
                GameType = gameTypeShortCode
            });
            listBox2.Items.Add("|" + gameTypeShortCode + "| " +
                               selectedGameTypeMapList[listBox1.SelectedIndex].MapName + " <" +
                               selectedGameTypeMapList[listBox1.SelectedIndex].MapFile + ">");
        }

        private void mapAction_clickLoadRotation(object sender, EventArgs e)
        {
            bool MapListEdited = false;
            // load rotation
            var popup_Load = new RC_PopupLoadRotation(_state, RCSetup, ArrayID);
            popup_Load.ShowDialog();
            if (RC_ServerManager.loadList.Count > 0) MapListEdited = true;
            if (MapListEdited)
            {
                listBox2.Items.Clear();
                foreach (var item in RC_ServerManager.loadList)
                {
                    listBox2.Items.Add("|" + item.GameType + "| " + item.MapName + " <" + item.MapFile + ">");
                }

                label_mapCount.Text = listBox2.Items.Count + " / 128";
                MessageBox.Show("Map rotation has been loaded!\nPlease click update maps to update the server.",
                    "Success");
            }
        }
    }
}