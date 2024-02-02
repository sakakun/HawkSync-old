using HawkSync_RC.classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace HawkSync_RC
{
    public partial class Profilelist : Form
    {
        AppState _state;
        RCSetup _setup;
        RC_ServerManager serverManager;
        Start_Game start_Game;
        public Timer Ticker;
        DataTable TVInstances;
        Bitmap img;
        Bitmap statusIMG;
        public Profilelist(AppState state, RCSetup setup)
        {
            InitializeComponent();
            _state = state;
            _setup = setup;
            TVInstances = new DataTable();
            Ticker = new Timer();
            Ticker.Tick += Ticker_Tick;
        }

        static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                // Save the bitmap to the stream in a specific format (e.g., PNG, JPEG)
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

                // Get the byte array from the memory stream
                return stream.ToArray();
            }
        }

        private void Ticker_Tick(object sender, EventArgs e)
        {
            UpdateButtons();
            foreach (var item in _state.Instances)
            {
                if ((_state.UserCodes.SuperAdmin == false) && (_state.UserCodes.Permissions.InstancePermissions.ContainsKey(item.Key)) && _state.UserCodes.Permissions.InstancePermissions[item.Key].Access == true)
                {
                    BindingSource bs = new BindingSource
                    {
                        DataSource = TVInstances
                    };
                    int rowID = bs.Find("ID", item.Value.Id);
                    if (rowID == -1)
                    {
                        break;
                    }
                    bs.Dispose();
                    DataRow updateRow = TVInstances.Rows[rowID];
                    if (item.Value.Status != classes.InstanceStatus.OFFLINE)
                    {
                        updateRow["Slots"] = item.Value.NumPlayers + "/" + item.Value.MaxSlots;
                        updateRow["Map"] = item.Value.Map;
                        updateRow["Game Type"] = item.Value.GameTypeName;
                        switch (item.Value.TimeRemaining)
                        {
                            case 0:
                                updateRow["Time Remaining"] = "< 1 Minute";
                                break;
                            case 1:
                                updateRow["Time Remaining"] = item.Value.TimeRemaining + " Minute";
                                break;
                            default:
                                updateRow["Time Remaining"] = item.Value.TimeRemaining + " Minutes";
                                break;
                        }
                        string path = string.Empty;
                        switch (item.Value.Status)
                        {
                            case classes.InstanceStatus.ONLINE:
                                statusIMG = HawkSync_RC.Properties.Resources.hosting;
                                break;
                            case classes.InstanceStatus.STARTDELAY:
                                statusIMG = HawkSync_RC.Properties.Resources.hosting;
                                break;
                            case classes.InstanceStatus.LOADINGMAP:
                                statusIMG = HawkSync_RC.Properties.Resources.loading;
                                break;
                            case classes.InstanceStatus.SCORING:
                                statusIMG = HawkSync_RC.Properties.Resources.scoring;
                                break;
                            default:
                                statusIMG = HawkSync_RC.Properties.Resources.notactive;
                                break;
                        }
                        updateRow["Server Status"] = BitmapToByteArray(statusIMG);
                        img.Dispose();
                        statusIMG.Dispose();
                    }
                    else
                    {
                        updateRow["Game Name"] = item.Value.GameName;
                        updateRow["Slots"] = string.Empty;
                        updateRow["Map"] = string.Empty;
                        updateRow["Game Type"] = string.Empty;
                        updateRow["Server Status"] = BitmapToByteArray(HawkSync_RC.Properties.Resources.notactive); 
                        img.Dispose();
                        statusIMG.Dispose();
                    }
                }else if (_state.UserCodes.SuperAdmin == true)
                {
                    DataRow updateRow = TVInstances.Rows[item.Key];
                    if (item.Value.Status != classes.InstanceStatus.OFFLINE)
                    {
                        updateRow["Slots"] = item.Value.NumPlayers + "/" + item.Value.MaxSlots;
                        updateRow["Map"] = item.Value.Map;
                        updateRow["Game Type"] = item.Value.GameTypeName;
                        switch (item.Value.TimeRemaining)
                        {
                            case 0:
                                updateRow["Time Remaining"] = "< 1 Minute";
                                break;
                            case 1:
                                updateRow["Time Remaining"] = item.Value.TimeRemaining + " Minute";
                                break;
                            default:
                                updateRow["Time Remaining"] = item.Value.TimeRemaining + " Minutes";
                                break;
                        }
                        string path = string.Empty;
                        switch (item.Value.Status)
                        {
                            case classes.InstanceStatus.ONLINE:
                                statusIMG = HawkSync_RC.Properties.Resources.hosting;
                                break;
                            case classes.InstanceStatus.STARTDELAY:
                                statusIMG = HawkSync_RC.Properties.Resources.hosting;
                                break;
                            case classes.InstanceStatus.LOADINGMAP:
                                statusIMG = HawkSync_RC.Properties.Resources.loading;
                                break;
                            case classes.InstanceStatus.SCORING:
                                statusIMG = HawkSync_RC.Properties.Resources.scoring;
                                break;
                            default:
                                statusIMG = HawkSync_RC.Properties.Resources.notactive;
                                break;
                        }
                        updateRow["Server Status"] = BitmapToByteArray(statusIMG);
                        img.Dispose();
                        statusIMG.Dispose();
                    }
                    else
                    {
                        updateRow["Game Name"] = item.Value.GameName;
                        updateRow["Slots"] = string.Empty;
                        updateRow["Map"] = string.Empty;
                        updateRow["Game Type"] = string.Empty;
                        updateRow["Server Status"] = BitmapToByteArray(HawkSync_RC.Properties.Resources.notactive);
                        img.Dispose();
                        statusIMG.Dispose();
                    }
                }
            }
        }

        private void UpdateButtons()
        {
            if (dataGridView1.CurrentCell == null)
            {
                return; // sanity check for sorting...
            }
            if (dataGridView1.CurrentCell.RowIndex == -1)
            {
                return; // no instance selected, don't open the winform
            }
            DataRow instanceRow = TVInstances.Rows[dataGridView1.CurrentCell.RowIndex];
            int serverid = Convert.ToInt32(instanceRow["ID"]);
            int ArrayID = -1;
            foreach (var instance in _state.Instances)
            {
                if (serverid == instance.Value.Id)
                {
                    ArrayID = instance.Key;
                    break;
                }
            }
            if (ArrayID == -1)
            {
                MessageBox.Show("We could not detect the instance that is selected.", "Oh no!");
                return;
            }
            if ((_state.UserCodes.SuperAdmin == true) || (_state.UserCodes.Permissions.InstancePermissions[ArrayID].Access == true && _state.UserCodes.Permissions.InstancePermissions[ArrayID] != null))
            {
                if (_state.Instances[ArrayID].Status == classes.InstanceStatus.OFFLINE)
                {
                    button1.Enabled = false;
                    button3.Text = "Start Game";
                }
                else
                {
                    button1.Enabled = true;
                    button3.Text = "Stop Game";
                }
            }
            else if (_state.UserCodes.Permissions.InstancePermissions[ArrayID].Access == false || _state.UserCodes.Permissions.InstancePermissions[ArrayID] == null)
            {
                button1.Enabled = false;
                button3.Enabled = false;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            byte[] memory = ms.ToArray();
            imageIn.Dispose();
            ms.Close();
            ms.Dispose();
            return memory;
        }

        private void Profilelist_Load(object sender, EventArgs e)
        {
            // setup timer interval
            Ticker.Interval = 1000;
            Ticker.Enabled = true;
            Ticker.Start();

            // setup datagridview and datatable...
            // prevents from having to rebind the datasource every time we update the table
            if (TVInstances.Columns.Count == 0)
            {
                TVInstances.Columns.Add("ID");
                TVInstances.Columns.Add("Game Name");
                TVInstances.Columns.Add("Mod", typeof(byte[]));
                TVInstances.Columns.Add("Slots");
                TVInstances.Columns.Add("Map");
                TVInstances.Columns.Add("Game Type");
                TVInstances.Columns.Add("Time Remaining");
                //TVInstances.Columns.Add("Web Stats Status", typeof(byte[]));
                TVInstances.Columns.Add("Web Stats Status");
                TVInstances.Columns.Add("Server Status", typeof(byte[]));
            }
            foreach (var instanceItem in _state.Instances)
            {
                if ((_state.UserCodes.SuperAdmin == false) && (_state.UserCodes.Permissions.InstancePermissions.ContainsKey(instanceItem.Key)) && _state.UserCodes.Permissions.InstancePermissions[instanceItem.Key].Access == true)
                {
                    if (_state.Instances[instanceItem.Key].Status != classes.InstanceStatus.OFFLINE)
                    {
                        DataRow newRow = TVInstances.NewRow();
                        newRow["ID"] = _state.Instances[instanceItem.Key].Id;
                        newRow["Game Name"] = _state.Instances[instanceItem.Key].GameName;
                        statusIMG = HawkSync_RC.Properties.Resources.notactive;
                        newRow["Slots"] = _state.Instances[instanceItem.Key].NumPlayers + "/" + _state.Instances[instanceItem.Key].MaxSlots;
                        newRow["Map"] = _state.Instances[instanceItem.Key].Map;
                        newRow["Game Type"] = _state.Instances[instanceItem.Key].GameTypeName;
                        switch (_state.Instances[instanceItem.Key].TimeRemaining)
                        {
                            case 0:
                                newRow["Time Remaining"] = "< 1 Minute";
                                break;
                            case 1:
                                newRow["Time Remaining"] = _state.Instances[instanceItem.Key].TimeRemaining + " Minute";
                                break;
                            default:
                                newRow["Time Remaining"] = _state.Instances[instanceItem.Key].TimeRemaining + " Minutes";
                                break;
                        }
                        switch (_state.Instances[instanceItem.Key].GameType)
                        {
                            case 1:
                                img = HawkSync_RC.Properties.Resources.bhdts;
                                break;
                            case 2:
                                img = HawkSync_RC.Properties.Resources.jo;
                                break;
                            default:
                                img = HawkSync_RC.Properties.Resources.bhd;
                                break;
                        }
                        newRow["Mod"] = BitmapToByteArray(img);
                        string path = string.Empty;
                        switch (_state.Instances[instanceItem.Key].Status)
                        {
                            case classes.InstanceStatus.ONLINE:
                                statusIMG = HawkSync_RC.Properties.Resources.hosting;
                                break;
                            case classes.InstanceStatus.STARTDELAY:
                                statusIMG = HawkSync_RC.Properties.Resources.hosting;
                                break;
                            case classes.InstanceStatus.LOADINGMAP:
                                statusIMG = HawkSync_RC.Properties.Resources.loading;
                                break;
                            case classes.InstanceStatus.SCORING:
                                statusIMG = HawkSync_RC.Properties.Resources.scoring;
                                break;
                            default:
                                statusIMG = HawkSync_RC.Properties.Resources.notactive;
                                break;
                        }
                        newRow["Server Status"] = BitmapToByteArray(statusIMG);
                        TVInstances.Rows.Add(newRow);
                        img.Dispose();
                        statusIMG.Dispose();
                    }
                    else
                    {
                        DataRow deadRow = TVInstances.NewRow();
                        deadRow["ID"] = _state.Instances[instanceItem.Key].Id;
                        deadRow["Game Name"] = _state.Instances[instanceItem.Key].GameName;
                        deadRow["Slots"] = string.Empty;
                        deadRow["Map"] = string.Empty;
                        deadRow["Game Type"] = string.Empty;
                        switch (_state.Instances[instanceItem.Key].GameType)
                        {
                            case 1:
                                img = HawkSync_RC.Properties.Resources.bhdts;
                                break;
                            case 2:
                                img = HawkSync_RC.Properties.Resources.jo;
                                break;
                            default:
                                img = HawkSync_RC.Properties.Resources.bhd;
                                break;
                        }
                        deadRow["Mod"] = BitmapToByteArray(img);
                        statusIMG = HawkSync_RC.Properties.Resources.notactive;
                        deadRow["Server Status"] = BitmapToByteArray(statusIMG);
                        TVInstances.Rows.Add(deadRow);
                        img.Dispose();
                        statusIMG.Dispose();
                    }
                }else if (_state.UserCodes.SuperAdmin == true)
                {
                    if (_state.Instances[instanceItem.Key].Status != classes.InstanceStatus.OFFLINE)
                    {
                        DataRow newRow = TVInstances.NewRow();
                        newRow["ID"] = _state.Instances[instanceItem.Key].Id;
                        newRow["Game Name"] = _state.Instances[instanceItem.Key].GameName;
                        statusIMG = HawkSync_RC.Properties.Resources.notactive;
                        newRow["Slots"] = _state.Instances[instanceItem.Key].NumPlayers + "/" + _state.Instances[instanceItem.Key].MaxSlots;
                        newRow["Map"] = _state.Instances[instanceItem.Key].Map;
                        newRow["Game Type"] = _state.Instances[instanceItem.Key].GameTypeName;
                        switch (_state.Instances[instanceItem.Key].TimeRemaining)
                        {
                            case 0:
                                newRow["Time Remaining"] = "< 1 Minute";
                                break;
                            case 1:
                                newRow["Time Remaining"] = _state.Instances[instanceItem.Key].TimeRemaining + " Minute";
                                break;
                            default:
                                newRow["Time Remaining"] = _state.Instances[instanceItem.Key].TimeRemaining + " Minutes";
                                break;
                        }
                        switch (_state.Instances[instanceItem.Key].GameType)
                        {
                            case 1:
                                img = HawkSync_RC.Properties.Resources.bhdts;
                                break;
                            case 2:
                                img = HawkSync_RC.Properties.Resources.jo;
                                break;
                            default:
                                img = HawkSync_RC.Properties.Resources.bhd;
                                break;
                        }
                        newRow["Mod"] = BitmapToByteArray(img);
                        string path = string.Empty;
                        switch (_state.Instances[instanceItem.Key].Status)
                        {
                            case classes.InstanceStatus.ONLINE:
                                statusIMG = HawkSync_RC.Properties.Resources.hosting;
                                break;
                            case classes.InstanceStatus.STARTDELAY:
                                statusIMG = HawkSync_RC.Properties.Resources.hosting;
                                break;
                            case classes.InstanceStatus.LOADINGMAP:
                                statusIMG = HawkSync_RC.Properties.Resources.loading;
                                break;
                            case classes.InstanceStatus.SCORING:
                                statusIMG = HawkSync_RC.Properties.Resources.scoring;
                                break;
                            default:
                                statusIMG = HawkSync_RC.Properties.Resources.notactive;
                                break;
                        }
                        newRow["Server Status"] = BitmapToByteArray(statusIMG);
                        TVInstances.Rows.Add(newRow);
                        img.Dispose();
                        statusIMG.Dispose();
                    }
                    else
                    {
                        DataRow deadRow = TVInstances.NewRow();
                        deadRow["ID"] = _state.Instances[instanceItem.Key].Id;
                        deadRow["Game Name"] = _state.Instances[instanceItem.Key].GameName;
                        deadRow["Slots"] = string.Empty;
                        deadRow["Map"] = string.Empty;
                        deadRow["Game Type"] = string.Empty;
                        switch (_state.Instances[instanceItem.Key].GameType)
                        {
                            case 1:
                                img = HawkSync_RC.Properties.Resources.bhdts;
                                break;
                            case 2:
                                img = HawkSync_RC.Properties.Resources.jo;
                                break;
                            default:
                                img = HawkSync_RC.Properties.Resources.bhd;
                                break;
                        }
                        deadRow["Mod"] = BitmapToByteArray(img);
                        statusIMG = HawkSync_RC.Properties.Resources.notactive;
                        deadRow["Server Status"] = BitmapToByteArray(statusIMG);
                        TVInstances.Rows.Add(deadRow);
                        img.Dispose();
                        statusIMG.Dispose();
                    }
                }
            }
            dataGridView1.DataSource = TVInstances;
            SetupTable();

            if (_state.UserCodes.SuperAdmin == true)
            {
                button6.Enabled = true;
            }
            if (_state.UserCodes.SubAdmin > 0)
            {
                button6.Enabled = false;
            }
            /*if (_state.UserCodes.SuperAdmin == true || _state.UserCodes.Permissions.)
            {
                
            }*/
        }
        private void SetupTable()
        {
            dataGridView1.Columns["ID"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["ID"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns["Game Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["Game Name"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns["Mod"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["Mod"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns["Slots"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["Slots"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns["Map"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["Map"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns["Game Type"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["Game Type"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns["Time Remaining"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["Time Remaining"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns["Web Stats Status"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["Web Stats Status"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns["Server Status"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["Server Status"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private void Profilelist_FormClosing(object sender, FormClosingEventArgs e)
        {
            Ticker.Stop();
            Ticker.Enabled = false;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            // rotation manager
            if (dataGridView1.CurrentCell.RowIndex != -1)
            {
                DataRow row = TVInstances.Rows[dataGridView1.CurrentCell.RowIndex];
                RC_RotationManager rotationManager = new RC_RotationManager(_state, _setup, Convert.ToInt32(row["ID"]));
                rotationManager.ShowDialog();
            }
            else
            {
                MessageBox.Show("We could not detect which profile you selected.\nPlease restart BMT RC.", "Unknown Error");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentCell.RowIndex == -1)
            {
                return; // no instance selected, don't open the winform
            }
            DataRow instanceRow = TVInstances.Rows[dataGridView1.CurrentCell.RowIndex];
            int serverid = Convert.ToInt32(instanceRow["ID"]);
            int ArrayID = -1;
            foreach (var instance in _state.Instances)
            {
                if (serverid == instance.Value.Id)
                {
                    ArrayID = instance.Key;
                    break;
                }
            }
            if (ArrayID == -1)
            {
                MessageBox.Show("We could not detect the instance that is selected.", "Oh no!");
                return;
            }
            serverManager = new RC_ServerManager(_state, _setup, ArrayID);
            serverManager.ShowDialog();
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentCell == null)
            {
                return; // sanity check for sorting...
            }
            if (dataGridView1.CurrentCell.RowIndex == -1)
            {
                return; // no instance selected, don't open the winform
            }

            DataRow instanceRow = TVInstances.Rows[dataGridView1.CurrentCell.RowIndex];
            int serverid = Convert.ToInt32(instanceRow["ID"]);
            int ArrayID = -1;
            foreach (var instance in _state.Instances)
            {
                if (serverid == instance.Value.Id)
                {
                    ArrayID = instance.Key;
                    break;
                }
            }
            if (ArrayID == -1)
            {
                MessageBox.Show("We could not detect the instance that is selected.", "Oh no!");
                return;
            }



            if (_state.UserCodes.SuperAdmin == false && _state.UserCodes.Permissions.InstancePermissions[ArrayID].StartInstance == true && _state.Instances[ArrayID].Status == InstanceStatus.OFFLINE)
            {
                button3.Enabled = false;
                button3.Text = "Start Game";
                //return;
            }
            else if (_state.UserCodes.SuperAdmin == true && _state.Instances[ArrayID].Status == InstanceStatus.OFFLINE)
            {
                button1.Enabled = false;
                button3.Text = "Start Game";
            }
            else if (_state.UserCodes.SuperAdmin == false && _state.UserCodes.Permissions.InstancePermissions[ArrayID].StartInstance == false && _state.Instances[ArrayID].Status == InstanceStatus.OFFLINE)
            {
                button3.Enabled = false;
            }













            if ((_state.UserCodes.SuperAdmin == true) || (_state.UserCodes.Permissions.InstancePermissions[ArrayID].Access == true && _state.UserCodes.Permissions.InstancePermissions[ArrayID] != null))
            {
                if (_state.Instances[ArrayID].Status == InstanceStatus.OFFLINE)
                {
                    button1.Enabled = false;
                    button3.Text = "Start Game";
                }
                else
                {
                    button1.Enabled = true;
                    button3.Text = "Stop Game";
                }
            }
            else if (_state.UserCodes.Permissions.InstancePermissions[ArrayID].Access == false || _state.UserCodes.Permissions.InstancePermissions[ArrayID] == null)
            {
                button1.Enabled = false;
                button3.Enabled = false;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            /*UserManager_Old userManager = new UserManager_Old(_state, _setup);
            userManager.ShowDialog();*/
            UserManager userManager = new UserManager(_state, _setup);
            userManager.ShowDialog();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentCell.RowIndex == -1)
            {
                return; // no instance selected, don't open the winform
            }
            DataRow instanceRow = TVInstances.Rows[dataGridView1.CurrentCell.RowIndex];
            int serverid = Convert.ToInt32(instanceRow["ID"]);
            int ArrayID = -1;
            foreach (var instance in _state.Instances)
            {
                if (serverid == instance.Value.Id)
                {
                    ArrayID = instance.Key;
                    break;
                }
            }
            if (ArrayID == -1)
            {
                MessageBox.Show("We could not detect the instance that is selected.", "Oh no!");
                return;
            }

            if (_state.Instances[ArrayID].Status == classes.InstanceStatus.OFFLINE)
            {
                start_Game = new Start_Game(_state, _setup, ArrayID);
                start_Game.ShowDialog();
            }
            else
            {
                DialogResult serverStopMsg = MessageBox.Show("Are you sure you want to stop this instance?", "Attention!", MessageBoxButtons.YesNo);
                if (serverStopMsg == DialogResult.Yes)
                {
                    try
                    {
                        Dictionary<string, dynamic> request = new Dictionary<string, dynamic>()
                        {
                            { "SessionID", _setup.SessionID },
                            { "action", "BMTRC.StopInstance" },
                            { "serverID", _state.Instances[ArrayID].Id }
                        };
                        byte[] responseBytes = _setup.SendCMD(request);
                        Dictionary<string, dynamic> response = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(Encoding.Default.GetString(responseBytes));
                        if ((OpenClass.Status)response["Status"] == OpenClass.Status.FAILURE)
                        {
                            MessageBox.Show("An unknown error has occurred!\nPlease try your request again later.", "Error");
                            return;
                        }
                    }
                    catch
                    {
                        MessageBox.Show("An unknown error has occurred!\nPlease try your request again later.", "Error");
                        return;
                    }
                }
            }
        }

    }
}
